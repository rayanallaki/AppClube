# Executar o aplicativo Android

O aplicativo está em `mobile/ClickClub.Mobile`, com bibliotecas de interface
e serviços em `mobile/ClickClub.Mobile.UI` e `mobile/ClickClub.Mobile.Core`.
O MySQL fica no servidor da API; o Android não conecta diretamente ao banco.
O SQLite no aparelho mantém um cache limitado dos dados da conta e a
carteirinha obtida anteriormente.

## Preparar o computador

No Windows, instale o Visual Studio 2022 com a carga **Desenvolvimento para
dispositivos móveis com .NET**, SDK .NET 8 e Android SDK/JDK compatíveis com o
workload. Abra a solução gerada por `Criar-Solucao.ps1`, ou o projeto mobile.
O VS Code pode editar o código, mas ainda precisa dos mesmos SDKs/workloads
para gerar o aplicativo.

Pela CLI, em um ambiente com Android SDK/JDK configurados:

```powershell
dotnet workload install maui-android
dotnet build .\mobile\ClickClub.Mobile\ClickClub.Mobile.csproj -f net8.0-android
```

Selecione emulador/aparelho pelo Visual Studio. Para instalar pela CLI em
um aparelho com depuração USB autorizada:

```powershell
adb devices
dotnet build .\mobile\ClickClub.Mobile\ClickClub.Mobile.csproj -t:Run -f net8.0-android
```

O alvo padrão é **Android ARM64** (aparelhos de 64 bits, API24/Android7 ou
superior). Para um emulador x64, instale o workload completo e compile esse
alvo explicitamente, em vez de instalar nele um APK ARM64:

```powershell
dotnet build .\mobile\ClickClub.Mobile\ClickClub.Mobile.csproj -f net8.0-android -r android-x64 -p:RuntimeIdentifiers=android-x64
```

Para ensaiar as mesmas telas no navegador, inicie a API e execute:

```powershell
dotnet run --project .\mobile\Preview\ClickClub.Mobile.Preview.csproj --urls http://localhost:5082
```

Abra `http://localhost:5082`. Essa prévia não é um emulador Android e usa
armazenamento temporário de sessão; os testes de SecureStorage e instalação
continuam sendo feitos no aparelho. Ela usa o mesmo código de interface e
serviços HTTP do aplicativo.

O ZIP inclui `mobile/apk/ClickClub.apk`, compilado em Debug ARM64 e com a
assinatura verificada. Copie para o telefone e abra o arquivo, ou instale com
`adb install -r mobile/apk/ClickClub.apk` em um aparelho autorizado.
As bibliotecas estão incluídas no instalador, sem Fast Deployment.
Ao recompilar, o APK fica na árvore
`mobile/ClickClub.Mobile/bin/Debug/net8.0-android`. A versão para loja exige
assinatura própria; não há chaves privadas de assinatura no ZIP.

## Conectar à API

Inicie API e Web pelo script da raiz:

```powershell
powershell -ExecutionPolicy Bypass -File .\Iniciar-ClickClub.ps1 -RedeLocal
```

Na configuração de servidor do app, use:

| Onde o app roda | URL da API |
|---|---|
| Emulador Android padrão no mesmo computador | `http://10.0.2.2:5080/` |
| Aparelho físico na mesma rede Wi-Fi | `http://IP-DO-COMPUTADOR:5080/` |
| Simulador no navegador do computador | `http://localhost:5080/` |

No aparelho físico, `localhost` significa o próprio telefone. Use `ipconfig`
no Windows para encontrar o IPv4 do computador e confirme que o firewall
permite a porta escolhida somente na sua rede de desenvolvimento. Teste
`http://IP-DO-COMPUTADOR:5080/api/saude` no navegador do telefone antes do login.

HTTP sem TLS atende à apresentação em rede local com dados fictícios. Para
usar em um servidor real, configure HTTPS e uma URL estável; não exponha as
contas de demonstração na internet.

## Demonstrar o SQLite e a carteirinha offline

1. Entre como `titular` com a senha fictícia `Teste@123`, com a API acessível.
2. Abra/renove a carteirinha e confira nome, QR e validade.
3. Desative a rede do aparelho e volte à carteirinha já carregada.
4. Mostre a informação de cache/offline e a validade conservada. Uma
   credencial vencida não deve ser apresentada como entrada autorizada.
5. Reative a rede e renove os dados. A portaria sempre decide pela API online.

O token de sessão fica no SecureStorage do Android; senhas e tokens não
devem ser gravados no SQLite. O cache é separado por usuário/servidor. Ao
trocar de conta, o app precisa carregar apenas os dados dessa conta.

A execução física de câmera, SecureStorage e offline depende de teste no
aparelho. Consulte `TESTES_REALIZADOS.md` para saber quais verificações foram
efetivamente executadas no ambiente desta entrega.
