# ClickClub Mobile — Android, SQLite e carteirinha offline

Aplicativo .NET MAUI Blazor Hybrid em português, com telas de login e adesão,
painel do associado, reservas/agenda/convidados, quitação e transferência
simuladas, turmas e eventos, financeiro do titular, dependentes, avisos,
ouvidoria e cadastro pessoal. As operações passam pela API; o aplicativo
não contém conexão MySQL.

| Projeto | Finalidade |
|---|---|
| `ClickClub.Mobile` | Aplicativo Android, WebView MAUI, ícone e SecureStorage. |
| `ClickClub.Mobile.UI` | Telas e identidade visual compartilhadas em Razor. |
| `ClickClub.Mobile.Core` | Cliente HTTP, SQLite, sessão e geração de QR Code. |
| `Tests` | Testes executáveis do cliente com SQLite real e HTTP controlado. |
| `Preview` | Prévia da mesma interface no navegador para ensaio no computador. |

O token fica no SecureStorage do Android. SQLite guarda a carteirinha e um
catálogo limitado de leitura, separado por conta/servidor. Dados pessoais,
financeiro e chamados não são armazenados nesse cache. O aplicativo identifica
validade da carteirinha e preserva uma credencial ainda válida por até 24h
mesmo após expirar a sessão de login de oito horas. Sair explicitamente ou
trocar de servidor apaga o cache. Toda alteração e a decisão na portaria
continuam exigindo API online.

## Compilar e conectar

Siga `../GUIA_ANDROID.md` para os SDKs e para conectar ao IP do computador.
O projeto gera APK ARM64, para aparelhos Android de 64 bits, Android 7/API24
ou superior, com Android System WebView atualizado. O endereço padrão
`http://10.0.2.2:5080/` corresponde ao computador visto do emulador; em um
telefone físico use `http://IP-DO-COMPUTADOR:5080/` nas configurações do app.

## Verificações e prévia

Na raiz do projeto, sem precisar instalar o workload Android:

```powershell
dotnet run --project .\mobile\Tests\ClickClub.Mobile.Tests.csproj
dotnet run --project .\mobile\Preview\ClickClub.Mobile.Preview.csproj --urls http://localhost:5082
```

A prévia usa a API em `http://localhost:5080/` e permite configurar outro
endereço na tela inicial. Ela reproduz a interface do app; o armazenamento
de sessão da prévia é temporário e não substitui SecureStorage no aparelho.
O cache SQLite da prévia é criado na pasta temporária do computador.

Consulte `../TESTES_REALIZADOS.md` para os testes observados e o estado da
compilação APK. A instalação e os controles do Android físico devem ser
ensaiados no aparelho que será utilizado na apresentação.
