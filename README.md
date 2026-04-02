# AuraPay API (Backend .NET)

API REST para simulação de operações financeiras (contas, transferências, cartões e remessas internacionais), construída com .NET 8, arquitetura em camadas e boas práticas de segurança.

---

## ✨ Principais Features

* Autenticação JWT
* Transferência entre contas
* Cartões virtuais (criação, bloqueio e visualização)
* Remessas internacionais com cálculo de taxas
* Arquitetura limpa (Domain, Application, Infrastructure, WebAPI)
* Testes unitários e de integração

---

## 🧭 Arquitetura

O projeto segue separação clara de responsabilidades:

* **WebAPI**: Controllers, middlewares e configuração
* **Application**: Regras de negócio, serviços, DTOs e validações
* **Domain**: Entidades e interfaces
* **Infrastructure**: EF Core, DbContext e repositórios
* **Tests**: Testes unitários e de integração

```text
AuraPay.sln
  src/
    AuraPay.WebAPI/
    AuraPay.Application/
    AuraPay.Domain/
    AuraPay.Infrastructure/
  tests/
```

---

## 🛠 Stack Tecnológica

* .NET 8 (ASP.NET Core)
* Entity Framework Core + PostgreSQL
* JWT Authentication
* FluentValidation
* Serilog
* Swagger
* xUnit + Moq + FluentAssertions

---

## ⚙️ Funcionalidades

* Cadastro e autenticação de usuário (JWT)
* Criação automática de conta com saldo inicial (dev)
* Consulta de saldo
* Transferências com validações de negócio
* Histórico de transações
* Cartões virtuais (criar, listar, bloquear)
* Simulação e execução de remessas internacionais

---

## 📌 Regras de Negócio

* Não permite transferência para a própria conta
* Não permite saldo insuficiente
* Conta destino deve existir (exceto remessa internacional)
* Valor deve ser maior que zero
* Validação de titularidade para dados sensíveis de cartão
* Remessa internacional aplica:

  * IOF: **1.1%**
  * Taxa fixa: **R$ 20,00**

---

## 🔐 Segurança

* JWT Bearer Authentication
* Extração de `UserId` via claim (`NameIdentifier`)
* Middleware global de exceções
* Senhas com hash (`BCrypt`)
* Endpoints protegidos com `[Authorize]`

---

## ▶️ Como Executar

### Pré-requisitos

* .NET 8 SDK
* PostgreSQL (local) ou Supabase
* EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
```

---

## ⚙️ Configuração

Este projeto **não armazena credenciais sensíveis no repositório**.
Antes de executar, configure os dados localmente.

### 🔹 Opção 1 — Configuração rápida

Crie o arquivo:

```bash
src/AuraPay.WebAPI/appsettings.Development.json
```

Com o conteúdo:

```json
{
  "JwtSettings": {
    "Secret": "SUA_CHAVE_COM_NO_MINIMO_32_CARACTERES",
    "Issuer": "AuraPayAPI",
    "Audience": "AuraPayUsers",
    "AccessTokenExpirationMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=aurapay_db;Username=postgres;Password=postgres"
  }
}
```

---

### 🔹 Opção 2 — User Secrets (recomendado)

```bash
dotnet user-secrets init --project src/AuraPay.WebAPI

dotnet user-secrets set "JwtSettings:Secret" "SUA_CHAVE_COM_NO_MINIMO_32_CARACTERES" --project src/AuraPay.WebAPI
dotnet user-secrets set "JwtSettings:Issuer" "AuraPayAPI" --project src/AuraPay.WebAPI
dotnet user-secrets set "JwtSettings:Audience" "AuraPayUsers" --project src/AuraPay.WebAPI

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=aurapay_db;Username=postgres;Password=postgres" --project src/AuraPay.WebAPI
```

> ℹ️ Em ambiente de desenvolvimento, os valores definidos via User Secrets sobrescrevem o `appsettings.json`.

---

## ▶️ Executando o projeto

```bash
dotnet restore
dotnet build
dotnet ef database update --project src/AuraPay.Infrastructure --startup-project src/AuraPay.WebAPI
dotnet run --project src/AuraPay.WebAPI
```

---

## 🌐 Swagger

Após iniciar a aplicação:

```
https://localhost:7250/swagger
```

Permite testar todos os endpoints da API.

---

## 🧪 Testes

```bash
dotnet test
```

* Testes unitários e de integração
* Integração utilizando banco em memória

---

## 📊 Observabilidade

* Logs estruturados com Serilog
* Saída em console e arquivos (`/logs`)

---

## 🚧 Roadmap

* Refresh Token
* Rate limiting no login
* Criptografia adicional de dados sensíveis
* CI/CD com validação automática de testes

---

## 📌 Sobre o Projeto

Projeto desenvolvido para demonstrar práticas reais de engenharia de software em ambiente financeiro, com foco em:

* Arquitetura limpa
* Segurança
* Regras de negócio bem definidas
* Testabilidade
* Organização de código em nível profissional

---
