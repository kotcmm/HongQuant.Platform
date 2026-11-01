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

console.log('PASS: 终端脚本初始化、菜单切换、刷新按钮和数据渲染均正常');
