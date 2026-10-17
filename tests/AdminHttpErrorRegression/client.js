const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.resolve(process.argv[2] || path.join(__dirname, '../..'));
const views = path.join(root, 'StoreCMS/Views/AdminPanel');
let checks = 0;
function check(value, label) { assert.ok(value, label); checks++; }
function allFiles(dir) {
    return fs.readdirSync(dir, { withFileTypes: true }).flatMap(e => e.isDirectory()
        ? allFiles(path.join(dir, e.name)) : [path.join(dir, e.name)]);
}
const calls = [];
for (const file of allFiles(views).filter(f => f.endsWith('.cshtml'))) {
    const source = fs.readFileSync(file, 'utf8');
    for (const m of source.matchAll(/createSendDataEventHandler\('(POST|DELETE|GET)', '([^']+)', ([^;\n]+)\)\);/g)) {
        const parts = m[3].split(',').map(p => p.trim());
        calls.push({ file, method: m[1], id: m[2], success: parts.length === 3 ? Number(parts[2]) : 200 });
    }
}
const visitorInline = fs.readFileSync(path.join(views, 'MainPage/GetVisitors.cshtml'), 'utf8');
check(visitorInline.includes("createSendDataEventHandler('GET', 'visitor-@visitor.ID', visitorActionsResponseHandler)(event)"), 'inline GET sender inventoried');
calls.push({ method: 'GET', id: 'visitor-@visitor.ID', success: 200 });
check(calls.filter(c => c.method !== 'GET').length === 38, '38 unsafe send_data callers');
check(calls.filter(c => c.success === 201).length === 12, '12 creation contracts require 201');
check(calls.every(c => [200, 201].includes(c.success)), 'explicit supported contracts');

function fixture(status, options = {}) {
    let reloads = 0, redirects = 0, mutations = 0, callbacks = 0, validation = 0, prevented = 0, sends = 0;
    const messages = [], entries = [], targets = [];
    const form = { id: options.id || 'save-settings', elements: [{ name: 'pageID', value: '53' }, { name: 'value', value: 'unsaved' }],
        checkValidity: () => options.valid !== false };
    class FormData {
        append(...args) { entries.push(args); }
    }
    class XMLHttpRequest {
        constructor() { this.status = status; }
        open(method, url, async) {
            check(async === false, 'shared helper retains synchronous XHR');
            this.url = url;
            if (options.throwAt === 'open') throw new Error('network');
        }
        send() { sends++; if (options.throwAt === 'send') throw new Error('network'); }
        getResponseHeader(name) { return name === 'Content-Type' ? (options.html ? 'text/html; charset=utf-8' : '') : '/admin?itemID=2'; }
    }
    const sandbox = { FormData, XMLHttpRequest, alert: msg => messages.push(msg),
        document: { getElementById: () => form },
        location: { origin: 'https://localhost', pathname: '/admin', reload: () => reloads++, replace: target => { redirects++; targets.push(target); }, assign: target => { redirects++; targets.push(target); } } };
    vm.createContext(sandbox);
    for (const file of ['send_data.js', 'send_file.js']) {
        vm.runInContext(fs.readFileSync(path.join(root, 'StoreCMS/Storage/scripts/admin_panel', file), 'utf8'), sandbox);
    }
    const callback = () => { callbacks++; mutations++; sandbox.location.reload(); sandbox.location.replace('/next'); };
    const event = { preventDefault: () => prevented++, currentTarget: { name: 'uploadedFile', files: [{ name: options.extension || 'test.jpg' }] } };
    return { sandbox, form, callback, event, validationError: () => validation++,
        state: () => ({ reloads, redirects, mutations, callbacks, validation, prevented, sends, messages, entries, targets }) };
}
const errors = [0, 400, 401, 403, 404, 409, 415, 422, 500, 503, 302, 204];
for (const call of calls) {
    for (const status of [...errors, call.success === 200 ? 201 : 200, call.success]) {
        const f = fixture(status, { id: call.id });
        const before = JSON.stringify(f.form.elements);
        f.sandbox.createSendDataEventHandler(call.method, call.id, f.callback, f.validationError, call.success)(f.event);
        const s = f.state(), ok = status === call.success;
        check(s.callbacks === (ok ? 1 : 0), `${call.id} callback only on ${call.success}, actual ${status}`);
        check(s.reloads === (ok ? 1 : 0) && s.redirects === (ok ? 1 : 0) && s.mutations === (ok ? 1 : 0), 'no success DOM/navigation effects on failure');
        check(s.messages.length === (ok ? 0 : 1), 'one understandable error without callback');
        check(before === JSON.stringify(f.form.elements) && s.prevented === 1, 'input preserved and native submit cancelled');
    }
    for (const throwAt of ['open', 'send']) {
        const f = fixture(0, { throwAt });
        f.sandbox.createSendDataEventHandler(call.method, call.id, f.callback, null, call.success)(f.event);
        const s = f.state();
        check(s.callbacks === 0 && s.messages.length === 1 && s.reloads === 0 && s.redirects === 0 && s.prevented === 1, 'network exception cannot invoke success or native submit');
    }
    if (call.method !== 'GET') {
        const f = fixture(call.success, { html: true });
        f.sandbox.createSendDataEventHandler(call.method, call.id, f.callback, null, call.success)(f.event);
        check(f.state().callbacks === 0 && f.state().messages.length === 1, 'HTML 200/201 cannot impersonate mutation success');
    }
}
for (const id of ['upload-file', 'upload-image']) {
    for (const status of [...errors, 201, 200]) {
        const f = fixture(status, { id });
        f.sandbox.createSendFileEventHandler(id, f.callback, f.validationError, '.jpg')(f.event);
        const s = f.state();
        check(s.callbacks === (status === 200 ? 1 : 0), 'PUT callback requires 200');
        check(s.messages.length === (status === 200 ? 0 : 1) && s.validation === 0, 'HTTP error distinct from extension validation');
        check(status === 200 || (s.reloads === 0 && s.redirects === 0 && s.mutations === 0), 'PUT failure preserves page');
    }
    for (const throwAt of ['open', 'send']) {
        const f = fixture(0, { throwAt });
        f.sandbox.createSendFileEventHandler(id, f.callback, f.validationError, '.jpg')(f.event);
        check(f.state().callbacks === 0 && f.state().messages.length === 1 && f.state().prevented === 1, 'PUT network refusal');
    }
    const f = fixture(200, { extension: 'bad.exe' });
    f.sandbox.createSendFileEventHandler(id, f.callback, f.validationError, '.jpg')(f.event);
    check(f.state().validation === 1 && f.state().sends === 0, 'local extension validation retained');
}
for (const [file, name, expected] of [['Settings.cshtml', 'responseHandler', 200], ['UserTypes.cshtml', 'responseFromAdding', 201]]) {
    const source = fs.readFileSync(path.join(views, file), 'utf8');
    const start = source.indexOf(`function ${name}(request)`);
    const end = start + source.slice(start).search(/\r?\n\s+document\./);
    check(start >= 0 && end > start, 'actual Razor callback found');
    let callbackSource = source.slice(start, end).replace(/@Html.Raw\(localization\.\w+\)/g, 'SUCCESS');
    for (const status of [...errors, 200, 201]) {
        const f = fixture(status);
        vm.runInContext(callbackSource, f.sandbox);
        f.sandbox[name]({ status });
        check(f.state().reloads === (status === expected ? 1 : 0) && f.state().messages.length === (status === expected ? 1 : 0), 'actual settings/role callback refuses false success');
    }
}
const initial = fs.readFileSync(path.join(views, 'ChangeInitialPassword.cshtml'), 'utf8');
for (const file of ['FileManager/EditCssFile.cshtml', 'FileManager/EditScriptFile.cshtml']) {
    const source = fs.readFileSync(path.join(views, file), 'utf8');
    const start = source.indexOf('function responseHandler(request, formElement)');
    const end = start + source.slice(start).search(/\r?\n\s+document\./);
    const callback = source.slice(start, end).replace(/@Html.Raw\(localization\.\w+\)/g, 'SUCCESS');
    const f = fixture(200);
    vm.runInContext(callback, f.sandbox);
    f.sandbox.createSendDataEventHandler('POST', 'edit', f.sandbox.responseHandler)(f.event);
    check(f.state().targets[0] === '/admin?itemID=2', 'actual CSS/JS callback retains Location');
}
const load = initial.match(/request.onload = function \(\) \{([\s\S]*?)\n                \};/)[1];
for (const status of [...errors, 200, 201]) {
    const f = fixture(status);
    f.sandbox.request = { status, method: 'delete', getResponseHeader: () => '' };
    vm.runInContext(load, f.sandbox);
    check(f.state().reloads === (status === 200 ? 1 : 0), 'initial password logout onload only reloads for 200');
    check(f.state().messages.length === (status === 200 ? 0 : 1), 'initial logout reports failure');
}
check(initial.includes('request.onerror = function () { showAdminPanelHttpError(null); };'), 'inline logout network handler');
const invalid = fixture(200, { valid: false });
invalid.sandbox.createSendDataEventHandler('POST', 'test', invalid.callback, invalid.validationError)(invalid.event);
check(invalid.state().validation === 1 && invalid.state().sends === 0 && invalid.state().prevented === 1, 'local form validation retained');
console.log(JSON.stringify({ checks, unsafeDataCallers: 38, uploadCallers: 2, inlineLogout: 1, ordinaryPasswordForm: 'unchanged', getCallers: calls.filter(c => c.method === 'GET').length }));
