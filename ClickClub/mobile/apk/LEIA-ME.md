# Instalador Android de demonstração

`ClickClub.apk` é um APK **Debug ARM64**, versão 1.0.0, para Android 7/API24
ou superior, com Android System WebView atualizado. Inclui suas bibliotecas
e pode ser instalado diretamente, sem depender de Fast Deployment do Visual Studio.

Copie para o telefone, abra e autorize a instalação da fonte escolhida quando
o Android solicitar. Em um aparelho com depuração USB autorizada, também pode
instalar com `adb install -r ClickClub.apk`.

Abra a engrenagem na tela de login e configure
`http://IP-DO-COMPUTADOR:5080/`. Ligue API/MySQL e mantenha computador e
telefone na mesma rede. Os detalhes estão em `../../GUIA_ANDROID.md`.

As credenciais de demonstração são `titular` / `Teste@123`. A assinatura
é de desenvolvimento; não é um pacote de distribuição na Play Store.
A compilação e a assinatura foram verificadas em Linux. A instalação e a
execução física no telefone do usuário não foram realizadas neste ambiente.

O arquivo SHA256.txt permite verificar se a cópia do APK permaneceu íntegra.
