using HongQuant.Analysis.Original;
using HongQuant.Analysis.YiTou;
using HongQuant.Analysis.YiTou.Configs;
using HongQuant.Analysis.YiTou.IncomeStatistics;
using HongQuant.Core.Models;
using HongQuant.Core.Services;
using HongQuant.Core.Stores;
using System.IO;
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

    private readonly HashSet<string> _selectedFileIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, double> _parameters = new()
    {
        ["ret"] = 100, ["months"] = 36, ["lev0"] = 1, ["levStep"] = 1,
        ["levMax"] = 6, ["interval"] = 3, ["period"] = 12
    };
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
        _parameters[key] = Math.Max(min, Math.Min(value, 10000));
    }

    internal void SelectSource(string id) => _sourceId = id;

    internal void SelectAssets(IEnumerable<int> indexes)
    {
        _selectedFileIds.Clear();
        foreach (var i in indexes)
            if (i >= 0 && i < _files.Count)
                _selectedFileIds.Add(_files[i].Id);
    }

    internal void LoadFiles(IEnumerable<string> paths)
    {
        var files = paths.Where(File.Exists).Select(path => new FileInfo(path)).ToList();
        if (files.Count == 0) return;
        // 导入新一批文件前清空旧的临时组合，避免不同数据批次交叉引用。
        foreach (var existing in _original.CombinationFileIndexs.Items.ToArray())
            _original.Remove(existing.Id);
        _original.LoadFile(files);
        if (bool.TryParse(GlobalSettingStroe.GetValue("TemplateSync"), out var sync) && sync)
        {
            foreach (var template in _original.ProductTemplates.Items)
            {
                var matches = _original.FileIndexs.Items.Where(f =>
                    template.Products.Any(p => p.Product == f.Product && p.Strategy == f.Strategy)).ToList();
                if (matches.Count > 0) _original.Combination(matches, template.TemplateName);
            }
        }
        _selectedFileIds.Clear();
        foreach (var file in _original.FileIndexs.Items) _selectedFileIds.Add(file.Id);
        _sourceId = string.Empty;
    }

    internal void SynchronizeCombination()
    {
        var files = _files.Where(x => _selectedFileIds.Contains(x.Id)).ToList();
        if (files.Count == 0) throw new InvalidOperationException("请先选择需要组合的品种。");
        _original.Combination(files, $"组合 ({files.Count}品种)");
        _sourceId = "combo-0";
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
            _sources.Add(new SourceEntry($"combo-{i}", name, name, "组合", name, combos[i]));
        }
        for (var i = 0; i < _files.Count; i++)
        {
            var f = _files[i];
            _sources.Add(new SourceEntry($"file-{i}", f.Product, f.Product, "单品", f.Name, f));
        }

        if (_sources.All(x => x.Id != _sourceId))
            _sourceId = _sources.FirstOrDefault()?.Id ?? string.Empty;

        _selectedFileIds.RemoveWhere(id => _files.All(f => f.Id != id));
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
                UseIncome = true,
                UseTime = true
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
        var usable = stats.Where(s => s.IncomeInfoResults.Count > 0 && s.DayCount > 0);
        if (compound)
        {
            // 目标收益与时间参数直接作用于原算法，不能对原型示例行筛选。
            usable = usable.Where(s => s.Total >= _parameters["ret"] - 1e-6
                && s.DayCount <= _parameters["months"] * 31 + 2
                && s.LastMultiples <= _parameters["levMax"] + 1e-6);
        }
        var rows = usable.ToList();
        return new
        {
            rows = rows.Select(s => Row(s, compound)).ToArray(),
            curves = rows.Select(Points).ToArray(),
            all = stats.Count
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
            x.MaxWithdraw * 100, x.WithdrawLevel * 100
        }).ToArray() ?? [];
        var assets = _files.Select(f => new[] { f.Product, f.Name, f.Name, f.Id }).ToArray();
        var selected = _files.Select((f, index) => (f, index))
            .Where(x => _selectedFileIds.Contains(x.f.Id)).Select(x => x.index).ToArray();
        var selectedSource = _sources.FirstOrDefault(x => x.Id == _sourceId);
        var result = new
        {
            requestId,
            sourceId = _sourceId,
            sources = _sources.Select(x => new
            {
                id = x.Id, name = x.Name, code = x.Code, grp = x.Group, note = x.Note
            }).ToArray(),
            assets, selected,
            equity,
            compound = source is null ? null : ComputeRows(source, "compound"),
            settle = source is null ? null : ComputeRows(source, "settle"),
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
