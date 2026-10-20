import json
import os
from pathlib import Path
import subprocess
import time
import urllib.request
import urllib.error

ROOT=Path(__file__).resolve().parents[2]
OUT=Path(os.environ.get('STORECMS_RAZOR_RESULTS',ROOT/'work/e01-razor')).resolve()
HOSTROOT=Path(os.environ.get('STORECMS_RAZOR_HOSTS',ROOT/'work/e0-razor10')).resolve()
OUT.mkdir(parents=True,exist_ok=True)
results=[]
for version in ('10','22'):
    base=HOSTROOT/('published'+version)
    imports=base/'Views/Templates/_ViewImports.cshtml'
    original=imports.read_bytes()
    port=51874
    def req():
        try:
            with urllib.request.urlopen(f'http://127.0.0.1:{port}/probe/view/importprobe',timeout=30) as response:
                return response.status,response.read().decode()
        except urllib.error.HTTPError as error:
            return error.code,error.read().decode()
    (base/'Views/Templates/importprobe.cshtml').write_text('@model Page\n@(new StringBuilder("IMPORT"))',encoding='utf8')
    with (OUT/f'imports{version}.log').open('w',encoding='utf8') as log:
        p=subprocess.Popen(['dotnet','StoreCMS.dll','--urls',f'http://127.0.0.1:{port}'],cwd=base,
                           env=dict(os.environ,ASPNETCORE_ENVIRONMENT='Production'),stdout=log,stderr=subprocess.STDOUT)
        try:
            before=None
            for _ in range(100):
                if p.poll() is not None: raise RuntimeError('host stopped')
                try:
                    before=req()
                    break
                except OSError: time.sleep(.15)
            imports.write_bytes(original+b'\n@using System.Text;\n')
            after=None
            for _ in range(40):
                after=req()
                if after==(200,'IMPORT'): break
                time.sleep(.2)
            results.append({'runtime':version,'name':'imports-invalidation','status':'PASS' if before is not None and before[0]==500 and after==(200,'IMPORT') else 'FAIL',
                            'before_code':None if before is None else before[0],'after':after})
        finally:
            p.terminate()
            p.wait(timeout=15)
            imports.write_bytes(original)
(OUT/'imports-results.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf8')
print(results)
assert all(r['status']=='PASS' for r in results)
