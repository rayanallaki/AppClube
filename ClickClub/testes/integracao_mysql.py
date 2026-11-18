"""Teste de integração destrutivo apenas no sentido de criar/alterar dados fictícios.
Não elimina schemas nem tabelas. Execute em banco de teste novo e vazio.
Python 3.10+ e SDK .NET 8. Sem dependências Python externas.
"""
import concurrent.futures
import datetime as dt
import json
import os
import argparse
import base64
import re
import secrets
import shutil
import tempfile
from pathlib import Path
import subprocess
import time
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description='Integração HTTP com MySQL de teste; inicia e reinicia a API. Não apaga o banco.')
parser.add_argument('--config', type=Path, help='JSON local privado com ConnectionStrings:ClickClub e Credenciais:Chave; alternativamente, use variáveis de ambiente.')
parser.add_argument('--api-url', default='http://127.0.0.1:5089', help='Porta livre para o processo iniciado pelo teste.')
parser.add_argument('--aceito-banco-teste', action='store_true', help='Confirma criação de dados fictícios em banco separado terminado em _test ou _teste.')
args = parser.parse_args()
if not args.aceito_banco_teste:
    parser.error('Confirme --aceito-banco-teste. Use somente um schema vazio separado para testes.')
CONFIG = json.loads(args.config.read_text(encoding='utf-8')) if args.config else {
    'ConnectionStrings': {'ClickClub': os.environ.get('CLICKCLUB_TEST_CONNECTION', '')},
    'Credenciais': {'Chave': os.environ.get('CLICKCLUB_TEST_KEY', base64.b64encode(secrets.token_bytes(32)).decode())}
}
connection = CONFIG.get('ConnectionStrings', {}).get('ClickClub', '')
database = re.search(r'(?:^|;)\s*(?:Database|Initial Catalog)\s*=\s*([^;]+)', connection, re.IGNORECASE)
if not database or not database.group(1).strip(' \"').lower().endswith(('_test', '_teste')):
    parser.error('A conexão deve indicar explicitamente um schema terminado em _test ou _teste. Não use banco de produção.')
DOTNET = shutil.which('dotnet')
if not DOTNET:
    parser.error('SDK .NET 8 não encontrado no PATH.')
BASE = args.api_url.rstrip('/')
DLL = ROOT/'api/bin/Debug/net8.0/ClickClub.Api.dll'
if not DLL.exists():
    parser.error('Compile primeiro: dotnet build api/ClickClub.Api.csproj')
work = Path(tempfile.mkdtemp(prefix='clickclub-test-'))
LOG = work/'api.log'
CLUB = '11111111-1111-1111-1111-111111111111'
MEMBER = '22222222-2222-2222-2222-222222222222'
SPACE = '33333333-3333-3333-3333-333333333333'
checks = []
process = None

def request(method, path, data=None, token=None, expected=(200, 201, 204)):
    headers = {'Content-Type': 'application/json'}
    if token:
        headers['Authorization'] = 'Bearer ' + token
    req = urllib.request.Request(BASE + path, data=None if data is None else json.dumps(data).encode(), headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, timeout=30) as response:
            status, body = response.status, response.read()
    except urllib.error.HTTPError as e:
        status, body = e.code, e.read()
    if status not in expected:
        raise AssertionError(f'{method} {path}: esperado {expected}, recebido HTTP {status}. Consulte o log local da API.')
    return (json.loads(body) if body else None), status

def check(name):
    checks.append(name)
    print('OK:', name, flush=True)

def start():
    global process
    env = dict(os.environ)
    env.update({'ASPNETCORE_ENVIRONMENT':'Development', 'ASPNETCORE_URLS':BASE,
                'ConnectionStrings__ClickClub':CONFIG['ConnectionStrings']['ClickClub'],
                'Credenciais__Chave':CONFIG['Credenciais']['Chave']})
    log = open(LOG, 'a', encoding='utf-8')
    process = subprocess.Popen([DOTNET, str(DLL)], cwd=ROOT/'api', env=env, stdout=log, stderr=log)
    log.close()
    for _ in range(100):
        if process.poll() is not None:
            raise RuntimeError(f'API encerrou. Log local do teste: {LOG}')
        try:
            request('GET','/api/saude')
            return
        except (OSError, AssertionError):
            time.sleep(.2)
    raise RuntimeError('API não iniciou em 20 segundos')

def stop():
    if process is not None and process.poll() is None:
        process.terminate()
        process.wait(timeout=15)

def login(name):
    return request('POST','/api/auth/login',{'login':name,'senha':'Teste@123'})[0]

def iso(value):
    return value.isoformat(timespec='seconds')

try:
    start()
    assert request('GET','/api/saude')[0]['banco'] == 'MySQL'
    check('Migração automática, seed inicial e saúde MySQL')
    admin = login('admin')['token']
    titular = login('titular')
    token = titular['token']
    user_id = titular['usuario']['id']
    porteiro = login('porteiro')['token']
    request('GET','/api/usuarios',token=token,expected=(403,))
    request('POST','/api/auth/login',{'login':'admin','senha':'incorreta'},expected=(401,))
    check('Login, sessão e autorização por perfil')
    users = request('GET','/api/usuarios',token=admin)[0]
    dependent = next(u for u in users if u['login']=='dependente')
    request('POST',f'/api/associacoes/{MEMBER}/dependentes',{'usuarioId':dependent['id']},token)
    dep_token = login('dependente')['token']
    profile = {'cpf':'529.982.247-25','cidade':'Foz do Iguaçu','interesses':['Natação','natação','Tênis']}
    assert request('PUT',f'/api/usuarios/{user_id}/dados',profile,token)[0]['cpf']=='52998224725'
    request('PUT',f'/api/usuarios/{user_id}/dados',{'cpf':'111.111.111-11'},token,expected=(400,))
    assert request('GET',f'/api/usuarios/{user_id}/dados',token=token)[0]['dados']['cpf']=='52998224725'
    request('PUT',f'/api/usuarios/{dependent["id"]}/dados',profile,dep_token,expected=(400,))
    check('Dependente vinculado; CPF válido normalizado; inválido e duplicado rejeitados')
    club = next(c for c in request('GET','/api/clubes',token=admin)[0] if c['id']==CLUB)
    club['nome'] = 'Clube MySQL de teste'
    request('PUT',f'/api/clubes/{CLUB}',club,admin)
    assert next(c for c in request('GET','/api/clubes',token=admin)[0] if c['id']==CLUB)['nome']==club['nome']
    request('PUT','/api/categorias/familiar',{'name':'FAMILIAR','valorMensal':150,'limiteDependentes':5},admin)
    request('POST','/api/atividades',{'nome':'Natação','descricao':'Piscina','tipo':'Esportiva'},admin)
    check('Atualização de entidade existente, categoria e atividade')
    now = dt.datetime.now(dt.timezone.utc).replace(microsecond=0)
    next_month = (now.replace(day=1)+dt.timedelta(days=32)).replace(day=1)
    start_time = next_month+dt.timedelta(days=10,hours=18)
    invoice = request('POST','/api/faturas',{'associacaoId':MEMBER,'competencia':next_month.strftime('%Y-%m'),'vencimento':(next_month+dt.timedelta(days=5)).date().isoformat(),'valor':150},admin)[0]
    booking_data = {'associacaoId':MEMBER,'espacoId':SPACE,'inicio':iso(start_time),'fim':iso(start_time+dt.timedelta(hours=3)), 'finalidade':'Teste MySQL', 'convidados':[{'nome':'Convidado fictício','identificacao':'TESTE-001'}]}
    clubs = request('GET','/api/clubes',token=admin)[0]
    another_club = next((c for c in clubs if c['id'] != CLUB), None)
    if another_club is None:
        another_club = request('POST','/api/clubes',{'nome':'Unidade isolada de teste','sigla':'QA-'+secrets.token_hex(3),'fusoHorario':'America/Sao_Paulo'},admin)[0]
    another_space = request('POST','/api/espacos',{'clubeId':another_club['id'],'nome':'Espaço de teste de escopo','tipo':'Quadra','capacidade':10,'taxa':0},admin)[0]
    employee_token = login('funcionario')['token']
    wrong_scope = dict(booking_data, espacoId=another_space['id'])
    request('POST','/api/reservas',wrong_scope,employee_token,expected=(403,))
    # A autorização de um dependente não pode ser consumida por reserva do titular.
    authorization = request('POST',f'/api/associacoes/{MEMBER}/autorizacoes',{'dependenteId':dependent['id'],'espacoId':SPACE},token)[0]
    booking_data['autorizacaoId'] = authorization['id']
    check('Funcionário não reserva em unidade fora de seu escopo')
    def reserve(_):
        return request('POST','/api/reservas',booking_data,token,expected=(201,400))
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
        attempts = list(pool.map(reserve,range(2)))
    assert sorted(x[1] for x in attempts)==[201,400]
    booking = next(x[0] for x in attempts if x[1]==201)
    saved_auth = next(a for a in request('GET',f'/api/associacoes/{MEMBER}/autorizacoes',token=token)[0] if a['id']==authorization['id'])
    assert saved_auth['usada'] is False
    check('Reserva do titular não consome autorização emitida para dependente')
    request('GET',f'/api/reservas/{booking["id"]}/transferencia-previa',token=token)
    for _ in range(2):
        request('POST',f'/api/reservas/{booking["id"]}/transferir',{'faturaId':invoice['id']},token)
    assert len(request('GET',f'/api/faturas/{invoice["id"]}/itens',token=token)[0])==2
    assert request('GET','/api/faturas',token=token)[0][0]['valor']==260
    check('Reservas simultâneas: apenas uma aceita; transferência financeira sem duplicação')
    request('POST',f'/api/reservas/{booking["id"]}/cancelar',{},token)
    assert request('GET',f'/api/faturas/{invoice["id"]}/itens',token=token)[0]==[]
    assert request('GET','/api/faturas',token=token)[0][0]['valor']==150
    booking_data['inicio']=iso(start_time+dt.timedelta(days=1))
    booking_data['fim']=iso(start_time+dt.timedelta(days=1,hours=3))
    kept_booking=request('POST','/api/reservas',booking_data,token)[0]
    check('Cancelamento ajusta fatura e exclui itens vinculados')
    course = request('POST','/api/turmas',{'clubeId':CLUB,'nome':'Natação','inicio':iso(now+dt.timedelta(days=3)),'fim':iso(now+dt.timedelta(days=4)),'capacidade':1},admin)[0]
    request('PUT',f'/api/turmas/{course["id"]}/aulas',[{'inicio':iso(now+dt.timedelta(days=3,hours=1)),'fim':iso(now+dt.timedelta(days=3,hours=2))}],admin)
    enrollment=request('POST',f'/api/turmas/{course["id"]}/inscricoes',{},token)[0]
    assert enrollment['status']=='Confirmada'
    assert request('POST',f'/api/turmas/{course["id"]}/inscricoes',{},dep_token)[0]['status']=='Espera'
    request('POST',f'/api/inscricoes/{enrollment["id"]}/cancelar',{},token)
    assert request('POST',f'/api/turmas/{course["id"]}/promover',{},admin)[0]['status']=='Confirmada'
    # Remove e reinsere chave composta em uma requisição.
    aulas=request('GET',f'/api/turmas/{course["id"]}/aulas',token=admin)[0]
    request('PUT',f'/api/turmas/{course["id"]}/aulas',aulas,admin)
    check('Turmas, aulas, lista de espera e promoção persistidas')
    event=request('POST','/api/eventos',{'clubeId':CLUB,'nome':'Evento de teste','inicio':iso(now-dt.timedelta(hours=1)),'fim':iso(now+dt.timedelta(hours=2)),'publicoAlvo':'PublicoGeral','capacidade':10},admin)[0]
    visitor=request('POST',f'/api/eventos/{event["id"]}/visitantes',{'nome':'Visitante fictício','identificacao':'VISITANTE-TESTE'},admin)[0]
    assert request('POST','/api/portaria/validar-visitante',{'clubeId':CLUB,'visitanteId':visitor['id'],'identificacao':'VISITANTE-TESTE'},porteiro)[0]['permitido']
    request('POST',f'/api/eventos/{event["id"]}/inscricoes',{},token)
    card=request('POST','/api/carteirinha',{},token)[0]['credencial']
    assert request('POST','/api/portaria/validar',{'clubeId':CLUB,'credencial':card},porteiro)[0]['permitido']
    check('Eventos, visitantes, inscrições, credencial assinada e validação na portaria')
    # Limite exato da tolerância, conforme a data local do clube de exemplo.
    today_local = dt.datetime.now(dt.timezone(dt.timedelta(hours=-3))).date()
    due10 = request('POST','/api/faturas',{'associacaoId':MEMBER,'competencia':'2000-01','vencimento':(today_local-dt.timedelta(days=10)).isoformat(),'valor':150},admin)[0]
    assert request('POST','/api/portaria/validar',{'clubeId':CLUB,'credencial':card},porteiro)[0]['permitido']
    due11 = request('POST','/api/faturas',{'associacaoId':MEMBER,'competencia':'2000-02','vencimento':(today_local-dt.timedelta(days=11)).isoformat(),'valor':150},admin)[0]
    assert request('POST','/api/portaria/validar',{'clubeId':CLUB,'credencial':card},porteiro)[0]['permitido'] is False
    request('POST','/api/carteirinha',{},dep_token,expected=(403,))
    request('POST',f'/api/faturas/{due11["id"]}/quitar',{},token)
    assert request('POST','/api/portaria/validar',{'clubeId':CLUB,'credencial':card},porteiro)[0]['permitido']
    check('Mensalidade: 10 dias sem bloqueio; 11 bloqueiam o grupo; quitação regulariza')
    request('POST','/api/avisos',{'clubeId':CLUB,'titulo':'Aviso MySQL','corpo':'Teste de persistência'},admin)
    ticket=request('POST','/api/chamados',{'clubeId':CLUB,'tipo':'Solicitacao','assunto':'Teste MySQL','descricao':'Teste'},token)[0]
    request('POST',f'/api/chamados/{ticket["id"]}/respostas',{'texto':'Resposta persistida'},admin)
    request('PUT',f'/api/chamados/{ticket["id"]}/status',{'status':'Concluido'},admin)
    candidate_login='candidato-'+secrets.token_hex(4)
    application=request('POST','/api/adesoes',{'clubeId':CLUB,'nome':'Candidato fictício','email':'ficticio@example.com','tipoVinculo':'Convidado','login':candidate_login,'senha':'Teste@123'})[0]
    request('POST',f'/api/adesoes/{application["id"]}/decisao',{'status':'Aprovada','categoria':'Individual','matricula':'MYSQL-002'},admin)
    check('Avisos, chamados com respostas e adesão pública aprovada')
    audit_count=len(request('GET','/api/auditoria',token=admin)[0])
    stop()
    start()
    assert request('GET','/api/auth/eu',token=token)[0]['id']==user_id
    assert request('GET',f'/api/usuarios/{user_id}/dados',token=token)[0]['dados']['cidade']=='Foz do Iguaçu'
    assert len(request('GET','/api/usuarios',token=admin)[0])==len(users)+1
    assert any(x['id']==kept_booking['id'] for x in request('GET','/api/reservas',token=token)[0])
    assert request('GET',f'/api/reservas/{kept_booking["id"]}/convidados',token=token)[0][0]['identificacao']=='TESTE-001'
    saved_ticket=next(t for t in request('GET','/api/chamados',token=token)[0] if t['id']==ticket['id'])
    assert saved_ticket['respostas'][0]['texto']=='Resposta persistida' and saved_ticket['status']=='Concluido'
    assert len(request('GET','/api/auditoria',token=admin)[0])==audit_count
    assert request('POST','/api/portaria/validar',{'clubeId':CLUB,'credencial':card},porteiro)[0]['permitido']
    assert login(candidate_login)['usuario']['papeis']==['Titular']
    check('Reinício preserva usuários/IDs, sessões, CPF, reservas, convidados, chamados, auditoria e credenciais; seed não duplica')
    request('POST','/api/auth/sair',{},token)
    request('GET','/api/auth/eu',token=token,expected=(401,))
    check('Logout revoga sessão persistida')
    print(json.dumps({'passed':len(checks),'checks':checks},ensure_ascii=False,indent=2),flush=True)
finally:
    stop()
