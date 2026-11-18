# ClickClub Web — ASP.NET Core MVC

Interface administrativa e área do associado em português, com painel, agenda,
reservas, financeiro simulado, carteirinha, portaria, cadastros e comunicação.
A Web consome exclusivamente a API por HTTP; não acessa o MySQL diretamente.

## Executar

Instale o SDK .NET 8. Configure e inicie a API com MySQL primeiro. Em outro
terminal na pasta `web`:

```powershell
dotnet restore
dotnet run --urls http://localhost:5081
```

Abra http://localhost:5081. O endereço da API é configurado em `Api:BaseUrl`
em `appsettings.json`, normalmente `http://localhost:5080/`.
Para alterar sem editar arquivos:

```powershell
$env:Api__BaseUrl = "http://localhost:5080/"
dotnet run --urls http://localhost:5081
```

Quando a população de demonstração estiver habilitada na API, os usuários
`admin`, `presidente`, `tesoureiro`, `funcionario`, `porteiro`, `titular` e
`dependente` usam a senha fictícia `Teste@123`. A conta `admin` permite
apresentar as áreas administrativas; `titular`, os fluxos do associado;
`porteiro`, a validação de acesso.

## Fluxo recomendado

1. Entre como administrador e conheça o painel.
2. Confira clubes, pessoas, associações, categorias e espaços.
3. Consulte a agenda e cadastre uma reserva pela conta titular.
4. Consulte ou quite a taxa, ou transfira para uma fatura futura elegível.
5. Inscreva o titular em uma turma/evento; acompanhe vagas e espera.
6. Emita a carteirinha e confira a credencial pela portaria.
7. Envie uma adesão pela tela pública e aprove pela administração.
8. Abra um chamado como associado e responda pela administração.

As operações seguem permissões verificadas pela API. O menu adapta-se aos
perfis, mas não substitui a autorização do servidor. A portaria só autoriza
mediante resposta online da API. Todos os valores financeiros são simulados.

## Estrutura

- `Controllers`: login, navegação e ações específicas de cada fluxo.
- `Services/ApiClient.cs`: comunicação autenticada com a API.
- `Models/PortalModel.cs`: componentes tipados de apresentação e formulários.
- `Views`: layout responsivo, painel, formulários e telas públicas.
- `wwwroot`: CSS e JavaScript locais, sem dependências de CDN.

O token da API permanece na sessão do servidor Web. O navegador recebe
cookies HTTP-only; os formulários de alteração possuem antiforgery. Reiniciar
o servidor Web encerra as sessões Web em memória. Para instalação distribuída,
substitua o armazenamento da sessão por cache compartilhado e configure HTTPS.
