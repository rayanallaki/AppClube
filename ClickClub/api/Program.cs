using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddClickClubMySql(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => {
    o.SwaggerDoc("v1", new() { Title="ClickClub API", Version="v1", Description="Gestão de clubes — operações financeiras simuladas." });
    o.AddSecurityDefinition("Bearer", new() { Type=Microsoft.OpenApi.Models.SecuritySchemeType.Http, Scheme="bearer", Description="Token retornado por /api/auth/login" });
    o.AddSecurityRequirement(new() { [new() { Reference=new() { Type=Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id="Bearer" } }]=Array.Empty<string>() });
});
builder.Services.AddRateLimiter(o => {
    o.RejectionStatusCode=429;
    o.AddPolicy("login", ctx => RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString()??"local", _ => new() { PermitLimit=30, Window=TimeSpan.FromMinutes(1), QueueLimit=0 }));
    o.AddPolicy("adesao", ctx => RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString()??"local", _ => new() { PermitLimit=10, Window=TimeSpan.FromMinutes(1), QueueLimit=0 }));
});
var app = builder.Build();
await app.Services.PrepararBancoAsync();
if(app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseRateLimiter();
app.UseMiddleware<PersistenciaMiddleware>();
app.Use(async (ctx, next) => {
    if (!ctx.Request.Path.StartsWithSegments("/api") || ctx.Request.Path.StartsWithSegments("/api/publico") || ctx.Request.Path == "/api/saude" || ctx.Request.Path == "/api/auth/login" || (ctx.Request.Path == "/api/adesoes" && ctx.Request.Method == "POST")) { await next(); return; }
    var db = ctx.RequestServices.GetRequiredService<Store>();
    var auth = ctx.Request.Headers.Authorization.ToString();
    var token = auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? auth[7..].Trim() : "";
    lock(db.Sync) {
        if (db.Sessions.TryGetValue(token, out var session) && session.Expires > DateTimeOffset.UtcNow && db.Users.TryGetValue(session.UserId, out var user) && user.Ativo) ctx.Items["user"] = user;
    }
    if (ctx.Items["user"] is null) { ctx.Response.StatusCode = 401; await ctx.Response.WriteAsJsonAsync(new { erro = "Sessão inválida ou expirada." }); return; }
    await next();
});
app.MapGet("/", () => Results.Ok(new { projeto="ClickClub", modo="demonstração com MySQL", instrucoes="Consulte README.md e testes/ClickClub.http" }));
app.MapGet("/api/saude", async (ClickClubDbContext db) =>
    await db.Database.CanConnectAsync()
        ? Results.Ok(new { status="ok", banco="MySQL", persistencia="permanente" })
        : Results.Json(new { status="indisponivel", banco="MySQL" }, statusCode:503));
app.MapPost("/api/auth/login", (LoginInput dto, Store db) => { lock(db.Sync) {
    if(string.IsNullOrWhiteSpace(dto.Login)||string.IsNullOrWhiteSpace(dto.Senha))return Store.Bad("Informe login e senha.");
    var user = db.Users.Values.FirstOrDefault(x => x.Login.Equals(dto.Login, StringComparison.OrdinalIgnoreCase));
    if(user is null || !user.Ativo || db.Hasher.VerifyHashedPassword(user, user.SenhaHash, dto.Senha) == PasswordVerificationResult.Failed) return Results.Unauthorized();
    var token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32)); var expiry=DateTimeOffset.UtcNow.AddHours(8);
    db.Sessions[token]=new Session(user.Id, expiry); return Results.Ok(new { token, expiraEm=expiry, usuario=Store.PublicUser(user) });
}}).RequireRateLimiting("login");
app.MapPost("/api/auth/sair", (HttpContext c, Store db) => { var h=c.Request.Headers.Authorization.ToString(); lock(db.Sync) db.Sessions.Remove(h[7..].Trim()); return Results.NoContent(); });
app.MapGet("/api/auth/eu", (HttpContext c) => Results.Ok(Store.PublicUser(Store.User(c))));
app.MapGet("/api/clubes", (Store db) => { lock(db.Sync) return Results.Ok(db.Clubs.Values.ToArray()); });
app.MapPost("/api/clubes", (Clube d, HttpContext c, Store db) => { lock(db.Sync) {
    if(!Store.Admin(c)) return Results.StatusCode(403);
    if(string.IsNullOrWhiteSpace(d.Nome)||string.IsNullOrWhiteSpace(d.Sigla)||string.IsNullOrWhiteSpace(d.FusoHorario)) return Store.Bad("Nome, sigla e fuso horário são obrigatórios.");
    try { TimeZoneInfo.FindSystemTimeZoneById(d.FusoHorario); } catch { return Store.Bad("Fuso horário inválido. Use um identificador reconhecido, como America/Sao_Paulo."); }
    if(db.Clubs.Values.Any(x=>x.Sigla.Equals(d.Sigla,StringComparison.OrdinalIgnoreCase))) return Store.Bad("Sigla já cadastrada.");
    d.Id=Guid.NewGuid(); db.Clubs[d.Id]=d; db.Log(c,"criar","clube",d.Id,d.Id); return Results.Created($"/api/clubes/{d.Id}",d);
}});
app.MapGet("/api/categorias", (Store db) => {lock(db.Sync)return Results.Ok(db.Categorias.Values.ToArray());});
app.MapGet("/api/tipos-vinculo", () => Results.Ok(new[]{new {nome="Associado/Fundador",direitoVoto=true},new {nome="Convidado",direitoVoto=false},new {nome="Simpatizante",direitoVoto=false}}));
app.MapPost("/api/adesoes", (AdesaoInput d, Store db) => { lock(db.Sync) {
    if(!db.Clubs.TryGetValue(d.ClubeId,out var club)||!club.Ativo) return Store.Bad("Clube inexistente ou inativo.");
    if(d.TipoVinculo is not ("Convidado" or "Simpatizante")) return Store.Bad("Autoinscrição disponível somente para Convidado ou Simpatizante.");
    if(d.Login?.Length>100||d.Nome?.Length>250||d.Email?.Length>250)return Store.Bad("Campo acima do tamanho permitido.");if(string.IsNullOrWhiteSpace(d.Nome)||string.IsNullOrWhiteSpace(d.Email)) return Store.Bad("Nome e e-mail são obrigatórios.");
    Usuario? candidato=null;
    if(!string.IsNullOrWhiteSpace(d.Login)||!string.IsNullOrWhiteSpace(d.Senha)){
      if(string.IsNullOrWhiteSpace(d.Login)||string.IsNullOrWhiteSpace(d.Senha)||d.Senha.Length<8||db.Users.Values.Any(x=>x.Login.Equals(d.Login,StringComparison.OrdinalIgnoreCase)))return Store.Bad("Login indisponível ou senha inferior a 8 caracteres.");
      candidato=new Usuario{Nome=d.Nome,Login=d.Login,Papeis=new(){"Candidato"},Clubes=new(){d.ClubeId},Dados=new DadosPessoa{Email=d.Email}};candidato.SenhaHash=db.Hasher.HashPassword(candidato,d.Senha);db.Users[candidato.Id]=candidato;
    }
    var pedido=new Adesao{Id=Guid.NewGuid(),ClubeId=d.ClubeId,Nome=d.Nome,Email=d.Email,TipoVinculo=d.TipoVinculo,UsuarioId=candidato?.Id,SolicitadaEm=DateTimeOffset.UtcNow};db.Applications[pedido.Id]=pedido;return Results.Created($"/api/adesoes/{pedido.Id}",new {pedido.Id,pedido.Status,contaCriada=candidato is not null});
}}).RequireRateLimiting("adesao");
app.MapGet("/api/adesoes", (HttpContext c, Store db) => { lock(db.Sync) { var u=Store.User(c); if(!Store.Has(u,"AdministradorGeral","Presidente")) return Results.StatusCode(403); return Results.Ok(db.Applications.Values.Where(a=>Store.Scope(u,a.ClubeId,"Presidente")).ToArray()); }});
app.MapPost("/api/adesoes/{id:guid}/decisao", (Guid id, Decisao d, HttpContext c, Store db) => { lock(db.Sync) {
    if(!db.Applications.TryGetValue(id,out var a)) return Results.NotFound(); var u=Store.User(c);
    if(!Store.Scope(u,a.ClubeId,"Presidente")) return Results.StatusCode(403);
    if(a.Status!="Pendente") return Store.Bad("Solicitação já analisada.");
    if(d.Status is not ("Aprovada" or "Rejeitada")) return Store.Bad("Informe Aprovada ou Rejeitada.");
    if(d.Status=="Aprovada"){
      var idTitular=a.UsuarioId??d.UsuarioId;
      if(!idTitular.HasValue||!db.Users.TryGetValue(idTitular.Value,out var novoTitular)||!novoTitular.Ativo||!(novoTitular.Papeis.Contains("Titular")||novoTitular.Papeis.Contains("Candidato")))return Store.Bad("Informe uma conta Titular ativa ou uma conta de candidato vinculada ao pedido.");
      if(d.Categoria is null||!db.Categorias.TryGetValue(d.Categoria??"",out var cat)||!cat.Ativa||string.IsNullOrWhiteSpace(d.Matricula)||db.Members.Values.Any(x=>x.Matricula==d.Matricula))return Store.Bad("Categoria ou matrícula inválida.");
      if(db.Members.Values.Any(x=>x.Ativa&&(x.UsuarioTitularId==novoTitular.Id||x.Dependentes.Contains(novoTitular.Id))))return Store.Bad("Pessoa já possui associação ativa.");
      if(novoTitular.Papeis.Contains("Candidato")){novoTitular.Papeis.Remove("Candidato");novoTitular.Papeis.Add("Titular");novoTitular.Atribuicoes.Add(new Atribuicao("Titular",a.ClubeId));}
      var associacao=new Associacao{Id=Guid.NewGuid(),UsuarioTitularId=novoTitular.Id,ClubeOrigemId=a.ClubeId,Categoria=cat.name,TipoVinculo=a.TipoVinculo,Matricula=d.Matricula};db.Members[associacao.Id]=associacao;a.AssociacaoId=associacao.Id;db.Log(c,"criar","associacao",associacao.Id,a.ClubeId);
    }
    a.Status=d.Status; a.DecididaEm=DateTimeOffset.UtcNow; a.DecididaPor=u.Id; a.Motivo=d.Motivo; db.Log(c,"decidir","adesao",id,a.ClubeId);
    return Results.Ok(new {a.Id,a.Status,a.DecididaEm, a.AssociacaoId});
}});
app.MapGet("/api/associacoes", (HttpContext c, Store db) => { lock(db.Sync) { var u=Store.User(c); return Results.Ok(db.Members.Values.Where(m=>Store.Admin(c)||Store.Scope(u,m.ClubeOrigemId,"Presidente","Tesoureiro")||m.UsuarioTitularId==u.Id||m.Dependentes.Contains(u.Id)).ToArray()); }});
app.MapPost("/api/associacoes", (Associacao m, HttpContext c, Store db) => { lock(db.Sync) {
    var u=Store.User(c); if(!Store.Scope(u,m.ClubeOrigemId,"Presidente")) return Results.StatusCode(403);
    if(!db.Clubs.TryGetValue(m.ClubeOrigemId,out var club)||!club.Ativo||(!db.Users.TryGetValue(m.UsuarioTitularId,out var pessoa)||!pessoa.Ativo||!pessoa.Papeis.Contains("Titular"))) return Store.Bad("Clube ou titular inválido.");
    if((!db.Categorias.TryGetValue(m.Categoria??"",out var cat)||!cat.Ativa)||!new[]{"Associado/Fundador","Convidado","Simpatizante"}.Contains(m.TipoVinculo)) return Store.Bad("Categoria ou vínculo inválido.");
    if(db.Members.Values.Any(x=>x.Ativa&&(x.UsuarioTitularId==m.UsuarioTitularId||x.Dependentes.Contains(m.UsuarioTitularId)))) return Store.Bad("Pessoa já possui associação ativa.");
    if(string.IsNullOrWhiteSpace(m.Matricula)||m.Matricula.Length>100||db.Members.Values.Any(x=>x.Matricula==m.Matricula)) return Store.Bad("Matrícula obrigatória e única.");
    m.Id=Guid.NewGuid(); m.Dependentes=new(); db.Members[m.Id]=m; db.Log(c,"criar","associacao",m.Id,m.ClubeOrigemId); return Results.Created($"/api/associacoes/{m.Id}",m);
}});
app.MapGet("/api/espacos", (Store db) => { lock(db.Sync) return Results.Ok(db.Spaces.Values.ToArray()); });
app.MapPost("/api/espacos", (Espaco e,HttpContext c,Store db) => { lock(db.Sync) {
    if(!Store.Scope(Store.User(c),e.ClubeId,"Presidente","Funcionario")) return Results.StatusCode(403);
    if(!db.Clubs.TryGetValue(e.ClubeId,out var club)||!club.Ativo||string.IsNullOrWhiteSpace(e.Nome)||e.Capacidade<1||e.Taxa<0) return Store.Bad("Dados do espaço inválidos.");
    if(string.IsNullOrWhiteSpace(e.Tipo)||e.Tipo.Length>60)return Store.Bad("Informe o tipo do espaço (até 60 caracteres).");
    e.Id=Guid.NewGuid(); db.Spaces[e.Id]=e; db.Log(c,"criar","espaco",e.Id,e.ClubeId); return Results.Created($"/api/espacos/{e.Id}",e);
}});
app.MapGet("/api/agenda", (Guid espacoId, DateTimeOffset inicio, DateTimeOffset fim, Store db) => { lock(db.Sync) {
    if(fim<=inicio) return Store.Bad("Período inválido.");
    return Results.Ok(db.Bookings.Values.Where(x=>x.EspacoId==espacoId&&x.Status=="Confirmada"&&x.Inicio<fim&&inicio<x.Fim).Select(x=>new{x.Inicio,x.Fim,x.Finalidade}).ToArray());
}});
app.MapPost("/api/reservas", (NovaReserva d,HttpContext c,Store db) => { lock(db.Sync) {
    var u=Store.User(c); if(!db.Members.TryGetValue(d.AssociacaoId,out var m)||!m.Ativa||(m.UsuarioTitularId!=u.Id&&!m.Dependentes.Contains(u.Id)&&!Store.Has(u,"AdministradorGeral","Presidente","Funcionario"))) return Results.StatusCode(403);
    if(!db.Spaces.TryGetValue(d.EspacoId,out var e)||!e.Ativo||!db.Clubs.TryGetValue(e.ClubeId,out var club)||!club.Ativo) return Store.Bad("Espaço ou clube indisponível.");
    if(!Extras.Active(db,m.UsuarioTitularId,out _)||!db.Categorias[m.Categoria].Ativa)return Store.Bad("Titular, categoria ou associação indisponível.");if(m.UsuarioTitularId!=u.Id&&!m.Dependentes.Contains(u.Id)&&!Store.Scope(u,e.ClubeId,"Presidente","Funcionario"))return Results.StatusCode(403);if(m.UsuarioTitularId!=u.Id && !Store.Scope(u,e.ClubeId,"Presidente","Funcionario") && e.Taxa>0 && (!d.AutorizacaoId.HasValue || !db.Autorizacoes.TryGetValue(d.AutorizacaoId.Value,out var authorization) || authorization.DependenteId!=u.Id || authorization.EspacoId!=e.Id || authorization.AssociacaoId!=m.Id || authorization.Usada))return Store.Bad("Dependente precisa de autorização do titular para locação paga.");
    if(d.Inicio<=DateTimeOffset.UtcNow||d.Fim<=d.Inicio) return Store.Bad("Período inválido ou início no passado.");
    if(e.Tipo=="Salao"&&d.Inicio-DateTimeOffset.UtcNow<TimeSpan.FromHours(24)) return Store.Bad("Salão exige pelo menos 24 horas de antecedência.");
    if((e.Tipo=="Salao"||e.Tipo=="Churrasqueira") && d.Inicio-DateTimeOffset.UtcNow<=TimeSpan.FromHours(24) && d.Convidados==null)return Store.Bad("Informe a lista completa de convidados na confirmação.");
    if(d.Convidados!=null && (d.Convidados.Count>e.Capacidade || d.Convidados.Any(x=>string.IsNullOrWhiteSpace(x.Nome)||string.IsNullOrWhiteSpace(x.Identificacao))))return Store.Bad("Lista de convidados inválida ou superior à capacidade.");
    if(!Complementos.Funciona(db,e.Id,d.Inicio,d.Fim,club.FusoHorario))return Store.Bad("Fora do horário de funcionamento do espaço.");
    if(db.Blocked(m.Id)) return Store.Bad("Associação bloqueada por mensalidade vencida além da tolerância.");
    if(db.Interdicoes.Any(x=>x.EspacoId==e.Id && x.Inicio<d.Fim && d.Inicio<x.Fim)) return Store.Bad("Espaço indisponível por manutenção.");
    if(db.Bookings.Values.Any(x=>x.EspacoId==e.Id&&x.Status=="Confirmada"&&x.Inicio<d.Fim&&d.Inicio<x.Fim)) return Store.Bad("Horário indisponível.");
    var local=TimeZoneInfo.ConvertTime(d.Inicio,TimeZoneInfo.FindSystemTimeZoneById(club.FusoHorario));
    var count=db.Bookings.Values.Count(x=>x.AssociacaoId==m.Id&&x.Status=="Confirmada"&&x.MesLocal==local.ToString("yyyy-MM"));
    if(count>=3) return Store.Bad("Limite de três reservas no mês atingido.");
    var r=new Reserva {Id=Guid.NewGuid(),EspacoId=e.Id,AssociacaoId=m.Id,Inicio=d.Inicio,Fim=d.Fim,Finalidade=d.Finalidade,Valor=e.Taxa,MesLocal=local.ToString("yyyy-MM"),Status="Confirmada",StatusTaxa=e.Taxa==0?"Isenta":"EmAberto"};
    db.Bookings[r.Id]=r; if(d.Convidados!=null)db.Convidados[r.Id]=d.Convidados; if(d.AutorizacaoId.HasValue && db.Autorizacoes.TryGetValue(d.AutorizacaoId.Value,out var used)&&used.DependenteId==u.Id&&used.EspacoId==e.Id&&used.AssociacaoId==m.Id)used.Usada=true; r.ResponsavelId=u.Id; db.Log(c,"criar","reserva",r.Id,e.ClubeId); return Results.Created($"/api/reservas/{r.Id}",r);
}});
app.MapGet("/api/reservas",(HttpContext c,Store db)=>{lock(db.Sync){var u=Store.User(c); return Results.Ok(db.Bookings.Values.Where(r=>db.Members.TryGetValue(r.AssociacaoId,out var m)&&(m.UsuarioTitularId==u.Id||m.Dependentes.Contains(u.Id)||Store.Scope(u,db.Spaces[r.EspacoId].ClubeId,"Presidente","Funcionario"))).ToArray());}});
app.MapPost("/api/reservas/{id:guid}/cancelar",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){
    if(!db.Bookings.TryGetValue(id,out var r)) return Results.NotFound(); if(!db.Members.TryGetValue(r.AssociacaoId,out var m)||(m.UsuarioTitularId!=Store.User(c).Id&&!Store.Scope(Store.User(c),db.Spaces[r.EspacoId].ClubeId,"Presidente","Funcionario"))) return Results.StatusCode(403);
    if(r.Status!="Confirmada"||r.Inicio<=DateTimeOffset.UtcNow) return Store.Bad("Reserva não pode ser cancelada.");
    
    r.Status="Cancelada"; var multa=r.Inicio-DateTimeOffset.UtcNow>=TimeSpan.FromHours(24)?0:Math.Min(50,r.Valor); var reembolso=r.StatusTaxa=="QuitadaSimulada"?r.Valor-multa:0; r.CanceladaEm=DateTimeOffset.UtcNow;r.Multa=multa;r.Reembolso=reembolso;if(reembolso>0)db.Creditos.Add(new Credito(Guid.NewGuid(),r.AssociacaoId,r.Id,reembolso,"Crédito simulado por cancelamento de taxa quitada"));Extras.CancelarTransferencia(db,r,multa);if(r.StatusTaxa!="Cancelada")r.StatusTaxa="Cancelada"; db.Log(c,"cancelar","reserva",id,db.Spaces[r.EspacoId].ClubeId);
    return Results.Ok(new {reserva=r, multaSimulada=multa, reembolsoSimulado=reembolso});
}});
app.MapPost("/api/reservas/{id:guid}/quitar-taxa",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){
    if(!db.Bookings.TryGetValue(id,out var r)) return Results.NotFound(); if(!db.Members.TryGetValue(r.AssociacaoId,out var m)||m.UsuarioTitularId!=Store.User(c).Id) return Results.StatusCode(403);
    if(r.Status!="Confirmada"||r.Inicio<=DateTimeOffset.UtcNow) return Store.Bad("Fora do prazo ou reserva cancelada.");
    if(r.StatusTaxa=="QuitadaSimulada"||r.StatusTaxa=="Isenta") return Results.Ok(r); if(r.StatusTaxa!="EmAberto") return Store.Bad("Taxa já transferida.");
    r.StatusTaxa="QuitadaSimulada"; db.Log(c,"quitar-simulado","reserva",id,db.Spaces[r.EspacoId].ClubeId); return Results.Ok(r);
}});
app.MapPost("/api/faturas",(NovaFatura d,HttpContext c,Store db)=>{lock(db.Sync){
    if(!db.Members.TryGetValue(d.AssociacaoId,out var m)) return Results.NotFound(); if(!Store.Scope(Store.User(c),m.ClubeOrigemId,"Presidente","Tesoureiro")) return Results.StatusCode(403);
    if(d.Valor<0||!DateOnly.TryParseExact(d.Competencia+"-01","yyyy-MM-dd",out _)) return Store.Bad("Valor ou competência inválido.");
    if(db.Invoices.Values.Any(x=>x.AssociacaoId==m.Id&&x.Competencia==d.Competencia)) return Store.Bad("Mensalidade já existe para esta competência.");
    var f=new Fatura {Id=Guid.NewGuid(),AssociacaoId=m.Id,Competencia=d.Competencia,Vencimento=d.Vencimento,ValorBase=d.Valor,Valor=d.Valor}; db.Invoices[f.Id]=f; db.Log(c,"criar","fatura",f.Id,m.ClubeOrigemId); return Results.Created($"/api/faturas/{f.Id}",f);
}});
app.MapGet("/api/faturas",(HttpContext c,Store db)=>{lock(db.Sync){var u=Store.User(c); if(!Store.Has(u,"Titular","AdministradorGeral","Presidente","Tesoureiro")) return Results.StatusCode(403); return Results.Ok(db.Invoices.Values.Where(f=>db.Members.TryGetValue(f.AssociacaoId,out var m)&&(m.UsuarioTitularId==u.Id||Store.Scope(u,m.ClubeOrigemId,"Presidente","Tesoureiro"))).ToArray());}});
app.MapPost("/api/faturas/{id:guid}/quitar",(Guid id,HttpContext c,Store db)=>{lock(db.Sync){if(!db.Invoices.TryGetValue(id,out var f)) return Results.NotFound(); var m=db.Members[f.AssociacaoId]; var u=Store.User(c); if(m.UsuarioTitularId!=u.Id&&!Store.Scope(u,m.ClubeOrigemId,"Tesoureiro")) return Results.StatusCode(403); f.Status="QuitadaSimulada"; db.Log(c,"quitar-simulado","fatura",id,m.ClubeOrigemId);return Results.Ok(f);}});
app.MapGet("/api/avisos",(HttpContext c,Store db)=>{lock(db.Sync){var u=Store.User(c);var clubs=db.Members.Values.Where(m=>m.Ativa&&(m.UsuarioTitularId==u.Id||m.Dependentes.Contains(u.Id))).Select(m=>m.ClubeOrigemId).ToHashSet();return Results.Ok(db.Notices.Where(n=>n.ClubeId is null||clubs.Contains(n.ClubeId.Value)||Store.Scope(u,n.ClubeId.Value,"Presidente","Funcionario","Tesoureiro","Porteiro")).ToArray());}});
app.MapPost("/api/avisos",(Aviso n,HttpContext c,Store db)=>{lock(db.Sync){if(n.ClubeId is null ? !Store.Admin(c) : !Store.Scope(Store.User(c),n.ClubeId.Value,"Presidente","Funcionario")) return Results.StatusCode(403); if(string.IsNullOrWhiteSpace(n.Titulo)||string.IsNullOrWhiteSpace(n.Corpo))return Store.Bad("Título e corpo obrigatórios."); n.Id=Guid.NewGuid();db.Notices.Add(n);db.Log(c,"criar","aviso",n.Id,n.ClubeId);return Results.Created($"/api/avisos/{n.Id}",n);}});
app.MapPost("/api/eventos",(Evento e,HttpContext c,Store db)=>{lock(db.Sync){
 if(!Store.Scope(Store.User(c),e.ClubeId,"Presidente","Funcionario"))return Results.StatusCode(403);
 if(!db.Clubs.TryGetValue(e.ClubeId,out var club)||!club.Ativo||string.IsNullOrWhiteSpace(e.Nome)||e.Fim<=e.Inicio||e.PublicoAlvo is not ("SomenteAssociados" or "PublicoGeral"))return Store.Bad("Dados do evento inválidos.");
 e.Id=Guid.NewGuid(); db.Events.Add(e); db.Log(c,"criar","evento",e.Id,e.ClubeId); return Results.Created($"/api/eventos/{e.Id}",e);
}});
app.MapGet("/api/eventos",(Store db)=>{lock(db.Sync)return Results.Ok(db.Events.ToArray());});
app.MapPost("/api/chamados",(Chamado t,HttpContext c,Store db)=>{lock(db.Sync){
 if(t.Tipo is not ("Solicitacao" or "Reclamacao")||string.IsNullOrWhiteSpace(t.Assunto)||string.IsNullOrWhiteSpace(t.Descricao))return Store.Bad("Informe tipo, assunto e descrição.");
 if(t.ClubeId is not null&&!db.Clubs.ContainsKey(t.ClubeId.Value))return Store.Bad("Clube inválido.");
 t.Id=Guid.NewGuid();t.UsuarioId=Store.User(c).Id;t.Status="Aberto";t.Respostas=new(); db.Tickets.Add(t);db.Log(c,"criar","chamado",t.Id,t.ClubeId);return Results.Created($"/api/chamados/{t.Id}",t);
}});
app.MapGet("/api/chamados",(HttpContext c,Store db)=>{lock(db.Sync){var u=Store.User(c);return Results.Ok(db.Tickets.Where(t=>t.UsuarioId==u.Id||Store.Admin(c)||(t.ClubeId.HasValue&&Store.Scope(u,t.ClubeId.Value,"Presidente","Funcionario"))).ToArray());}});
app.MapPost("/api/chamados/{id:guid}/respostas",(Guid id,RespostaInput d,HttpContext c,Store db)=>{lock(db.Sync){var t=db.Tickets.FirstOrDefault(x=>x.Id==id);if(t is null)return Results.NotFound();var u=Store.User(c);if(t.UsuarioId!=u.Id&&!Store.Admin(c)&&!(t.ClubeId.HasValue&&Store.Scope(u,t.ClubeId.Value,"Presidente","Funcionario")))return Results.StatusCode(403);if(string.IsNullOrWhiteSpace(d.Texto))return Store.Bad("Resposta vazia.");t.Respostas.Add(new Resposta(u.Id,d.Texto,DateTimeOffset.UtcNow));db.Log(c,"responder","chamado",t.Id,t.ClubeId);return Results.Ok(t);}});
app.MapGet("/api/auditoria",(HttpContext c,Store db)=>{lock(db.Sync){if(!Store.Admin(c))return Results.StatusCode(403);return Results.Ok(db.Audit.ToArray());}});
Extras.Map(app);
Complementos.Map(app);
Operacoes.Map(app);
app.Run();

