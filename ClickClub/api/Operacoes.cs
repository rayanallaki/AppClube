using System.Globalization;
using Microsoft.AspNetCore.Identity;
using QRCoder;

// Casos de uso complementares, mantendo os contratos já consumidos pelos clientes.
static class Operacoes
{
    static IResult Negado() => Results.StatusCode(403);
    public static bool PodeGerenciarPessoa(Store db, Usuario operador, Usuario alvo)
    {
        if (operador.Papeis.Contains("AdministradorGeral")) return true;
        if (alvo.Papeis.Contains("AdministradorGeral")) return false;
        if(alvo.Atribuicoes.Any(a=>new[]{"Presidente","Tesoureiro","Funcionario","Porteiro"}.Contains(a.Papel)&&!Store.Scope(operador,a.ClubeId,"Presidente")))return false;
        var m = db.Members.Values.FirstOrDefault(x => x.Ativa && (x.UsuarioTitularId == alvo.Id || x.Dependentes.Contains(alvo.Id)));
        return m is not null ? Store.Scope(operador, m.ClubeOrigemId, "Presidente")
            : alvo.Clubes.Any(clube => Store.Scope(operador, clube, "Presidente"));
    }

    public static object Carteirinha(Store db, Usuario u, Associacao m, Credencial cr, string token)
    {
        using var data = QRCodeGenerator.GenerateQrCode(token, QRCodeGenerator.ECCLevel.M);
        using var svg = new SvgQRCode(data);
        using var png = new PngByteQRCode(data);
        return new { cr.Id, credencial=token, cr.EmitidaEm, cr.ValidaAte, nome=u.Nome, matricula=m.Matricula,
            clube=db.Clubs[m.ClubeOrigemId].Nome, categoria=m.Categoria, qrSvg=svg.GetGraphic(5), qrPngBase64=Convert.ToBase64String(png.GetGraphic(8)) };
    }

    public static Fatura NovaMensalidade(Store db, Associacao m, string competencia, DateOnly vencimento, decimal valor)
    {
        var f = new Fatura { Id=Guid.NewGuid(), AssociacaoId=m.Id, Competencia=competencia, Vencimento=vencimento, ValorBase=valor, Valor=valor };
        db.Invoices[f.Id] = f;
        return f;
    }

    static object DetalheAssociacao(Store db, Associacao m, Guid usuarioId) => new {
        associacao=m, titular=Store.PublicUser(db.Users[m.UsuarioTitularId]),
        dependentes=m.Dependentes.Where(db.Users.ContainsKey).Select(id => Pessoa(db.Users[id], usuarioId==m.UsuarioTitularId||usuarioId==id||PodeGerenciarPessoa(db,db.Users[usuarioId],db.Users[id]))).ToArray(),
        clube=db.Clubs[m.ClubeOrigemId], bloqueada=db.Blocked(m.Id), usuarioId,
        direitoVoto=m.TipoVinculo=="Associado/Fundador"
    };
    static object Pessoa(Usuario u, bool dadosCompletos=true) => new { u.Id, u.Nome, u.Login, u.Ativo, dados=dadosCompletos?u.Dados:null };
    static bool FamiliaOuGestor(Store db, Usuario u, Associacao m) => m.UsuarioTitularId==u.Id || m.Dependentes.Contains(u.Id) || Store.Scope(u,m.ClubeOrigemId,"Presidente");
    static bool DonoOuFinanceiro(Usuario u, Associacao m) => m.UsuarioTitularId==u.Id || Store.Scope(u,m.ClubeOrigemId,"Presidente","Tesoureiro");

    public static void Map(WebApplication app)
    {
        app.MapGet("/api/publico/clubes", (Store db) => Results.Ok(db.Clubs.Values.Where(x=>x.Ativo).ToArray()));
        app.MapGet("/api/publico/eventos", (Store db) => Results.Ok(db.Events.Where(x=>x.Status=="Ativo"&&x.PublicoAlvo=="PublicoGeral"&&x.Fim>DateTimeOffset.UtcNow&&db.Clubs[x.ClubeId].Ativo).ToArray()));

        app.MapPost("/api/auth/alterar-senha", (AlterarSenha d,HttpContext c,Store db) => {
            var u=Store.User(c);
            if(string.IsNullOrEmpty(d.SenhaAtual)||string.IsNullOrWhiteSpace(d.NovaSenha)||d.NovaSenha.Length<8)return Store.Bad("Informe a senha atual e uma nova senha com pelo menos oito caracteres.");
            if(db.Hasher.VerifyHashedPassword(u,u.SenhaHash,d.SenhaAtual)==PasswordVerificationResult.Failed)return Store.Bad("Senha atual incorreta.");
            u.SenhaHash=db.Hasher.HashPassword(u,d.NovaSenha);
            var atual=c.Request.Headers.Authorization.ToString()[7..].Trim();
            foreach(var key in db.Sessions.Where(x=>x.Value.UserId==u.Id&&x.Key!=atual).Select(x=>x.Key).ToArray())db.Sessions.Remove(key);
            db.Log(c,"alterar-senha","usuario",u.Id,null);
            return Results.Ok(new{mensagem="Senha alterada. Outras sessões foram encerradas."});
        });
        app.MapPut("/api/usuarios/{id:guid}",(Guid id,UsuarioEdicao d,HttpContext c,Store db)=>{
            if(!db.Users.TryGetValue(id,out var u))return Results.NotFound();
            if(!PodeGerenciarPessoa(db,Store.User(c),u))return Negado();
            if(string.IsNullOrWhiteSpace(d.Nome)||string.IsNullOrWhiteSpace(d.Login)||d.Login.Length>100||db.Users.Values.Any(x=>x.Id!=id&&x.Login.Equals(d.Login,StringComparison.OrdinalIgnoreCase)))return Store.Bad("Informe nome e login único de até 100 caracteres.");
            u.Nome=d.Nome.Trim();u.Login=d.Login.Trim();db.Log(c,"alterar","usuario",id,null);return Results.Ok(Store.PublicUser(u));
        });
        app.MapPost("/api/usuarios/{id:guid}/redefinir-senha",(Guid id,NovaSenha d,HttpContext c,Store db)=>{
            if(!db.Users.TryGetValue(id,out var u))return Results.NotFound();
            if(!PodeGerenciarPessoa(db,Store.User(c),u))return Negado();
            if(string.IsNullOrWhiteSpace(d.Senha)||d.Senha.Length<8)return Store.Bad("Senha deve ter pelo menos oito caracteres.");
            u.SenhaHash=db.Hasher.HashPassword(u,d.Senha);
            foreach(var key in db.Sessions.Where(x=>x.Value.UserId==id).Select(x=>x.Key).ToArray())db.Sessions.Remove(key);
            db.Log(c,"redefinir-senha","usuario",id,null);return Results.NoContent();
        });
        app.MapGet("/api/minha-associacao",(HttpContext c,Store db)=>{
            var u=Store.User(c);var m=db.Members.Values.FirstOrDefault(x=>x.Ativa&&(x.UsuarioTitularId==u.Id||x.Dependentes.Contains(u.Id)))??db.Members.Values.FirstOrDefault(x=>x.UsuarioTitularId==u.Id||x.Dependentes.Contains(u.Id));
            return m is null?Results.NotFound(new{erro="Nenhuma associação vinculada. Consulte a situação do pedido de adesão."}):Results.Ok(DetalheAssociacao(db,m,u.Id));
        });
        app.MapGet("/api/associacoes/{id:guid}",(Guid id,HttpContext c,Store db)=>{
            if(!db.Members.TryGetValue(id,out var m))return Results.NotFound();if(!FamiliaOuGestor(db,Store.User(c),m))return Negado();return Results.Ok(DetalheAssociacao(db,m,Store.User(c).Id));
        });
        app.MapPut("/api/associacoes/{id:guid}",(Guid id,AssociacaoEdicao d,HttpContext c,Store db)=>{
            if(!db.Members.TryGetValue(id,out var m))return Results.NotFound();var u=Store.User(c);
            if(!Store.Scope(u,m.ClubeOrigemId,"Presidente")||!Store.Scope(u,d.ClubeOrigemId,"Presidente"))return Negado();
            if(!db.Clubs.TryGetValue(d.ClubeOrigemId,out var club)||!club.Ativo||!db.Categorias.TryGetValue(d.Categoria??"",out var cat)||!cat.Ativa||d.TipoVinculo is not("Associado/Fundador" or "Convidado" or "Simpatizante")||string.IsNullOrWhiteSpace(d.Matricula)||db.Members.Values.Any(x=>x.Id!=id&&x.Matricula.Equals(d.Matricula,StringComparison.OrdinalIgnoreCase)))return Store.Bad("Clube, categoria, vínculo ou matrícula inválidos.");
            if(cat.LimiteDependentes is int limite&&m.Dependentes.Count>limite)return Store.Bad("A categoria não comporta os dependentes atuais.");
            if(d.Ativa&&db.Members.Values.Any(x=>x.Id!=id&&x.Ativa&&(x.UsuarioTitularId==m.UsuarioTitularId||x.Dependentes.Contains(m.UsuarioTitularId)||m.Dependentes.Contains(x.UsuarioTitularId)||x.Dependentes.Any(m.Dependentes.Contains))))return Store.Bad("Há pessoa vinculada a outra associação ativa.");
            m.Categoria=cat.name;m.TipoVinculo=d.TipoVinculo;m.ClubeOrigemId=d.ClubeOrigemId;m.Matricula=d.Matricula.Trim();m.Ativa=d.Ativa;
            db.Log(c,"alterar","associacao",id,m.ClubeOrigemId);return Results.Ok(m);
        });
        app.MapGet("/api/associacoes/{id:guid}/dependentes",(Guid id,HttpContext c,Store db)=>{
            if(!db.Members.TryGetValue(id,out var m))return Results.NotFound();if(!FamiliaOuGestor(db,Store.User(c),m))return Negado();return Results.Ok(m.Dependentes.Where(db.Users.ContainsKey).Select(x=>Pessoa(db.Users[x],m.UsuarioTitularId==Store.User(c).Id||x==Store.User(c).Id||PodeGerenciarPessoa(db,Store.User(c),db.Users[x]))).ToArray());
        });
        app.MapGet("/api/minhas-autorizacoes",(HttpContext c,Store db)=>{
            var u=Store.User(c);return Results.Ok(db.Autorizacoes.Values.Where(a=>a.DependenteId==u.Id&&db.Members.TryGetValue(a.AssociacaoId,out var m)&&m.Ativa&&m.Dependentes.Contains(u.Id)).Select(a=>new{a.Id,a.AssociacaoId,a.DependenteId,a.EspacoId,a.Usada,espaco=db.Spaces[a.EspacoId].Nome}).ToArray());
        });
        app.MapPost("/api/associacoes/{id:guid}/dependentes/cadastro",(Guid id,DependenteCadastro d,HttpContext c,Store db)=>{
            if(!db.Members.TryGetValue(id,out var m))return Results.NotFound();var u=Store.User(c);
            if(m.UsuarioTitularId!=u.Id&&!Store.Scope(u,m.ClubeOrigemId,"Presidente"))return Negado();
            if(!m.Ativa||!db.Categorias.TryGetValue(m.Categoria,out var cat)||!cat.Ativa)return Store.Bad("Associação ou categoria inativa.");
            if(cat.LimiteDependentes is int limite&&m.Dependentes.Count>=limite)return Store.Bad("Limite de dependentes da categoria atingido.");
            if(string.IsNullOrWhiteSpace(d.Nome)||string.IsNullOrWhiteSpace(d.Login)||d.Login.Length>100||string.IsNullOrWhiteSpace(d.Senha)||d.Senha.Length<8||db.Users.Values.Any(x=>x.Login.Equals(d.Login,StringComparison.OrdinalIgnoreCase)))return Store.Bad("Informe nome, login disponível e senha com oito ou mais caracteres.");
            var cpf=string.IsNullOrWhiteSpace(d.Cpf)?null:new string(d.Cpf.Where(char.IsDigit).ToArray());
            if(cpf is not null&&(!Complementos.CpfValido(cpf)||db.Users.Values.Any(x=>x.Dados.Cpf==cpf)))return Store.Bad("CPF inválido ou já cadastrado.");
            if(d.DataNascimento>DateOnly.FromDateTime(DateTime.Today))return Store.Bad("Data de nascimento no futuro.");
            var dep=new Usuario{Nome=d.Nome.Trim(),Login=d.Login.Trim(),Papeis=new(){"Dependente"},Clubes=new(){m.ClubeOrigemId},Atribuicoes=new(){new("Dependente",m.ClubeOrigemId)},Dados=new(){Cpf=cpf,DataNascimento=d.DataNascimento}};
            dep.SenhaHash=db.Hasher.HashPassword(dep,d.Senha);db.Users[dep.Id]=dep;m.Dependentes.Add(dep.Id);
            db.Log(c,"criar-dependente","usuario",dep.Id,m.ClubeOrigemId);return Results.Created($"/api/usuarios/{dep.Id}/dados",Pessoa(dep));
        });

        app.MapGet("/api/espacos/{id:guid}/interdicoes",(Guid id,Store db)=>db.Spaces.ContainsKey(id)?Results.Ok(db.Interdicoes.Where(x=>x.EspacoId==id).ToArray()):Results.NotFound());
        app.MapDelete("/api/interdicoes/{id:guid}",(Guid id,HttpContext c,Store db)=>{
            var i=db.Interdicoes.FirstOrDefault(x=>x.Id==id);if(i is null)return Results.NotFound();var e=db.Spaces[i.EspacoId];if(!Store.Scope(Store.User(c),e.ClubeId,"Presidente","Funcionario"))return Negado();db.Interdicoes.Remove(i);db.Log(c,"remover","interdicao",id,e.ClubeId);return Results.NoContent();
        });
        app.MapDelete("/api/clubes/{id:guid}",(Guid id,HttpContext c,Store db)=>{
            if(!db.Clubs.TryGetValue(id,out var e))return Results.NotFound();if(!Store.Admin(c))return Negado();e.Ativo=false;db.Log(c,"desativar","clube",id,id);return Results.NoContent();
        });
        app.MapDelete("/api/espacos/{id:guid}",(Guid id,HttpContext c,Store db)=>{
            if(!db.Spaces.TryGetValue(id,out var e))return Results.NotFound();if(!Store.Scope(Store.User(c),e.ClubeId,"Presidente","Funcionario"))return Negado();e.Ativo=false;db.Log(c,"desativar","espaco",id,e.ClubeId);return Results.NoContent();
        });
        app.MapPut("/api/turmas/{id:guid}",(Guid id,Turma d,HttpContext c,Store db)=>{
            if(!db.Turmas.TryGetValue(id,out var t))return Results.NotFound();if(!Store.Scope(Store.User(c),t.ClubeId,"Presidente","Funcionario"))return Negado();
            if(d.ClubeId!=t.ClubeId||string.IsNullOrWhiteSpace(d.Nome)||d.Fim<=d.Inicio||d.Capacidade<db.Inscricoes.Count(x=>x.TurmaId==id&&x.Status=="Confirmada")||d.Capacidade<1)return Store.Bad("Dados inválidos ou capacidade abaixo dos inscritos confirmados.");
            if(d.AtividadeId is Guid atividade&&!db.Atividades.Any(x=>x.Id==atividade))return Store.Bad("Atividade inexistente.");
            var periodo=new Turma{Id=id,Inicio=d.Inicio,Fim=d.Fim};
            if(db.Aulas.Any(x=>x.TurmaId==id&&(x.Inicio<d.Inicio||x.Fim>d.Fim)))return Store.Bad("As aulas devem permanecer dentro do período da turma.");
            if(!db.Aulas.Any(x=>x.TurmaId==id)&&db.Inscricoes.Where(x=>x.TurmaId==id&&x.Status=="Confirmada").Any(x=>db.Inscricoes.Any(y=>y.UsuarioId==x.UsuarioId&&y.TurmaId!=id&&y.Status=="Confirmada"&&Complementos.Conflito(db,periodo,db.Turmas[y.TurmaId]))))return Store.Bad("Mudança cria conflito para um inscrito.");
            t.Nome=d.Nome;t.Descricao=d.Descricao;t.Instrutor=d.Instrutor;t.AtividadeId=d.AtividadeId;t.Inicio=d.Inicio;t.Fim=d.Fim;t.Capacidade=d.Capacidade;t.Ativa=d.Ativa;db.Log(c,"alterar","turma",id,t.ClubeId);return Results.Ok(t);
        });
        app.MapDelete("/api/turmas/{id:guid}",(Guid id,HttpContext c,Store db)=>{
            if(!db.Turmas.TryGetValue(id,out var t))return Results.NotFound();if(!Store.Scope(Store.User(c),t.ClubeId,"Presidente","Funcionario"))return Negado();t.Ativa=false;db.Log(c,"desativar","turma",id,t.ClubeId);return Results.NoContent();
        });
        app.MapPut("/api/atividades/{id:guid}",(Guid id,Atividade d,HttpContext c,Store db)=>{
            if(!Store.Admin(c))return Negado();var a=db.Atividades.FirstOrDefault(x=>x.Id==id);if(a is null)return Results.NotFound();if(string.IsNullOrWhiteSpace(d.Nome))return Store.Bad("Informe o nome.");a.Nome=d.Nome;a.Descricao=d.Descricao;a.Tipo=d.Tipo;db.Log(c,"alterar","atividade",id,null);return Results.Ok(a);
        });
        app.MapDelete("/api/atividades/{id:guid}",(Guid id,HttpContext c,Store db)=>{
            if(!Store.Admin(c))return Negado();var a=db.Atividades.FirstOrDefault(x=>x.Id==id);if(a is null)return Results.NotFound();if(db.Turmas.Values.Any(x=>x.AtividadeId==id))return Store.Bad("Atividade vinculada a turma; remova o vínculo antes de excluir.");db.Atividades.Remove(a);db.Log(c,"remover","atividade",id,null);return Results.NoContent();
        });
        app.MapGet("/api/eventos/{id:guid}/visitantes",(Guid id,HttpContext c,Store db)=>{
            var e=db.Events.FirstOrDefault(x=>x.Id==id);if(e is null)return Results.NotFound();if(!Store.Scope(Store.User(c),e.ClubeId,"Presidente","Funcionario","Porteiro"))return Negado();return Results.Ok(db.Visitas.Where(x=>x.EventoId==id).ToArray());
        });
        app.MapDelete("/api/eventos/{id:guid}",(Guid id,HttpContext c,Store db)=>{
            var e=db.Events.FirstOrDefault(x=>x.Id==id);if(e is null)return Results.NotFound();if(!Store.Scope(Store.User(c),e.ClubeId,"Presidente","Funcionario"))return Negado();e.Status="Cancelado";db.Log(c,"cancelar","evento",id,e.ClubeId);return Results.NoContent();
        });
        app.MapPost("/api/eventos/inscricoes/{id:guid}/cancelar",(Guid id,HttpContext c,Store db)=>{
            var item=db.InscricoesEventos.FirstOrDefault(x=>x.Id==id);if(item is null)return Results.NotFound();var e=db.Events.First(x=>x.Id==item.EventoId);if(item.UsuarioId!=Store.User(c).Id&&!Store.Scope(Store.User(c),e.ClubeId,"Presidente","Funcionario"))return Negado();db.InscricoesEventos.Remove(item);db.InscricoesEventos.Add(item with{Status="Cancelada"});db.Log(c,"cancelar-inscricao","evento",e.Id,e.ClubeId);return Results.NoContent();
        });
        app.MapPut("/api/avisos/{id:guid}",(Guid id,Aviso d,HttpContext c,Store db)=>{
            var a=db.Notices.FirstOrDefault(x=>x.Id==id);if(a is null)return Results.NotFound();if(a.ClubeId is Guid clube?!Store.Scope(Store.User(c),clube,"Presidente","Funcionario"):!Store.Admin(c))return Negado();if(d.ClubeId!=a.ClubeId||string.IsNullOrWhiteSpace(d.Titulo)||string.IsNullOrWhiteSpace(d.Corpo))return Store.Bad("Título e corpo obrigatórios; mantenha a unidade.");a.Titulo=d.Titulo;a.Corpo=d.Corpo;db.Log(c,"alterar","aviso",id,a.ClubeId);return Results.Ok(a);
        });
        app.MapDelete("/api/avisos/{id:guid}",(Guid id,HttpContext c,Store db)=>{
            var a=db.Notices.FirstOrDefault(x=>x.Id==id);if(a is null)return Results.NotFound();if(a.ClubeId is Guid clube?!Store.Scope(Store.User(c),clube,"Presidente","Funcionario"):!Store.Admin(c))return Negado();db.Notices.Remove(a);db.Log(c,"remover","aviso",id,a.ClubeId);return Results.NoContent();
        });

        app.MapPost("/api/faturas/gerar",(GerarFaturas d,HttpContext c,Store db)=>{
            var u=Store.User(c);if(d.ClubeId is Guid club?!Store.Scope(u,club,"Presidente","Tesoureiro"):!Store.Admin(c))return Negado();
            if(!DateOnly.TryParseExact(d.Competencia+"-01","yyyy-MM-dd",out _)||d.ValorPadrao<0)return Store.Bad("Competência deve ser yyyy-MM e valor não negativo.");
            var members=db.Members.Values.Where(x=>x.Ativa&&(d.ClubeId is null||x.ClubeOrigemId==d.ClubeId)).ToArray();
            var pendentes=members.Where(m=>!db.Invoices.Values.Any(x=>x.AssociacaoId==m.Id&&x.Competencia==d.Competencia)).ToArray();
            if(pendentes.Any(m=>d.ValorPadrao is null&&(!db.Categorias.TryGetValue(m.Categoria,out var cat)||cat.ValorMensal is null)))return Store.Bad("Configure valor mensal de todas as categorias ou informe valorPadrao.");
            var novas=pendentes.Select(m=>NovaMensalidade(db,m,d.Competencia,d.Vencimento,d.ValorPadrao??db.Categorias[m.Categoria].ValorMensal!.Value)).ToArray();
            foreach(var f in novas)db.Log(c,"gerar-mensalidade","fatura",f.Id,db.Members[f.AssociacaoId].ClubeOrigemId);
            return Results.Ok(new{criadas=novas.Length,existentes=members.Length-novas.Length,faturas=novas});
        });
        app.MapGet("/api/faturas/{id:guid}/detalhes",(Guid id,HttpContext c,Store db)=>{
            if(!db.Invoices.TryGetValue(id,out var f))return Results.NotFound();if(!DonoOuFinanceiro(Store.User(c),db.Members[f.AssociacaoId]))return Negado();
            var itens=new List<object>{new{id=f.Id,faturaId=f.Id,reservaId=(Guid?)null,descricao="Mensalidade "+f.Competencia,valor=f.ValorBase}};
            itens.AddRange(db.Itens.Where(x=>x.FaturaId==id).Cast<object>());return Results.Ok(new{fatura=f,itens,simulado=true});
        });
        app.MapPost("/api/reservas/{id:guid}/preparar-transferencia",(Guid id,HttpContext c,Store db)=>{
            if(!db.Bookings.TryGetValue(id,out var r))return Results.NotFound();var m=db.Members[r.AssociacaoId];if(m.UsuarioTitularId!=Store.User(c).Id)return Negado();
            if(r.Status!="Confirmada"||r.StatusTaxa!="EmAberto"||r.Inicio<=DateTimeOffset.UtcNow||r.Valor<=0)return Store.Bad("Taxa não disponível para transferência.");
            var club=db.Clubs[m.ClubeOrigemId];var hoje=DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,TimeZoneInfo.FindSystemTimeZoneById(club.FusoHorario)).DateTime);var mes=new DateOnly(hoje.Year,hoje.Month,1).AddMonths(1);
            var f=db.Invoices.Values.Where(x=>x.AssociacaoId==m.Id&&x.Status=="EmAberto"&&string.CompareOrdinal(x.Competencia,mes.ToString("yyyy-MM"))>=0&&x.Vencimento>hoje).OrderBy(x=>x.Competencia).FirstOrDefault();
            if(f is null){
                if(!db.Categorias.TryGetValue(m.Categoria,out var cat)||cat.ValorMensal is null)return Store.Bad("Administração precisa configurar o valor mensal da categoria.");
                while(db.Invoices.Values.Any(x=>x.AssociacaoId==m.Id&&x.Competencia==mes.ToString("yyyy-MM")))mes=mes.AddMonths(1);
                f=NovaMensalidade(db,m,mes.ToString("yyyy-MM"),mes.AddDays(9),cat.ValorMensal.Value);db.Log(c,"gerar-mensalidade","fatura",f.Id,m.ClubeOrigemId);
            }
            return Results.Ok(new{f.Id,f.Competencia,f.Vencimento,principal=r.Valor,acrescimo=decimal.Round(r.Valor*0.1m,2),total=decimal.Round(r.Valor*1.1m,2)});
        });
        app.MapGet("/api/painel",(HttpContext c,Store db)=>{
            var u=Store.User(c);bool Gestor(Guid club)=>Store.Scope(u,club,"Presidente","Funcionario","Tesoureiro","Porteiro");
            var minhas=db.Members.Values.Where(x=>x.UsuarioTitularId==u.Id||x.Dependentes.Contains(u.Id)).ToArray();var clubes=minhas.Select(x=>x.ClubeOrigemId).ToHashSet();
            var reservas=db.Bookings.Values.Where(x=>minhas.Any(m=>m.Id==x.AssociacaoId)||Gestor(db.Spaces[x.EspacoId].ClubeId)).ToArray();
            var faturas=db.Invoices.Values.Where(x=>db.Members[x.AssociacaoId].UsuarioTitularId==u.Id||Store.Scope(u,db.Members[x.AssociacaoId].ClubeOrigemId,"Presidente","Tesoureiro")).ToArray();
            return Results.Ok(new{usuario=Store.PublicUser(u),clubes=db.Clubs.Values.Count(x=>x.Ativo),associacoes=db.Members.Values.Count(x=>x.Ativa&&(minhas.Contains(x)||Gestor(x.ClubeOrigemId))),reservasConfirmadas=reservas.Count(x=>x.Status=="Confirmada"),espacos=db.Spaces.Values.Count(x=>x.Ativo),turmas=db.Turmas.Values.Count(x=>x.Ativa),eventos=db.Events.Count(x=>x.Status=="Ativo"&&x.Fim>DateTimeOffset.UtcNow),faturasAbertas=faturas.Count(x=>x.Status=="EmAberto"),valorEmAberto=faturas.Where(x=>x.Status=="EmAberto").Sum(x=>x.Valor),proximasReservas=reservas.Where(x=>x.Status=="Confirmada"&&x.Fim>DateTimeOffset.UtcNow).OrderBy(x=>x.Inicio).Take(5).Select(x=>new{x.Id,x.Inicio,x.Fim,x.Finalidade,espaco=db.Spaces[x.EspacoId].Nome}).ToArray(),adesoesPendentes=db.Applications.Values.Count(x=>x.Status=="Pendente"&&Store.Scope(u,x.ClubeId,"Presidente"))});
        });
        app.MapGet("/api/relatorios/periodo",(Guid clubeId,DateTimeOffset inicio,DateTimeOffset fim,HttpContext c,Store db)=>{
            if(!Store.Scope(Store.User(c),clubeId,"Presidente","Funcionario","Tesoureiro"))return Negado();if(fim<=inicio)return Store.Bad("Período inválido.");
            return Results.Ok(new{clubeId,inicio,fim,reservas=db.Bookings.Values.Where(x=>db.Spaces[x.EspacoId].ClubeId==clubeId&&x.Inicio<fim&&x.Fim>inicio).ToArray(),acessos=db.Acessos.Where(x=>x.ClubeId==clubeId&&x.DataHora>=inicio&&x.DataHora<fim).ToArray(),inscricoes=db.Inscricoes.Where(x=>db.Turmas[x.TurmaId].ClubeId==clubeId&&x.InscritaEm>=inicio&&x.InscritaEm<fim).ToArray()});
        });
    }
}

record AlterarSenha(string SenhaAtual,string NovaSenha);
record NovaSenha(string Senha);
record UsuarioEdicao(string Nome,string Login);
record AssociacaoEdicao(string Categoria,string TipoVinculo,Guid ClubeOrigemId,string Matricula,bool Ativa);
record DependenteCadastro(string Nome,string Login,string Senha,string? Cpf,DateOnly? DataNascimento);
record GerarFaturas(Guid? ClubeId,string Competencia,DateOnly Vencimento,decimal? ValorPadrao);
