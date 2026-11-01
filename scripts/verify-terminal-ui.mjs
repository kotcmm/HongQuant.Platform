// 独立运行：node scripts/verify-terminal-ui.mjs
// 无需浏览器或额外 npm 依赖。验证终端脚本不会在注册菜单事件时中断。
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import vm from 'node:vm';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const html = readFileSync(resolve(root, 'HongQuant.Platform/Resources/TerminalUi/hongquant-terminal.html'), 'utf8');
const match = html.match(/<script>([\s\S]*?)<\/script>/);
assert.ok(match, '终端页面缺少 JavaScript 代码');
const script = match[1];

// $ 使用 querySelector 只返回一个节点；遍历节点集合必须使用 $$ (querySelectorAll)。
assert.doesNotMatch(
  script,
  /(^|\n)\s*\$\([^\n;]*?\)\s*\.\s*(?:forEach|map|filter|reduce|some|every)\s*\(/m,
  '禁止对 $() 返回的单个 DOM 节点使用数组方法；请改成 $$()'
);

const callbacks = new Map();
const elements = new Map();
const messages = [];

function makeElement(key) {
  const element = {
    id: key, value: '', min: '0', max: '100', checked: false, hidden: true,
    textContent: '', innerHTML: '', scrollTop: 0, tagName: 'DIV',
    clientWidth: key === '#chartSample' ? 0 : 600,
    clientHeight: 360, offsetWidth: 220, offsetHeight: 180,
    dataset: { key: 'ret', view: 'data', panel: 'data', idx: '0', set: 'ret:100', goto: 'data', ind: 'ret' },
    style: { display: '', setProperty() {} },
    classList: { add() {}, remove() {}, toggle() {}, contains() { return false; } },
    addEventListener(event, handler) {
      const name = key + ':' + event;
      if (!callbacks.has(name)) callbacks.set(name, []);
      callbacks.get(name).push(handler);
    },
    setAttribute() {}, getAttribute() { return null; },
    appendChild(child) { child.parentNode = this; },
    focus() {}, contains() { return false; },
    querySelector() { return makeElement('nested'); },
    querySelectorAll() { return []; },
    closest() { return this; },
    getBoundingClientRect() { return { left: 0, right: 600, top: 0, bottom: 300, width: 600, height: 300 }; }
  };
  element.parentNode = { style: {}, appendChild() {} };
  return element;
}

function get(key) {
  if (!elements.has(key)) elements.set(key, makeElement(key));
  return elements.get(key);
}

// 对品种列表提供最小可用的复选框 DOM，实现过滤、全选和清空的动态验证。
const assetList = get('#assetList');
let assetMarkup = '';
let assetInputs = [];
Object.defineProperty(assetList, 'innerHTML', {
  get() { return assetMarkup; },
  set(markup) {
    assetMarkup = markup;
    assetInputs = Array.from(markup.matchAll(/<input type="checkbox" data-idx="(\d+)"( checked)?>/g),
      match => ({ dataset: { idx: match[1] }, checked: Boolean(match[2]) }));
  }
});
assetList.querySelectorAll = selector =>
  selector === 'input[data-idx]' ? assetInputs : [];

const document = {
  documentElement: get('html'),
  body: get('body'),
  querySelector: get,
  querySelectorAll(selector) {
    return [get(selector + ':1'), get(selector + ':2')];
  },
  createElement: makeElement,
  getElementById: get,
  addEventListener(event, handler) {
    const name = 'document:' + event;
    if (!callbacks.has(name)) callbacks.set(name, []);
    callbacks.get(name).push(handler);
  }
};
const window = {
  chrome: { webview: { postMessage(message) { messages.push(message); } } },
  innerWidth: 1200,
  innerHeight: 880,
  devicePixelRatio: 1,
  addEventListener(event, handler) {
    const name = 'window:' + event;
    if (!callbacks.has(name)) callbacks.set(name, []);
    callbacks.get(name).push(handler);
  }
};

vm.runInNewContext(script, {
  window, document,
  location: { hash: '' },
  setTimeout: () => 1,
  clearTimeout: () => {},
  getComputedStyle: () => ({ getPropertyValue: () => 'auto' })
}, { timeout: 5000, filename: 'hongquant-terminal.html' });

function requireListener(key) {
  const list = callbacks.get(key);
  assert.ok(list?.length, '页面初始化中断，没有注册事件：' + key);
  return list[0];
}
assert.ok(window.__hqNative?.applyState, 'WPF 数据桥没有成功初始化');
assert.ok(messages.some(msg => msg.action === 'ready'), '页面没有向 WPF 发送 ready');

const nav = get('.nav-item[data-view]:1');
nav.dataset.view = 'settle';
requireListener('.nav-item[data-view]:1:click')();
assert.ok(messages.some(msg => msg.action === 'view-changed' && msg.name === 'settle'),
  '点击周期结算菜单后未发送切页事件');

requireListener('.view[data-panel="data"] .toolbar button:1:click')();
assert.ok(messages.some(msg => msg.action === 'refresh'),
  '基础数据工具栏没有正常注册刷新按钮');

window.__hqNative.applyState({
  requestId: 1,
  sourceId: 'file-0',
  sources: [{ id: 'file-0', name: 'ag', code: 'ag', grp: '单品', note: '示例文件名仅用于测试' }],
  assets: [['ag', 'ag_GD20', 'ag_GD20.csv', 'file-path']],
  selected: [0],
  equity: [['2020-01-01', 1000000, 0, 0, 0, 0, 0, 0]],
  compound: null, settle: null, status: '数据源已连接',
  dateStart: '2020-01-01', dateEnd: '2020-01-01', activeSourceName: 'ag', templates: []
});
assert.match(get('#assetList').innerHTML, /ag_GD20/, '真实数据快照未渲染到品种列表');
assert.equal(get('#dataSub').hidden, false, '有真实权益记录时，回测说明应显示');
assert.equal(get('.view[data-panel="data"] .statusbar').hidden, false,
  '有真实权益记录时，底部行数/列数/更新时间应显示');
assert.doesNotMatch(get('#dataTabs').innerHTML, /单个品种/,
  '没有计算组合时，不能默认生成“单个品种”标签');


// 过滤后的计数必须针对当前显示结果；旧选择不能因为搜索改变而丢失。
window.__hqNative.applyState({
  requestId: 2,
  sourceId: 'file-0',
  sources: [{ id:'file-0', name:'ag', code:'ag', grp:'单品', note:'ag' }],
  assets: [
    ['ag','ag_GD20','ag_GD20_history.csv','f0'],
    ['ao','ao_GD20','ao_GD20_history.csv','f1'],
    ['ag','ag_GD25','ag_GD25_history.csv','f2'],
    ['ao','ao_GD25','ao_GD25_history.csv','f3'],
    ['CF','CF_GD20','CF_GD20_history.csv','f4']
  ],
  selected: [0,2],
  equity: [], compound: null, settle: null, status:'数据源已连接',
  activeSourceName:'ag', templates: []
});
const search = get('#assetSearch');
search.value = 'GD20';
requireListener('#assetSearch:input')({ target: search });
assert.equal(get('#selCount').textContent, 1, '筛选后只应统计当前可见的已选品种');
assert.equal(get('#selTotal').textContent, 3, '筛选后的分母必须是当前可见的三个品种');
assert.equal(assetInputs.length, 3, '列表应当只渲染匹配 GD20 的品种');
assert.doesNotMatch(assetMarkup, /GD25/, 'GD25 应被过滤隐藏');
assert.equal(get('#dataSub').hidden, true, '无权益数据时不能显示回测说明');
assert.equal(get('.view[data-panel="data"] .statusbar').hidden, true,
  '无权益数据时不能显示行数、列数和更新时间');
assert.equal(get('#dataTabs').innerHTML, '', '只有单品种来源时底部标签应为空');


requireListener('#selectAll:click')();
const afterSelectAll = messages.at(-1);
assert.equal(afterSelectAll.action, 'select-assets');
assert.deepEqual([...afterSelectAll.indexes], [0,1,2,4],
  '全选只能新增筛选结果，原已选的隐藏品种必须保留');
assert.equal(get('#selCount').textContent, 3, '当前筛选结果全选后应显示 3/3');
assert.equal(get('#selTotal').textContent, 3);

requireListener('#clearAll:click')();
const afterClear = messages.at(-1);
assert.deepEqual([...afterClear.indexes], [2],
  '清空当前筛选结果不能清除其他过滤条件下的选择');
assert.equal(get('#selCount').textContent, 0);
assert.equal(get('#selTotal').textContent, 3);

search.value = '';
requireListener('#assetSearch:input')({ target: search });
assert.equal(get('#selCount').textContent, 1, '去掉过滤后，应恢复全量列表的已选计数');
assert.equal(get('#selTotal').textContent, 5);

assert.match(html, /-webkit-user-select:\s*none/,
  '必须禁止品种列表 Shift 连选时出现浏览器原生文字高亮');
const selectStartEvent = {
  target: { closest: () => ({}) },
  prevented: false,
  preventDefault() { this.prevented = true; }
};
requireListener('#assetList:selectstart')(selectStartEvent);
assert.ok(selectStartEvent.prevented, 'Shift 连选时不得触发浏览器原生文本选择');

// 计算组合存在时只显示组合页签，不自动显示单品种。
window.__hqNative.applyState({
  requestId: 3,
  sourceId: 'combo-1',
  sources: [
    {id:'file-0',name:'ag',code:'ag',grp:'单品',note:'ag'},
    {id:'combo-1',name:'组合 221819 (19品种)',code:'组合',grp:'组合',note:'组合'}
  ],
  assets: [['ag','ag_GD20','ag_GD20.csv','f0']],
  selected: [0],
  equity: [['2020-03-02',7988868,-30103,-.38,.06,-.87,-.98,88.53]],
  compound: null, settle: null, status: '数据源已连接',
  activeSourceName: '组合 221819 (19品种)', templates: []
});
assert.match(get('#dataTabs').innerHTML, /组合 221819/,
  '已计算的组合必须继续显示在底部页签');
assert.doesNotMatch(get('#dataTabs').innerHTML, /单个品种/,
  '任何状态都不要自动生成“单个品种”页签');
assert.equal(get('#dataSub').hidden, false, '有组合权益数据时应显示回测说明');
assert.equal(get('.view[data-panel="data"] .statusbar').hidden, false,
  '有组合权益数据时应显示底部统计信息');

// 左侧引擎/数据源仍保留，左侧分隔线按组合模板与全选操作栏实际高度动态对齐。
assert.doesNotMatch(html, /data-view="ds"/, '左侧不再显示设计规范入口');
assert.match(html, /class="sb-meta"/, '必须保留引擎及数据源信息');
assert.match(html, /\.sidebar-foot\s*\{[^}]*--hq-asset-footer-height/, '左侧底部栏应随右侧模板区高度变化');
assert.match(html, /\.tabbar\s*\{[^}]*height:36px/, '右侧底部标签栏高度必须为 36px');
assert.match(html, /#dataSub\[hidden\]/, '无数据说明需要明确的隐藏样式');
assert.match(html, /\.statusbar\[hidden\]/, '无数据统计栏需要明确的隐藏样式');


assert.match(html, /--hq-asset-footer-height/, '左侧横线应使用动态同步高度');
assert.match(html, /function alignSidebarFooterDivider\(/, '需要监听组合模板高度变化以对齐横线');
assert.ok(callbacks.get('window:resize')?.length,
  '应在窗口尺寸变化时重新计算左右分隔线的位置');
// 模拟模板区和操作栏高度随窗口变化，分隔线须追随模板上边线。
const activeDataView = get('.view[data-panel="data"]');
activeDataView.classList.contains = cls => cls === 'active';
const templatePane = get('.view[data-panel="data"] .pane-template');
const assetActions = get('.view[data-panel="data"] .pane-foot');
templatePane.getBoundingClientRect = () => ({ top: 202, bottom: 332 });
assetActions.getBoundingClientRect = () => ({ top: 332, bottom: 400 });
let alignedHeight = '';
get('.sidebar-foot').style.setProperty = (key, value) => {
  if (key === '--hq-asset-footer-height') alignedHeight = value;
};
callbacks.get('window:resize')[0]();
assert.equal(alignedHeight, '198px',
  '左侧横线必须动态与组合模板顶线对齐，而非使用固定的 36px');


// 模板管理对话框不允许回退到 MahApps 蓝色标题栏。
const templateWindow = readFileSync(resolve(root,
  'HongQuant.Platform/Views/Original/ProductTemplateSettingView.xaml'), 'utf8');
const templateCode = readFileSync(resolve(root,
  'HongQuant.Platform/Views/Original/ProductTemplateSettingView.xaml.cs'), 'utf8');
assert.match(templateWindow, /<Window\s+x:Class="HongQuant.Platform.Views.Original.ProductTemplateSettingView"/,
  '模板管理必须使用原生 WPF Window');
assert.doesNotMatch(templateWindow, /mah:MetroWindow|LightBlue|ShowMessageAsync/,
  '不能再使用 MahApps 蓝色标题栏');
assert.match(templateWindow, /WindowChrome\.WindowChrome/, '弹窗必须保留系统拖动与缩放');
assert.match(templateWindow, /DeleteConfirmationOverlay/, '删除操作应使用主题一致的确认浮层');
assert.match(templateWindow, /SaveCommand/, '必须保留保存命令绑定');
assert.match(templateWindow, /DeleteCommand/, '必须保留删除命令绑定');
assert.match(templateCode, /RegisterHandler\(/, '必须保留原有 ReactiveUI 删除确认机制');
assert.match(templateCode, /_confirmationHandler\.Dispose\(\)/, '关闭窗口需释放确认交互订阅');
assert.doesNotMatch(templateWindow, /<DataGrid\b/,
  '品种清单不允许继续触发 DataGrid 选中单元格流程');
assert.match(templateWindow, /<ItemsControl ItemsSource="\{Binding ProductTemplates\}"/,
  '模板列表使用只读行容器，保留原 ProductTemplates 数据源');
assert.match(templateWindow, /Text="\{Binding Products\}"[^>]*IsHitTestVisible="False"/s,
  '品种清单必须是无法获得鼠标输入的只读文本');
assert.doesNotMatch(templateWindow, /ToolTip="\{Binding Products\}"/,
  '移除超长品种清单的悬停 ToolTip 绑定');
assert.match(templateWindow, /Width="164" Height="30"\s+Margin="14,0,12,0"/,
  '组合名称输入框应缩窄且左右留白');
assert.match(html, /\.sidebar-foot\s*\{[^}]*align-items:center;justify-content:center/,
  '引擎/数据源信息必须在底部区域居中');
assert.match(html, /\.sb-meta\s*\{[^}]*text-align:center/,
  '引擎和数据源的两行文字必须水平居中');

const templateItemViewModel = readFileSync(resolve(root,
  'HongQuant.Platform/ViewModels/Original/ProductTemplatesViewModel.cs'), 'utf8');
assert.match(templateItemViewModel, /public string Products => _products/,
  '品种清单展示文本应使用缓存快照而不是每次绑定时重新枚举源集合');
assert.match(templateItemViewModel, /RaisePropertyChanged\(nameof\(Products\)\)/,
  '品种清单更新时必须通知 WPF 重新绑定');


console.log('PASS: 页面行为、底部文字居中、模板列表安全点击与输入框留白');

