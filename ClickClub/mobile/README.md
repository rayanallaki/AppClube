# ClickClub Mobile para Windows

O aplicativo é um projeto .NET MAUI 10 com Blazor Hybrid. Ele abre em uma
janela redimensionável, inicialmente no formato de celular. As telas, o cliente
HTTP e o cache SQLite ficam neste projeto.

## Abrir pelo PowerShell

Na raiz `ClickClub`, com a API iniciada, execute:

```powershell
cd .\mobile
dotnet run
```

O `global.json` desta pasta seleciona o SDK .NET 10 instalado. O app requer
Windows 10 versão 2004 ou posterior (x64), workload `maui-windows` e Microsoft
Edge WebView2 Runtime. A primeira compilação restaura os pacotes do NuGet.

No notebook, rode `dotnet --version` dentro de `mobile` e confira que o SDK
selecionado é `10.0.x`. Use `dotnet workload list` para conferir `maui-windows`.
Se faltar a carga de trabalho, instale-a no Visual Studio Installer ou rode
`dotnet workload install maui-windows` nesse terminal.

## Abrir pelo Visual Studio

Abra `ClickClub.sln`, selecione `ClickClub.Mobile` como projeto de inicialização
e execute no destino **Windows Machine**.

A API padrão é `http://localhost:5080/`. Para mudar esse endereço, abra a
engrenagem na tela de login. O app usa o SecureStorage para a sessão e SQLite
para a carteirinha e os dados permitidos offline. Reservas, pagamentos simulados
e validação de acesso precisam da API online.

## Teste automatizado do cliente

Na raiz do projeto, execute:

```powershell
dotnet run --project .\testes\ClickClub.Mobile.Tests -- .\tmp_tests
```

O teste usa SQLite local e um servidor HTTP simulado. Os 14 cenários incluem
login, expiração da sessão, carteirinha offline e troca de servidor.
