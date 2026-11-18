using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
namespace ClickClub.Web.Services;
public sealed class ApiException(int status,string message):Exception(message){public int Status{get;}=status;}
public sealed class ApiClient(HttpClient client,IHttpContextAccessor accessor)
{
 public async Task<JsonNode> Get(string path)=>await Send(HttpMethod.Get,path);
 public async Task<JsonNode> Send(HttpMethod method,string path,object? payload=null,bool anonymous=false)
 {
  using var request=new HttpRequestMessage(method,path.TrimStart('/'));
  if(!anonymous&&accessor.HttpContext?.Session.GetString("token") is string token) request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);
  if(payload is not null)request.Content=JsonContent.Create(payload);
  try{
   using var response=await client.SendAsync(request);
   var raw=await response.Content.ReadAsStringAsync();
   JsonNode? value=null;try{if(!string.IsNullOrWhiteSpace(raw))value=JsonNode.Parse(raw);}catch(System.Text.Json.JsonException){}
   if(!response.IsSuccessStatusCode)throw new ApiException((int)response.StatusCode,value?["erro"]?.ToString()??(response.StatusCode switch{HttpStatusCode.Unauthorized=>"Sua sessão expirou. Entre novamente.",HttpStatusCode.Forbidden=>"Seu perfil não tem permissão para esta operação.",HttpStatusCode.NotFound=>"Registro não encontrado.",_=>"A API não conseguiu concluir esta operação. Confira os dados e tente novamente."}));
   return value??new JsonObject();
  }catch(HttpRequestException){throw new ApiException(503,"A API está indisponível. Confira se ela foi iniciada e tente novamente.");}
  catch(TaskCanceledException){throw new ApiException(503,"O servidor demorou para responder. Consulte os dados antes de repetir a operação.");}
 }
}
