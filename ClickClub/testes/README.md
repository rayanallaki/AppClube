# Testes do ClickClub

Os testes criam registros fictícios. Use um banco exclusivo, diferente do
banco da apresentação. Reiniciar a API não limpa os dados.

## Verificações rápidas contra uma API já iniciada

Na raiz do projeto, com API na porta 5080:

```powershell
powershell -ExecutionPolicy Bypass -File .\testes\smoke.ps1
powershell -ExecutionPolicy Bypass -File .\testes\cpf.ps1
```

O arquivo `ClickClub.http` também contém chamadas para o REST Client do VS
Code. Os scripts originais podem encontrar registros duplicados quando
executados novamente no mesmo banco. `fluxos-adicionais.ps1` pressupõe um
conjunto inicial sem o dependente vinculado nem mensalidade da competência.

## Suíte HTTP com reinício real da API

`integracao_mysql.py` usa apenas a biblioteca padrão do Python 3.10+ e inicia
a API já compilada na porta 5089. Não executa DROP, não apaga tabelas e não
restaura banco. Entretanto, cria/altera registros e presume banco de teste
novo, com as contas de demonstração. Não aponte para sua base principal.

1. Crie no MySQL um schema vazio, por exemplo `clickclub_test`, e um usuário
   autorizado somente nesse schema. O nome precisa terminar em `_test` ou
   `_teste` para o script aceitar a conexão.
2. Compile a API:

```powershell
dotnet build .\api\ClickClub.Api.csproj
```

3. Crie um JSON privado fora do projeto com os mesmos campos de configuração
   da API, sem compartilhá-lo:

```json
{
  "ConnectionStrings": {
    "ClickClub": "Server=localhost;Port=3306;Database=clickclub_test;User=USUARIO_TESTE;Password=SENHA_LOCAL;"
  },
  "Credenciais": {
    "Chave": "CHAVE_BASE64_DE_32_BYTES"
  }
}
```

4. Execute com o caminho do seu arquivo privado:

```powershell
python .\testes\integracao_mysql.py --config C:\configuracao-privada\clickclub-test.json --aceito-banco-teste
```

Alternativamente, forneça `CLICKCLUB_TEST_CONNECTION` e
`CLICKCLUB_TEST_KEY` por variáveis de ambiente; não digite segredos em um
terminal compartilhado ou grave esses valores no Git. Sem chave fornecida
por variável, o teste gera uma chave temporária para a execução.

A suíte cobre autenticação, CPF, escopo, dependentes, atualizações, reservas
concorrentes, financeiro, turmas/espera, eventos/visitantes, chamados,
aprovação de adesão, persistência após reinício e logout. Os resultados
observados da entrega estão em `../TESTES_REALIZADOS.md`.

Teste isolado de falha de gravação e comportamento do app MAUI são verificações
adicionais documentadas; não são implicitamente executadas por este script.

## Suíte do cliente mobile

```powershell
dotnet run --project .\testes\ClickClub.Mobile.Tests -- .\tmp_tests
```

O executável usa o código do cliente que o MAUI compila, com HTTP simulado e
um SQLite descartável em `tmp_tests`.
