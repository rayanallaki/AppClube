using static ClickClub.Web.Models.PortalModel;
using System.Globalization;
using System.Text.Json.Nodes;
using ClickClub.Web.Services;
using Microsoft.AspNetCore.Mvc;
namespace ClickClub.Web.Controllers;
public partial class PortalController
{
 [HttpPost("/painel/executar")]
 public async Task<IActionResult> Execute()
 {
  var f=await Request.ReadFormAsync();string V(string key)=>f[key].FirstOrDefault()?.Trim()??"";
  bool B(string key)=>f[key].Any(v=>string.Equals(v,"true",StringComparison.OrdinalIgnoreCase));
  decimal D(string key)=>decimal.TryParse(V(key),NumberStyles.Number,CultureInfo.InvariantCulture,out var d)?d:throw new ApiException(400,"Informe um valor numérico válido em "+key+".");
  decimal? DN(string key)=>string.IsNullOrWhiteSpace(V(key))?null:D(key);
  int N(string key)=>int.TryParse(V(key),out var n)?n:throw new ApiException(400,"Informe um número inteiro válido em "+key+".");
  int? NN(string key)=>string.IsNullOrWhiteSpace(V(key))?null:N(key);
  string? Nullable(string key)=>string.IsNullOrWhiteSpace(V(key))?null:V(key);
  var id=V("id");var page=Pages.Contains(V("page"))?V("page"):"inicio";string? selection=null;
  object? response=null;var op=V("operation");
  try{
   switch(op){
    case "agenda-filtrar":return Redirect("/painel/agenda?espacoId="+Uri.EscapeDataString(V("espacoId"))+"&mes="+Uri.EscapeDataString(V("mes")));
    case "relatorio-filtrar":return Redirect("/painel/relatorios?id="+Uri.EscapeDataString(V("clubeId")));
    case "clube-salvar": response=await api.Send(id==""?HttpMethod.Post:HttpMethod.Put,"api/clubes"+(id==""?"":"/"+id),new{nome=V("nome"),sigla=V("sigla"),endereco=V("endereco"),bairro=V("bairro"),municipio=V("municipio"),uf=V("uf"),telefone=V("telefone"),email=V("email"),fusoHorario=V("fusoHorario"),ativo=B("ativo")});break;
    case "usuario-criar":response=await api.Send(HttpMethod.Post,"api/usuarios",new{nome=V("nome"),login=V("login"),senha=V("senha"),papel=V("papel"),clubeId=V("clubeId")});break;
    case "senha-redefinir":response=await api.Send(HttpMethod.Post,$"api/usuarios/{id}/redefinir-senha",new{senha=V("senha")});selection=id;break;
    case "usuario-editar":response=await api.Send(HttpMethod.Put,$"api/usuarios/{id}",new{nome=V("nome"),login=V("login")});selection=id;break;
    case "usuario-ativo":response=await api.Send(HttpMethod.Put,$"api/usuarios/{id}/ativo",new{ativo=B("ativo")});selection=id;break;
    case "atribuicao-adicionar":{
     var u=(await Users()).First(x=>Val(x,"id")==id);var assignments=(u["atribuicoes"]?.AsArray()??new()).Where(a=>a is not null).Select(a=>new{papel=Val(a,"papel"),clubeId=Val(a,"clubeId")}).ToList();var next=new{papel=V("papel"),clubeId=V("clubeId")};if(!assignments.Contains(next))assignments.Add(next);response=await api.Send(HttpMethod.Put,$"api/usuarios/{id}/atribuicoes",assignments);selection=id;break;}
    case "dados-salvar":response=await api.Send(HttpMethod.Put,$"api/usuarios/{id}/dados",new{cpf=Nullable("cpf"),dataNascimento=Nullable("dataNascimento"),email=Nullable("email"),telefone=Nullable("telefone"),endereco=Nullable("endereco"),bairro=Nullable("bairro"),cidade=Nullable("cidade"),uf=Nullable("uf"),sexo=Nullable("sexo"),estadoCivil=Nullable("estadoCivil"),interesses=V("interesses").Split(',',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries)});if(page=="pessoas")selection=id;break;
    case "senha-alterar":response=await api.Send(HttpMethod.Post,"api/auth/alterar-senha",new{senhaAtual=V("senhaAtual"),novaSenha=V("novaSenha")});break;
    case "associacao-criar":response=await api.Send(HttpMethod.Post,"api/associacoes",new{usuarioTitularId=V("usuarioTitularId"),clubeOrigemId=V("clubeOrigemId"),matricula=V("matricula"),categoria=V("categoria"),tipoVinculo=V("tipoVinculo"),ativa=true});break;
    case "associacao-editar":response=await api.Send(HttpMethod.Put,$"api/associacoes/{id}",new{clubeOrigemId=V("clubeOrigemId"),matricula=V("matricula"),categoria=V("categoria"),tipoVinculo=V("tipoVinculo"),ativa=B("ativa")});selection=id;break;
    case "dependente-cadastrar":response=await api.Send(HttpMethod.Post,$"api/associacoes/{id}/dependentes/cadastro",new{nome=V("nome"),login=V("login"),senha=V("senha"),cpf=Nullable("cpf"),dataNascimento=Nullable("dataNascimento")});selection=id;break;
    case "dependente-vincular":response=await api.Send(HttpMethod.Post,$"api/associacoes/{id}/dependentes",new{usuarioId=V("usuarioId")});selection=id;break;
    case "dependente-remover":response=await api.Send(HttpMethod.Delete,$"api/associacoes/{id}/dependentes/{V("dependenteId")}");selection=id;break;
    case "dependente-autorizar":response=await api.Send(HttpMethod.Post,$"api/associacoes/{id}/autorizacoes",new{dependenteId=V("dependenteId"),espacoId=V("espacoId")});if(response is JsonNode auth)TempData["success"]="Autorização registrada: "+Val(auth,"id")+". Informe este código ao dependente.";selection=id;break;
    case "adesao-decidir":response=await api.Send(HttpMethod.Post,$"api/adesoes/{id}/decisao",new{status=V("status"),motivo=Nullable("motivo"),categoria=Nullable("categoria"),matricula=Nullable("matricula"),usuarioId=Nullable("usuarioId")});break;
    case "categoria-salvar":response=await api.Send(HttpMethod.Put,"api/categorias/"+Uri.EscapeDataString(V("name")),new{name=V("name"),valorMensal=DN("valorMensal"),limiteDependentes=NN("limiteDependentes"),beneficios=Nullable("beneficios"),ativa=B("ativa")});break;
    case "espaco-salvar":response=await api.Send(id==""?HttpMethod.Post:HttpMethod.Put,"api/espacos"+(id==""?"":"/"+id),new{nome=V("nome"),clubeId=V("clubeId"),tipo=V("tipo"),capacidade=N("capacidade"),taxa=D("taxa"),ativo=B("ativo")});selection=id;break;
    case "horario-adicionar":{
     var hours=await List($"api/espacos/{id}/horarios");var data=hours.Select(h=>new{diaSemana=int.Parse(Val(h,"diaSemana")),inicio=Val(h,"inicio"),fim=Val(h,"fim")}).ToList();data.Add(new{diaSemana=N("diaSemana"),inicio=V("inicio"),fim=V("fim")});response=await api.Send(HttpMethod.Put,$"api/espacos/{id}/horarios",data);selection=id;break;}
    case "horario-remover":{var hours=await List($"api/espacos/{id}/horarios");var data=hours.Where(h=>!(Val(h,"diaSemana")==V("diaSemana")&&Val(h,"inicio")==V("inicio")&&Val(h,"fim")==V("fim"))).Select(h=>new{diaSemana=int.Parse(Val(h,"diaSemana")),inicio=Val(h,"inicio"),fim=Val(h,"fim")});response=await api.Send(HttpMethod.Put,$"api/espacos/{id}/horarios",data);selection=id;break;}
    case "interdicao-criar":response=await api.Send(HttpMethod.Post,$"api/espacos/{id}/interdicoes",new{inicio=await Moment(V("inicio"),V("clubeId")),fim=await Moment(V("fim"),V("clubeId")),motivo=V("motivo")});selection=id;break;
    case "reserva-criar":{
     var space=(await List("api/espacos")).FirstOrDefault(s=>Val(s,"id")==V("espacoId"))??throw new ApiException(400,"Selecione um espaço válido.");response=await api.Send(HttpMethod.Post,"api/reservas",new{associacaoId=V("associacaoId"),espacoId=V("espacoId"),inicio=await Moment(V("inicio"),Val(space,"clubeId")),fim=await Moment(V("fim"),Val(space,"clubeId")),finalidade=V("finalidade"),convidados=Guests(V("convidados")),autorizacaoId=Nullable("autorizacaoId")});break;}
    case "reserva-quitar":response=await api.Send(HttpMethod.Post,$"api/reservas/{id}/quitar-taxa",new{});selection=id;break;
    case "reserva-cancelar":response=await api.Send(HttpMethod.Post,$"api/reservas/{id}/cancelar",new{});selection=id;HttpContext.Session.Remove("transferencia:"+id);break;
    case "reserva-preparar-transferencia":{var preview=await api.Send(HttpMethod.Post,$"api/reservas/{id}/preparar-transferencia",new{});HttpContext.Session.SetString("transferencia:"+id,preview.ToJsonString());TempData["success"]="Prévia carregada. Confira os valores e confirme a transferência abaixo.";selection=id;break;}
    case "reserva-transferir":response=await api.Send(HttpMethod.Post,$"api/reservas/{id}/transferir",new{faturaId=V("faturaId")});selection=id;HttpContext.Session.Remove("transferencia:"+id);break;
    case "convidados-salvar":response=await api.Send(HttpMethod.Put,$"api/reservas/{id}/convidados",Guests(V("convidados")));selection=id;break;
    case "fatura-criar":response=await api.Send(HttpMethod.Post,"api/faturas",new{associacaoId=V("associacaoId"),competencia=V("competencia"),vencimento=V("vencimento"),valor=D("valor")});break;
    case "faturas-gerar":{var result=await api.Send(HttpMethod.Post,"api/faturas/gerar",new{clubeId=Nullable("clubeId"),competencia=V("competencia"),vencimento=V("vencimento"),valorPadrao=DN("valorPadrao")});TempData["success"]=$"Mensalidades criadas: {Val(result,"criadas")}. Já existentes: {Val(result,"existentes")}.";break;}
    case "fatura-quitar":response=await api.Send(HttpMethod.Post,$"api/faturas/{id}/quitar",new{});break;
    case "credito-aplicar":response=await api.Send(HttpMethod.Post,$"api/creditos/{id}/aplicar",new{faturaId=V("faturaId")});break;
    case "atividade-criar":response=await api.Send(HttpMethod.Post,"api/atividades",new{nome=V("nome"),descricao=V("descricao"),tipo=V("tipo")});break;
    case "turma-criar":response=await api.Send(HttpMethod.Post,"api/turmas",new{nome=V("nome"),clubeId=V("clubeId"),inicio=await Moment(V("inicio"),V("clubeId")),fim=await Moment(V("fim"),V("clubeId")),capacidade=N("capacidade"),descricao=V("descricao"),instrutor=V("instrutor"),atividadeId=Nullable("atividadeId")});break;
    case "turma-editar":response=await api.Send(HttpMethod.Put,$"api/turmas/{id}",new{nome=V("nome"),clubeId=V("clubeId"),inicio=await Moment(V("inicio"),V("clubeId")),fim=await Moment(V("fim"),V("clubeId")),capacidade=N("capacidade"),descricao=V("descricao"),instrutor=V("instrutor"),atividadeId=Nullable("atividadeId"),ativa=B("ativa")});selection=id;break;
    case "turma-inscrever":response=await api.Send(HttpMethod.Post,$"api/turmas/{id}/inscricoes",new{});selection=id;break;
    case "turma-promover":response=await api.Send(HttpMethod.Post,$"api/turmas/{id}/promover",new{});selection=id;break;
    case "inscricao-cancelar":response=await api.Send(HttpMethod.Post,$"api/inscricoes/{id}/cancelar",new{});break;
    case "aula-adicionar":{var classes=await List($"api/turmas/{id}/aulas");var data=classes.Select(a=>new{inicio=Val(a,"inicio"),fim=Val(a,"fim")}).ToList();data.Add(new{inicio=(await Moment(V("inicio"),V("clubeId"))).ToString("o"),fim=(await Moment(V("fim"),V("clubeId"))).ToString("o")});response=await api.Send(HttpMethod.Put,$"api/turmas/{id}/aulas",data);selection=id;break;}
    case "evento-salvar":response=await api.Send(id==""?HttpMethod.Post:HttpMethod.Put,"api/eventos"+(id==""?"":"/"+id),new{nome=V("nome"),clubeId=V("clubeId"),descricao=V("descricao"),inicio=await Moment(V("inicio"),V("clubeId")),fim=await Moment(V("fim"),V("clubeId")),publicoAlvo=V("publicoAlvo"),capacidade=N("capacidade"),status=V("status")});selection=id;break;
    case "evento-inscricao-cancelar":response=await api.Send(HttpMethod.Post,$"api/eventos/inscricoes/{id}/cancelar",new{});break;
    case "evento-inscrever":response=await api.Send(HttpMethod.Post,$"api/eventos/{id}/inscricoes",new{});selection=id;break;
    case "visitante-criar":response=await api.Send(HttpMethod.Post,$"api/eventos/{id}/visitantes",new{nome=V("nome"),identificacao=V("identificacao")});selection=id;break;
    case "aviso-editar":response=await api.Send(HttpMethod.Put,$"api/avisos/{id}",new{titulo=V("titulo"),corpo=V("corpo"),clubeId=Nullable("clubeId")});break;
    case "aviso-remover":response=await api.Send(HttpMethod.Delete,$"api/avisos/{id}");break;
    case "aviso-criar":response=await api.Send(HttpMethod.Post,"api/avisos",new{titulo=V("titulo"),clubeId=Nullable("clubeId"),corpo=V("corpo")});break;
    case "chamado-criar":response=await api.Send(HttpMethod.Post,"api/chamados",new{assunto=V("assunto"),tipo=V("tipo"),clubeId=Nullable("clubeId"),descricao=V("descricao")});break;
    case "chamado-responder":response=await api.Send(HttpMethod.Post,$"api/chamados/{id}/respostas",new{texto=V("texto")});selection=id;break;
    case "chamado-status":response=await api.Send(HttpMethod.Put,$"api/chamados/{id}/status",new{status=V("status")});selection=id;break;
    case "carteirinha-renovar":{var card=await api.Send(HttpMethod.Post,"api/carteirinha",new{});HttpContext.Session.SetString("carteirinha",card.ToJsonString());break;}
    case "portaria-validar":await GateResult("api/portaria/validar",new{clubeId=V("clubeId"),credencial=V("credencial"),reservaId=Nullable("reservaId")});break;
    case "portaria-convidado":await GateResult("api/portaria/validar-convidado",new{clubeId=V("clubeId"),reservaId=V("reservaId"),identificacao=V("identificacao")});break;
    case "portaria-visitante":await GateResult("api/portaria/validar-visitante",new{clubeId=V("clubeId"),visitanteId=V("visitanteId"),identificacao=V("identificacao")});break;
    default:throw new ApiException(400,"Operação não reconhecida.");
   }
   if(!TempData.ContainsKey("success")&&!TempData.ContainsKey("error"))TempData["success"]="Operação concluída com sucesso.";
  }catch(ApiException e){TempData["error"]=e.Message;if(id!=""&&page is "pessoas" or "associacoes" or "espacos" or "reservas" or "eventos" or "chamados")selection=id;}
  catch(FormatException){TempData["error"]="Confira o formato dos valores e das datas informadas.";}
  return Redirect("/painel/"+page+(string.IsNullOrWhiteSpace(selection)?"":"?id="+Uri.EscapeDataString(selection)));
 }
 async Task<DateTimeOffset> Moment(string value,string clubId)
 {
  if(!DateTime.TryParseExact(value,["yyyy-MM-ddTHH:mm","yyyy-MM-ddTHH:mm:ss"],CultureInfo.InvariantCulture,DateTimeStyles.None,out var local))throw new ApiException(400,"Informe uma data e horário válidos.");
  var clubs=await List("api/clubes");var club=clubs.FirstOrDefault(c=>Val(c,"id")==clubId)??throw new ApiException(400,"Unidade não encontrada.");
  var zone=TimeZoneInfo.FindSystemTimeZoneById(Text(club,"fusoHorario","America/Sao_Paulo"));local=DateTime.SpecifyKind(local,DateTimeKind.Unspecified);
  if(zone.IsInvalidTime(local))throw new ApiException(400,"Esse horário não existe no fuso da unidade. Escolha outro horário.");
  return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local,zone));
 }
 static List<object> Guests(string value)
 {
  var guests=new List<object>();
  foreach(var line in value.Split('\n',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries)){
   var fields=line.Split('|',2,StringSplitOptions.TrimEntries);if(fields.Length!=2||fields.Any(string.IsNullOrWhiteSpace))throw new ApiException(400,"Informe os convidados no formato Nome | Identificação, um por linha.");
   guests.Add(new{nome=fields[0],identificacao=fields[1]});
  }
  return guests;
 }
 async Task GateResult(string path,object payload){var result=await api.Send(HttpMethod.Post,path,payload);var allowed=True(result,"permitido");TempData[allowed?"success":"error"]=(allowed?"ACESSO PERMITIDO · ":"ACESSO NEGADO · ")+Text(result,"motivo");}
}
