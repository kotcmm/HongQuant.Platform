# HONG QUANT 研究终端 · WPF 界面改造

## 参考设计

改造遵循本次交付的五份 UI 设计稿：`hongquant-terminal.html`、`tokens.css`、`components.css`、`design-tokens.html`、`components.html`。这些文件是 **HTML/CSS 视觉规范**，应用仍然保持 **.NET 8 WPF 原生实现**；没有引入 WebView 或模拟数据。

## 页面映射

| 终端导航 | 对应原业务 | 说明 |
|---|---|---|
| 基础数据 | `OriginalMain` + `ShowDataView` | 保留目录导入、策略/品种过滤、模板、勾选组合、数据导出、图表入口及底部组合标签 |
| 定投复利 | `YiTouMain` + `Multiple5Config` | 通过 `InitialAlgorithmName` 选中原 `Multiple5` 算法，参数更新仍触发原统计服务 |
| 周期结算 | `YiTouMain` + `CycleConfig` | 通过 `InitialAlgorithmName` 选中原 `Cycle` 算法 |
| 图表分析 | 原 `ShowChartView` / `CycleIncomeBarChartView` | 沿用已存在的「查看图表」「图表分析」入口，不增加无数据的新页面 |
| 其他旧页面 | `Views/Hong`、`Views/RealTime` | 保留原页面和命令，统一替换硬编码浅色背景及旧按钮样式；原本在主窗口注释掉的模块不擅自恢复上线 |

## 主题资源

- `HongQuant.Platform/Resources/TerminalLight.xaml` / `TerminalDark.xaml`：与 CSS tokens 对应的两套 WPF `SolidColorBrush`。
- `HongQuant.Platform/Resources/TerminalControls.xaml`：按钮、输入、筛选、列表、表格、导航、下拉及统计卡等公共视觉规则。
- `MainWindow.xaml`：46px 顶栏、192px 左导航、内容区和 28px 状态栏；右上角切换深浅主题。
- 样式使用 `DynamicResource Hq.*`，避免在各业务页面重复写色值。
- 原 MaterialDesign / MahApps 仍保留依赖，以兼容现有 DialogHost、消息框与旧组件；界面颜色由终端主题覆盖。

## 数据与状态

定投复利和周期结算拥有独立的 `YiTouMainViewModel` 和 `CycleIncomeService` 实例，避免两个页面同时存在时互相覆盖统计列表。原始数据服务 `IOriginalService` 保持共享，因此在基础数据导入的组合仍可供两个算法页读取。

## 需要在 Windows 环境验证

本次通过远程 GitHub 文件接口修改源代码，当前环境未提供 Windows/.NET SDK，**未执行 `dotnet build` 或 GUI 自动化**。切勿把本提交当作运行验证通过。

1. `dotnet restore HongQuant.Platform.sln`，再运行 `dotnet build HongQuant.Platform.sln -c Release`。
2. 启动软件，检查侧栏切换三个模块、顶栏窗口最小化/最大化/关闭、浅色/深色切换。
3. 基础数据：指定目录、更新数据、输入策略/品种过滤、品种勾选、组合计算、模板保存及导出。
4. 分别在定投复利和周期结算切换组合并修改参数，确保结果不会互相覆盖。
5. 图表窗口、模板配置窗口、旧的独立设置窗口，以及表格横向滚动/右键菜单/组合标签关闭。
6. 检查深浅主题下的 DataGrid、ComboBox、DialogHost 及 LiveCharts 是否存在背景或对比度不一致。

所有改造留在 `feat/terminal-ui-redesign`；不合并默认分支 `master`。
