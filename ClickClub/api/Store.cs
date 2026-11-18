using Microsoft.AspNetCore.Identity;

partial class Store {
 public Dictionary<Guid,Turma> Turmas {get;}=new(); public List<Inscricao> Inscricoes {get;}=new(); public Dictionary<Guid,Credencial> Credenciais {get;}=new(); public List<RegistroPortaria> Acessos {get;}=new(); public List<ItemTransferido> Itens {get;}=new(); public List<Credito> Creditos {get;}=new(); public Dictionary<Guid,List<Convidado>> Convidados {get;}=new(); public List<Interdicao> Interdicoes {get;}=new();
 public Dictionary<Guid,AutorizacaoDependente> Autorizacoes {get;}=new(); public List<HorarioFuncionamento> Horarios {get;}=new(); public Dictionary<string,Categoria> Categorias {get;}=new(StringComparer.OrdinalIgnoreCase); public HashSet<Guid> CreditosAplicados {get;}=new(); public List<EntradaVisitante> Visitas {get;}=new(); public List<InscricaoEvento> InscricoesEventos {get;}=new(); public List<Atividade> Atividades {get;}=new(); public List<Aula> Aulas {get;}=new();
 public List<Evento> Events {get;}=new(); public List<Chamado> Tickets {get;}=new();
 public object Sync {get;}=new(); public PasswordHasher<Usuario> Hasher {get;}=new();
 public Dictionary<Guid,Usuario> Users {get;}=new(); public Dictionary<string,Session> Sessions {get;}=new(); public Dictionary<Guid,Clube> Clubs {get;}=new(); public Dictionary<Guid,Adesao> Applications {get;}=new(); public Dictionary<Guid,Associacao> Members {get;}=new(); public Dictionary<Guid,Espaco> Spaces {get;}=new(); public Dictionary<Guid,Reserva> Bookings {get;}=new(); public Dictionary<Guid,Fatura> Invoices {get;}=new(); public List<Aviso> Notices {get;}=new(); public List<RegistroAuditoria> Audit {get;}=new();
 public void SeedDemo(){
  foreach(var name in new[]{"Familiar","Individual","Atleta"}) Categorias[name]=new Categoria{name=name,Ativa=true,ValorMensal=name=="Familiar"?150:name=="Individual"?95:120,LimiteDependentes=name=="Familiar"?5:0,Beneficios=name=="Familiar"?"Acesso familiar, esportes e convivência":"Acesso às atividades e espaços"};
  var club=new Clube{Id=Guid.Parse("11111111-1111-1111-1111-111111111111"),Nome="Clube Foz (demonstração)",Sigla="FOZ",Municipio="Foz do Iguaçu",Uf="PR"}; Clubs[club.Id]=club;
  var admin=Seed("Administrador", "admin", "AdministradorGeral",club.Id); var titular=Seed("Titular de teste", "titular", "Titular",club.Id); Seed("Presidente de teste", "presidente", "Presidente",club.Id); Seed("Tesoureiro de teste", "tesoureiro", "Tesoureiro",club.Id); Seed("Funcionário de teste", "funcionario", "Funcionario",club.Id); Seed("Porteiro de teste", "porteiro", "Porteiro",club.Id); Seed("Dependente de teste", "dependente", "Dependente",club.Id);
  var member=new Associacao{Id=Guid.Parse("22222222-2222-2222-2222-222222222222"),UsuarioTitularId=titular.Id,ClubeOrigemId=club.Id,Matricula="DEMO-001",Categoria="Familiar",TipoVinculo="Associado/Fundador"}; Members[member.Id]=member;
  var space=new Espaco{Id=Guid.Parse("33333333-3333-3333-3333-333333333333"),ClubeId=club.Id,Nome="Salão demonstrativo",Tipo="Salao",Capacidade=50,Taxa=100}; Spaces[space.Id]=space;
  SeedApresentacao(club);
 }
 Usuario Seed(string nome,string login,string role,Guid clube){ var u=new Usuario{Nome=nome,Login=login,Papeis=new(){role},Clubes=new(){clube}}; u.Atribuicoes.Add(new Atribuicao(role,clube)); u.SenhaHash=Hasher.HashPassword(u,"Teste@123"); Users[u.Id]=u; return u; }
 public static Usuario User(HttpContext c)=>(Usuario)c.Items["user"]!; public static bool Admin(HttpContext c)=>User(c).Papeis.Contains("AdministradorGeral"); public static bool Has(Usuario u,params string[] roles)=>u.Papeis.Any(roles.Contains);
 public static bool Scope(Usuario u,Guid club,params string[] roles)=>u.Papeis.Contains("AdministradorGeral")||u.Atribuicoes.Any(a=>a.ClubeId==club&&roles.Contains(a.Papel));
 public static object PublicUser(Usuario u)=>new {u.Id,u.Nome,u.Login,u.Papeis,u.Clubes,u.Ativo,u.Atribuicoes}; public static IResult Bad(string message)=>Results.BadRequest(new{erro=message});
 public bool Blocked(Guid member){ var m=Members[member]; var zone=TimeZoneInfo.FindSystemTimeZoneById(Clubs[m.ClubeOrigemId].FusoHorario); var today=DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,zone).DateTime); return Invoices.Values.Any(x=>x.AssociacaoId==member&&x.Status=="EmAberto"&&today.DayNumber-x.Vencimento.DayNumber>10); }
 public void Log(HttpContext c,string action,string entity,Guid id,Guid? club)=>Audit.Add(new RegistroAuditoria(Guid.NewGuid(),User(c).Id,action,entity,id,club,DateTimeOffset.UtcNow));
}
