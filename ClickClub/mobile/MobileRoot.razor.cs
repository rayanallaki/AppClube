using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Components;

namespace ClickClub.Mobile;

public partial class MobileRoot
{
    bool initializing=true,busy,offline,agendaChecked;
    string page="login",reservationTab="mine",error="",message="",modalError="",modalInfo="",modalKind="",saveLabel="Salvar";
    string? modal;
    JsonObject? association, selected;
    CardView? card;
    JsonArray clubs=new(),spaces=new(),reservations=new(),courses=new(),events=new(),invoices=new(),notices=new(),tickets=new(),dependents=new(),applications=new(),agenda=new(),authorizations=new();
    Dictionary<string,string> form=new();
    List<Choice> detailLines=new();
    Func<Task>? saveAction;
    readonly CultureInfo br=CultureInfo.GetCultureInfo("pt-BR");
    static readonly List<Choice> LinkOptions=[new("Convidado","Convidado"),new("Simpatizante","Simpatizante")];
    static readonly List<Choice> TicketOptions=[new("Solicitacao","Solicitação"),new("Reclamacao","Reclamação")];
    List<Choice> ClubOptions=>clubs.Select(c=>new Choice(c.Text("id"),c.Text("nome"))).ToList();
    List<Choice> SpaceOptions=>spaces.Where(s=>s.Flag("ativo")).Select(s=>new Choice(s.Text("id"),s.Text("nome")+" · "+ClubName(s.Text("clubeId")))).ToList();
    List<Choice> AuthorizationOptions=>authorizations.Where(a=>!a.Flag("usada") && a.Text("espacoId")==selected.Text("id")).Select(a=>new Choice(a.Text("id"),"Autorização do titular · "+a.Text("espaco"))).ToList();
    string AssociationId=>association?["associacao"].Text("id")??"";

    protected override async Task OnInitializedAsync()
    {
        if(await Api.ResumeAsync())await Navigate("home");
        else ShowLogin();
        initializing=false;
    }

    void ShowLogin(){page="login";form=new();error="";}
    void OpenSettings(){page="config";form=new(){{"server",Api.ApiUrl}};error="";}
    async Task SaveConfiguration()=>await Run(async()=>{await Api.ConfigureUrlAsync(Get("server"));ShowLogin();message="Conexão configurada. Entre com sua conta.";});
    async Task ShowSignup()=>await Run(async()=>{clubs=(await Api.GetAsync("/api/publico/clubes",false,false)).Array();page="signup";form=new(){{"tipoVinculo","Convidado"}};});
    async Task Login()=>await Run(async()=>{await Api.LoginAsync(Get("login"),Get("senha"));form.Clear();await Navigate("home");});
    async Task SignUp()=>await Run(async()=>
    {
        if(Get("senha").Length<8)throw new ApiException("Escolha uma senha com pelo menos 8 caracteres.");
        var login=Get("login");var password=Get("senha");
        await Api.SendAsync(HttpMethod.Post,"/api/adesoes",new{clubeId=Guid.Parse(Get("clubeId")),nome=Get("nome"),email=Get("email"),tipoVinculo=Get("tipoVinculo"),login,senha=password},false);
        await Api.LoginAsync(login,password);form.Clear();await Navigate("applications");message="Pedido enviado! Acompanhe a análise por aqui.";
    });

    async Task Navigate(string destination)
    {
        page=destination;error="";message="";offline=false;
        await Run(async()=>
        {
            if(Api.SessionExpired){offline=true;if(destination=="home"){message="Sua sessão expirou. A carteirinha local continua disponível até sua validade. Saia e entre para sincronizar.";return;}}
            if(destination=="card"){await LoadCardCore();return;}
            if(clubs.Count==0)clubs=await Read("/api/clubes");
            switch(destination)
            {
                case "home":
                    notices=await Read("/api/avisos");
                    if(!Api.Candidate)await ReadAssociation();
                    break;
                case "reservations":
                    await ReadAssociation();spaces=await Read("/api/espacos");reservations=await Read("/api/reservas");if(Api.Dependent)authorizations=await Read("/api/minhas-autorizacoes");break;
                case "courses": courses=await Read("/api/turmas");break;
                case "events": events=await Read("/api/eventos");break;
                case "invoices":
                    if(Api.Dependent)throw new ApiException("O financeiro é reservado ao titular da associação.",403);
                    invoices=await Read("/api/faturas");break;
                case "notices":notices=await Read("/api/avisos");break;
                case "tickets":tickets=await Read("/api/chamados");break;
                case "family":
                    await ReadAssociation();spaces=await Read("/api/espacos");
                    dependents=await Read($"/api/associacoes/{AssociationId}/dependentes");break;
                case "applications":applications=await Read("/api/adesoes/minhas");break;
            }
        });
    }

    async Task<JsonArray> Read(string path){var value=(await Api.GetAsync(path)).Array();offline|=Api.LastReadOffline;return value;}
    async Task ReadAssociation(){association=(await Api.GetAsync("/api/minha-associacao")).AsObject();offline|=Api.LastReadOffline;}
    async Task LoadCard()=>await Run(LoadCardCore);
    async Task LoadCardCore(){card=await Api.GetCardAsync();offline=card.Offline;}

    void OpenModal(string title,string kind,Func<Task> action,string info="",string label="Salvar")
    {modal=title;modalKind=kind;modalInfo=info;modalError="";saveAction=action;saveLabel=label;form=new();}
    void CloseModal(){modal=null;modalError="";saveAction=null;form=new();}
    async Task SaveModal()
    {
        if(saveAction is null||busy)return;
        busy=true;modalError="";
        try{await saveAction();}
        catch(Exception e){modalError=Friendly(e);}
        finally{busy=false;}
    }
    async Task Run(Func<Task> action)
    {
        var previous=busy;busy=true;error="";
        try{await action();}
        catch(Exception e){error=Friendly(e);if(!Api.SignedIn)page="login";}
        finally{busy=previous;}
    }
    static string Friendly(Exception e)=>e switch
    {
        ApiException=>e.Message,
        FormatException=>"Confira os campos: data, número ou identificação está em formato inválido.",
        _=>"Não foi possível concluir esta operação. Verifique sua conexão e tente novamente."
    };
    void Confirm(string title,string info,Func<Task> action)=>OpenModal(title,"confirm",async()=>{await action();CloseModal();},info,"Confirmar");
    async Task Mutate(string path,object data){await Api.PostAsync(path,data);CloseModal();await Navigate(page);message="Operação concluída.";}

    void OpenBooking(JsonNode? item)
    {
        selected=item!.DeepClone().AsObject();
        OpenModal("Reservar "+selected.Text("nome"),"booking",async()=>
        {
            var authorization=Get("autorizacaoId");
            await Api.PostAsync("/api/reservas",new{associacaoId=Guid.Parse(AssociationId),espacoId=Guid.Parse(selected.Text("id")),inicio=Instant(Get("inicio")),fim=Instant(Get("fim")),finalidade=Get("finalidade"),convidados=ParseGuests(Get("convidados")),autorizacaoId=string.IsNullOrWhiteSpace(authorization)?(Guid?)null:Guid.Parse(authorization)});
            CloseModal();reservationTab="mine";await Navigate("reservations");message="Reserva confirmada. Seu próximo encontro já tem lugar!";
        },$"{ClubName(selected.Text("clubeId"))} · {selected.Text("capacidade")} pessoas · taxa {Money(selected.Text("taxa"))}. Horários no fuso deste aparelho. Salão exige ao menos 24h de antecedência.","Confirmar reserva");
        var start=DateTime.Now.Date.AddDays(3).AddHours(18);
        if(Api.Dependent && AuthorizationOptions.Count>0)form["autorizacaoId"]=AuthorizationOptions[0].Value;
        form["inicio"]=start.ToString("yyyy-MM-ddTHH:mm");form["fim"]=start.AddHours(3).ToString("yyyy-MM-ddTHH:mm");
    }

    void OpenAgenda(JsonNode? item)
    {
        selected=item!.DeepClone().AsObject();agenda=new();agendaChecked=false;
        OpenModal("Agenda · "+selected.Text("nome"),"agenda",async()=>
        {
            var day=DateTime.Parse(Get("dia"),CultureInfo.InvariantCulture);
            var from=new DateTimeOffset(DateTime.SpecifyKind(day,DateTimeKind.Local));
            var path=$"/api/agenda?espacoId={selected.Text("id")}&inicio={Uri.EscapeDataString(from.ToString("O"))}&fim={Uri.EscapeDataString(from.AddDays(1).ToString("O"))}";
            agenda=(await Api.GetAsync(path,false)).Array();agendaChecked=true;
        },"A agenda mostra as reservas confirmadas. Manutenção e regras também são conferidas pela API no momento da confirmação.","Consultar agenda");
        form["dia"]=DateTime.Today.ToString("yyyy-MM-dd");
    }

    async Task OpenGuests(JsonNode? reservation)=>await Run(async()=>
    {
        selected=reservation!.DeepClone().AsObject();
        var guests=(await Api.GetAsync($"/api/reservas/{selected.Text("id")}/convidados")).Array();
        OpenModal("Convidados da reserva","guests",async()=>
        {
            await Api.PutAsync($"/api/reservas/{selected.Text("id")}/convidados",ParseGuests(Get("convidados")));
            CloseModal();message="Lista de convidados atualizada.";
        },"A edição é permitida somente até 24 horas antes do início. A portaria confere a identificação online.");
        form["convidados"]=string.Join("\n",guests.Select(x=>x.Text("nome")+"; "+x.Text("identificacao")));
    });

    async Task OpenTransfer(JsonNode? reservation)=>await Run(async()=>
    {
        var id=reservation.Text("id");
        var target=await Api.PostAsync($"/api/reservas/{id}/preparar-transferencia");
        Confirm("Transferir para mensalidade?",$"Competência {target.Text("competencia")} · vencimento {DateOnly(target.Text("vencimento"))}. Principal {Money(target.Text("principal"))} + acréscimo único {Money(target.Text("acrescimo"))} = {Money(target.Text("total"))}. Pagamento simulado.",()=>Mutate($"/api/reservas/{id}/transferir",new{faturaId=Guid.Parse(target.Text("id"))}));
    });

    async Task Register(JsonNode? item)
    {
        var path=page=="courses"?"turmas":"eventos";
        Confirm("Participar de "+item.Text("nome")+"?","A API verificará seu vínculo, os horários e a disponibilidade de vagas.",async()=>
        {
            var result=await Api.PostAsync($"/api/{path}/{item.Text("id")}/inscricoes");
            CloseModal();message=result.Text("status")=="Espera"?"Turma lotada: você entrou na lista de espera.":"Inscrição confirmada. Esperamos você!";
        });await Task.CompletedTask;
    }

    async Task ViewEnrollment(JsonNode? item)=>await Run(async()=>
    {
        var path=page=="courses"?"turmas":"eventos";
        var entries=(await Api.GetAsync($"/api/{path}/{item.Text("id")}/inscricoes",false)).Array();
        var mine=entries.LastOrDefault(x=>x.Text("usuarioId")==Api.UserId && x.Text("status")!="Cancelada");
        if(mine is null){message="Você ainda não tem uma inscrição ativa nesta atividade.";return;}
        if(path=="turmas")Confirm("Minha inscrição: "+mine.Text("status"),"Você pode cancelar sua inscrição para liberar a vaga.",async()=>{await Api.PostAsync($"/api/inscricoes/{mine.Text("id")}/cancelar");CloseModal();message="Inscrição cancelada.";});
        else{OpenModal("Minha inscrição","details",()=>Task.CompletedTask,"Inscrição "+mine.Text("status"));detailLines=[new(item.Text("nome"),"Evento"),new(mine.Text("status"),"Situação")];}
    });

    async Task InvoiceDetails(JsonNode? invoice)=>await Run(async()=>
    {
        var result=await Api.GetAsync($"/api/faturas/{invoice.Text("id")}/detalhes");
        detailLines=result["itens"].Array().Select(x=>new Choice(x.Text("descricao"),Money(x.Text("valor")))).ToList();
        detailLines.Add(new("Total",Money(invoice.Text("valor"))));
        OpenModal("Composição da mensalidade","details",()=>Task.CompletedTask,"Competência "+invoice.Text("competencia")+" · valores simulados.");
    });

    void OpenTicket()=>OpenModal("Novo atendimento","ticket",async()=>
    {
        var club=association?["associacao"].Text("clubeOrigemId");
        await Api.PostAsync("/api/chamados",new{tipo=Get("tipo"),assunto=Get("assunto"),descricao=Get("descricao"),clubeId=Guid.TryParse(club,out var id)?(Guid?)id:null});
        CloseModal();await Navigate("tickets");message="Atendimento enviado. Acompanhe a resposta aqui.";
    },label:"Enviar atendimento");
    void OpenReply(JsonNode? ticket)=>OpenModal("Responder atendimento","reply",async()=>{await Api.PostAsync($"/api/chamados/{ticket.Text("id")}/respostas",new{texto=Get("texto")});CloseModal();await Navigate("tickets");message="Resposta enviada.";});
    void OpenDependent()=>OpenModal("Novo dependente","dependent",async()=>
    {
        await Api.PostAsync($"/api/associacoes/{AssociationId}/dependentes/cadastro",new{nome=Get("nome"),login=Get("login"),senha=Get("senha"),cpf=Null("cpf"),dataNascimento=Null("dataNascimento")});
        CloseModal();await Navigate("family");message="Dependente cadastrado e vinculado à associação.";
    });
    async Task DeleteDependent(JsonNode? dependent){await Api.DeleteAsync($"/api/associacoes/{AssociationId}/dependentes/{dependent.Text("id")}");CloseModal();await Navigate("family");message="Dependente desvinculado.";}
    void OpenAuthorization(JsonNode? dependent)=>OpenModal("Autorizar locação","authorization",async()=>
    {
        var result=await Api.PostAsync($"/api/associacoes/{AssociationId}/autorizacoes",new{dependenteId=Guid.Parse(dependent.Text("id")),espacoId=Guid.Parse(Get("espacoId"))});
        var code=result.Text("id");CloseModal();OpenModal("Autorização registrada","details",()=>Task.CompletedTask,"Informe o código ao dependente. Ele deverá usar este código ao confirmar a reserva paga.");detailLines=[new("Código",code)];
    },"Autorize o uso de um espaço pago pelo dependente. A autorização só vale para o espaço selecionado.");

    async Task OpenProfile()=>await Run(async()=>
    {
        var profile=(await Api.GetAsync($"/api/usuarios/{Api.UserId}/dados"))["dados"]?.AsObject()??new();
        OpenModal("Meus dados","profile",async()=>
        {
            await Api.PutAsync($"/api/usuarios/{Api.UserId}/dados",new{cpf=Null("cpf"),dataNascimento=Null("dataNascimento"),email=Null("email"),telefone=Null("telefone"),endereco=Null("endereco"),bairro=Null("bairro"),cidade=Null("cidade"),uf=Null("uf"),sexo=Null("sexo"),estadoCivil=Null("estadoCivil"),interesses=Get("interesses").Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries)});
            CloseModal();message="Seus dados foram atualizados.";
        });
        foreach(var item in profile)form[item.Key]=item.Value?.ToString()??"";
        form["interesses"]=string.Join(", ",profile["interesses"].Array().Select(x=>x?.ToString()));
    });
    async Task ClearCache()=>await Run(async()=>{await Api.ClearCacheAsync();message="Dados offline removidos. Abra a carteirinha com internet para guardá-la novamente.";});
    async Task Logout(){await Api.LogoutAsync();CloseModal();clubs=new();association=null;card=null;ShowLogin();message="Você saiu da conta. Até a próxima!";}
    string Get(string name)=>form.GetValueOrDefault(name,"").Trim();
    string? Null(string name)=>string.IsNullOrWhiteSpace(Get(name))?null:Get(name);
    static DateTimeOffset Instant(string value)=>new(DateTime.SpecifyKind(DateTime.Parse(value,CultureInfo.InvariantCulture),DateTimeKind.Local));
    static object[] ParseGuests(string value)=>value.Split('\n',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Select(line=>
    {
        var parts=line.Split(';',2,StringSplitOptions.TrimEntries);
        if(parts.Length!=2||parts.Any(string.IsNullOrWhiteSpace))throw new ApiException("Informe cada convidado no formato Nome; identificação.");
        return (object)new{nome=parts[0],identificacao=parts[1]};
    }).ToArray();
    string ClubName(string id)=>string.IsNullOrEmpty(id)||id=="null"?"Todos os clubes":clubs.FirstOrDefault(c=>c.Text("id")==id).Text("nome","Clube");
    string SpaceName(string id)=>spaces.FirstOrDefault(s=>s.Text("id")==id).Text("nome","Espaço do clube");
    string Money(string value)=>decimal.TryParse(value,NumberStyles.Any,CultureInfo.InvariantCulture,out var n)?n.ToString("C",br):"R$ 0,00";
    string Date(string value)=>DateTimeOffset.TryParse(value,out var d)?d.ToLocalTime().ToString("dd/MM/yyyy 'às' HH:mm",br):"—";
    string DateOnly(string value)=>System.DateOnly.TryParse(value,out var d)?d.ToString("dd/MM/yyyy",br):"—";
    static string Day(string value)=>DateTimeOffset.TryParse(value,out var d)?d.Day.ToString("00"):"—";
    string Month(string value)=>DateTimeOffset.TryParse(value,out var d)?d.ToString("MMM",br).ToUpperInvariant():"";
    static string FirstName(string value)=>value.Split(' ',StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()??"";
    static string Initials(string value)=>string.Concat(value.Split(' ',StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x=>x[0])).ToUpperInvariant();
    static string SpaceColor(JsonNode? s)=>s.Text("tipo") switch{"Quadra"=>"mint","Churrasqueira"=>"peach",_=>"lavender"};
    static string SpaceIcon(JsonNode? s)=>s.Text("tipo") switch{"Quadra"=>"◉","Churrasqueira"=>"♧",_=>"◇"};
}
