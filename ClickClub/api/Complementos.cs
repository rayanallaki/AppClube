using System.Globalization;

static class Complementos {
 static IResult Proibido()=>Results.StatusCode(403);
 static readonly string[] Papeis={"AdministradorGeral","Presidente","Tesoureiro","Funcionario","Porteiro","Titular","Dependente"};
 public static bool CpfValido(string cpf){
  if(cpf.Length!=11||cpf.Any(c=>c<'0'||c>'9')||cpf.All(c=>c==cpf[0]))return false;
  for(var digito=9;digito<=10;digito++){
   var soma=0;
   for(var i=0;i<digito;i++)soma+=(cpf[i]-'0')*(digito+1-i);
   var resto=(soma*10)%11;
   if(resto==10)resto=0;
   if(cpf[digito]-'0'!=resto)return false;
  }
  return true;
 }
 public static bool Funciona(Store db,Guid espacoId,DateTimeOffset inicio,DateTimeOffset fim,string fuso){
  var periodos=db.Horarios.Where(x=>x.EspacoId==espacoId).ToArray();if(periodos.Length==0)return true;
  var zone=TimeZoneInfo.FindSystemTimeZoneById(fuso);var localInicio=TimeZoneInfo.ConvertTime(inicio,zone);var localFim=TimeZoneInfo.ConvertTime(fim,zone);
  var cursor=localInicio.Date;var ultimo=localFim.Date;
  while(cursor<=ultimo){
   var start=cursor==localInicio.Date?TimeOnly.FromDateTime(localInicio.DateTime):TimeOnly.MinValue;
   var end=cursor==localFim.Date?TimeOnly.FromDateTime(localFim.DateTime):TimeOnly.MaxValue;
   if(start<end&&!periodos.Any(h=>h.DiaSemana==(int)cursor.DayOfWeek&&h.Inicio<=start&&end<=h.Fim))return false;
   cursor=cursor.AddDays(1);
  }
  return true;
 }
 public static bool Conflito(Store db,Turma a,Turma b){
  var aa=db.Aulas.Where(x=>x.TurmaId==a.Id).ToArray();var bb=db.Aulas.Where(x=>x.TurmaId==b.Id).ToArray();
  if(aa.Length==0)aa=new[]{new Aula(a.Id,a.Inicio,a.Fim)};if(bb.Length==0)bb=new[]{new Aula(b.Id,b.Inicio,b.Fim)};
  return aa.Any(x=>bb.Any(y=>x.Inicio<y.Fim&&y.Inicio<x.Fim));
 }
 public static void Map(WebApplication app){
 app.MapGet("/api/adesoes/minhas",(HttpContext c,Store db)=>{lock(db.Sync){var id=Store.User(c).Id;return Results.Ok(db.Applications.Values.Where(x=>x.UsuarioId==id).Select(x=>new{x.Id,x.ClubeId,x.TipoVinculo,x.Status,x.SolicitadaEm,x.DecididaEm,x.Motivo,x.AssociacaoId}).ToArray());}});
 app.MapPut("/api/usuarios/{id:guid}/dados",(Guid id,DadosPessoa d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Users.TryGetValue(id,out var target))return Results.NotFound();var user=Store.User(c);
  var member=db.Members.Values.FirstOrDefault(m=>m.Ativa&&(m.UsuarioTitularId==id||m.Dependentes.Contains(id)));
  if(user.Id!=id && !Store.Admin(c) && (member is null||!Store.Scope(user,member.ClubeOrigemId,"Presidente")))return Proibido();
  if(string.IsNullOrWhiteSpace(d.Cpf))d.Cpf=null;if(d.DataNascimento>DateOnly.FromDateTime(DateTime.Today))return Store.Bad("Data de nascimento no futuro.");if(d.Uf is not null&&d.Uf.Length!=2)return Store.Bad("UF deve ter duas letras.");if(d.Cpf is not null){d.Cpf=new string(d.Cpf.Where(char.IsDigit).ToArray());if(!CpfValido(d.Cpf))return Store.Bad("CPF inválido (verifique os 11 dígitos e os dígitos verificadores).");if(db.Users.Values.Any(x=>x.Id!=id&&x.Dados.Cpf==d.Cpf))return Store.Bad("CPF já cadastrado.");}
  d.Interesses=(d.Interesses??new()).Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x=>x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();target.Dados=d;db.Log(c,"alterar","pessoa",id,member?.ClubeOrigemId);return Results.Ok(d);
 }});
 app.MapGet("/api/usuarios/{id:guid}/dados",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Users.TryGetValue(id,out var target))return Results.NotFound();var u=Store.User(c);var member=db.Members.Values.FirstOrDefault(m=>m.Ativa&&(m.UsuarioTitularId==id||m.Dependentes.Contains(id)));
  if(u.Id!=id&&!Store.Admin(c)&&(member is null||!Store.Scope(u,member.ClubeOrigemId,"Presidente")))return Proibido();return Results.Ok(new{target.Id,target.Nome,target.Dados});
 }});
 app.MapPut("/api/usuarios/{id:guid}/atribuicoes",(Guid id,List<Atribuicao> assignments,HttpContext c,Store db)=>{lock(db.Sync){
  if(!Store.Admin(c))return Proibido();if(!db.Users.TryGetValue(id,out var target))return Results.NotFound();
  if(assignments is null||assignments.Count==0||assignments.Any(a=>!Papeis.Contains(a.Papel)||!db.Clubs.ContainsKey(a.ClubeId)))return Store.Bad("Atribuições inválidas.");
  if(target.Papeis.Contains("AdministradorGeral")&&!assignments.Any(x=>x.Papel=="AdministradorGeral")&&!db.Users.Values.Any(x=>x.Id!=id&&x.Ativo&&x.Papeis.Contains("AdministradorGeral")))return Store.Bad("Mantenha pelo menos um administrador geral ativo.");target.Atribuicoes=assignments.Distinct().ToList();target.Papeis=assignments.Select(a=>a.Papel).Distinct().ToList();target.Clubes=assignments.Select(a=>a.ClubeId).Distinct().ToList();db.Log(c,"alterar","permissoes",id,null);return Results.Ok(Store.PublicUser(target));
 }});
 app.MapPut("/api/usuarios/{id:guid}/ativo",(Guid id,StatusAtivo d,HttpContext c,Store db)=>{lock(db.Sync){if(!db.Users.TryGetValue(id,out var target))return Results.NotFound();if(!Operacoes.PodeGerenciarPessoa(db,Store.User(c),target))return Proibido();if(target.Papeis.Contains("AdministradorGeral")&&!Store.Admin(c))return Proibido();if(!d.Ativo&&target.Papeis.Contains("AdministradorGeral")&&!db.Users.Values.Any(x=>x.Id!=id&&x.Ativo&&x.Papeis.Contains("AdministradorGeral")))return Store.Bad("Mantenha pelo menos um administrador geral ativo.");if(target.Id==Store.User(c).Id&&!d.Ativo)return Store.Bad("Não desative sua própria conta.");target.Ativo=d.Ativo;if(!d.Ativo){foreach(var token in db.Sessions.Where(x=>x.Value.UserId==id).Select(x=>x.Key).ToArray())db.Sessions.Remove(token);foreach(var cr in db.Credenciais.Values.Where(x=>x.UsuarioId==id))cr.Revogada=true;}db.Log(c,"alterar","usuario",id,null);return Results.Ok(Store.PublicUser(target));}});
 app.MapPut("/api/clubes/{id:guid}",(Guid id,Clube d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!Store.Scope(Store.User(c),id,"Presidente"))return Proibido();if(!db.Clubs.ContainsKey(id))return Results.NotFound();if(string.IsNullOrWhiteSpace(d.Nome)||string.IsNullOrWhiteSpace(d.Sigla)||db.Clubs.Values.Any(x=>x.Id!=id&&x.Sigla.Equals(d.Sigla,StringComparison.OrdinalIgnoreCase)))return Store.Bad("Nome ou sigla inválidos ou duplicados.");
  try{TimeZoneInfo.FindSystemTimeZoneById(d.FusoHorario);}catch{return Store.Bad("Fuso horário inválido.");}d.Id=id;db.Clubs[id]=d;db.Log(c,"alterar","clube",id,id);return Results.Ok(d);
 }});
 app.MapPut("/api/espacos/{id:guid}",(Guid id,Espaco d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Spaces.TryGetValue(id,out var old))return Results.NotFound();if(!Store.Scope(Store.User(c),old.ClubeId,"Presidente","Funcionario"))return Proibido();if(d.ClubeId!=old.ClubeId||d.Capacidade<1||d.Taxa<0||string.IsNullOrWhiteSpace(d.Nome)||string.IsNullOrWhiteSpace(d.Tipo)||d.Tipo.Length>60)return Store.Bad("Dados inválidos.");d.Id=id;db.Spaces[id]=d;db.Log(c,"alterar","espaco",id,d.ClubeId);return Results.Ok(d);
 }});
 app.MapPut("/api/espacos/{id:guid}/horarios",(Guid id,List<HorarioInput> d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Spaces.TryGetValue(id,out var e))return Results.NotFound();if(!Store.Scope(Store.User(c),e.ClubeId,"Presidente","Funcionario"))return Proibido();if(d is null||d.GroupBy(h=>new{h.DiaSemana,h.Inicio}).Any(g=>g.Count()>1)||d.Any(h=>h.DiaSemana<0||h.DiaSemana>6||h.Inicio>=h.Fim))return Store.Bad("Horários inválidos; não podem atravessar meia-noite.");
  db.Horarios.RemoveAll(h=>h.EspacoId==id);db.Horarios.AddRange(d.Select(h=>new HorarioFuncionamento(id,h.DiaSemana,h.Inicio,h.Fim)));db.Log(c,"alterar","horarios",id,e.ClubeId);return Results.Ok(db.Horarios.Where(x=>x.EspacoId==id).ToArray());
 }});
 app.MapGet("/api/espacos/{id:guid}/horarios",(Guid id,Store db)=>{lock(db.Sync)return Results.Ok(db.Horarios.Where(x=>x.EspacoId==id).ToArray());});
 app.MapPost("/api/associacoes/{id:guid}/autorizacoes",(Guid id,AutorizacaoInput d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Members.TryGetValue(id,out var m))return Results.NotFound();if(m.UsuarioTitularId!=Store.User(c).Id)return Proibido();if(!Extras.Active(db,m.UsuarioTitularId,out _)||!Extras.Active(db,d.DependenteId,out _)||!m.Dependentes.Contains(d.DependenteId)||!db.Spaces.TryGetValue(d.EspacoId,out var espaco)||!espaco.Ativo||!db.Clubs[espaco.ClubeId].Ativo)return Store.Bad("Dependente ou espaço inválido.");
  var a=new AutorizacaoDependente{Id=Guid.NewGuid(),AssociacaoId=id,DependenteId=d.DependenteId,EspacoId=d.EspacoId};db.Autorizacoes[a.Id]=a;db.Log(c,"autorizar","dependente",a.Id,m.ClubeOrigemId);return Results.Ok(a);
 }});
 app.MapGet("/api/associacoes/{id:guid}/autorizacoes",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){if(!db.Members.TryGetValue(id,out var m))return Results.NotFound();if(m.UsuarioTitularId!=Store.User(c).Id)return Proibido();return Results.Ok(db.Autorizacoes.Values.Where(a=>a.AssociacaoId==id).ToArray());}});
 app.MapPut("/api/associacoes/{id:guid}/situacao",(Guid id,StatusAtivo d,HttpContext c,Store db)=>{lock(db.Sync){if(!db.Members.TryGetValue(id,out var m))return Results.NotFound();if(!Store.Scope(Store.User(c),m.ClubeOrigemId,"Presidente"))return Proibido();if(d.Ativo&&db.Members.Values.Any(x=>x.Id!=id&&x.Ativa&&(x.UsuarioTitularId==m.UsuarioTitularId||x.Dependentes.Contains(m.UsuarioTitularId)||x.Dependentes.Any(m.Dependentes.Contains)||m.Dependentes.Contains(x.UsuarioTitularId))))return Store.Bad("Há pessoa vinculada a outra associação ativa.");m.Ativa=d.Ativo;db.Log(c,"alterar","associacao",id,m.ClubeOrigemId);return Results.Ok(m);}});
 app.MapPost("/api/creditos/{id:guid}/aplicar",(Guid id,DestinoInput d,HttpContext c,Store db)=>{lock(db.Sync){
  var cred=db.Creditos.FirstOrDefault(x=>x.Id==id);if(cred is null)return Results.NotFound();var m=db.Members[cred.AssociacaoId];if(!Store.Scope(Store.User(c),m.ClubeOrigemId,"Tesoureiro"))return Proibido();
  if(db.CreditosAplicados.Contains(id))return Store.Bad("Crédito já aplicado.");if(!db.Invoices.TryGetValue(d.FaturaId,out var f)||f.AssociacaoId!=cred.AssociacaoId||f.Status!="EmAberto"||f.Valor<cred.Valor)return Store.Bad("Mensalidade aberta incompatível ou inferior ao crédito.");
  f.Valor-=cred.Valor;db.Itens.Add(new ItemTransferido(Guid.NewGuid(),f.Id,cred.ReservaId,"Crédito simulado",-cred.Valor));db.CreditosAplicados.Add(id);db.Log(c,"aplicar","credito",id,m.ClubeOrigemId);return Results.Ok(f);
 }});
 app.MapPut("/api/categorias/{nome}",(string nome,Categoria d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!Store.Admin(c))return Proibido();if(string.IsNullOrWhiteSpace(nome)||string.IsNullOrWhiteSpace(d.name)||!nome.Equals(d.name,StringComparison.OrdinalIgnoreCase)||d.ValorMensal<0||d.LimiteDependentes<0)return Store.Bad("Dados de categoria inválidos.");if(db.Categorias.TryGetValue(nome,out var existente))d.name=existente.name;db.Categorias[nome]=d;db.Log(c,"alterar","categoria",Guid.Empty,null);return Results.Ok(d);
 }});
 app.MapPost("/api/atividades",(Atividade d,HttpContext c,Store db)=>{lock(db.Sync){if(!Store.Admin(c))return Proibido();if(string.IsNullOrWhiteSpace(d.Nome))return Store.Bad("Nome obrigatório.");d.Id=Guid.NewGuid();db.Atividades.Add(d);db.Log(c,"criar","atividade",d.Id,null);return Results.Ok(d);}});
 app.MapGet("/api/atividades",(Store db)=>{lock(db.Sync)return Results.Ok(db.Atividades.ToArray());});
 app.MapPut("/api/turmas/{id:guid}/aulas",(Guid id,List<AulaInput> aulas,HttpContext c,Store db)=>{lock(db.Sync){
  if(!db.Turmas.TryGetValue(id,out var t))return Results.NotFound();if(!Store.Scope(Store.User(c),t.ClubeId,"Presidente","Funcionario"))return Proibido();
  if(aulas is null||aulas.Count==0||aulas.GroupBy(x=>new{x.Inicio,x.Fim}).Any(g=>g.Count()>1)||aulas.Any(x=>x.Fim<=x.Inicio||x.Inicio<t.Inicio||x.Fim>t.Fim))return Store.Bad("Horários fora do período da turma ou inválidos.");
  var atual=db.Aulas.Where(x=>x.TurmaId==id).ToList();db.Aulas.RemoveAll(x=>x.TurmaId==id);db.Aulas.AddRange(aulas.Select(x=>new Aula(id,x.Inicio,x.Fim)));
  bool conflito=db.Inscricoes.Where(x=>x.TurmaId==id&&x.Status=="Confirmada").Any(x=>db.Inscricoes.Any(y=>y.UsuarioId==x.UsuarioId&&y.TurmaId!=id&&y.Status=="Confirmada"&&db.Turmas.TryGetValue(y.TurmaId,out var outra)&&Conflito(db,t,outra)));
  if(conflito){db.Aulas.RemoveAll(x=>x.TurmaId==id);db.Aulas.AddRange(atual);return Store.Bad("A mudança criaria conflito para inscritos.");}
  db.Log(c,"alterar","aulas",id,t.ClubeId);return Results.Ok(db.Aulas.Where(x=>x.TurmaId==id).ToArray());
 }});
 app.MapGet("/api/turmas/{id:guid}/aulas",(Guid id,Store db)=>{lock(db.Sync){if(!db.Turmas.ContainsKey(id))return Results.NotFound();return Results.Ok(db.Aulas.Where(x=>x.TurmaId==id).ToArray());}});
 app.MapPut("/api/eventos/{id:guid}",(Guid id,Evento d,HttpContext c,Store db)=>{lock(db.Sync){var e=db.Events.FirstOrDefault(x=>x.Id==id);if(e is null)return Results.NotFound();if(!Store.Scope(Store.User(c),e.ClubeId,"Presidente","Funcionario"))return Proibido();if(d.ClubeId!=e.ClubeId||string.IsNullOrWhiteSpace(d.Nome)||d.Fim<=d.Inicio||d.PublicoAlvo is not ("SomenteAssociados" or "PublicoGeral")||d.Capacidade<0)return Store.Bad("Dados inválidos.");e.Nome=d.Nome;e.Descricao=d.Descricao;e.Inicio=d.Inicio;e.Fim=d.Fim;e.PublicoAlvo=d.PublicoAlvo;e.Capacidade=d.Capacidade;e.Status=d.Status;db.Log(c,"alterar","evento",id,e.ClubeId);return Results.Ok(e);}});
 app.MapPost("/api/eventos/{id:guid}/inscricoes",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){
  var e=db.Events.FirstOrDefault(x=>x.Id==id);if(e is null)return Results.NotFound();if(e.Status!="Ativo"||e.Fim<=DateTimeOffset.UtcNow)return Store.Bad("Evento indisponível.");var u=Store.User(c);
  if(!db.Clubs[e.ClubeId].Ativo)return Store.Bad("Clube inativo.");if(e.PublicoAlvo=="SomenteAssociados"&&!Extras.Active(db,u.Id,out _))return Proibido();
  if(db.InscricoesEventos.Any(x=>x.EventoId==id&&x.UsuarioId==u.Id&&x.Status=="Confirmada"))return Store.Bad("Inscrição duplicada.");if(e.Capacidade>0&&db.InscricoesEventos.Count(x=>x.EventoId==id&&x.Status=="Confirmada")+db.Visitas.Count(x=>x.EventoId==id)>=e.Capacidade)return Store.Bad("Evento lotado.");var i=new InscricaoEvento(Guid.NewGuid(),id,u.Id,"Confirmada");db.InscricoesEventos.Add(i);db.Log(c,"inscrever","evento",id,e.ClubeId);return Results.Ok(i);
 }});
 app.MapGet("/api/eventos/{id:guid}/inscricoes",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){var e=db.Events.FirstOrDefault(x=>x.Id==id);if(e is null)return Results.NotFound();var u=Store.User(c);return Results.Ok(db.InscricoesEventos.Where(x=>x.EventoId==id&&(x.UsuarioId==u.Id||Store.Scope(u,e.ClubeId,"Presidente","Funcionario"))).ToArray());}});
 app.MapPost("/api/eventos/{id:guid}/visitantes",(Guid id,VisitanteInput d,HttpContext c,Store db)=>{lock(db.Sync){var e=db.Events.FirstOrDefault(x=>x.Id==id);if(e is null)return Results.NotFound();if(!Store.Scope(Store.User(c),e.ClubeId,"Presidente","Funcionario"))return Proibido();if(e.Status!="Ativo"||e.Fim<=DateTimeOffset.UtcNow||!db.Clubs[e.ClubeId].Ativo)return Store.Bad("Evento indisponível.");if(e.PublicoAlvo!="PublicoGeral"||string.IsNullOrWhiteSpace(d.Nome)||string.IsNullOrWhiteSpace(d.Identificacao))return Store.Bad("Evento não é público ou dados incompletos.");if(e.Capacidade>0&&db.InscricoesEventos.Count(x=>x.EventoId==id&&x.Status=="Confirmada")+db.Visitas.Count(x=>x.EventoId==id)>=e.Capacidade)return Store.Bad("Evento lotado.");var v=new EntradaVisitante(Guid.NewGuid(),id,d.Nome,d.Identificacao);db.Visitas.Add(v);db.Log(c,"cadastrar","visitante",v.Id,e.ClubeId);return Results.Ok(v);}});
 app.MapPost("/api/portaria/validar-visitante",(ValidarVisitante d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!Store.Scope(Store.User(c),d.ClubeId,"Porteiro"))return Proibido();if(!db.Clubs.TryGetValue(d.ClubeId,out var club)||!club.Ativo)return Store.Bad("Clube inválido.");var v=db.Visitas.FirstOrDefault(x=>x.Id==d.VisitanteId);var e=v is null?null:db.Events.FirstOrDefault(x=>x.Id==v.EventoId);var agora=DateTimeOffset.UtcNow;
  bool ok=v is not null&&e is not null&&e.ClubeId==d.ClubeId&&e.Status=="Ativo"&&e.PublicoAlvo=="PublicoGeral"&&agora>=e.Inicio&&agora<e.Fim&&string.Equals(v.Identificacao,d.Identificacao,StringComparison.OrdinalIgnoreCase);
  var access=new RegistroPortaria(Guid.NewGuid(),d.ClubeId,null,Store.User(c).Id,agora,ok,ok?"Visitante autorizado":"Visitante não autorizado");db.Acessos.Add(access);db.Log(c,"validar","visitante",access.Id,d.ClubeId);return Results.Ok(access);
 }});
 app.MapPost("/api/portaria/validar-convidado",(ValidarConvidado d,HttpContext c,Store db)=>{lock(db.Sync){
  if(!Store.Scope(Store.User(c),d.ClubeId,"Porteiro"))return Proibido();var agora=DateTimeOffset.UtcNow;bool ok=false;
  if(db.Bookings.TryGetValue(d.ReservaId,out var r)&&db.Spaces.TryGetValue(r.EspacoId,out var e)&&e.ClubeId==d.ClubeId&&db.Clubs.TryGetValue(d.ClubeId,out var club)&&club.Ativo&&r.Status=="Confirmada"&&agora>=r.Inicio&&agora<r.Fim&&r.StatusTaxa!="EmAberto"&&db.Members.TryGetValue(r.AssociacaoId,out var m)&&Extras.Active(db,m.UsuarioTitularId,out _))ok=db.Convidados.GetValueOrDefault(r.Id)?.Any(x=>x.Identificacao.Equals(d.Identificacao,StringComparison.OrdinalIgnoreCase))==true;
  var access=new RegistroPortaria(Guid.NewGuid(),d.ClubeId,null,Store.User(c).Id,agora,ok,ok?"Convidado autorizado":"Convidado não autorizado");db.Acessos.Add(access);db.Log(c,"validar","convidado",access.Id,d.ClubeId);return Results.Ok(access);
 }});
 app.MapGet("/api/relatorios/financeiro",(Guid clubeId,HttpContext c,Store db)=>{lock(db.Sync){
  if(!Store.Scope(Store.User(c),clubeId,"Tesoureiro","Presidente"))return Proibido();if(!db.Clubs.TryGetValue(clubeId,out var club))return Results.NotFound();var zone=TimeZoneInfo.FindSystemTimeZoneById(club.FusoHorario);var hoje=DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,zone).DateTime);
  var f=db.Invoices.Values.Where(x=>db.Members[x.AssociacaoId].ClubeOrigemId==clubeId).ToArray();return Results.Ok(new {clubeId,pagas=f.Count(x=>x.Status=="QuitadaSimulada"),emAberto=f.Count(x=>x.Status=="EmAberto"&&x.Vencimento>=hoje),vencidas=f.Count(x=>x.Status=="EmAberto"&&x.Vencimento<hoje),totalPago=f.Where(x=>x.Status=="QuitadaSimulada").Sum(x=>x.Valor),totalAberto=f.Where(x=>x.Status=="EmAberto").Sum(x=>x.Valor)});
 }});
 app.MapPut("/api/chamados/{id:guid}/status",(Guid id,StatusChamado d,HttpContext c,Store db)=>{lock(db.Sync){var t=db.Tickets.FirstOrDefault(x=>x.Id==id);if(t is null)return Results.NotFound();if(!Store.Admin(c)&&(!t.ClubeId.HasValue||!Store.Scope(Store.User(c),t.ClubeId.Value,"Presidente","Funcionario")))return Proibido();if(d.Status is not ("Aberto" or "EmAndamento" or "Concluido"))return Store.Bad("Status inválido.");t.Status=d.Status;db.Log(c,"alterar","chamado",id,t.ClubeId);return Results.Ok(t);}});
 }
}
record Atribuicao(string Papel,Guid ClubeId);
class DadosPessoa{public string? Cpf{get;set;} public DateOnly? DataNascimento{get;set;} public string? Endereco{get;set;} public string? Bairro{get;set;} public string? Cidade{get;set;} public string? Uf{get;set;} public string? Sexo{get;set;} public string? EstadoCivil{get;set;} public string? Telefone{get;set;} public string? Email{get;set;} public List<string> Interesses{get;set;}=new();}
record StatusAtivo(bool Ativo);
record HorarioInput(int DiaSemana,TimeOnly Inicio,TimeOnly Fim);
record HorarioFuncionamento(Guid EspacoId,int DiaSemana,TimeOnly Inicio,TimeOnly Fim);
record AutorizacaoInput(Guid DependenteId,Guid EspacoId);
class AutorizacaoDependente{public Guid Id{get;set;} public Guid AssociacaoId{get;set;} public Guid DependenteId{get;set;} public Guid EspacoId{get;set;} public bool Usada{get;set;}}
class Atividade{public Guid Id{get;set;} public string Nome{get;set;}=""; public string Descricao{get;set;}=""; public string Tipo{get;set;}="";}
record InscricaoEvento(Guid Id,Guid EventoId,Guid UsuarioId,string Status);
record EntradaVisitante(Guid Id,Guid EventoId,string Nome,string Identificacao);
record VisitanteInput(string Nome,string Identificacao);
record ValidarVisitante(Guid ClubeId,Guid VisitanteId,string Identificacao);
record ValidarConvidado(Guid ClubeId,Guid ReservaId,string Identificacao);
record StatusChamado(string Status);

class Categoria{public string name{get;set;}=""; public bool Ativa{get;set;}=true; public decimal? ValorMensal{get;set;} public int? LimiteDependentes{get;set;} public string? Beneficios{get;set;}}

record Aula(Guid TurmaId,DateTimeOffset Inicio,DateTimeOffset Fim);
record AulaInput(DateTimeOffset Inicio,DateTimeOffset Fim);
