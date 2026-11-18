partial class Store
{
    // Apenas executado junto do seed de um banco vazio. Nunca reinicializa dados existentes.
    void SeedApresentacao(Clube origem)
    {
        origem.Nome="ClickClub Foz do Iguaçu";origem.Endereco="Av. das Comunidades, 1200";origem.Bairro="Centro";origem.Telefone="(45) 3000-1000";origem.Email="foz@example.com";
        var guaira=new Clube{Id=Guid.Parse("11111111-1111-1111-1111-111111111112"),Nome="ClickClub Guaíra",Sigla="GUA",Municipio="Guaíra",Uf="PR",Bairro="Centro",Endereco="Rua do Encontro, 450",Email="guaira@example.com",Telefone="(44) 3000-2000"};Clubs[guaira.Id]=guaira;
        var curitiba=new Clube{Id=Guid.Parse("11111111-1111-1111-1111-111111111113"),Nome="ClickClub Curitiba",Sigla="CWB",Municipio="Curitiba",Uf="PR",Bairro="Jardim Social",Endereco="Alameda dos Esportes, 85",Email="curitiba@example.com",Telefone="(41) 3000-3000"};Clubs[curitiba.Id]=curitiba;
        var sala=Spaces[Guid.Parse("33333333-3333-3333-3333-333333333333")];sala.Nome="Salão Ipê";sala.Capacidade=80;
        foreach(var e in new[]{
            new Espaco{Id=Guid.Parse("33333333-3333-3333-3333-333333333334"),ClubeId=origem.Id,Nome="Churrasqueira Araucária",Tipo="Churrasqueira",Capacidade=20,Taxa=60},
            new Espaco{Id=Guid.Parse("33333333-3333-3333-3333-333333333335"),ClubeId=origem.Id,Nome="Quadra das Cataratas",Tipo="Quadra",Capacidade=20,Taxa=0},
            new Espaco{Id=Guid.Parse("33333333-3333-3333-3333-333333333336"),ClubeId=guaira.Id,Nome="Salão do Lago",Tipo="Salao",Capacidade=100,Taxa=180},
            new Espaco{Id=Guid.Parse("33333333-3333-3333-3333-333333333337"),ClubeId=guaira.Id,Nome="Quadra Paraná",Tipo="Quadra",Capacidade=20,Taxa=0},
            new Espaco{Id=Guid.Parse("33333333-3333-3333-3333-333333333338"),ClubeId=curitiba.Id,Nome="Espaço Jardim",Tipo="Churrasqueira",Capacidade=30,Taxa=90}
        })Spaces[e.Id]=e;
        var natacao=new Atividade{Id=Guid.Parse("44444444-4444-4444-4444-444444444441"),Nome="Natação",Descricao="Movimento e bem-estar na água",Tipo="Esportiva"};
        var tenis=new Atividade{Id=Guid.Parse("44444444-4444-4444-4444-444444444442"),Nome="Tênis",Descricao="Técnica e convivência nas quadras",Tipo="Esportiva"};
        Atividades.AddRange(new[]{natacao,tenis});
        var dia=DateTimeOffset.UtcNow.AddDays(14);
        foreach(var t in new[]{
            new Turma{Id=Guid.Parse("55555555-5555-5555-5555-555555555551"),ClubeId=origem.Id,Nome="Natação • Iniciantes",Descricao="Uma nova forma de cuidar de você.",Instrutor="Prof. Helena Martins",AtividadeId=natacao.Id,Inicio=dia,Fim=dia.AddHours(1),Capacidade=12},
            new Turma{Id=Guid.Parse("55555555-5555-5555-5555-555555555552"),ClubeId=guaira.Id,Nome="Tênis • Adultos",Descricao="Aprenda, pratique e conheça pessoas.",Instrutor="Prof. André Souza",AtividadeId=tenis.Id,Inicio=dia.AddDays(1),Fim=dia.AddDays(1).AddHours(1),Capacidade=8}
        })Turmas[t.Id]=t;
        Events.AddRange(new[]{
            new Evento{Id=Guid.Parse("66666666-6666-6666-6666-666666666661"),ClubeId=origem.Id,Nome="Encontro da comunidade",Descricao="Um fim de tarde para reunir famílias, conversar e celebrar o clube.",Inicio=dia.AddDays(2),Fim=dia.AddDays(2).AddHours(4),PublicoAlvo="PublicoGeral",Capacidade=100},
            new Evento{Id=Guid.Parse("66666666-6666-6666-6666-666666666662"),ClubeId=guaira.Id,Nome="Café com os associados",Descricao="Boas conversas e novidades do clube à mesa.",Inicio=dia.AddDays(4),Fim=dia.AddDays(4).AddHours(2),PublicoAlvo="SomenteAssociados",Capacidade=40}
        });
        Notices.AddRange(new[]{
            new Aviso{Id=Guid.NewGuid(),Titulo="Seu próximo encontro começa aqui",Corpo="Reserve espaços, acompanhe atividades e mantenha sua carteirinha sempre por perto."},
            new Aviso{Id=Guid.NewGuid(),ClubeId=origem.Id,Titulo="Agenda de espaços aberta",Corpo="Salão Ipê, churrasqueira e quadras já podem ser reservados. Consulte os horários e planeje seu encontro."},
            new Aviso{Id=Guid.NewGuid(),ClubeId=guaira.Id,Titulo="Novas turmas de tênis",Corpo="Encontre seu ritmo nas quadras. As inscrições estão disponíveis na área de atividades."}
        });
        Users.Values.First(x=>x.Login=="titular").Dados=new DadosPessoa{Cidade="Foz do Iguaçu",Uf="PR",Email="titular@example.com",Interesses=new(){"Esportes","Convivência"}};
    }
}
