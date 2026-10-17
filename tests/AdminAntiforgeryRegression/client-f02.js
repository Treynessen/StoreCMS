const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.resolve(process.argv[2] || path.join(__dirname, '../..'));
const expectF02 = process.argv.includes('--expect-f02');
let checks = 0, cases = 0;
function check(value, label) { assert.ok(value, label); checks++; }
const script = fs.readFileSync(path.join(root, 'StoreCMS/Storage/scripts/admin_panel/send_data.js'), 'utf8');
const users = fs.readFileSync(path.join(root, 'StoreCMS/Views/AdminPanel/Users.cshtml'), 'utf8');
function fixture(status, body = '', options = {}) {
    let sends = 0, reloads = 0, success = 0, reports = 0;
    const messages = [], headers = {}, controls = {
        'user-password': { value: options.password === undefined ? 'admin' : options.password },
        'user-login': { value: options.login === undefined ? 'admin2' : options.login }
    };
    const form = { id: 'add-user', elements: [{ name: 'UserModel.Login', value: 'admin2' }, { name: 'UserModel.NewPassword', value: 'admin' }], checkValidity: () => options.valid !== false, reportValidity() { reports++; } };
    const before = JSON.stringify(form.elements);
    class FormData { append() {} }
    class XMLHttpRequest {
        constructor() { this.status = status; this.responseText = body; }
        open() {}
        setRequestHeader(name, value) { headers[name] = value; }
        send() { sends++; if (options.network) throw new Error('Network failure'); }
        getResponseHeader() { return options.contentType === undefined ? 'application/json; charset=utf-8' : options.contentType; }
    }
    const sandbox = { FormData, XMLHttpRequest,
        alert: message => messages.push(message),
        location: { origin: 'https://localhost', pathname: '/admin', reload() { reloads++; } },
        document: { addEventListener() {}, querySelector: () => ({ content: 'fixture-csrf' }), getElementById: id => controls[id] || form } };
    vm.createContext(sandbox);
    vm.runInContext(script, sandbox);
    const callbacks = users.slice(users.indexOf('function errorMsg('), users.indexOf('</script>')).replace(/@Html.Raw\(localization\.\w+\)/g, 'SUCCESS');
    vm.runInContext(callbacks, sandbox);
    const event = { preventDefault() {}, stopImmediatePropagation() {} };
    sandbox.createSendDataEventHandler('POST', 'add-user', request => { success++; sandbox.responseHandler(request); }, sandbox.errorMsg, 201)(event);
    check(before === JSON.stringify(form.elements), 'user fields preserved');
    if (options.valid !== false) check(headers['X-CSRF-TOKEN'] === 'fixture-csrf', 'CSRF header retained on mutation');
    check(sends <= 1, 'no automatic replay');
    cases++;
    return { messages, sends, reloads, success, reports, sandbox, event };
}
if (expectF02) {
    const f = fixture(422, '', { contentType: '' });
    check(f.messages[0].includes('Данные не приняты (HTTP 422)'), 'F-02 generic diagnostic reproduced');
    check(f.sends === 1 && f.reloads === 0 && f.success === 0, 'reproduction has no false success');
    console.log('F-02 client reproduced: empty 422 produces generic validation message.');
}
else {
    const reasons = [
        [422,'user_data_required','обязательные данные'],[422,'user_login_required','Введите логин'],
        [422,'user_login_invalid','латинские буквы'],[422,'user_password_required','Введите пароль'],
        [422,'user_password_invalid','не менее 5'],[422,'user_role_required','тип пользователя'],
        [422,'user_role_invalid','не существует'],[422,'user_id_required','идентификатор'],
        [422,'user_idle_time_invalid','от 10 до 10080'],[409,'user_login_taken','уже существует']
    ];
    for (const [status, code, fragment] of reasons) {
        const f = fixture(status, JSON.stringify({ error: code }));
        check(f.messages.length === 1 && f.messages[0].includes(fragment), 'specific reason shown from allowlisted code');
        check(f.sends === 1 && f.reloads === 0 && f.success === 0, 'validation refusal cannot succeed or reload');
        if (code === 'user_password_invalid') check(f.messages[0].includes('admin запрещён без учёта регистра') && f.messages[0].includes('Пробелы'), 'actual password policy explained');
    }
    for (const body of ['', '{', 'null', '[]', '{}', '{"error":"unknown","message":"SECRET_SQL_EXCEPTION"}', '{"error":"__proto__"}', '{"error":"user_login_taken"}']) {
        const f = fixture(422, body);
        check(f.messages.length === 1 && f.messages[0].includes('Данные не приняты (HTTP 422)'), 'invalid unknown or status-mismatched payload uses safe fallback');
        check(!f.messages[0].includes('SECRET_SQL_EXCEPTION') && !f.success && !f.reloads, 'untrusted response text not reflected');
    }
    for (const status of [400,401,403,409,500,0]) {
        const f = fixture(status, '{"error":"user_password_invalid","message":"SECRET_SQL_EXCEPTION"}');
        check(f.messages.length === 1 && !f.messages[0].includes('не менее 5') && !f.messages[0].includes('SECRET_SQL_EXCEPTION'), 'other HTTP statuses keep their own diagnostics');
        check(!f.success && !f.reloads, 'no false HTTP success');
    }
    const network = fixture(0, '', { network: true });
    check(network.messages[0].includes('Результат операции неизвестен') && !network.success && !network.reloads, 'network behavior unchanged');
    const legacy = fixture(422, '', { contentType: '' });
    check(legacy.messages[0].includes('Данные не приняты'), 'legacy empty 422 remains compatible');
    const html = fixture(422, '{"error":"user_password_invalid"}', { contentType: 'text/html' });
    check(html.messages[0].includes('Данные не приняты'), 'HTML body not parsed as validation JSON');
    const good = fixture(201, '', { contentType: '' });
    check(good.success === 1 && good.reloads === 1 && good.messages.length === 1, 'real Users success callback preserved');
    const invalid = fixture(422, '', { valid: false });
    check(invalid.sends === 0 && invalid.reports === 1 && invalid.messages.length === 0, 'native validity reports required field without sending');
    const local = users.match(/addButton.addEventListener\('click', \(e\) => \{([\s\S]*?)\n                \}\);/)[1].replace(/@Html.Raw\(localization\.\w+\)/g, 'MINIMUM_FIVE');
    for (const [password, login, message] of [['','admin2','Введите пароль.'],['valid-password','','Введите логин.'],['abcd','admin2','MINIMUM_FIVE']]) {
        const f = fixture(422, '', { password, login });
        f.messages.length = 0;
        f.sandbox.event = f.event;
        vm.runInContext('(function(e) {' + local + '})(event)', f.sandbox);
        check(f.messages.length === 1 && f.messages[0] === message, 'actual Users button reports local field problem');
    }
}
console.log(JSON.stringify({ checks, cases, expectF02 }));
