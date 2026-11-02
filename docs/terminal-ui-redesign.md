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


## 2026-11-01 · 窗口外观与基础数据旧版交互恢复

- **窗口外框**：WPF 最外层增加 1px 可见边线及 8px 圆角；Windows 11 使用 DWM 原生圆角与阴影，非 Win11 以系统支持为准。最大化时取消窗口圆角和内缩。
- **最大化裁切**：用 `WM_GETMINMAXINFO` 将最大化范围限定在所在显示器工作区域；所有主区、视图、列表和表格采用 `min-height:0` 的 flex 收缩策略，表格滚动而底部的“全选/清空”、组合标签与状态条固定在窗口内。
- **原组合操作**：左侧列表允许独立多勾选；右键弹出“组合计算 / 最新查看 / 保存模板”。组合只以选中品种为输入，不再把单个品种的查看切换当作组合计算。
- **底部导航**：保留单个品种页签；每次组合计算生成独立的组合结果标签，可选择、关闭；不再为所有导入的单品重复生成一个底部标签。
- **组合模板**：左下恢复模板下拉，支持“全部品种”和已保存模板；保存模板仍通过现有 `IOriginalService.SaveProductTemplate` 写入持久存储。
- **同步组合**：顶部“同步组合”恢复为原有的布尔设置（`TemplateSync`），不再作为“计算当前选择”的替代按钮。
- **复选框修复**：原 `BasicDataFile.Id` 为空，不可直接用作选择键；改为 `BasicDataFile.FileInfo.FullName`，避免不同策略同品种互相误选。
- **最新查看**：调用原 `ShowLastView` 和 `ShowLastViewModel`，确保多品种最新权益查看保留原窗口行为。

### Windows 手动验收清单

1. 普通窗口检查细描边、四角小圆角、投影；最大化确认不出现内部白边。
2. 分别在 100% / 125% DPI、任务栏底部且最大化的情况下，确认“全选”“清空”、模板下拉、底部组合标签均可见。
3. 同品种不同策略文件分别勾选，检查选中计数准确；清空与全选均应包含被搜索隐藏的品种。
4. 勾选两项及以上，在左侧品种列表任意位置右键选择“组合计算”，确认生成新标签并打开组合权益；点击“单个品种”可回到原单品，点击组合 × 删除标签。
5. “保存模板”输入名称后，确认左下能选到此模板并正确勾选其品种；“最新查看”正确显示多品种最新数据。
6. “同步组合”设置切换后重新导入数据，检查按已保存模板生成组合。

当前环境没有 Windows/.NET SDK，所列界面、DWM 窗口阴影、多显示器/任务栏、WPF 运行与真实数据联调**尚未实际运行验证**。已检查 HTML 中 JS 语法及宿主消息名称，但不能用静态检查代替端到端验收。

## 2026-11-01 · 原生窗口交互、过滤全选与 Shift 连选

### 窗口流畅性（重点）

上一版把整个 WebView2 当作窗口的客户区，HTML 顶栏的 `mousedown` 通过 `postMessage` 通知 WPF，再在 C# 中手工恢复最大化窗口并发送 `WM_NCLBUTTONDOWN`。这条路径存在异步消息往返和恢复窗口时的瞬时跳动。

现调整为：
- 窗口使用 `WindowStyle="SingleBorderWindow"` + `WindowChrome`，保留操作系统的原生窗口样式/动画通路；
- 标题栏 46px 改成原生 WPF 绘制；WebView2 从第二行开始，不覆盖窗口标题栏；
- `WindowChrome.CaptionHeight=40`（配合顶部 6px 调整边框），由 Windows 直接处理鼠标拖动、双击最大化、边缘缩放和系统贴靠；
- 标题栏按钮使用 `SystemCommands.MinimizeWindow/MaximizeWindow/RestoreWindow/CloseWindow`；
- 删除 HTML 向后端转发 `window-drag/window-minimize/window-toggle/window-close` 的逻辑，以及 `WM_GETMINMAXINFO` 自定义最大化钩子；
- HTML 中原版顶栏保留为主题切换的内部实现入口，但通过 CSS 隐藏，业务区其余 UI 沿用原设计稿。

### 基础数据：过滤后全选

- 搜索结果只渲染当前满足过滤条件的品种。
- “全选”重置选择集合为**当前可见**的文件索引，不对过滤隐藏项执行全选；点击后后端接收相同集合。
- “清空”仍取消全部当前勾选。
- 勾选范围始终以源文件索引关联真实记录，不改变历史数据的计算顺序。

### 基础数据：Shift 批量勾选

- 单击条目：单独切换该条目勾选状态，记录 Shift 起点。
- 按住 Shift 单击另一个可见条目：按**当前搜索结果中的显示顺序**，把两端之间的所有条目选中。
- Shift 连选保留范围之外之前已经选中的内容；过滤文字改变后清空 Shift 起点，防止跨两份不同过滤结果误选。
- 拦截复选框浏览器默认点击，范围勾选在事件结束后应用，避免选中状态被默认行为回滚。

### 标题栏来源下拉

- 标题栏下拉在 WPF 层建立，只列出后端 `grp=组合` 的数据源；单品种不会在此出现。
- 没有组合或者当前正在查看单品种时显示“选择组合”占位；选择组合后同步更新基础数据、定投、周期结算和图表。
- 基础数据底部原有“单个品种 / 已计算组合”标签保持不变，仍可用于切换单品和关闭组合。

### Windows 环境需要验证

1. 普通窗口拖动、最大化后按住标题栏拖出、双击切换最大化、最小化到任务栏、任务栏恢复和鼠标靠边贴靠。
2. 最小化/恢复与最大化/还原的 DWM 动画是否与同一台 Windows 上的其他 WPF/系统窗口一致，100% 和 125% Windows 缩放均检查。
3. 搜索 `GD20` 后点击“全选”：当前结果全部选中，其他被隐藏的 `GD25` 等文件不应选中；清空后应为 0。
4. 普通单选第三条，按 Shift 点击第九条：三到九连续勾选；组合计算使用实际选中结果；在搜索过滤后重复连选。
5. 顶部来源下拉只出现组合，不出现单品种；底部原单品种标签仍可使用。

当前没有 Windows/.NET 编译及鼠标操作运行环境，以上均为待验收项；HTML JavaScript 语法与 GitHub 提交状态可静态检查。

## 2026-11-01 · 标题栏视觉修复

- 取消 WPF 标题栏上的深浅主题按钮，WebView2 的 `hongquant-terminal.html` 也移除切换入口和本地主题偏好读取；强制使用浅色，避免此前的 `hq-theme=dark` 影响显示。
- 标题栏组合下拉的数据从复杂 `HeaderSourceChoice` 对象改为**纯组合名称字符串**，单独维护与之对应的组合 ID，避免显示 `HeaderSourceCh...` 等 C# 类型名。
- 下拉框默认 `SelectedIndex=-1`、无占位选项。无组合时保持空白且禁用；已有组合但未选择时也保持空白。只有选中实际组合后才显示组合名称并同步业务数据。
- 最小化、最大化、还原和关闭使用 WPF `Path` 矢量图标（非 Unicode 符号），统一尺寸、笔画与悬停效果；窗口最大化时自动切换为“还原”图标。
- WindowChrome 标题栏和 Windows 原生 `SystemCommands` 不变，此轮仅调整视觉和选择框的数据展示。

**待在 Windows 验证：** 首次打开空组合列表、已有组合时初始空白、选中组合后的回显、没有组合时禁用、最大化与还原切换图标、鼠标悬停关闭按钮颜色，以及在非默认 DPI 下按钮视觉一致性。当前环境仅完成 XAML 标签匹配与 JS 语法的静态检查，未运行 Windows WPF 构建。

## 2026-11-01 · 修复主内容区无法点击

### 问题与修复

- 排查发现主工作区通过 `Microsoft.Web.WebView2.Wpf.WebView2`（`HwndHost` 子窗口）显示，而自定义标题栏使用 WPF `WindowChrome`；这种宿主组合存在独立 HWND 与 WPF 命中测试/airspace 兼容风险。改为微软提供的 `WebView2CompositionControl`（WPF `Control`），让鼠标事件由控件的 `OnMouseDown/Up/Move` 转发给浏览器。
- 只对第二行的浏览器控件设置 `WindowChrome.IsHitTestVisibleInChrome=True`，保持第一行原标题栏可由 Windows 拖动。保留 WebView2 的 `CoreWebView2`、消息桥与 HTML 原型。
- 移除宿主边框的 `ClipToBounds`，避免 HwndHost/组合宿主与 WPF 裁剪边界发生额外干扰。
- 首次加载不再由 JS `ready` 与 `NavigationCompleted` 双重执行完整数据计算。
- 前端勾选品种后立即更新选中状态，C# 只保存勾选集合，不重新构建、计算并重绘整张表。
- 在 `TerminalDataBridge` 缓存当前数据源、当前参数对应的两类统计结果；仅在数据导入、切换来源、调整参数、创建或关闭组合时失效。
- 增加 `window.error` / `unhandledrejection` 启动时注册、页面桥接完整性检查及首个 `pointerdown` 诊断。页面实际运行出现错误时显示信息，并在 VS 调试输出中记录输入是否成功到达 DOM。

### 需要在 Windows 上复测

1. 中间四个页面导航、搜索框、勾选框、右键菜单、组合标签、滑块及表格是否可点击。
2. 连续勾选几十项时是否即时响应，不能每次都触发收益统计重算。
3. 普通窗口及最大化下的标题栏拖动/窗口动画是否仍正常，网页区域不能误判为标题栏。
4. 如果仍无法点击，查看 VS 调试输出是否出现 `[HongQuant WebView2] Pointer received`；如果有，说明鼠标已到 DOM，需要继续查 JS；如果没有，说明窗口输入层仍有问题。
5. 运行前确认 NuGet WebView2 SDK 的 `WebView2CompositionControl` 可用（仓库使用 `Microsoft.Web.WebView2 1.0.4314.35`）。

**当前验证范围：** 已核对 Microsoft 官方的 WPF WebView2CompositionControl 替换与鼠标输入机制，但当前容器没有 Windows .NET SDK，不能完成 WPF 编译与实机鼠标验证。不能宣称此问题已经运行验证通过。


## 2026-11-01 · 页面脚本中断导致按钮失效（复现后修复）

用户截图指出 `hongquant-terminal.html:1993` 报错：`Uncaught TypeError: $(...).forEach is not a function`。

**明确根因**：页面 JS 定义 `$ = document.querySelector`（单元素）、`$$ = document.querySelectorAll`（数组）。导航初始化错误地写为 `$('.nav-item[data-view]').forEach(...)`。加载到这里便抛出异常，后续导航、参数、工具栏、图表等监听器没有注册，所以窗口能显示但中央页面无法操作。

**修复**：将导航初始化改成 `$$('.nav-item[data-view]').forEach(...)`。同时扫描了整段 JavaScript，确认没有其他 `$().forEach/map/filter/reduce/some/every` 类型误用。保留现有 WPF 标题栏、WebView2CompositionControl 和既有业务接口，不再盲目重构鼠标输入机制。

**回归测试**：新增 `scripts/verify-terminal-ui.mjs`，以 Node.js 自带 `vm` 在模拟 DOM 上执行完整终端脚本，检查无运行时异常、页面完成 `ready`、菜单切换能向宿主发送消息、基础数据刷新按钮注册及通过真实数据结构渲染品种列表。同时加入单元素选择器错误模式检测。运行命令：

```powershell
node scripts/verify-terminal-ui.mjs
```

本次已在 V8 模拟 DOM 环境执行等效回归测试，初始化和主要事件均通过；**仍需在用户 Windows 机器上验证真实 WebView2 鼠标操作**。如果程序继续加载旧版 HTML，清理输出目录后重新生成，确认 `Resources/TerminalUi/hongquant-terminal.html` 已更新。


## 2026-11-01 · 品种列表过滤计数与 Shift 文字选择修复

- 左侧品种标签添加 `user-select:none` / `-webkit-user-select:none`，并取消标签 `selectstart` 与 `dragstart` 默认行为；搜索框不受影响，可正常选择输入的文字。
- 每次过滤时计算当前实际显示的品种索引集合，右上角计数显示 **当前显示已选数 / 当前显示总数**，不是之前全局选择的数量；例如全部选择 14 个，搜 GD20 后仅 4 个匹配勾选，即显示 `4/24`。
- 原有多次过滤的已选记录仍累计保留供组合计算。当前过滤条件下“全选”仅追加当前显示品种，“清空”仅取消当前显示品种，不会误清除其他过滤条件下已选的项目。
- 取消过滤后重新展示全量数据，此时已选计数也恢复全量范围。保留原有 Shift 连选及右键组合计算。
- 增强 `scripts/verify-terminal-ui.mjs`，检查过滤前后计数、隐藏选择保留、局部全选/清空、文本选择阻止以及页面脚本能完整初始化。

回归测试运行：`node scripts/verify-terminal-ui.mjs`。本轮已用模拟 DOM 执行相应逻辑，尚未在 Windows GUI 做实机鼠标验证。


## 2026-11-01 · 基础数据页底部对齐与无数据展示

- 左侧导航移除“设计规范”按钮，只保留底部的引擎（.NET 8 · 本地）和数据源信息；对应隐藏的设计参考页面没有进入正常导航。
- 左侧 `sidebar-foot` 设为固定 36px 高，并移除侧栏底部额外的 10px 内距；右侧 `tabbar` 使用同样的 36px border-box 高度。两边分割横线位于主工作区同一高度。窄窗口下也避免附加底部空隙。
- 基础数据的“回测权益明细 / 品种数 / 数据区间”只在当前来源实际有权益行时显示；无数据时隐藏，不使用原型里的示意日期和品种数。
- 右下角“行数 / 列 / 更新”与上述说明采用相同的当前权益数据判定；没数据时整体隐藏，有真实记录时继续显示。
- 底部 `renderSourceTabs()` 仅列出真实计算所得组合；不再默认插入“单个品种”标签。单品种仍可作为业务数据来源用于计算，不会自动占用界面标签。
- `scripts/verify-terminal-ui.mjs` 增加无数据/有数据状态、仅单品种/组合存在时的标签展示及左右底部分隔高度检查。

本次已通过模拟 DOM 执行的初始化和显示逻辑回归测试；Windows 实际视觉像素和窗口缩放还需本机检验。


## 2026-11-01 · 左侧分隔线和「编辑模板」窗口统一

### 分隔线精确对齐

截图中左侧引擎/数据源信息区的上边线位于右侧「组合模板」顶部横线的下方：以前按 36px 底栏固定高度，仅与最下面的「全选 / 清空」栏对齐，忽略了上面的组合模板下拉区域。

现在左侧信息栏使用 `--hq-asset-footer-height`，由 `alignSidebarFooterDivider()` 实时计算右侧「组合模板」顶部至「全选 / 清空」底部的实际距离，并设置为左侧信息栏高度。通过 `ResizeObserver`、窗口 `resize` 和页面切换重新同步，避免字体/DPI/窗口宽度变化造成视觉错位。引擎和数据源文字仍在左侧底部。

### 编辑模板窗口

`ProductTemplateSettingView` 从 MahApps `MetroWindow` 改为普通 WPF `Window + WindowChrome`，使用主界面一致的浅色背景、白色 46px 标题栏、蓝色应用图标、圆角、描边、正常的最小化/最大化/还原/关闭矢量按钮。

模板表格使用自适应布局：组合名称可以直接编辑，超长的品种列表用省略号展示、悬停看完整列表；保存和删除按钮都保留原有的 `ProductTemplateSettingViewModel` 命令与 `IOriginalService` 持久化流程。删除确认不再使用 MahApps 蓝色系统弹窗，改为本窗口中的同主题确认浮层。窗口关闭时释放确认事件注册及 ViewModel 订阅，避免多次打开出现重复处理。

### 验收

- 打开基础数据页，左侧信息栏上边线应与右侧「组合模板」上边线精确对齐，而非最下方「全选 / 清空」上边线。
- 缩放窗口、调整 Windows 显示比例后重新检查横线。
- 点击「编辑模板」，应出现浅色圆角 WPF 窗口，不再出现青蓝色 MahApps 标题栏和横向大滚动条。
- 修改组合名称并保存、删除时选择取消/确认，重新打开确认后端数据确实变化。
- `node scripts/verify-terminal-ui.mjs` 检查页面初始化、动态分隔线计算和模板窗口结构/命令绑定。

当前执行环境没有 Windows WPF/.NET SDK，已通过模拟 DOM 测试及 XAML 标签平衡检查，但 Windows 实际渲染、窗口动画和业务命令尚待本机运行验收。


## 2026-11-01 · 模板列表点击崩溃风险处理与底部文字居中

用户反馈：在「组合模板管理」窗口点击中间的「品种清单」后程序直接退出，且组合名称输入框过宽；左侧引擎/数据源文字需要在底部区域内居中。

- 当前无法取得实际异常堆栈，因此不能断言原崩溃的最终根因。已排查到中间列属于 `DataGridTemplateColumn`：其文本同时绑定 `Text` 与超长 `ToolTip`，点击会走 DataGrid 单元格焦点/选中交互。这一列实际只是只读展示，不应进入编辑和选中流程。
- 模板窗口使用 `ItemsControl + ScrollViewer + Grid` 替换原 `DataGrid`，不再产生 `DataGridCell` 选择与焦点状态；中间品种清单仅使用 `TextBlock` 显示，设置 `Focusable=False`、`IsHitTestVisible=False`，移除超长 `ToolTip` 绑定。保存、删除仍调用原有命令和后端数据源。
- `ProductTemplatesEditViewModel.Products` 改为从品种集合生成的字符串快照；源列表发生赋值时主动通知 WPF 刷新，避免 UI 重绘时反复遍历集合。
- 组合名称输入框改为 164px 宽，在所在列中设置左 14px、右 12px 的外边距，避免紧贴相邻单元格。
- 左侧 `.sidebar-foot` 调整为纵横居中，`.sb-meta` 增加 `text-align:center`。前一轮分隔线的动态对齐机制保留不变。
- `scripts/verify-terminal-ui.mjs` 补充检查：模板中不再出现 `DataGrid`、产品列必须不接受鼠标输入、保存/删除命令仍存在、产品清单快照更新、输入框与左侧底栏样式。

已通过脚本初始化、CSS/XAML 静态结构和命令绑定回归检查。**WPF 编译、实际鼠标点击与崩溃复现目前无法在此执行**。如果本机仍发生退出，请保留 `logs/yyyyMMdd.log` 和 Windows 事件查看器中的异常类型及调用堆栈，以进一步定位。


## 2026-11-02 · 定投复利与周期结算显示全部样本

### 原因

此前 `TerminalDataBridge.ComputeRows` 在生成 JSON 前，就按定投复利的收益目标、交易天数和杠杆上限过滤出达标样本。因此实际计算 78 组时，页面表格、图表样本选择器和明细栏只能看到达标的 25 组。JS 的 `compoundFilter()` 已不再过滤，但从服务端收到的明细本身已经不完整。

### 修改

- C# 数据桥以算法实际产生交易记录的 **全部样本** 作为 `rows`，并让 `curves` 与每一行保持相同的顺序和索引；未达标及负收益样本不再提前丢弃。
- 另返回 `qualifiedIndexes` 保存达标样本的行索引。定投复利保留原有达标条件（收益、目标时间与杠杆上限）；周期结算则包含所有有交易记录的结算期。
- JS `STATE.rows`、样本明细表、样本下拉框、柱状图和选中样本的净值/回撤曲线全部使用完整 `rows`。
- 平均收益、最大回撤峰值、平均交易天数等 KPI 保持原达标样本口径。有效样本显示为 `达标数/全部样本数`（例如 `25/78`），不因放开明细而错误地变为 `78/78`。
- 明细表头明确标记 `全部 78/78 组` 或 `全部 N/N 期`。定投复利页面副标题同时说明“全部组数”和“达标组数”；结算页面说明实际结算期数。
- 表格下方“筛选规则”改为“达标条件”（周期结算则是“结算规则”），说明这些条件用于 KPI 统计，不代表表格隐藏了未达标记录。

### 验证

`node scripts/verify-terminal-ui.mjs` 回归测试增加：
- 4 组定投复利，2 组达标，表格/下拉仍显示 4 组，KPI 保持 2/4；
- 周期结算 3 期，包含亏损期，表格/下拉都显示完整 3 期；
- 服务端必须返回与明细分离的 `qualifiedIndexes`，且不能重新对 `rows` 执行达标条件筛选。

已在脚本模拟环境中执行相同的 DOM/数据桥接测试并通过。由于没有 Windows WPF 编译运行环境，需在本机用真实组合确认 78 组均可滚动查看、选择及绘图。
