using System.Security.Claims;
using System.Text.Json.Nodes;
using ClickClub.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ClickClub.Web.Controllers;
[Route("conta")]
public class ContaController(ApiClient api):Controller
{
 [HttpGet("entrar")][AllowAnonymous]public IActionResult Entrar()=>User.Identity?.IsAuthenticated==true?Redirect("/painel"):View();
 [HttpPost("entrar")][AllowAnonymous]public async Task<IActionResult> Entrar(string login,string senha)
 {
  try{
   if(string.IsNullOrWhiteSpace(login)||string.IsNullOrWhiteSpace(senha))throw new ApiException(400,"Informe seu usuário e sua senha.");
   var r=await api.Send(HttpMethod.Post,"api/auth/login",new{login,senha},true);var u=r["usuario"]!;
   HttpContext.Session.Clear();HttpContext.Session.SetString("token",r["token"]!.ToString());HttpContext.Session.SetString("usuario",u.ToJsonString());
   var claims=new List<Claim>{new(ClaimTypes.NameIdentifier,u["id"]!.ToString()),new(ClaimTypes.Name,u["nome"]!.ToString())};
   foreach(var role in u["papeis"]!.AsArray())claims.Add(new Claim(ClaimTypes.Role,role!.ToString()));
   await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme)),new AuthenticationProperties{ExpiresUtc=DateTimeOffset.Parse(r["expiraEm"]!.ToString())});
   return Redirect("/painel");
  }catch(ApiException e){ViewBag.Error=e.Message;ViewBag.Login=login;return View();}
 }
 [HttpPost("sair")][Authorize]public async Task<IActionResult> Sair(){try{await api.Send(HttpMethod.Post,"api/auth/sair");}catch(ApiException){}HttpContext.Session.Clear();await HttpContext.SignOutAsync();return RedirectToAction(nameof(Entrar));}
 [HttpGet("adesao")][AllowAnonymous]public async Task<IActionResult> Adesao(){try{ViewBag.Clubes=await api.Send(HttpMethod.Get,"api/publico/clubes",anonymous:true);}catch(ApiException e){ViewBag.Clubes=new JsonArray();ViewBag.Error=e.Message;}return View();}
 [HttpPost("adesao")][AllowAnonymous]public async Task<IActionResult> Adesao(string clubeId,string nome,string email,string tipoVinculo,string login,string senha){try{await api.Send(HttpMethod.Post,"api/adesoes",new{clubeId,nome,email,tipoVinculo,login,senha},true);TempData["success"]="Solicitação enviada. Entre com sua conta para acompanhar a decisão.";return RedirectToAction(nameof(Entrar));}catch(ApiException e){TempData["error"]=e.Message;return RedirectToAction(nameof(Adesao));}}
 [HttpGet("negado")]public IActionResult Negado(){TempData["error"]="Seu perfil não tem acesso a essa área.";return Redirect("/painel");}
 [HttpGet("erro")][AllowAnonymous]public IActionResult Erro()=>View();
}
