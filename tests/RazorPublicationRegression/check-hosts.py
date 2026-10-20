import concurrent.futures as cf
import hashlib
import json
import os
from pathlib import Path
import subprocess
import time
import threading
import sys
import urllib.request
import urllib.error
from PIL import Image

REPO=Path(__file__).resolve().parents[2]
ROOT=Path(os.environ.get('STORECMS_RAZOR_RESULTS',REPO/'work/e01-razor')).resolve()
HOSTROOT=Path(os.environ.get('STORECMS_RAZOR_HOSTS',ROOT.parent/'e0-razor10')).resolve()
ROOT.mkdir(parents=True,exist_ok=True)
manifest=json.loads((HOSTROOT/'source-manifest.json').read_text(encoding='utf-8-sig'))
for name,digest in manifest.items():
    for source in (HOSTROOT/'shared/Production'/name,REPO/'StoreCMS'/name):
        assert hashlib.sha256(source.read_bytes()).hexdigest().upper()==digest,source
(ROOT/'manifest-check.json').write_text(json.dumps({'status':'PASS','files':len(manifest)}),encoding='utf8')
if not (ROOT/'e0-evidence.json').exists():
    baseline={p.relative_to(HOSTROOT).as_posix():p.read_text(encoding='utf8') for p in (HOSTROOT/'evidence').glob('*/*.json')}
    (ROOT/'e0-evidence.json').write_text(json.dumps(baseline,ensure_ascii=False),encoding='utf8')
RESULTS=[]
PROCS=[]
def req(port,path,data=None):
    body=None if data is None else json.dumps(data).encode()
    r=urllib.request.Request(f'http://127.0.0.1:{port}/probe/{path}',body,{'Content-Type':'application/json'})
    try:
        with urllib.request.urlopen(r,timeout=40) as response: return response.status,response.read().decode()
    except urllib.error.HTTPError as error: return error.code,error.read().decode()
def save(name,port,value):
    dest=ROOT/'evidence'/str(port)
    dest.mkdir(parents=True,exist_ok=True)
    (dest/(name+'.json')).write_text(json.dumps(value,ensure_ascii=False,indent=2),encoding='utf8')
def record(name,ok,detail):
    RESULTS.append({'name':name,'status':'PASS' if ok else 'FAIL','detail':detail})
    (ROOT/'results.json').write_text(json.dumps(RESULTS,ensure_ascii=False,indent=2),encoding='utf8')
    print(name,RESULTS[-1]['status'],flush=True)
def start(version,port):
    folder=HOSTROOT/('published'+version)
    log=open(ROOT/f'host{version}-{len(PROCS)}.log','w',encoding='utf8')
    env=dict(os.environ,ASPNETCORE_ENVIRONMENT='Production',DOTNET_ENVIRONMENT='Production')
    p=subprocess.Popen(['dotnet','StoreCMS.dll','--urls',f'http://127.0.0.1:{port}'],cwd=folder,env=env,stdout=log,stderr=subprocess.STDOUT)
    PROCS.append(p)
    for _ in range(100):
        if p.poll() is not None: raise RuntimeError(f'Host {version} stopped; see log')
        try:
            status,text=req(port,'health')
            if status==200: save('health',port,text); return p
        except OSError: pass
        time.sleep(.15)
    raise RuntimeError('Host timeout')
def generate(port,name,source,folder='Templates'):
    result=req(port,f'generate/{folder}/{name}',{'Source':source})
    save('generated-'+name,port,result)
    return result
def view(port,name):
    result=req(port,'view/'+name)
    save('html-'+name.replace('?','_'),port,result)
    return result
def eventual(port,name,expected):
    attempts=[]
    for _ in range(40):
        s,t=req(port,'view/'+name); attempts.append((s,t))
        if s==200 and t.strip()==expected:
            return True,len(attempts)
        time.sleep(.2)
    save('eventual-'+name,port,attempts)
    return False,len(attempts)

try:
    for version in ('10','22'):
        base=HOSTROOT/('published'+version)
        for name in ('late','broken','race','rawmulti','cold'):
            path=base/('Views/Templates/'+name+'.cshtml')
            if path.exists(): path.unlink()
        (base/'Settings').mkdir(exist_ok=True)
        (base/'Settings/product_block.cshtml').write_text('@model ProductPage\n<span>[email]@Model.PageName</span>',encoding='utf8')
        image=base/'Storage/images/products/41/probe.jpg'
        image.parent.mkdir(parents=True,exist_ok=True)
        Image.new('RGB',(8,6),(20,100,180)).save(image)
    modern=start('10',51870)
    legacy=start('22',51872)
    ports=(51870,51872)
    cases={
        'plain':('<h1>Plain</h1>','<h1>Plain</h1>'),
        'values':('[Page:Title]|[Page:Name]|[Page:Content]|[Counter id=1][Counter id=1]','Title <b>raw</b>|Product|<em>body</em>|12'),
        'nested':('[#alpha]','AB'),
        'condition':('[Product:IfSpecial(YES [Product:Name])]|[Product:IfStock(STOCK)]','YES Product|STOCK'),
        'loop':('[ProductList]','<span>[email]Product</span><span>[email]Second</span>'),
        'multiline':('first\n[Page:Name]\nlast','first\nProduct\nlast'),
        'pagination':('<page-buttons current-path="/catalog" current-page="2" pages-count="3" class="pages"></page-buttons>',None),
        'image':('<image src="/images/products/41/probe.jpg" width="4" height="3" alt="probe"></image>',None),
        'multicond':('[Product:IfSpecial(first\nsecond)]','first\nsecond'),
        'condcrlf':('[Product:IfSpecial(first\r\nsecond)]','first\r\nsecond'),
        'condcr':('[Product:IfSpecial(first\rsecond)]','first\rsecond'),
        'condquotes':('[Product:IfSpecial(<b>"Привет"</b> C:\\folder\\file | \\n | @mail | 雪)]','<b>"Привет"</b> C:\\folder\\file | \\n | @mail | 雪'),
        'condnested':('[Product:IfSpecial(до\r\n[Product:IfStock(<b>вложено</b>\n[Product:Name])]\rпосле)]','до\r\n<b>вложено</b>\nProduct\rпосле'),
        'condmultiimage':('[Product:IfSpecial(до\r\n<image src="/images/products/41/probe.jpg" width="4" height="3"></image>\nпосле)]',None),
        'conditionimage':('[Product:IfSpecial(<image src="/images/products/41/probe.jpg" width="4" height="3"></image>)]',None)
    }
    for port in ports:
        generate(port,'beta','B','Chunks'); generate(port,'alpha','A[#beta]','Chunks')
    for name,(source,expected) in cases.items():
        outputs=[]
        for port in ports:
            g=generate(port,name,source)
            out=view(port,name); outputs.append(out)
            assert g[0]==200,g
        valid=all(s==200 for s,t in outputs)
        if expected is not None: valid=valid and all(t.strip()==expected for s,t in outputs)
        elif name=='pagination': valid=valid and all('<ul' in t and '<span>2</span>' in t and '<page-buttons' not in t for s,t in outputs)
        elif name in ('image','conditionimage','condmultiimage'):
            valid=valid and all('<img' in t and 'probe_4x3.jpg' in t for s,t in outputs)
            valid=valid and all(Image.open(HOSTROOT/('published'+v)/'Storage/images/products/41/probe_4x3.jpg').size==(4,3) for v in ('10','22'))
        record(name,valid,{'codes':[s for s,t in outputs],'html_equal':outputs[0]==outputs[1],'expected':expected})
        if expected is not None:
            exact=expected+('\n' if name in ('plain','nested','multiline') else '')
            record('exact-html-'+name,all(t==exact for s,t in outputs),{'expected':exact})
        if name=='condmultiimage':
            exact='до\r\n<img src="/images/products/41/probe_4x3.jpg"></img>\nпосле'
            record('multiline-taghelper-html',all(t==exact for s,t in outputs),{'expected':exact})
        if name in ('plain','values','nested','condition','loop','multiline','pagination','image','conditionimage'):
            baseline=json.loads((ROOT/'e0-evidence.json').read_text(encoding='utf8'))
            same=all(json.loads(baseline[f'evidence/{port}/html-{name}.json'])==list(out) for port,out in zip(ports,outputs))
            record('e0-html-unchanged-'+name,same,{'comparison':'точный HTTP-статус и HTML прежнего E0'})
    for port in ports:
        false=view(port,'condition?special=false')
        record('condition-false-'+str(port),false[0]==200 and false[1].strip()=='|STOCK',{'actual':false})
        base=HOSTROOT/('published10' if port==51870 else 'published22')
        (base/'Views/Templates/rawmulti.cshtml').write_text('@model Page\n@{\nvar number = 2;\nnumber += 3;\n}\n<p>@number</p>',encoding='utf8')
        raw=view(port,'rawmulti')
        record('raw-razor-multiline-'+str(port),raw[0]==200 and raw[1].strip()=='<p>5</p>',{})
        generate(port,'cold','[Page:Name]')
        with cf.ThreadPoolExecutor(max_workers=16) as pool:
            cold=list(pool.map(lambda i:req(port,'view/cold'),range(32)))
        record('cold-parallel-'+str(port),all(s==200 and t.strip()=='Product' for s,t in cold),{'requests':32})
        generate(port,'beta','CHANGED','Chunks')
        ok,count=eventual(port,'nested','ACHANGED')
        record('nested-invalidation-'+str(port),ok,{'attempts':count})
        generate(port,'beta','B','Chunks')
        eventual(port,'nested','AB')
        absent=view(port,'late')
        generate(port,'late','LATE1')
        ok,count=eventual(port,'late','LATE1')
        record('new-after-miss-'+str(port),absent[0]==500 and ok,{'before':absent[0],'attempts':count})
        generate(port,'late','LATE2')
        ok,count=eventual(port,'late','LATE2')
        record('update-'+str(port),ok,{'attempts':count})
        record('repeat-'+str(port),req(port,'view/late')==(200,'LATE2\n'),{})
        before=hashlib.sha256((HOSTROOT/('published10' if port==51870 else 'published22')/'Views/Templates/plain.cshtml').read_bytes()).hexdigest()
        path=HOSTROOT/('published10' if port==51870 else 'published22')/'Views/Templates/broken.cshtml'
        path.write_text('@model ProductPage\n@(NoSuchSymbol)',encoding='utf8')
        broken=view(port,'broken')
        after=hashlib.sha256((path.parent/'plain.cshtml').read_bytes()).hexdigest()
        record('compile-error-'+str(port),broken[0]==500 and before==after and req(port,'view/plain')[0]==200,{'code':broken[0],'unrelated_hash_unchanged':before==after})
        path.write_text('@model ProductPage\nRECOVERED',encoding='utf8')
        ok,count=eventual(port,'broken','RECOVERED')
        record('recover-'+str(port),ok,{'attempts':count})
        missing=view(port,'missing')
        record('missing-'+str(port),missing[0]==500,{'code':missing[0]})
        with cf.ThreadPoolExecutor(max_workers=16) as pool:
            parallel=list(pool.map(lambda i:req(port,'view/nested'),range(64)))
        record('parallel-'+str(port),all(s==200 and t.strip()=='AB' for s,t in parallel),{'requests':len(parallel)})
        generate(port,'race','START')
        barrier=threading.Barrier(64)
        write_intervals=[]
        read_intervals=[]
        def write_race(i):
            barrier.wait(timeout=30)
            began=time.monotonic()
            result=req(port,'generate/Templates/race',{'Source':'VERSION'+str(i)+'x'*5000})
            write_intervals.append((began,time.monotonic()))
            return result
        def read_race(i):
            barrier.wait(timeout=30)
            outputs=[]
            for _ in range(16):
                began=time.monotonic()
                outputs.append(req(port,'view/race'))
                read_intervals.append((began,time.monotonic()))
            return outputs
        with cf.ThreadPoolExecutor(max_workers=64) as pool:
            jobs=[pool.submit(write_race,i) for i in range(32)]
            reads=[pool.submit(read_race,i) for i in range(32)]
            races=[j.result() for j in jobs]
            concurrent_reads=[out for j in reads for out in j.result()]
        save('race-writes',port,races)
        save('race-reads',port,concurrent_reads)
        final=req(port,'view/race')
        allowed={'START'}|{'VERSION'+str(i)+'x'*5000 for i in range(32)}
        overlaps=sum(a<d and c<b for a,b in write_intervals for c,d in read_intervals)
        record('concurrent-update-'+str(port),overlaps>0 and all(s==200 for s,t in races) and all(s==200 and t.strip() in allowed for s,t in concurrent_reads) and final[0]==200 and final[1].strip() in allowed,{'write_codes':[s for s,t in races],'read_codes':[s for s,t in concurrent_reads],'final_code':final[0],'overlapping_http_intervals':overlaps})
        record('paths-'+str(port),req(port,'view/Plain')[0]==200,{'os':'Windows','linux_case_sensitivity':'NOT TESTED'})
    modern.terminate(); modern.wait(timeout=15)
    modern=start('10',51870)
    record('restart10',req(51870,'view/late')==(200,'LATE2\n'),{})
    record('published-host',True,{'environment':'Production','launch':'dotnet StoreCMS.dll','ports':ports,'runtime_compilation':'10.0.13'})
finally:
    for p in PROCS:
        if p.poll() is None:
            p.terminate()
            p.wait(timeout=15)
print('TOTAL',len(RESULTS),'FAIL',sum(r['status']=='FAIL' for r in RESULTS))
sys.exit(1 if any(r['status']=='FAIL' for r in RESULTS) else 0)
