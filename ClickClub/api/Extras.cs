using System.Security.Cryptography;
using System.Text;

static class Extras
{
 static byte[] Chave = Array.Empty<byte>();
 public static void ConfigurarChave(byte[] chave) => Chave = chave;
 static IResult Forbidden()=>Results.StatusCode(403);
 static bool Own(Store db,Guid association,Guid user)=>db.Members.TryGetValue(association,out var m)&&(m.UsuarioTitularId==user||m.Dependentes.Contains(user));
 public static bool Active(Store db,Guid user,out Associacao? member){member=db.Members.Values.FirstOrDefault(m=>m.Ativa&&(m.UsuarioTitularId==user||m.Dependentes.Contains(user)));return member is not null&&db.Users.TryGetValue(user,out var pessoa)&&pessoa.Ativo&&db.Users.TryGetValue(member.UsuarioTitularId,out var titular)&&titular.Ativo&&db.Clubs.TryGetValue(member.ClubeOrigemId,out var clube)&&clube.Ativo&&db.Categorias.TryGetValue(member.Categoria,out var categoria)&&categoria.Ativa&&!db.Blocked(member.Id);}
 static string Sign(string data)=>Convert.ToHexString(HMACSHA256.HashData(Chave,Encoding.UTF8.GetBytes(data)));
 static bool Valid(string token,out Guid id){id=Guid.Empty;var p=token.Split('.'); if(p.Length!=2||!Guid.TryParseExact(p[0],"N",out id))return false; var expected=Sign(p[0]);return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected),Encoding.ASCII.GetBytes(p[1]));}
 public static void Map(WebApplication app){
 app.MapPost("/api/associacoes/{id:guid}/dependentes",(Guid id,DependenteInput d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Members.TryGetValue(id,out var m))return Results.NotFound(); var u=Store.User(c);
  if(u.Id!=m.UsuarioTitularId&&!Store.Scope(u,m.ClubeOrigemId,"Presidente"))return Forbidden();
  if(!m.Ativa)return Store.Bad("Associação inativa.");if(db.Categorias.TryGetValue(m.Categoria,out var categoria)&&categoria.LimiteDependentes is int limite&&m.Dependentes.Count>=limite)return Store.Bad("Limite de dependentes da categoria atingido.");if(!db.Users.TryGetValue(d.UsuarioId,out var dep)||!dep.Ativo||!dep.Papeis.Contains("Dependente"))return Store.Bad("Usuário dependente inválido.");
  if(db.Members.Values.Any(x=>x.Ativa&&(x.UsuarioTitularId==d.UsuarioId||x.Dependentes.Contains(d.UsuarioId))))return Store.Bad("Pessoa já possui associação ativa.");
  m.Dependentes.Add(d.UsuarioId);db.Log(c,"vincular","dependente",d.UsuarioId,m.ClubeOrigemId);return Results.Ok(m);
 }});
 app.MapDelete("/api/associacoes/{id:guid}/dependentes/{dep:guid}",(Guid id,Guid dep,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Members.TryGetValue(id,out var m))return Results.NotFound();if(Store.User(c).Id!=m.UsuarioTitularId&&!Store.Scope(Store.User(c),m.ClubeOrigemId,"Presidente"))return Forbidden();
  if(!m.Dependentes.Remove(dep))return Results.NotFound();db.Log(c,"desvincular","dependente",dep,m.ClubeOrigemId);return Results.NoContent();
 }});
 app.MapGet("/api/usuarios",(HttpContext c,Store db)=>{lock(db.Sync){var op=Store.User(c);if(!Store.Has(op,"AdministradorGeral","Presidente"))return Forbidden();return Results.Ok(db.Users.Values.Where(x=>Store.Admin(c)||Operacoes.PodeGerenciarPessoa(db,op,x)).Select(Store.PublicUser).ToArray());}});
 app.MapPost("/api/usuarios",(NovoUsuario d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!Store.Scope(Store.User(c),d.ClubeId,"Presidente"))return Forbidden();if(!Store.Admin(c)&&d.Papel=="AdministradorGeral")return Forbidden(); var valid=new[]{"AdministradorGeral","Presidente","Tesoureiro","Funcionario","Porteiro","Titular","Dependente"};
  if(string.IsNullOrWhiteSpace(d.Nome)||string.IsNullOrWhiteSpace(d.Login)||string.IsNullOrWhiteSpace(d.Senha)||d.Senha.Length<8||!valid.Contains(d.Papel)||(!db.Clubs.TryGetValue(d.ClubeId,out var clube)||!clube.Ativo))return Store.Bad("Nome, login, senha (mínimo 8), papel e clube válido são obrigatórios.");
  if(db.Users.Values.Any(x=>x.Login.Equals(d.Login,StringComparison.OrdinalIgnoreCase)))return Store.Bad("Login já existe.");
  var u=new Usuario{Nome=d.Nome,Login=d.Login,Papeis=new(){d.Papel},Clubes=new(){d.ClubeId}};u.Atribuicoes.Add(new Atribuicao(d.Papel,d.ClubeId));u.SenhaHash=db.Hasher.HashPassword(u,d.Senha);db.Users.Add(u.Id,u);db.Log(c,"criar","usuario",u.Id,d.ClubeId);return Results.Created($"/api/usuarios/{u.Id}",Store.PublicUser(u));
 }});
 app.MapPost("/api/espacos/{id:guid}/interdicoes",(Guid id,NovaInterdicao d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Spaces.TryGetValue(id,out var e))return Results.NotFound();if(!Store.Scope(Store.User(c),e.ClubeId,"Funcionario","Presidente"))return Forbidden();
  if(d.Fim<=d.Inicio||db.Bookings.Values.Any(x=>x.EspacoId==id&&x.Status=="Confirmada"&&x.Inicio<d.Fim&&d.Inicio<x.Fim))return Store.Bad("Período inválido ou reservado.");
  var it=new Interdicao(Guid.NewGuid(),id,d.Inicio,d.Fim,d.Motivo);db.Interdicoes.Add(it);db.Log(c,"criar","interdicao",it.Id,e.ClubeId);return Results.Ok(it);
 }});
 app.MapGet("/api/reservas/{id:guid}/convidados",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Bookings.TryGetValue(id,out var r))return Results.NotFound();var u=Store.User(c);var e=db.Spaces[r.EspacoId];
  if(!Own(db,r.AssociacaoId,u.Id)&&!Store.Scope(u,e.ClubeId,"Presidente","Funcionario","Porteiro"))return Forbidden();return Results.Ok(db.Convidados.GetValueOrDefault(id)??new());
 }});
 app.MapPut("/api/reservas/{id:guid}/convidados",(Guid id,List<Convidado> lista,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Bookings.TryGetValue(id,out var r))return Results.NotFound(); if(!db.Members.TryGetValue(r.AssociacaoId,out var m)||m.UsuarioTitularId!=Store.User(c).Id)return Forbidden();
  var e=db.Spaces[r.EspacoId];if(r.Status!="Confirmada"||DateTimeOffset.UtcNow>=r.Inicio.AddHours(-24))return Store.Bad("Prazo de convidados encerrado.");
  if(lista.Count>e.Capacidade||lista.Any(x=>string.IsNullOrWhiteSpace(x.Nome)||string.IsNullOrWhiteSpace(x.Identificacao)))return Store.Bad("Lista inválida ou acima da capacidade.");
  db.Convidados[id]=lista;db.Log(c,"alterar","convidados",id,e.ClubeId);return Results.Ok(lista);
 }});
 app.MapGet("/api/reservas/{id:guid}/transferencia-previa",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Bookings.TryGetValue(id,out var r))return Results.NotFound();if(!db.Members.TryGetValue(r.AssociacaoId,out var m)||m.UsuarioTitularId!=Store.User(c).Id)return Forbidden();
  if(r.Status!="Confirmada"||r.StatusTaxa!="EmAberto"||r.Inicio<=DateTimeOffset.UtcNow)return Store.Bad("Taxa não disponível para transferência.");
  var f=Destino(db,r);if(f is null)return Store.Bad("Cadastre antes uma mensalidade futura aberta elegível.");return Results.Ok(new {f.Id,f.Competencia,f.Vencimento,principal=r.Valor,acrescimo=decimal.Round(r.Valor*0.10m,2),total=decimal.Round(r.Valor*1.10m,2)});
 }});
 app.MapPost("/api/reservas/{id:guid}/transferir",(Guid id,DestinoInput d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Bookings.TryGetValue(id,out var r))return Results.NotFound();if(!db.Members.TryGetValue(r.AssociacaoId,out var m)||m.UsuarioTitularId!=Store.User(c).Id)return Forbidden();
  if(r.StatusTaxa=="Transferida")return Results.Ok(new {status="Transferida",itens=db.Itens.Where(x=>x.ReservaId==id).ToArray()});
  if(r.Status!="Confirmada"||r.StatusTaxa!="EmAberto"||r.Inicio<=DateTimeOffset.UtcNow)return Store.Bad("Taxa não disponível para transferência.");
  var f=Destino(db,r);if(f is null||f.Id!=d.FaturaId)return Store.Bad("Destino alterado ou não elegível; consulte a prévia novamente.");
  var extra=decimal.Round(r.Valor*0.1m,2);db.Itens.Add(new ItemTransferido(Guid.NewGuid(),f.Id,id,"Locação",r.Valor));db.Itens.Add(new ItemTransferido(Guid.NewGuid(),f.Id,id,"Acréscimo 10%",extra));f.Valor+=r.Valor+extra;r.StatusTaxa="Transferida";db.Log(c,"transferir","reserva",id,m.ClubeOrigemId);return Results.Ok(new {f.Id,f.Competencia,f.Vencimento,f.Valor,itens=db.Itens.Where(x=>x.ReservaId==id).ToArray()});
 }});
 app.MapGet("/api/faturas/{id:guid}/itens",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Invoices.TryGetValue(id,out var f))return Results.NotFound();var m=db.Members[f.AssociacaoId];var u=Store.User(c);if(m.UsuarioTitularId!=u.Id&&!Store.Scope(u,m.ClubeOrigemId,"Tesoureiro","Presidente"))return Forbidden();return Results.Ok(db.Itens.Where(x=>x.FaturaId==id).ToArray());
 }});
 app.MapGet("/api/creditos",(HttpContext c,Store db)=>{lock(db.Sync){var u=Store.User(c);return Results.Ok(db.Creditos.Where(x=>db.Members.TryGetValue(x.AssociacaoId,out var m)&&(m.UsuarioTitularId==u.Id||Store.Scope(u,m.ClubeOrigemId,"Tesoureiro"))).ToArray());}});
 app.MapPost("/api/turmas",(Turma t,HttpContext c,Store db)=>{lock(db.Sync){
  if(!Store.Scope(Store.User(c),t.ClubeId,"Presidente","Funcionario"))return Forbidden();if(!db.Clubs.TryGetValue(t.ClubeId,out var club)||!club.Ativo||string.IsNullOrWhiteSpace(t.Nome)||t.Fim<=t.Inicio||t.Capacidade<1)return Store.Bad("Dados da turma inválidos.");
  t.Id=Guid.NewGuid();db.Turmas.Add(t.Id,t);db.Log(c,"criar","turma",t.Id,t.ClubeId);return Results.Created($"/api/turmas/{t.Id}",t);
 }});
 app.MapGet("/api/turmas",(Store db)=>{lock(db.Sync)return Results.Ok(db.Turmas.Values.ToArray());});
 app.MapPost("/api/turmas/{id:guid}/inscricoes",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){
  var u=Store.User(c);if(!Active(db,u.Id,out _))return Forbidden();if(!db.Turmas.TryGetValue(id,out var t)||!t.Ativa||!db.Clubs[t.ClubeId].Ativo||t.Fim<=DateTimeOffset.UtcNow)return Store.Bad("Turma indisponível.");
  if(db.Inscricoes.Any(x=>x.TurmaId==id&&x.UsuarioId==u.Id&&x.Status!="Cancelada"))return Store.Bad("Inscrição duplicada.");
  if(db.Inscricoes.Any(x=>x.UsuarioId==u.Id&&x.Status=="Confirmada"&&db.Turmas.TryGetValue(x.TurmaId,out var outra)&&Complementos.Conflito(db,outra,t)))return Store.Bad("Conflito de horário com outra turma.");
  var inscr=new Inscricao(Guid.NewGuid(),id,u.Id,db.Inscricoes.Count(x=>x.TurmaId==id&&x.Status=="Confirmada")>=t.Capacidade?"Espera":"Confirmada",DateTimeOffset.UtcNow,null);db.Inscricoes.Add(inscr);db.Log(c,"inscrever","turma",id,t.ClubeId);return Results.Ok(inscr);
 }});
 app.MapGet("/api/turmas/{id:guid}/inscricoes",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){if(!db.Turmas.TryGetValue(id,out var t))return Results.NotFound();var u=Store.User(c);return Results.Ok(db.Inscricoes.Where(x=>x.TurmaId==id&&(x.UsuarioId==u.Id||Store.Scope(u,t.ClubeId,"Funcionario","Presidente"))).ToArray());}});
 app.MapPost("/api/inscricoes/{id:guid}/cancelar",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){var i=db.Inscricoes.FirstOrDefault(x=>x.Id==id);if(i is null)return Results.NotFound();var t=db.Turmas[i.TurmaId];if(i.UsuarioId!=Store.User(c).Id&&!Store.Scope(Store.User(c),t.ClubeId,"Funcionario","Presidente"))return Forbidden();i.Status="Cancelada";db.Log(c,"cancelar","inscricao",i.Id,t.ClubeId);return Results.Ok(i);}});
 app.MapPost("/api/turmas/{id:guid}/promover",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Turmas.TryGetValue(id,out var t))return Results.NotFound();if(!Store.Scope(Store.User(c),t.ClubeId,"Funcionario","Presidente"))return Forbidden();if(!t.Ativa||!db.Clubs[t.ClubeId].Ativo||t.Fim<=DateTimeOffset.UtcNow)return Store.Bad("Turma indisponível.");if(db.Inscricoes.Count(x=>x.TurmaId==id&&x.Status=="Confirmada")>=t.Capacidade)return Store.Bad("Turma sem vaga.");
  var i=db.Inscricoes.Where(x=>x.TurmaId==id&&x.Status=="Espera").OrderBy(x=>x.InscritaEm).FirstOrDefault(x=>Active(db,x.UsuarioId,out _)&&!db.Inscricoes.Any(a=>a.UsuarioId==x.UsuarioId&&a.Status=="Confirmada"&&db.Turmas.TryGetValue(a.TurmaId,out var other)&&Complementos.Conflito(db,other,t)));
  if(i is null)return Store.Bad("Não há pessoa elegível na espera.");i.Status="Confirmada";i.PromovidoPor=Store.User(c).Id;db.Log(c,"promover","inscricao",i.Id,t.ClubeId);return Results.Ok(i);
 }});
 app.MapPost("/api/carteirinha",(HttpContext c,Store db)=>{lock(db.Sync){
  var u=Store.User(c);if(!Active(db,u.Id,out var m))return Forbidden();var cr=new Credencial(Guid.NewGuid(),u.Id,m!.Id,DateTimeOffset.UtcNow,DateTimeOffset.UtcNow.AddHours(24),false);db.Credenciais[cr.Id]=cr;var raw=cr.Id.ToString("N");var token=raw+"."+Sign(raw);db.Log(c,"emitir","credencial",cr.Id,m.ClubeOrigemId);return Results.Ok(Operacoes.Carteirinha(db,u,m,cr,token));
 }});
 app.MapPost("/api/carteirinha/{id:guid}/revogar",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){if(!db.Credenciais.TryGetValue(id,out var cr))return Results.NotFound();var m=db.Members[cr.AssociacaoId];if(cr.UsuarioId!=Store.User(c).Id&&!Store.Scope(Store.User(c),m.ClubeOrigemId,"Presidente"))return Forbidden();cr.Revogada=true;db.Log(c,"revogar","credencial",id,m.ClubeOrigemId);return Results.NoContent();}});
 app.MapPost("/api/portaria/validar",(Validacao d,HttpContext c,Store db)=>{lock(db.Sync){
  var op=Store.User(c);if(!Store.Scope(op,d.ClubeId,"Porteiro"))return Forbidden();if(!db.Clubs.TryGetValue(d.ClubeId,out var club)||!club.Ativo)return Store.Bad("Clube indisponível.");
  string motivo="Credencial inválida";Guid? usuarioId=null;
  if(Valid(d.Credencial??"",out var id)&&db.Credenciais.TryGetValue(id,out var cr)){
   usuarioId=cr.UsuarioId;
   if(cr.Revogada)motivo="Credencial revogada";
   else if(cr.ValidaAte<=DateTimeOffset.UtcNow)motivo="Credencial expirada";
   else if(!Active(db,cr.UsuarioId,out var m)||m!.Id!=cr.AssociacaoId)motivo="Associação inativa ou bloqueada";
   else if(d.ReservaId is Guid rid){
    if(!db.Bookings.TryGetValue(rid,out var r)||r.AssociacaoId!=cr.AssociacaoId||db.Spaces[r.EspacoId].ClubeId!=d.ClubeId||r.Status!="Confirmada"||DateTimeOffset.UtcNow<r.Inicio||DateTimeOffset.UtcNow>=r.Fim)motivo="Reserva inválida para esta pessoa, horário ou clube";
    else if(r.StatusTaxa=="EmAberto")motivo="Taxa de locação em aberto";
    else motivo="Permitido";
   }else motivo="Permitido";
  }
  var log=new RegistroPortaria(Guid.NewGuid(),d.ClubeId,usuarioId,op.Id,DateTimeOffset.UtcNow,motivo=="Permitido",motivo);db.Acessos.Add(log);db.Log(c,"validar","acesso",log.Id,d.ClubeId);return Results.Ok(log);
 }});
 app.MapGet("/api/portaria/acessos",(HttpContext c,Store db)=>{lock(db.Sync){var u=Store.User(c);return Results.Ok(db.Acessos.Where(a=>a.UsuarioId==u.Id||Store.Scope(u,a.ClubeId,"Porteiro","Funcionario","Presidente")).ToArray());}});
 app.MapGet("/api/relatorios/resumo",(Guid clubeId,HttpContext c,Store db)=>{lock(db.Sync){
  if(!Store.Scope(Store.User(c),clubeId,"Presidente","Tesoureiro","Funcionario"))return Forbidden();
  return Results.Ok(new{clubeId,reservas=db.Bookings.Values.Count(r=>db.Spaces[r.EspacoId].ClubeId==clubeId&&r.Status=="Confirmada"),turmas=db.Turmas.Values.Count(x=>x.ClubeId==clubeId),inscricoes=db.Inscricoes.Count(x=>db.Turmas[x.TurmaId].ClubeId==clubeId&&x.Status=="Confirmada"),acessos=db.Acessos.Count(x=>x.ClubeId==clubeId&&x.Permitido)});
 }});
 }
 static Fatura? Destino(Store db,Reserva r){var m=db.Members[r.AssociacaoId];var zone=TimeZoneInfo.FindSystemTimeZoneById(db.Clubs[m.ClubeOrigemId].FusoHorario);var today=DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,zone).DateTime);var next=new DateOnly(today.Year,today.Month,1).AddMonths(1).ToString("yyyy-MM");return db.Invoices.Values.Where(x=>x.AssociacaoId==r.AssociacaoId&&x.Status=="EmAberto"&&string.CompareOrdinal(x.Competencia,next)>=0&&x.Vencimento>today).OrderBy(x=>x.Competencia).FirstOrDefault();}
 public static void CancelarTransferencia(Store db,Reserva r,decimal multa){if(r.StatusTaxa!="Transferida")return;var itens=db.Itens.Where(x=>x.ReservaId==r.Id).ToList();if(itens.Count==0)return;var f=db.Invoices[itens[0].FaturaId];var total=itens.Sum(x=>x.Valor);if(f.Status=="EmAberto"){f.Valor-=total-multa;db.Itens.RemoveAll(x=>x.ReservaId==r.Id);if(multa>0)db.Itens.Add(new ItemTransferido(Guid.NewGuid(),f.Id,r.Id,"Multa de cancelamento",multa));}else{db.Creditos.Add(new Credito(Guid.NewGuid(),r.AssociacaoId,r.Id,total-multa,"Crédito simulado por cancelamento"));}r.StatusTaxa="Cancelada";}
}
record NovoUsuario(string Nome,string Login,string Senha,string Papel,Guid ClubeId);
record DependenteInput(Guid UsuarioId);
record NovaInterdicao(DateTimeOffset Inicio,DateTimeOffset Fim,string Motivo);
record Interdicao(Guid Id,Guid EspacoId,DateTimeOffset Inicio,DateTimeOffset Fim,string Motivo);
record Convidado(string Nome,string Identificacao);
record DestinoInput(Guid FaturaId);
record ItemTransferido(Guid Id,Guid FaturaId,Guid ReservaId,string Descricao,decimal Valor);
record Credito(Guid Id,Guid AssociacaoId,Guid ReservaId,decimal Valor,string Descricao);
class Turma{public Guid Id{get;set;} public Guid ClubeId{get;set;} public string Nome{get;set;}=""; public DateTimeOffset Inicio{get;set;} public DateTimeOffset Fim{get;set;} public int Capacidade{get;set;} public string Descricao{get;set;}=""; public string Instrutor{get;set;}=""; public Guid? AtividadeId{get;set;} public bool Ativa{get;set;}=true;}
class Inscricao{public Guid Id{get;set;} public Guid TurmaId{get;set;} public Guid UsuarioId{get;set;} public string Status{get;set;} public DateTimeOffset InscritaEm{get;set;} public Guid? PromovidoPor{get;set;} public Inscricao(Guid id,Guid turmaId,Guid usuarioId,string status,DateTimeOffset inscritaEm,Guid? promovidoPor){Id=id;TurmaId=turmaId;UsuarioId=usuarioId;Status=status;InscritaEm=inscritaEm;PromovidoPor=promovidoPor;}}
class Credencial{public Credencial(){} public Guid Id{get;set;} public Guid UsuarioId{get;set;} public Guid AssociacaoId{get;set;} public DateTimeOffset EmitidaEm{get;set;} public DateTimeOffset ValidaAte{get;set;} public bool Revogada{get;set;} public Credencial(Guid id,Guid user,Guid assoc,DateTimeOffset emitida,DateTimeOffset valida,bool revogada){Id=id;UsuarioId=user;AssociacaoId=assoc;EmitidaEm=emitida;ValidaAte=valida;Revogada=revogada;}}
record Validacao(Guid ClubeId,string? Credencial,Guid? ReservaId);
record RegistroPortaria(Guid Id,Guid ClubeId,Guid? UsuarioId,Guid PorteiroId,DateTimeOffset DataHora,bool Permitido,string Motivo);
