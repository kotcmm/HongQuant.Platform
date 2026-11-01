# HONG QUANT Terminal · 四个主界面高保真实现

## 选择的实现方式

本分支以用户提供的 `hongquant-terminal.html` 为最终 UI 规范，直接在 WPF `MainWindow` 的 Microsoft Edge WebView2 中渲染完整 HTML/CSS/SVG。这样标题栏、侧边导航、参数卡、滑块、KPI、表格、图表的**布局与 CSS 都来自设计源文件**，避免上一版 WPF 控件重绘的字号、间距、选中效果不一致问题。

**不是改为网页后端项目**：应用入口、数据导入、本地存储、量化业务服务、Excel 导出仍是原有 .NET 8 / WPF / C#。WebView2 只是界面呈现层；浏览器端通过 `window.chrome.webview.postMessage` 触发明确列举的 C# 操作，C# 回传真实数据快照。

## 四个页面

| 主导航 | 设计稿实现 | C# 数据/功能 |
|---|---|---|
| 基础数据 | 品种池、搜索、选中计数、9 列权益表、来源页签、5 项工具按钮 | `IOriginalService` 的 `FileIndexs`、`CombinationFileIndexs` 与 `IDataFile.Items`；可导入 CSV、生成组合、导出 xlsx、编辑模板、选择目录 |
| 定投复利 | 六组参数滑块、四张 KPI 卡、样本明细表 | `CycleIncomeService.StatisticsData` + `Multiple5ConfigIncome`；参数影响**原有真实算法** |
| 周期结算 | 结算周期滑块、结算规则、共享 KPI 与明细 | `CycleIncomeService.StatisticsData` + `CycleConfigIncome`，任意支持的月数重新计算 |
| 图表分析 | 样本柱图、指标筛选、净值折线与回撤折线、悬停十字线 | 实际的 `BaseIncomeStatistics.IncomeInfoResults` 逐日序列；不再用插值/随机数生成示意曲线 |

以上四个页面保持原稿结构。浏览器侧不储存虚假的权益、收益或样本数，不把示例数值用作实际结果。**没有导入数据时显示空数据状态**，不会展示演示截图里的 GD20 回测数字。

## 窗口行为

- 主窗口使用无系统标题栏的 WPF Window，内部顶部标题栏来自原版 HTML。
- 点击最小化、最大化、关闭按钮，WebView2 消息转发给 WPF。
- 鼠标左键按住标题栏非交互区域，发送 `WM_NCLBUTTONDOWN / HTCAPTION` 给 Windows。窗口最大化时先恢复，随后进入系统拖动模式，可继续贴靠窗口。
- WPF `WindowChrome` 保留窗口边缘调整尺寸的能力。

## 数据安全与来源

- 实际权益明细从已导入的 CSV 计算；不会用原型中的 GD20 固定数组、虚构单品收益倍数或随机折线。
- 切换数据源时，四个页面共用当前选择。
- 刷新会优先使用指定目录下的既有批量导出程序和 Export 目录；不存在可读取文件时让用户选择真实 CSV。
- 导入新文件会清除旧内存组合，必要时根据既有 `TemplateSync` 设置自动重新生成模板组合，避免不同批次数据混用。
- 加载本地 `https://hongquant.local` 虚拟主机页面，不加载外部网页。WebView2 运行时由微软维护。

## 环境要求与验收

- Windows 10/11、.NET 8、Edge WebView2 Runtime（使用 NuGet `Microsoft.Web.WebView2`）。
- 在 Windows 工作目录运行 `dotnet restore HongQuant.Platform.sln` 和 `dotnet build HongQuant.Platform.sln -c Release`。
- 依次测试：最大化状态拖动标题栏恢复窗口；双击标题栏；最小化/最大化/关闭；边缘拖放改变窗口大小。
- 用真实 CSV 数据测试：品种搜索/全选/清空、数据源菜单/底部来源页签、数据更新、组合计算、编辑模板、Excel 导出。
- 用同一组真实数据比较旧算法结果：目标收益、目标时间、杠杆和结算月数改变后两种算法的结果；图表与被选中行对应；回撤与净值折线取自真实日期。
- 比较设计稿原 HTML 和软件在 **相同屏幕分辨率、Windows 显示缩放比例以及 WebView2 100% 缩放** 下的外观；允许不同字体栅格化造成的亚像素差异。

**验证状态：** 本次可以对嵌入 HTML 的脚本做 JS 语法检查、对 GitHub 源码做静态检查；当前环境没有 Windows/.NET SDK，因此**尚未完成 WPF 编译、Windows 拖动交互、渲染截图像素对比与真实数据联调**。禁止将以上测试项记为已通过。

参考素材原样存放在 `docs/terminal-ui-reference/`，应用用的页面为 `HongQuant.Platform/Resources/TerminalUi/hongquant-terminal.html`。所有改动留在 `feat/terminal-ui-redesign`，不得合并到 `master`。
