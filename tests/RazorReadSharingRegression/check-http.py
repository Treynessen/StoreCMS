import concurrent.futures as cf
import ctypes
from ctypes import wintypes
import hashlib
import json
import os
from pathlib import Path
import subprocess
import threading
import time
import urllib.error
import urllib.request

REPO = Path(__file__).resolve().parents[2]
WORK = Path(os.environ.get('STORECMS_RAZOR_RESULTS', REPO / 'work/e04-razor')).resolve()
HOSTS = Path(os.environ.get('STORECMS_RAZOR_HOSTS', WORK / 'hosts')).resolve()
RESULTS = []


def req(port, path, data=None):
    start = time.perf_counter_ns()
    body = None if data is None else json.dumps(data).encode()
    request = urllib.request.Request(f'http://127.0.0.1:{port}/probe/{path}', body, {'Content-Type': 'application/json'})
    try:
        with urllib.request.urlopen(request, timeout=90) as response:
            code, text = response.status, response.read().decode()
    except urllib.error.HTTPError as error:
        code, text = error.code, error.read().decode()
    return dict(code=code, text=text, start=start, end=time.perf_counter_ns())


def record(value):
    RESULTS.append(value)
    (WORK / 'sharing-http-results.json').write_text(json.dumps(RESULTS, ensure_ascii=False, indent=2), encoding='utf8')
    print(value['runtime'], value['kind'], value.get('cold'), value.get('writers'), value.get('series'), value['status'], flush=True)


native = ctypes.WinDLL('kernel32', use_last_error=True)
native.CreateFileW.argtypes = [wintypes.LPCWSTR, wintypes.DWORD, wintypes.DWORD, ctypes.c_void_p, wintypes.DWORD, wintypes.DWORD, wintypes.HANDLE]
native.CreateFileW.restype = wintypes.HANDLE
native.CloseHandle.argtypes = [wintypes.HANDLE]

for version in ('10', '22'):
    port = 51910 if version == '10' else 51912
    folder = HOSTS / ('published' + version)
    env = dict(os.environ, ASPNETCORE_ENVIRONMENT='Production', DOTNET_ENVIRONMENT='Production')
    env.pop('DOTNET_STARTUP_HOOKS', None)
    with (WORK / ('sharing-host' + version + '.log')).open('w', encoding='utf8') as log:
        process = subprocess.Popen(['dotnet', 'StoreCMS.dll', '--urls', f'http://127.0.0.1:{port}'], cwd=folder, env=env, stdout=log, stderr=subprocess.STDOUT)
        try:
            for _ in range(200):
                if process.poll() is not None:
                    raise RuntimeError('host stopped')
                try:
                    if req(port, 'health')['code'] == 200:
                        break
                except OSError:
                    pass
                time.sleep(.1)
            else:
                raise RuntimeError('host timeout')
            for cold in (True, False):
                name = 'held' + str(int(cold))
                assert req(port, 'generate/Templates/' + name, {'Source': 'OLD'})['code'] == 200
                if not cold:
                    assert req(port, 'view/' + name)['text'] == 'OLD\n'
                    assert req(port, 'generate/Templates/' + name, {'Source': 'NEW'})['code'] == 200
                    time.sleep(.5)
                handle = native.CreateFileW(str(folder / 'Views/Templates' / (name + '.cshtml')), 0x10000, 7, None, 3, 0x80, None)
                assert handle != ctypes.c_void_p(-1).value, ctypes.get_last_error()
                try:
                    reads = [req(port, 'view/' + name) for _ in range(3)]
                    expected = ('OLD' if cold else 'NEW') + '\n'
                    record(dict(runtime=version, kind='held-delete-http', cold=cold, status='PASS' if all(r['code'] == 200 and r['text'] == expected for r in reads) else 'FAIL', reads=reads))
                finally:
                    native.CloseHandle(handle)
            for writers, readers in ((1, 32), (4, 4), (32, 32)):
                for cold in (True, False):
                    for series in range(12):
                        name = f'e04r{writers}c{int(cold)}s{series}'
                        assert req(port, 'generate/Templates/' + name, {'Source': 'START'})['code'] == 200
                        if not cold:
                            assert req(port, 'view/' + name)['text'] == 'START\n'
                        barrier = threading.Barrier(writers + readers)

                        def writer(i):
                            barrier.wait(timeout=60)
                            return req(port, 'generate/Templates/' + name, {'Source': 'VERSION' + str(i) + 'x' * 5000})

                        def reader(i):
                            barrier.wait(timeout=60)
                            return [req(port, 'view/' + name) for _ in range(16)]

                        with cf.ThreadPoolExecutor(max_workers=writers + readers) as pool:
                            writes = [pool.submit(writer, i) for i in range(writers)]
                            reads = [pool.submit(reader, i) for i in range(readers)]
                            writes = [job.result() for job in writes]
                            reads = [r for job in reads for r in job.result()]
                        allowed = {'START\n'} | {'VERSION' + str(i) + 'x' * 5000 + '\n' for i in range(writers)}
                        errors = [r for r in reads if r['code'] != 200 or r['text'] not in allowed]
                        write_errors = [r for r in writes if r['code'] != 200]
                        post = [req(port, 'view/' + name) for _ in range(3)]
                        post_errors = [r for r in post if r['code'] != 200 or r['text'] not in allowed]
                        overlaps = sum(a['start'] < b['end'] and b['start'] < a['end'] for a in writes for b in reads)
                        def intervals(items):
                            return [[r['start'], r['end'], r['code'], hashlib.sha256(r['text'].encode()).hexdigest()] for r in items]
                        record(dict(runtime=version, kind='race', cold=cold, writers=writers, readers=readers, series=series,
                                    writes=len(writes), reads=len(reads), overlap_pairs=overlaps, errors=errors, write_errors=write_errors,
                                    post_errors=post_errors, read_intervals=intervals(reads), write_intervals=intervals(writes),
                                    status='PASS' if not errors and not write_errors and not post_errors and overlaps > 0 else 'FAIL'))
        finally:
            process.terminate()
            process.wait(timeout=20)
assert all(r['status'] == 'PASS' for r in RESULTS)
print('TOTAL PASS', len(RESULTS), flush=True)
