const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { spawnSync } = require('node:child_process');
const root = path.resolve(process.argv[2] || path.join(__dirname, '../..'));
const wire = process.argv.includes('--wire') ? JSON.parse(fs.readFileSync(0, 'utf8')) : null;
const views = path.join(root, 'StoreCMS/Views/AdminPanel');
const scripts = path.join(root, 'StoreCMS/Storage/scripts/admin_panel');
let checks = 0, wireRequests = 0;
function check(value, label) { assert.ok(value, label); checks++; }
function files(dir) { return fs.readdirSync(dir, { withFileTypes: true }).flatMap(e => e.isDirectory() ? files(path.join(dir, e.name)) : [path.join(dir, e.name)]); }
const sources = files(views).filter(f => f.endsWith('.cshtml')).map(file => ({ file, text: fs.readFileSync(file, 'utf8') }));
const calls = sources.flatMap(({ file, text }) => [...text.matchAll(/createSendDataEventHandler\('(POST|DELETE|GET)', '([^']+)', ([^;\n]+)\)\);/g)].map(m => ({ file, method: m[1], id: m[2] })));
check(calls.filter(c => c.method !== 'GET').length === 38, 'all 38 data senders inventoried');
let unsafeForms = 0;
for (const { text } of sources) {
    for (const m of text.matchAll(/<form[^\n]*>[\s\S]*?<\/form>/g)) {
        const unsafe = m[0].includes('@Html.AntiForgeryToken()');
        if (unsafe) {
            unsafeForms++;
            check(/^<form[^\n]*method="post"/.test(m[0]), 'token form cannot natively submit as GET');
        }
        else check(!/^<form[^\n]*method="post"/.test(m[0]), 'every POST form has hidden token');
    }
}
check(unsafeForms === 38, '38 unsafe form declarations, shared edit/delete forms counted once');

// [Комментарий ИИ-агента]: Мост заменяет только транспорт XMLHttpRequest; реальные помощники формируют заголовки и FormData, затем отдельный Node-процесс отправляет их по HTTPS.
const bridge = `const https=require('node:https'),fs=require('node:fs');
const x=JSON.parse(fs.readFileSync(0,'utf8')), boundary='storecms_csrf_fixture_boundary';
let body=''; for(const [name,value,filename] of x.entries){
 body+='--'+boundary+'\\r\\nContent-Disposition: form-data; name="'+name+'"'+(filename?'; filename="'+filename+'"':'')+'\\r\\n\\r\\n'+(typeof value==='object'?'fixture-file':value)+'\\r\\n';
} body+='--'+boundary+'--\\r\\n';
const req=https.request(x.url,{method:x.method,rejectUnauthorized:false,headers:{...x.headers,Cookie:x.cookie,'Content-Type':'multipart/form-data; boundary='+boundary,'Content-Length':Buffer.byteLength(body)}},res=>{res.resume();res.on('end',()=>process.stdout.write(String(res.statusCode)));});
req.on('error',()=>process.exit(2));req.end(body);`;
function fixture(options = {}) {
    const token = wire ? wire.token : (options.token || 'fixture-request-token');
    let sent = 0, callback = 0, prevented = 0;
    const requests = [], messages = [], listeners = {};
    const form = { id: options.id || 'fixture', checkValidity: () => true,
        elements: [{ name: 'pageID', value: options.page || '53' }, { name: 'payload', value: 'unchanged' }, { name: '__RequestVerificationToken', value: token }] };
    class FormData {
        constructor(source) { this.entries = source ? source.elements.map(e => [e.name, e.value]) : []; }
        append(...entry) { this.entries.push(entry); }
    }
    class XMLHttpRequest {
        constructor() { this.headers = {}; this.status = options.status || 200; requests.push(this); }
        open(method, url, async) { this.opened = true; this.method = method.toLowerCase(); this.url = url; this.async = async; }
        setRequestHeader(name, value) { check(this.opened && !this.sent, 'header set after open before send'); this.headers[name] = value; }
        send(data) {
            this.sent = true; this.data = data; sent++;
            if (wire && options.network) {
                const result = spawnSync(process.execPath, ['-e', bridge], { input: JSON.stringify({ url: this.url, method: this.method, headers: this.headers, entries: data.entries, cookie: wire.cookie }), encoding: 'utf8', timeout: 10000 });
                check(result.status === 0 && result.stdout === '204', 'actual HTTPS pair accepted; multipart payload preserved');
                wireRequests++;
            }
        }
        getResponseHeader() { return ''; }
    }
    const document = { getElementById: () => form, querySelector: () => options.missingMeta ? null : { content: token }, addEventListener: (name, handler) => { listeners[name] = handler; } };
    const location = { origin: wire ? wire.origin : 'https://localhost', pathname: wire ? '/__csrf_probe' : '/private/control-room', reload() { callback++; } };
    const sandbox = { document, location, XMLHttpRequest, FormData, alert: m => messages.push(m) };
    vm.createContext(sandbox);
    for (const name of ['send_data.js', 'send_file.js']) vm.runInContext(fs.readFileSync(path.join(scripts, name), 'utf8'), sandbox);
    const event = { preventDefault() { prevented++; this.defaultPrevented = true; }, currentTarget: { name: 'uploadedFile', files: [{ name: 'fixture.jpg' }] } };
    function verify(method, expectedSend = 1) {
        check(sent === expectedSend, 'exactly one send or local refusal, never replay');
        if (!expectedSend) return;
        const req = requests[0];
        check(req.method === method.toLowerCase(), 'HTTP method preserved');
        check(!req.url.includes(token) && !req.url.includes('__RequestVerificationToken'), 'token absent from URL');
        if (method !== 'GET') {
            check((req.headers['X-Requested-With'] === 'XMLHttpRequest') === (method === 'POST'), 'explicit AJAX marker for POST only');
            check(req.headers['X-CSRF-TOKEN'] === token, 'actual sender supplies meta request token');
            check(req.data.entries.some(e => e[0] === '__RequestVerificationToken' && e[1] === token), 'FormData preserves hidden token');
        }
        else check(!req.headers['X-CSRF-TOKEN'], 'GET has no CSRF header');
        check(!req.headers['Content-Type'], 'sender leaves multipart boundary to transport');
        check(!messages.some(m => m.includes(token)), 'error messages never contain token');
    }
    return { sandbox, form, event, verify, listeners, requests, token, messages, state: () => ({ sent, callback, prevented }) };
}
for (const call of calls) {
    const f = fixture({ id: call.id });
    f.sandbox.createSendDataEventHandler(call.method, call.id, () => {})(f.event);
    f.verify(call.method);
}
const expected = {
    POST: [13,14,16,18,20,21,26,27,29,30,32,33,36,37,38,39,40,42,43,46,47,49,50,52,53,54,59],
    PUT: [24,35], DELETE: [15,17,22,25,28,31,34,41,44,48,51,58]
};
const covered = new Set();
for (const [method, pages] of Object.entries(expected)) for (const page of pages) {
    const f = fixture({ page: String(page), network: true });
    if (method === 'PUT') f.sandbox.createSendFileEventHandler('fixture', () => {}, null, '.jpg')(f.event);
    else if (page === 59) {
        const req = new f.sandbox.XMLHttpRequest();
        req.open('POST', f.sandbox.location.origin + f.sandbox.location.pathname, false);
        req.send(new f.sandbox.FormData(f.form));
        check(!req.headers['X-CSRF-TOKEN'], 'native HTML form uses hidden token without AJAX header');
        continue;
    }
    else f.sandbox.createSendDataEventHandler(method, 'fixture', () => {})(f.event);
    f.verify(method);
    covered.add(method + ':' + page);
}
check(covered.size === 40, '40 XHR operation/method pairs plus native initial password POST');
for (const status of [400,401,403,404,409,415,422,500]) {
    const f = fixture({ status });
    f.sandbox.createSendDataEventHandler('POST', 'fixture', () => { throw Error('false success'); })(f.event);
    f.verify('POST');
    check(f.messages.length === 1 && f.state().sent === 1, 'HTTP errors preserve single-send behavior');
}
for (const mode of ['data', 'file']) {
    const f = fixture({ missingMeta: true });
    if (mode === 'data') f.sandbox.createSendDataEventHandler('POST', 'fixture', () => {})(f.event);
    else f.sandbox.createSendFileEventHandler('fixture', () => {}, null, '.jpg')(f.event);
    f.verify('POST', 0);
    check(f.messages.length === 1, 'missing token causes visible local error without mutation');
}
const get = fixture();
get.sandbox.createSendDataEventHandler('GET', 'fixture', () => {})(get.event);
get.verify('GET');
const initial = sources.find(s => s.file.endsWith('ChangeInitialPassword.cshtml')).text;
const inline = initial.match(/document.getElementById\('initial-password-exit'\).addEventListener\('submit', function \(event\) \{([\s\S]*?)\n            \}\);/)[1];
const logout = fixture({ network: true, page: '58' });
logout.form.previousElementSibling = { action: logout.sandbox.location.origin + logout.sandbox.location.pathname };
logout.sandbox.form = logout.form;
logout.sandbox.event = logout.event;
vm.runInContext('(function(event) {' + inline + '}).call(form,event)', logout.sandbox);
logout.event.target = logout.form;
logout.listeners.submit(logout.event);
logout.verify('DELETE');
const enter = fixture();
enter.form.hasAttribute = () => true;
enter.form.querySelector = () => ({ click: () => enter.sandbox.createSendDataEventHandler('POST', 'fixture', () => {})(enter.event) });
enter.listeners.submit({ target: enter.form, preventDefault() {} });
enter.verify('POST');
check(enter.state().sent === 1, 'submit fallback invokes existing click validation once');
console.log(JSON.stringify({ checks, unsafeForms, xhrPairs: covered.size, nativePasswordPost: 1, wireRequests }));
