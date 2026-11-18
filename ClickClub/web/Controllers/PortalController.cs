using System.Globalization;
using System.Text.Json.Nodes;
using ClickClub.Web.Models;
using ClickClub.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using static ClickClub.Web.Models.PortalModel;
namespace ClickClub.Web.Controllers;
[Authorize]
public partial class PortalController(ApiClient api):Controller
{
 readonly Dictionary<string,TimeZoneInfo> _zones=new();readonly Dictionary<string,string> _spaceClubs=new();TimeZoneInfo _displayZone=TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
 TimeZoneInfo Zone(JsonNode? n){var club=Val(n,"clubeId");if(club==""&&_spaceClubs.TryGetValue(Val(n,"espacoId"),out var spaceClub))club=spaceClub;return _zones.GetValueOrDefault(club)??_displayZone;}
 static readonly HashSet<string> Pages=new("inicio clubes pessoas associacoes adesoes categorias espacos agenda reservas financeiro atividades eventos avisos chamados portaria carteirinha perfil relatorios auditoria".Split(' '));
 bool Has(params string[] roles)=>roles.Any(User.IsInRole);
 string UserId=>User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
 bool Admin=>Has("AdministradorGeral"); bool Staff=>Has("AdministradorGeral","Presidente","Funcionario"); bool Manager=>Has("AdministradorGeral","Presidente");bool Member=>Has("Titular","Dependente");
 [HttpGet("/")]public IActionResult Home()=>Redirect("/painel");
 [HttpGet("/painel/{page?}")]
 public async Task<IActionResult> Index(string page="inicio",string? id=null,string? espacoId=null,string? mes=null)
 {
  page=Pages.Contains(page)?page:"inicio";var m=new PortalModel{Page=page};
  if(HttpContext.Session.GetString("token") is null){await HttpContext.SignOutAsync();return Redirect("/conta/entrar");}
  try{
   var me=await api.Get("api/auth/eu");ViewBag.Me=me;
   var actualRoles=me["papeis"]!.AsArray().Select(x=>x!.ToString()).Order().ToArray();
   if(!actualRoles.SequenceEqual(User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(x=>x.Value).Order())||User.Identity!.Name!=Text(me,"nome")){
    var claims=new List<System.Security.Claims.Claim>{new(System.Security.Claims.ClaimTypes.NameIdentifier,Text(me,"id")),new(System.Security.Claims.ClaimTypes.Name,Text(me,"nome"))};claims.AddRange(actualRoles.Select(r=>new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role,r)));
    var principal=new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(claims,Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme));var existing=await HttpContext.AuthenticateAsync();await HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,principal,existing.Properties);HttpContext.User=principal;HttpContext.Session.SetString("usuario",me.ToJsonString());
   }
   var clubs=await List("api/clubes");ViewBag.Clubes=clubs;foreach(var club in clubs)_zones[Val(club,"id")]=TimeZoneInfo.FindSystemTimeZoneById(Text(club,"fusoHorario","America/Sao_Paulo"));
   switch(page){
    case "inicio": await Dashboard(m,me,clubs);break;
    case "clubes": Clubs(m,clubs);break;
    case "pessoas": await People(m,clubs,id);break;
    case "perfil": await Profile(m,UserId);break;
    case "associacoes": await Associations(m,clubs,id);break;
    case "adesoes": await Applications(m,clubs);break;
    case "categorias": await Categories(m);break;
    case "espacos": await Spaces(m,clubs,id);break;
    case "agenda": await Agenda(m,espacoId,mes);break;
    case "reservas": await Bookings(m,clubs,id);break;
    case "financeiro": await Finance(m,clubs,id);break;
    case "atividades": await Activities(m,clubs,id);break;
    case "eventos": await Events(m,clubs,id);break;
    case "avisos": await Notices(m,clubs);break;
    case "chamados": await Tickets(m,clubs,id);break;
    case "portaria": await Gate(m,clubs);break;
    case "carteirinha": Card(m);break;
    case "relatorios": await Reports(m,clubs,id);break;
    case "auditoria": await Audit(m);break;
   }
  }catch(ApiException e){if(e.Status==401){await HttpContext.SignOutAsync();HttpContext.Session.Clear();TempData["error"]=e.Message;return Redirect("/conta/entrar");}TempData["error"]=e.Message;}
  return View("Index",m);
 }
 async Task<List<JsonNode>> List(string path)=>(await api.Get(path)).AsArray().Where(x=>x is not null).Select(x=>x!).ToList();
 static List<Option> Options(IEnumerable<JsonNode> rows,string key="id",string label="nome")=>rows.Select(x=>new Option(Text(x,key),Text(x,label))).ToList();
 static Field F(string name,string label,string type="text",string? value=null,bool required=false,List<Option>? options=null,string? hint=null)=>new(name,label,type,value,required,options,hint);
 static Field Select(string name,string label,List<Option> options,string? value=null,bool required=true)=>F(name,label,"select",value,required,options);
 static List<Option> Choices(params string[] values)=>values.Select(x=>new Option(x,x)).ToList();
 static ActionForm Form(string op,string label,List<Field>? fields=null,string? id=null,string? confirm=null,string style="primary",Dictionary<string,string>? hidden=null)=>new(op,label,fields,id,confirm,style,hidden);
 static Cell C(string value,string? tone=null)=>new(value,tone);
 static Cell Status(JsonNode x,string key="status") {var value=Text(x,key);return C(value,value.Contains("Quitada")||value is "Confirmada" or "Aprovada" or "Concluido" or "Permitido"?"success":value is "Cancelada" or "Rejeitada"?"danger":"warning");}
 static string Money(JsonNode? n,string key="valor")=>decimal.TryParse(n?[key]?.ToString(),NumberStyles.Any,CultureInfo.InvariantCulture,out var v)?v.ToString("C",CultureInfo.GetCultureInfo("pt-BR")):"—";
 string When(JsonNode? n,string key)=>DateTimeOffset.TryParse(n?[key]?.ToString(),out var d)?TimeZoneInfo.ConvertTime(d,Zone(n)).ToString("dd/MM/yyyy HH:mm"):"—";
 static string Date(JsonNode? n,string key)=>DateOnly.TryParse(n?[key]?.ToString(),out var d)?d.ToString("dd/MM/yyyy"):"—";
 static string Val(JsonNode? n,string key)=>Text(n,key,"");
 static bool True(JsonNode n,string key)=>bool.TryParse(n[key]?.ToString(),out var result)&&result;
 static string Name(IEnumerable<JsonNode> list,string? id,string key="nome")=>Text(list.FirstOrDefault(x=>Text(x,"id")==id),key);
 string Local(JsonNode n,string key)=>DateTimeOffset.TryParse(Val(n,key),out var d)?TimeZoneInfo.ConvertTime(d,Zone(n)).ToString("yyyy-MM-ddTHH:mm"):"";
 static string Today=>DateTime.Today.ToString("yyyy-MM-dd");
 static string Future=>DateTime.Today.AddDays(7).AddHours(18).ToString("yyyy-MM-ddTHH:mm");
 static string FutureEnd=>DateTime.Today.AddDays(7).AddHours(22).ToString("yyyy-MM-ddTHH:mm");
 static string Who(JsonNode u)=>Text(u,"nome")+" · "+Text(u,"login");
 static List<Option> UserOptions(IEnumerable<JsonNode> rows)=>rows.Select(x=>new Option(Text(x,"id"),Who(x))).ToList();
 static string OnOff(JsonNode n,string key="ativa")=>True(n,key)?"Ativa":"Inativa";
 static List<Option> Roles=>Choices("AdministradorGeral","Presidente","Tesoureiro","Funcionario","Porteiro","Titular","Dependente");
 async Task<List<JsonNode>> Users()=>await List("api/usuarios");
 async Task<List<Option>> CategoriesOptions()=>Options(await List("api/categorias"),"name","name");
 async Task Dashboard(PortalModel m,JsonNode me,List<JsonNode> clubs)
 {
  m.Title="Olá, "+Text(me,"nome").Split(' ')[0]+".";m.Description="Acompanhe o movimento do clube e encontre sua próxima atividade.";
  var spaces=await List("api/espacos");var events=await List("api/eventos");var notices=await List("api/avisos");
  m.Stats.AddRange([new("Unidades",clubs.Count.ToString(),"Uma organização, vários encontros"),new("Espaços",spaces.Count(x=>True(x,"ativo")).ToString(),"Para esporte e convivência"),new("Eventos",events.Count(x=>Text(x,"status")=="Ativo").ToString(),"Experiências para compartilhar"),new("Avisos",notices.Count.ToString(),"Novidades da sua comunidade")]);
  m.Sections.Add(new("Acontece no clube","Fique por dentro da programação.",Table:new(["Evento","Unidade","Quando","Público"],events.Take(5).Select(e=>new Row([C(Text(e,"nome")),C(Name(clubs,Val(e,"clubeId"))),C(When(e,"inicio")),C(Text(e,"publicoAlvo"))],Link:"/painel/eventos?id="+Val(e,"id"),LinkLabel:"Ver evento")).ToList())));
  m.Sections.Add(new("Mural de avisos","Informações publicadas pela administração.",Table:new(["Título","Mensagem"],notices.Take(5).Select(e=>new Row([C(Text(e,"titulo")),C(Text(e,"corpo"))])).ToList())));
  if(Has("Candidato")){var apps=await List("api/adesoes/minhas");m.Sections.Add(new("Sua solicitação de adesão","O acesso aos serviços é liberado após a aprovação.",Table:new(["Unidade","Vínculo","Situação","Decisão"],apps.Select(a=>new Row([C(Name(clubs,Val(a,"clubeId"))),C(Text(a,"tipoVinculo")),Status(a),C(Text(a,"motivo"))])).ToList())));}
 }
 void Clubs(PortalModel m,List<JsonNode> clubs)
 {
  m.Title="Clubes e unidades";m.Description="Cada unidade tem seus espaços, equipe e programação.";
  foreach(var club in clubs){var id=Val(club,"id");m.Sections.Add(new(Text(club,"nome"),Text(club,"municipio")+" / "+Text(club,"uf"),Forms:Admin?[ClubForm(club)]:null,Table:new(["Sigla","Endereço","Contato","Fuso","Situação"],[new([C(Text(club,"sigla")),C(Text(club,"endereco")),C(Text(club,"telefone")),C(Text(club,"fusoHorario")),C(OnOff(club,"ativo"))])])));}
  if(Admin)m.Sections.Add(new("Nova unidade",Forms:[ClubForm(null)]));
 }
 ActionForm ClubForm(JsonNode? c)=>Form("clube-salvar",c is null?"Cadastrar unidade":"Salvar unidade",[F("nome","Nome",value:c is null?null:Val(c,"nome"),required:true),F("sigla","Sigla",value:c is null?null:Val(c,"sigla"),required:true),F("endereco","Endereço",value:c is null?null:Val(c,"endereco")),F("bairro","Bairro",value:c is null?null:Val(c,"bairro")),F("municipio","Município",value:c is null?null:Val(c,"municipio")),F("uf","UF",value:c is null?null:Val(c,"uf")),F("telefone","Telefone",value:c is null?null:Val(c,"telefone")),F("email","E-mail","email",c is null?null:Val(c,"email")),F("fusoHorario","Fuso horário",value:c is null?"America/Sao_Paulo":Val(c,"fusoHorario"),required:true),F("ativo","Unidade ativa","checkbox",c is null||True(c,"ativo")?"true":"false")],c is null?null:Val(c,"id"));
 async Task People(PortalModel m,List<JsonNode> clubs,string? id)
 {
  m.Title="Pessoas e equipe";m.Description="Gerencie contas, dados cadastrais e atribuições por unidade.";var users=await Users();
  if(id is not null){var u=users.FirstOrDefault(x=>Val(x,"id")==id);if(u is not null){await Profile(m,id);m.Title=Text(u,"nome");m.Sections.Add(new("Redefinição de senha",Forms:[Form("senha-redefinir","Redefinir senha desta conta",[F("senha","Nova senha","password",required:true,hint:"Mínimo de 8 caracteres. Informe a nova senha à pessoa com segurança.")],id)]));m.Sections.Add(new("Conta de acesso",Forms:[Form("usuario-editar","Salvar conta",[F("nome","Nome",value:Val(u,"nome"),required:true),F("login","Usuário",value:Val(u,"login"),required:true)],id),Form("usuario-ativo",True(u,"ativo")?"Desativar conta":"Ativar conta",id:id,confirm:"Confirmar mudança da situação desta conta?",hidden:new(){{"ativo",True(u,"ativo")?"false":"true"}})]));if(Admin){m.Sections.Add(new("Atribuições por unidade",Forms:[Form("atribuicao-adicionar","Adicionar atribuição",[Select("papel","Perfil",Roles),Select("clubeId","Unidade",Options(clubs))],id)],Table:new(["Perfil","Unidade"],(u["atribuicoes"]?.AsArray()??new()).Select(a=>new Row([C(Text(a,"papel")),C(Name(clubs,Val(a,"clubeId")))])).ToList())));}return;}}
  m.Sections.Add(new("Contas cadastradas",Table:new(["Pessoa","Usuário","Perfis","Situação"],users.Select(u=>new Row([C(Text(u,"nome")),C(Text(u,"login")),C(string.Join(", ",u["papeis"]?.AsArray().Select(x=>x?.ToString())??[])),C(OnOff(u,"ativo"))],Link:"/painel/pessoas?id="+Val(u,"id"),LinkLabel:"Gerenciar")).ToList())));
  m.Sections.Add(new("Criar conta",Forms:[Form("usuario-criar","Cadastrar pessoa",[F("nome","Nome completo",required:true),F("login","Usuário",required:true),F("senha","Senha inicial","password",required:true,hint:"Mínimo de 8 caracteres."),Select("papel","Perfil",Admin?Roles:Roles.Where(r=>r.Value!="AdministradorGeral").ToList()),Select("clubeId","Unidade",Options(clubs))])]));
 }
 async Task Profile(PortalModel m,string id)
 {
  m.Title="Meu cadastro";m.Description="Mantenha seus dados atualizados para aproveitar o clube.";var response=await api.Get("api/usuarios/"+id+"/dados");var d=response["dados"]!;
  m.Sections.Add(new("Dados pessoais",Forms:[Form("dados-salvar","Salvar meus dados",[F("cpf","CPF",value:Val(d,"cpf")),F("dataNascimento","Nascimento","date",Val(d,"dataNascimento")),F("email","E-mail","email",Val(d,"email")),F("telefone","Telefone",value:Val(d,"telefone")),F("endereco","Endereço",value:Val(d,"endereco")),F("bairro","Bairro",value:Val(d,"bairro")),F("cidade","Cidade",value:Val(d,"cidade")),F("uf","UF",value:Val(d,"uf")),F("sexo","Sexo",value:Val(d,"sexo")),F("estadoCivil","Estado civil",value:Val(d,"estadoCivil")),F("interesses","Interesses (separados por vírgula)",value:string.Join(", ",d["interesses"]?.AsArray().Select(x=>x?.ToString())??[]))],id)]));
  if(id==UserId)m.Sections.Add(new("Segurança da conta",Forms:[Form("senha-alterar","Alterar senha",[F("senhaAtual","Senha atual","password",required:true),F("novaSenha","Nova senha","password",required:true,hint:"Mínimo de 8 caracteres.")])]));
 }
 async Task Associations(PortalModel m,List<JsonNode> clubs,string? id)
 {
  m.Title="Associações e dependentes";m.Description="Um vínculo associativo reúne o titular e sua família.";var members=await List("api/associacoes");var users=Manager?await Users():new List<JsonNode>();
  m.Sections.Add(new("Vínculos associativos",Table:new(["Matrícula","Titular","Unidade de origem","Categoria","Vínculo","Situação"],members.Select(a=>new Row([C(Text(a,"matricula")),C(Name(users,Val(a,"usuarioTitularId"))),C(Name(clubs,Val(a,"clubeOrigemId"))),C(Text(a,"categoria")),C(Text(a,"tipoVinculo")),C(OnOff(a))],Link:"/painel/associacoes?id="+Val(a,"id"),LinkLabel:"Detalhes")).ToList())));
  var cat=await CategoriesOptions();
  if(Manager)m.Sections.Add(new("Cadastrar associação",Forms:[Form("associacao-criar","Criar associação",[Select("usuarioTitularId","Titular",UserOptions(users.Where(u=>u["papeis"]!.AsArray().Any(p=>p!.ToString()=="Titular")))),Select("clubeOrigemId","Unidade de origem",Options(clubs)),F("matricula","Matrícula",required:true),Select("categoria","Categoria",cat),Select("tipoVinculo","Tipo de vínculo",Choices("Associado/Fundador","Convidado","Simpatizante"))])]));
  var a=members.FirstOrDefault(x=>Val(x,"id")==id);if(a is null)return;
  var deps=await List($"api/associacoes/{id}/dependentes");
  m.Sections.Add(new("Dependentes · "+Text(a,"matricula"),Table:new(["Nome","Usuário","Situação"],deps.Select(d=>new Row([C(Text(d,"nome")),C(Text(d,"login")),C(OnOff(d,"ativo"))],Actions:Manager||Has("Titular")?[Form("dependente-remover","Desvincular",id:id,confirm:"Desvincular este dependente?",style:"danger",hidden:new(){{"dependenteId",Val(d,"id")}})]:null)).ToList())));
  if(Manager||Has("Titular"))m.Sections.Add(new("Cadastrar novo dependente",Forms:[Form("dependente-cadastrar","Cadastrar e vincular dependente",[F("nome","Nome completo",required:true),F("login","Usuário",required:true),F("senha","Senha inicial","password",required:true),F("cpf","CPF (opcional)"),F("dataNascimento","Nascimento","date")],id)]));
  if(Manager||Has("Titular"))m.Sections.Add(new("Vincular dependente",Forms:[Form("dependente-vincular","Vincular dependente",Manager?[Select("usuarioId","Conta de dependente",UserOptions(users.Where(u=>u["papeis"]!.AsArray().Any(p=>p!.ToString()=="Dependente"))))]:[F("usuarioId","Identificador da conta do dependente",required:true,hint:"A administração cria a conta e informa o identificador.")],id)]));
  if(Manager)m.Sections.Add(new("Editar vínculo",Forms:[Form("associacao-editar","Salvar vínculo",[F("matricula","Matrícula",value:Val(a,"matricula"),required:true),Select("categoria","Categoria",cat,Val(a,"categoria")),Select("tipoVinculo","Tipo de vínculo",Choices("Associado/Fundador","Convidado","Simpatizante"),Val(a,"tipoVinculo")),Select("clubeOrigemId","Unidade de origem",Options(clubs),Val(a,"clubeOrigemId")),F("ativa","Associação ativa","checkbox",True(a,"ativa")?"true":"false")],id)]));
  if(Has("Titular")){var auths=await List($"api/associacoes/{id}/autorizacoes");m.Sections.Add(new("Autorizações já registradas",Table:new(["Código","Espaço","Utilizada"],auths.Select(x=>new Row([C(Text(x,"id")),C(Text(x,"espacoId")),C(True(x,"usada")?"Sim":"Não")])).ToList())));var spaces=await List("api/espacos");m.Sections.Add(new("Autorizar locação por dependente",Forms:[Form("dependente-autorizar","Registrar autorização",[Select("dependenteId","Dependente",Options(deps)),Select("espacoId","Espaço",Options(spaces))],id)]));}
 }
 async Task Applications(PortalModel m,List<JsonNode> clubs)
 {
  m.Title="Solicitações de adesão";m.Description="Acompanhe pedidos e a entrada de novos membros.";var apps=await List(Manager?"api/adesoes":"api/adesoes/minhas");var cats=await CategoriesOptions();
  m.Sections.Add(new("Pedidos recebidos",Table:new(["Nome","Unidade","Vínculo","Solicitada em","Situação"],apps.Select(a=>new Row([C(Text(a,"nome")),C(Name(clubs,Val(a,"clubeId"))),C(Text(a,"tipoVinculo")),C(When(a,"solicitadaEm")),Status(a)],Manager&&Val(a,"status")=="Pendente"?[Form("adesao-decidir","Registrar decisão",[Select("status","Decisão",Choices("Aprovada","Rejeitada")),Select("categoria","Categoria",cats),F("matricula","Matrícula (para aprovação)"),F("motivo","Observação","textarea"),F("usuarioId","Conta do titular (somente pedido sem conta)")],Val(a,"id"))]:null)).ToList())));
 }
 async Task Categories(PortalModel m)
 {
  m.Title="Categorias associativas";m.Description="Benefícios e mensalidades são diferentes dos perfis de acesso.";var cats=await List("api/categorias");
  foreach(var c in cats)m.Sections.Add(new(Text(c,"name"),Text(c,"beneficios"),Forms:Admin?[Form("categoria-salvar","Salvar categoria",[F("name","Nome",value:Val(c,"name"),required:true),F("valorMensal","Mensalidade base","number",Val(c,"valorMensal")),F("limiteDependentes","Limite de dependentes","number",Val(c,"limiteDependentes")),F("beneficios","Benefícios","textarea",Val(c,"beneficios")),F("ativa","Categoria ativa","checkbox",True(c,"ativa")?"true":"false")])]:null,Table:new(["Mensalidade","Limite de dependentes","Situação"],[new([C(Money(c,"valorMensal")),C(Text(c,"limiteDependentes")),C(OnOff(c))])])));
  if(Admin)m.Sections.Add(new("Nova categoria",Forms:[Form("categoria-salvar","Criar categoria",[F("name","Nome",required:true),F("valorMensal","Mensalidade base","number"),F("limiteDependentes","Limite de dependentes","number"),F("beneficios","Benefícios","textarea"),F("ativa","Categoria ativa","checkbox","true")])]));
 }
}
