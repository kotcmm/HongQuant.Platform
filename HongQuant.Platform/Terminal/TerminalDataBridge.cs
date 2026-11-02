using HongQuant.Analysis.Original;
using HongQuant.Analysis.YiTou;
using HongQuant.Analysis.YiTou.Configs;
using HongQuant.Analysis.YiTou.IncomeStatistics;
using HongQuant.Core.Models;
using HongQuant.Core.Services;
using HongQuant.Core.Stores;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Splat;

namespace HongQuant.Platform.Terminal;

/// <summary>
/// 原生业务数据到研究终端 HTML 的单向快照。所有数值来自实际导入的 IDataFile，
/// 计算使用原有 CycleIncomeService；前端原型的演示常量绝不参与计算。
/// </summary>
internal sealed class TerminalDataBridge
{
    private readonly IOriginalService _original = Locator.Current.GetService<IOriginalService>()
        ?? throw new InvalidOperationException("未注册基础数据服务");

    // BasicDataFile.Id 没有赋值，不能用它做复选框身份；以实际文件路径区分同品种不同策略。
    private readonly HashSet<string> _selectedFileIds = new(StringComparer.OrdinalIgnoreCase);
    private string _activeTemplateId = string.Empty;
    // 初始保持与当前版本一致：收益目标和时间上限都参与计算。
    private bool _useIncome = true;
    private bool _useTime = true;

    internal void SetStoppingOptions(bool useIncome, bool useTime)
    {
        if (_useIncome == useIncome && _useTime == useTime) return;
        _useIncome = useIncome;
        _useTime = useTime;
        InvalidateStatistics();
    }

    private readonly Dictionary<string, double> _parameters = new()
    {
        ["ret"] = 100, ["months"] = 36, ["lev0"] = 1, ["levStep"] = 1,
        ["levMax"] = 6, ["interval"] = 3, ["period"] = 12
    };
    // 两种统计计算开销较大，缓存当前数据源和参数下的结果；
    // 勾选品种、刷新标题栏等操作不应重复计算收益序列。
    private IDataFile? _cachedSource;
    private object? _cachedCompound;
    private object? _cachedSettle;

    private void InvalidateStatistics()
    {
        _cachedSource = null;
        _cachedCompound = null;
        _cachedSettle = null;
    }

    private List<IDataFile> _files = [];
    private List<SourceEntry> _sources = [];
    private string _sourceId = string.Empty;
    internal string SourceId => _sourceId;
    internal string FolderPath
    {
        get => GlobalSettingStroe.GetValue("FolderPath") ?? string.Empty;
        set => GlobalSettingStroe.SetValue(new GlobalSetting { Key = "FolderPath", Value = value });
    }

    internal void SetParameter(string key, double value)
    {
        if (!_parameters.ContainsKey(key) || !double.IsFinite(value)) return;
        var min = key switch { "months" or "period" or "interval" => 1, "ret" => 1, "lev0" => .5, "levStep" => .5, _ => 1 };
        var normalized = Math.Max(min, Math.Min(value, 10000));
        if (_parameters[key] == normalized) return;
        _parameters[key] = normalized;
        InvalidateStatistics();
    }

    internal void SelectSource(string id)
    {
        if (_sourceId == id) return;
        _sourceId = id;
        InvalidateStatistics();
    }

    internal bool TemplateSync =>
        bool.TryParse(GlobalSettingStroe.GetValue("TemplateSync"), out var enabled) && enabled;

    internal void ToggleTemplateSync() =>
        GlobalSettingStroe.SetValue(new GlobalSetting
        {
            Key = "TemplateSync",
            Value = (!TemplateSync).ToString()
        });

    private static string FileKey(IDataFile file) => file is BasicDataFile basic
        ? basic.FileInfo.FullName
        : file.Name;

    private static string ComboSourceId(IDataFile combo)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combo.Id));
        return "combo-" + Convert.ToHexString(hash.AsSpan(0, 12));
    }

    internal void CloseCombination(string sourceId)
    {
        IndexSources();
        var combo = _sources.FirstOrDefault(x => x.Id == sourceId && x.Group == "组合");
        if (combo is null) return;
        _original.Remove(combo.File.Id);
        InvalidateStatistics();
        if (_sourceId == sourceId)
            _sourceId = _sources.FirstOrDefault(x => x.Group == "单品")?.Id ?? string.Empty;
    }

    internal void SelectTemplate(string templateId)
    {
        _activeTemplateId = templateId;
        _selectedFileIds.Clear();
        if (Guid.TryParse(templateId, out var guid))
        {
            var template = _original.ProductTemplates.Items.FirstOrDefault(x => x.Id == guid);
            if (template is not null)
                foreach (var file in _files.Where(file =>
                    template.Products.Any(p => p.Product == file.Product && p.Strategy == file.Strategy)))
                    _selectedFileIds.Add(FileKey(file));
        }
    }

    internal void SaveSelectionAsTemplate(string name)
    {
        var selected = _files.Where(f => _selectedFileIds.Contains(FileKey(f))).ToList();
        if (selected.Count == 0) throw new InvalidOperationException("请先勾选要保存为模板的品种。");
        if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("请输入组合模板名称。");
        name = name.Trim();
        var existing = _original.ProductTemplates.Items.FirstOrDefault(x => x.TemplateName == name);
        var template = new ProductTemplate
        {
            Id = existing?.Id ?? Guid.NewGuid(),
            TemplateName = name,
            Products = selected.Select(f => new SelectProduct(f.Product, f.Strategy)).Distinct().ToList()
        };
        _original.SaveProductTemplate(template);
        _activeTemplateId = template.Id.ToString("D");
    }

    internal IReadOnlyList<IDataFile> GetSelectedFiles() =>
        _files.Where(f => _selectedFileIds.Contains(FileKey(f))).ToList();

    internal void SelectAssets(IEnumerable<int> indexes)
    {
        // 手工修改勾选后退出模板筛选，避免只显示之前模板中的文件。
        _activeTemplateId = string.Empty;
        _selectedFileIds.Clear();
        foreach (var i in indexes)
            if (i >= 0 && i < _files.Count)
                _selectedFileIds.Add(FileKey(_files[i]));
    }

    internal void LoadFiles(IEnumerable<string> paths)
    {
        var files = paths.Where(File.Exists).Select(path => new FileInfo(path)).ToList();
        if (files.Count == 0) return;
        // 导入新一批文件前清空旧的临时组合，避免不同数据批次交叉引用。
        foreach (var existing in _original.CombinationFileIndexs.Items.ToArray())
            _original.Remove(existing.Id);
        _original.LoadFile(files);
        InvalidateStatistics();
        if (bool.TryParse(GlobalSettingStroe.GetValue("TemplateSync"), out var sync) && sync)
        {
            foreach (var template in _original.ProductTemplates.Items)
            {
                var matches = _original.FileIndexs.Items.Where(f =>
                    template.Products.Any(p => p.Product == f.Product && p.Strategy == f.Strategy)).ToList();
                if (matches.Count > 0) _original.Combination(matches, template.TemplateName);
            }
        }
        _selectedFileIds.Clear(); // 与旧版保持一致：导入后由用户勾选，不自动全选。
        _activeTemplateId = string.Empty;
        _sourceId = string.Empty;
    }

    internal void SynchronizeCombination()
    {
        IndexSources();
        var files = _files.Where(x => _selectedFileIds.Contains(FileKey(x))).ToList();
        if (files.Count == 0) throw new InvalidOperationException("请先勾选参与组合计算的品种。");
        // 直接使用原有 CombinationBasicDataFile.DataNameToString 命名规则：
        // YD01(20)；多个策略则使用 GD20(12), GD25(8) 等分组名称。
        // 用原有组合 ID 找回结果，不另造“组合 + 时间”的命名或依赖显示名查找。
        var combinationId = new CombinationBasicDataFile(files).Id;
        _original.Combination(files);
        var result = _original.CombinationFileIndexs.Items.FirstOrDefault(x => x.Id == combinationId);
        if (result is null) throw new InvalidOperationException("组合计算未生成结果。");
        _sourceId = ComboSourceId(result);
        InvalidateStatistics();
    }

    internal void ExportSelected(string folder)
    {
        Directory.CreateDirectory(folder);
        var exports = _files.Concat(_original.CombinationFileIndexs.Items).ToList();
        if (exports.Count == 0) throw new InvalidOperationException("当前没有可导出的数据。");
        // 与改造前的 OriginalMain.Export 一致：导出全部基础数据与组合。
        foreach (var item in exports) _original.Save(folder, item);
    }

    internal void ExportStatistics(string filename, string mode)
    {
        var source = _sources.FirstOrDefault(x => x.Id == _sourceId)?.File;
        if (source is null) throw new InvalidOperationException("当前没有可导出的统计结果。");
        var service = Calculate(source, mode);
        service.Save(filename, service.IncomeStatistics.Items);
    }

    private sealed record SourceEntry(string Id, string Code, string Name, string Group, string Note, IDataFile File);

    private void IndexSources()
    {
        _files = _original.FileIndexs.Items.OrderBy(x => x.Product, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
        var combos = _original.CombinationFileIndexs.Items.OrderBy(x => x.Name).ToList();
        _sources = [];
        for (var i = 0; i < combos.Count; i++)
        {
            var name = combos[i].Name;
            _sources.Add(new SourceEntry(ComboSourceId(combos[i]), name, name, "组合", name, combos[i]));
        }
        for (var i = 0; i < _files.Count; i++)
        {
            var f = _files[i];
            _sources.Add(new SourceEntry($"file-{i}", f.Product, f.Product, "单品", f.Name, f));
        }

        if (_sources.All(x => x.Id != _sourceId))
            _sourceId = _sources.FirstOrDefault(x => x.Group == "单品")?.Id
                ?? _sources.FirstOrDefault()?.Id ?? string.Empty;

        _selectedFileIds.RemoveWhere(id => _files.All(f => FileKey(f) != id));
    }

    private CycleIncomeService Calculate(IDataFile source, string mode)
    {
        var config = mode == "settle"
            ? (IConfigIncome)new CycleConfigIncome { Month = (int)_parameters["period"] }
            : new Multiple5ConfigIncome
            {
                MaxIncome = _parameters["ret"],
                MaxTime = (int)_parameters["months"],
                Start = _parameters["lev0"],
                Increment = _parameters["levStep"],
                Max = Math.Max(_parameters["levMax"], _parameters["lev0"]),
                Month = (int)_parameters["interval"],
                UseIncome = _useIncome,
                UseTime = _useTime
            };
        var algorithm = mode == "settle" ? AlgorithmType.Cycle : AlgorithmType.Multiple5;
        var records = source.Items.OrderBy(x => x.TradeDate)
            .Select(x => new IncomeInfoModel(x.TradeDate.ToDateTime(TimeOnly.MinValue), 0, x.Yields))
            .ToList();
        var service = new CycleIncomeService();
        service.StatisticsData(config, algorithm, records);
        return service;
    }

    private static string Day(DateTime date) => date.ToString("yyyyMMdd");
    private static object Row(BaseIncomeStatistics s, bool compound) => new object[]
    {
        Day(s.Start), Day(s.End), s.Total, s.MaxRetracement,
        Day(s.RetracementDate), s.MaxTotal, Day(s.MaxTotalDate), s.DayCount,
        compound ? s.LastMultiples : 0
    };

    private static object[] Points(BaseIncomeStatistics s)
    {
        var lev = s is IMultiples withLev ? withLev.Multiples : null;
        return s.IncomeInfoResults.Select((x, i) => (object)new object?[]
        {
            Day(x.Date), x.NetWorth, x.RetracementRate / 100d,
            lev is not null && i < lev.Count ? lev[i].Multiples : null
        }).ToArray();
    }

    private object ComputeRows(IDataFile source, string mode)
    {
        var stats = Calculate(source, mode).IncomeStatistics.Items.ToList();
        var compound = mode == "compound";

        // 明细必须呈现所有实际产生交易记录的样本（含未达标、未完成目标的样本）。
        // 只把达标判断用于 KPI，不得在数据桥上提前裁剪表格/图表的数据。
        // 与原算法一致：无任何交易日的空对象不构成可展示的样本。
        var rows = stats.Where(s => s.IncomeInfoResults.Count > 0 && s.DayCount > 0)
            .ToList();
        var qualifiedIndexes = rows
            .Select((sample, index) => (sample, index))
            .Where(x => !compound ||
                ((!_useIncome || x.sample.Total >= _parameters["ret"] - 1e-6) &&
                 (!_useTime || x.sample.DayCount <= _parameters["months"] * 31 + 2) &&
                 x.sample.LastMultiples <= _parameters["levMax"] + 1e-6))
            .Select(x => x.index)
            .ToArray();

        return new
        {
            rows = rows.Select(s => Row(s, compound)).ToArray(),
            curves = rows.Select(Points).ToArray(),
            qualifiedIndexes,
            all = rows.Count
        };
    }

    internal string BuildState(int requestId = 0)
    {
        IndexSources();
        var source = _sources.FirstOrDefault(x => x.Id == _sourceId)?.File;
        var equity = source?.Items.OrderBy(x => x.TradeDate).Select(x => (object)new object[]
        {
            x.TradeDate.ToString("yyyy-MM-dd"), x.Equity, x.TodayProfitAndLoss,
            x.Yields * 100, x.TotalYields * 100, x.WithdrawDeep * 100,
            x.MaxWithdraw * 100, x.WithdrawLevel * 100, x.Product
        }).ToArray() ?? [];
        var assets = _files.Select(f => new[] { f.Product, f.Name, f.Name, FileKey(f) }).ToArray();
        var selected = _files.Select((f, index) => (f, index))
            .Where(x => _selectedFileIds.Contains(FileKey(x.f))).Select(x => x.index).ToArray();
        var selectedSource = _sources.FirstOrDefault(x => x.Id == _sourceId);

        if (source is not null && !ReferenceEquals(source, _cachedSource))
        {
            _cachedSource = null;
            _cachedCompound = ComputeRows(source, "compound");
            _cachedSettle = ComputeRows(source, "settle");
            _cachedSource = source;
        }
        var result = new
        {
            requestId,
            sourceId = _sourceId,
            sources = _sources.Select(x => new
            {
                id = x.Id, name = x.Name, code = x.Code, grp = x.Group, note = x.Note
            }).ToArray(),
            assets, selected,
            templateSync = TemplateSync,
            stoppingOptions = new { useIncome = _useIncome, useTime = _useTime },
            activeTemplateId = _activeTemplateId,
            templates = _original.ProductTemplates.Items.OrderBy(x => x.TemplateName)
                .Select(x => new { id = x.Id.ToString("D"), name = x.TemplateName }).ToArray(),
            singleSourceId = _sources.FirstOrDefault(x => x.Group == "单品")?.Id ?? string.Empty,
            equity,
            compound = source is null ? null : _cachedCompound,
            settle = source is null ? null : _cachedSettle,
            dateStart = equity.Length > 0 ? ((object[])equity[0])[0] : null,
            dateEnd = equity.Length > 0 ? ((object[])equity[^1])[0] : null,
            status = source is null ? "未导入数据" : "数据源已连接",
            folderPath = FolderPath,
            parameters = _parameters,
            activeSourceName = selectedSource?.Name ?? "未选择"
        };
        return JsonSerializer.Serialize(result);
    }
}
