const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.resolve(process.argv[2] || path.join(__dirname, '../..'));
const expectDefect = process.argv.includes('--expect-defect');
let checks = 0, cases = 0;
function check(value, label) { assert.ok(value, label); checks++; }
const initial = fs.readFileSync(path.join(root, 'StoreCMS/Views/AdminPanel/ChangeInitialPassword.cshtml'), 'utf8');
const inline = initial.match(/document.getElementById\('initial-password-exit'\).addEventListener\('submit', function \(event\) \{([\s\S]*?)\n            \}\);/)[1];
function fixture(kind, options = {}) {
    let sends = 0, reloads = 0, callbacks = 0, prevented = 0;
    const messages = [], requests = [];
    const form = { id: 'fixture', elements: [{ name: 'pageID', value: '58' }, { name: 'payload', value: 'unsaved' }],
        previousElementSibling: { action: 'https://localhost/admin' }, checkValidity: () => true };
    const event = { preventDefault() { prevented++; this.defaultPrevented = true; }, currentTarget: { name: 'uploadedFile', files: [{ name: 'fixture.jpg' }] } };
    const inputBefore = JSON.stringify([form.elements, event.currentTarget.files]);
    class FormData {
        constructor(source) { this.entries = source ? source.elements.map(e => [e.name, e.value]) : []; }
        append(...entry) { this.entries.push(entry); }
    }
    class XMLHttpRequest {
        constructor() { this.status = options.status === undefined ? 200 : options.status; this.headers = {}; requests.push(this); }
        open(method, url) { this.method = method.toLowerCase(); this.url = url; }
        setRequestHeader(name, value) {
            if (options.headerError) throw new Error('Header failure');
            this.headers[name] = value;
        }
        send(data) {
            sends++;
            this.data = data;
            // [Комментарий ИИ-агента]: Даже совпадающий текст исключения после начала send не должен превращаться в локальный CSRF-отказ.
            if (options.sendError) throw new Error('Форма устарела. Обновите страницу перед отправкой.');
        }
        getResponseHeader() { return options.html ? 'text/html' : ''; }
    }
    const sandbox = { XMLHttpRequest, FormData, form, event,
        alert: message => messages.push(message),
        document: { getElementById: () => form, addEventListener() {}, querySelector: () => options.missingMeta ? null : { content: options.emptyMeta ? '' : 'fixture-token' } },
        location: { origin: 'https://localhost', pathname: '/admin', reload() { reloads++; } } };
    vm.createContext(sandbox);
    for (const name of ['send_data.js', 'send_file.js'])
        vm.runInContext(fs.readFileSync(path.join(root, 'StoreCMS/Storage/scripts/admin_panel', name), 'utf8'), sandbox);
    if (kind === 'data' || kind === 'get') sandbox.createSendDataEventHandler(kind === 'get' ? 'GET' : 'POST', 'fixture', () => callbacks++)(event);
    else if (kind === 'file') sandbox.createSendFileEventHandler('fixture', () => callbacks++, null, '.jpg')(event);
    else {
        vm.runInContext('(function(event) {' + inline + '}).call(form,event)', sandbox);
        if (sends && !options.sendError) {
            if (options.networkError) requests[0].onerror();
            else requests[0].onload();
        }
    }
    check(JSON.stringify([form.elements, event.currentTarget.files]) === inputBefore, kind + ': user input preserved');
    check(prevented === 1, kind + ': native submission cancelled once');
    check(!messages.some(m => m.includes('fixture-token')), kind + ': no token in error');
    check(requests.every(r => !r.url.includes('fixture-token')), kind + ': no token in URL');
    cases++;
    return { sends, reloads, callbacks, messages, requests };
}
for (const kind of ['data', 'file', 'inline']) {
    for (const state of ['missingMeta', 'emptyMeta']) {
        const f = fixture(kind, { [state]: true });
        check(f.sends === 0, kind + ': token refusal occurs before send');
        check(f.reloads === 0 && f.callbacks === 0, kind + ': no reload or success callback');
        check(f.messages.length === 1, kind + ': one diagnostic');
        const message = f.messages[0];
        if (expectDefect) {
            check(message.includes('Результат операции неизвестен'), kind + ': F-01 independently reproduced');
            console.log(kind + '/' + state + ': send=0; incorrect unknown-result diagnostic reproduced');
        }
        else {
            check(/форма устарела|защитный токен/i.test(message) && /обновите страницу/i.test(message), kind + ': explicit stale-form diagnostic');
            check(!/результат операции неизвестен|ответ сервера/i.test(message), kind + ': local refusal is not a network error');
        }
    }
    if (expectDefect) continue;
    for (const options of [{ sendError: true }, { status: 0, networkError: true }, { headerError: true }]) {
        const f = fixture(kind, options);
        check(f.sends === (options.headerError ? 0 : 1), kind + ': no automatic retry');
        check(f.messages.length === 1 && f.messages[0].includes('Результат операции неизвестен'), kind + ': existing transport/exception diagnostic retained');
        check(f.reloads === 0 && f.callbacks === 0, kind + ': transport error has no success effect');
    }
    for (const status of [400,401,403,404,409,415,422,500]) {
        const f = fixture(kind, { status });
        check(f.sends === 1 && f.messages.length === 1, kind + ': HTTP failure handled after one send');
        check(f.reloads === 0 && f.callbacks === 0, kind + ': HTTP failure cannot reload or succeed');
        check(!f.messages[0].includes('отсутствует защитный токен'), kind + ': HTTP error is not a local token refusal');
    }
    const html = fixture(kind, { html: true });
    check(html.sends === 1 && html.reloads === 0 && html.callbacks === 0 && html.messages.length === 1, kind + ': unexpected HTML behavior preserved');
    const success = fixture(kind);
    check(success.sends === 1 && success.messages.length === 0, kind + ': success still sends once');
    check(kind === 'inline' ? success.reloads === 1 : success.callbacks === 1, kind + ': success contract preserved');
}
if (!expectDefect) {
    const get = fixture('get', { missingMeta: true });
    check(get.sends === 1 && get.callbacks === 1 && get.messages.length === 0, 'GET remains independent of CSRF meta');
}
console.log(JSON.stringify({ checks, cases, expectDefect }));
