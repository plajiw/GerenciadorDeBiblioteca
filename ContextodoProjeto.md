# Contexto do Projeto: Gerenciador de Biblioteca

Este documento descreve o contexto, objetivos, requisitos e estrutura de um projeto pessoal para reforçar conhecimentos em **SAPUI5**, **testes OPA5**, **JavaScript** (frontend) e **ASP.NET Core** (backend), utilizando a arquitetura **Onion** (Clean Architecture). O projeto é análogo a um sistema de gestão de equipamentos, mas foca na gestão de uma biblioteca, com funcionalidades adicionais como autenticação, filtros avançados e relatórios. O banco de dados será implementado com **RavenDB**, um banco NoSQL orientado a documentos, com detalhes específicos sobre sua configuração, métodos, propriedades e enums.

---

## 1. Visão Geral do Projeto

**Nome do Projeto**: Gerenciador de Biblioteca  
**Repositório**: GerenciadorDeBiblioteca  
**Descrição**: Uma aplicação web para gerenciar uma biblioteca, permitindo o cadastro, edição, exclusão e listagem de livros, gerenciamento de empréstimos, autenticação de usuários e geração de relatórios. O projeto reforça conceitos profissionais (SAPUI5, testes OPA5, ASP.NET Core) e introduz desafios adicionais, como autenticação JWT, filtros avançados e relatórios.

**Objetivos**:
- Reforçar conhecimentos em SAPUI5, testes OPA5, ASP.NET Core e mock de APIs.
- Aprender autenticação com JWT, filtros avançados e relatórios.
- Implementar a arquitetura Onion no backend.
- Utilizar **RavenDB** como banco de dados NoSQL.
- Aplicar práticas profissionais, como testes unitários (xUnit), validações (FluentValidation) e documentação (Swagger).

**Por que é análogo ao seu projeto?**  
- **Similaridades**: Envolve listagem, cadastro, detalhes, navegação entre telas, testes OPA5 e mock de APIs no frontend, com backend ASP.NET Core e endpoints REST.  
- **Adições para aprendizado**:
  - Autenticação JWT.
  - Filtros avançados (por autor, gênero, disponibilidade).
  - Relatórios (livros mais emprestados).
  - Exclusão de registros (DELETE).
  - Validações com FluentValidation.
  - Testes unitários com xUnit.

---

## 2. Requisitos do Projeto

### 2.1. Frontend (SAPUI5)

- **Telas**:
  - **Login**: Autenticação de usuários com JWT.
  - **Listagem de Livros**: Exibe livros com filtros (autor, gênero, disponibilidade) e navegação para detalhes.
  - **Cadastro/Edição de Livros**: Formulário para criar ou editar livros.
  - **Empréstimo de Livros**: Associa livros a usuários para empréstimos.
  - **Relatório**: Lista livros emprestados com informações resumidas.
- **Testes**: Testes OPA5 para todos os fluxos (login, listagem, cadastro, empréstimo, relatório).
- **Mock de APIs**: Simulação de chamadas HTTP (GET, POST, PUT, DELETE) com `fetch` no `Startup.js`.

### 2.2. Backend (ASP.NET Core)

- **Endpoints REST**:
  - `POST /api/auth/login`: Autenticação de usuários (retorna JWT).
  - `GET /api/livros`: Lista todos os livros.
  - `GET /api/livros/{id}`: Detalhes de um livro.
  - `POST /api/livros`: Cadastra um livro.
  - `PUT /api/livros/{id}`: Edita um livro.
  - `DELETE /api/livros/{id}`: Exclui um livro.
  - `POST /api/emprestimos`: Registra um empréstimo.
  - `GET /api/relatorios/emprestados`: Relatório de livros emprestados.
- **Validações**: Usar FluentValidation para validar entradas (ex.: título não nulo).
- **Testes**: Testes unitários com xUnit para serviços e controladores.

### 2.3. Banco de Dados (RavenDB)

- **Nome do Banco de Dados**: `BibliotecaDB`
- **Descrição**: RavenDB é um banco NoSQL orientado a documentos, ideal para aplicações com dados semiestruturados, como livros e empréstimos. Ele será usado para armazenar entidades como `Livro`, `Emprestimo` e `Usuario`.
- **Configuração**:
  - Instalar o pacote NuGet `RavenDB.Client`.
  - Configurar a conexão no `appsettings.json`:
    ```json
    {
      "RavenDB": {
        "Urls": ["http://localhost:8080"],
        "Database": "BibliotecaDB"
      }
    }
    ```
  - Inicializar o `IDocumentStore` no `Program.cs`:
    ```csharp
    builder.Services.AddSingleton<IDocumentStore>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var store = new DocumentStore
        {
            Urls = config.GetSection("RavenDB:Urls").Get<string[]>(),
            Database = config.GetSection("RavenDB:Database").Get<string>()
        };
        store.Initialize();
        return store;
    });
    ```

- **Métodos do RavenDB** (usados nos repositórios):
  - `StoreAsync(T entity)`: Salva um novo documento (ex.: livro ou empréstimo).
  - `LoadAsync<T>(string id)`: Carrega um documento por ID.
  - `Query<T>()`: Consulta documentos com filtros (ex.: livros por autor).
  - `DeleteAsync(string id)`: Exclui um documento por ID.
  - `SaveChangesAsync()`: Persiste alterações na sessão.

- **Propriedades das Entidades**:
  - **Livro**:
    - `Id`: string (ex.: `Livro-1`, gerado automaticamente).
    - `Titulo`: string (obrigatório).
    - `Autor`: string (obrigatório).
    - `Genero`: string (enum `GeneroLivro`).
    - `QuantidadeEmEstoque`: int (não negativo).
  - **Emprestimo**:
    - `Id`: string (ex.: `Emprestimo-1`).
    - `LivroId`: string (referência ao livro).
    - `UsuarioId`: string (referência ao usuário).
    - `DataEmprestimo`: DateTime.
    - `DataDevolucao`: DateTime? (nulo se não devolvido).
  - **Usuario**:
    - `Id`: string (ex.: `Usuario-1`).
    - `Nome`: string.
    - `Email`: string (único, usado no login).
    - `SenhaHash`: string (hash da senha).

- **Enums**:
  ```csharp
  public enum GeneroLivro
  {
      Ficcao,
      NaoFiccao,
      Romance,
      Suspense,
      Infantil,
      Tecnico
  }
  ```

### 2.4. Arquitetura Onion

- **Camada de Domínio**: Contém entidades (`Livro`, `Emprestimo`, `Usuario`), enums e interfaces de repositórios.
- **Camada de Aplicação**: Inclui DTOs, serviços (`LivroService`, `EmprestimoService`, `AuthService`) e interfaces de serviços.
- **Camada de Infraestrutura**: Implementa repositórios (`LivroRepository`, `EmprestimoRepository`) com RavenDB e configurações (ex.: JWT).
- **Camada de Apresentação**: Contém controladores REST (`LivrosController`, `EmprestimosController`, `AuthController`).

### 2.5. Práticas Profissionais

- **Código Limpo**: Nomes claros e consistentes (ex.: `LivroDTO`, `ILivroService`).
- **Organização**: Estrutura de pastas clara no frontend e backend.
- **Documentação**: Swagger para documentar a API.
- **Testes**: Testes OPA5 (frontend) e xUnit (backend) cobrindo fluxos principais.
- **CORS**: Configurado para permitir chamadas do SAPUI5.

---

## 3. Orientações para Implementação

### 3.1. Foco no Aprendizado

- **SAPUI5**:
  - Utilize controles como `sap.m.Table` (listagem), `sap.m.Input` (formulários) e `sap.ui.layout.form.SimpleForm` (cadastro).
  - Implemente roteamento com `sap.ui.core.routing.Router` para navegação.
  - Gerencie estados com `sap.ui.model.json.JSONModel`.

- **Testes OPA5**:
  - Crie **Page Objects** para cada tela (`Login.js`, `LivroLista.js`, etc.).
  - Simule chamadas HTTP no `Startup.js` com `mockFetch` para GET, POST, PUT e DELETE.
  - Teste cenários de erro (ex.: falha na API com erro 500).

- **Backend**:
  - Configure autenticação JWT com `Microsoft.AspNetCore.Authentication.JwtBearer`.
  - Use FluentValidation para validar DTOs (ex.: `Titulo` não nulo em `LivroDTO`).
  - Implemente testes unitários com xUnit, mockando repositórios com Moq.

- **RavenDB**:
  - Use índices para consultas eficientes (ex.: índice para buscar livros por autor ou gênero).
  - Exemplo de índice:
    ```csharp
    public class LivrosPorAutor : AbstractIndexCreationTask<Livro>
    {
        public LivrosPorAutor()
        {
            Map = livros => from livro in livros
                           select new { livro.Autor, livro.Genero };
        }
    }
    ```

### 3.2. Passo a Passo

1. **Backend**:
   - Configure a arquitetura Onion com RavenDB (`BibliotecaDB`).
   - Implemente endpoints REST básicos (`/api/livros`, `/api/auth/login`).
   - Adicione validações com FluentValidation e testes com xUnit.

2. **Frontend**:
   - Crie a tela de login com autenticação JWT.
   - Desenvolva telas de listagem, cadastro, empréstimo e relatório.
   - Configure roteamento no `manifest.json`.

3. **Testes**:
   - Escreva testes OPA5 para cada tela, usando Page Objects.
   - Adicione testes unitários no backend para serviços e controladores.

4. **Integração**:
   - Teste a comunicação frontend-backend com CORS habilitado.
   - Valide fluxos completos (ex.: login → listar livros → cadastrar).

---

## 4. Tecnologias

- **Frontend**:
  - **SAPUI5**: Interface web (usar CDN do OpenUI5 para testes).
  - **QUnit/OPA5**: Testes de integração.
  - **JavaScript**: Lógica do frontend e mock de APIs.

- **Backend**:
  - **ASP.NET Core 8**: API REST.
  - **FluentValidation**: Validações de entrada.
  - **xUnit**: Testes unitários.
  - **Swashbuckle (Swagger)**: Documentação da API.
  - **RavenDB.Client**: Acesso ao banco NoSQL.

- **Outros**:
  - **Git**: Controle de versão.
  - **Visual Studio**: Desenvolvimento do backend.
  - **VS Code**: Desenvolvimento do frontend.
  - **RavenDB**: Banco de dados NoSQL (servidor local em `http://localhost:8080`).

---

## 5. Estrutura do Projeto

### 5.1. Backend (ASP.NET Core)

**Estrutura de Pastas**:
```
GerenciadorDeBiblioteca.Api/
├── src/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── Livro.cs
│   │   │   ├── Emprestimo.cs
│   │   │   ├── Usuario.cs
│   │   ├── Enums/
│   │   │   ├── GeneroLivro.cs
│   │   ├── Interfaces/
│   │   │   ├── ILivroRepository.cs
│   │   │   ├── IEmprestimoRepository.cs
│   │   │   ├── IUsuarioRepository.cs
│   ├── Application/
│   │   ├── DTOs/
│   │   │   ├── LivroDTO.cs
│   │   │   ├── EmprestimoDTO.cs
│   │   │   ├── LoginDTO.cs
│   │   ├── Services/
│   │   │   ├── LivroService.cs
│   │   │   ├── EmprestimoService.cs
│   │   │   ├── AuthService.cs
│   │   ├── Interfaces/
│   │   │   ├── ILivroService.cs
│   │   │   ├── IEmprestimoService.cs
│   │   │   ├── IAuthService.cs
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   ├── BibliotecaDBContext.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── LivroRepository.cs
│   │   │   │   ├── EmprestimoRepository.cs
│   │   │   │   ├── UsuarioRepository.cs
│   │   │   ├── Indexes/
│   │   │   │   ├── LivrosPorAutor.cs
│   │   ├── Configurations/
│   │   │   ├── JwtConfig.cs
│   ├── Presentation/
│   │   ├── Controllers/
│   │   │   ├── LivrosController.cs
│   │   │   ├── EmprestimosController.cs
│   │   │   ├── AuthController.cs
│   │   │   ├── RelatoriosController.cs
├── tests/
│   ├── GerenciadorDeBiblioteca.Tests/
│   │   ├── Services/
│   │   │   ├── LivroServiceTests.cs
│   │   │   ├── EmprestimoServiceTests.cs
│   │   ├── Controllers/
│   │   │   ├── LivrosControllerTests.cs
├── Program.cs
├── appsettings.json
```

### 5.2. Frontend (SAPUI5)

**Estrutura de Pastas**:
```
wwwroot/
├── controller/
│   ├── Login.controller.js
│   ├── LivroLista.controller.js
│   ├── LivroCadastro.controller.js
│   ├── Emprestimo.controller.js
│   ├── Relatorio.controller.js
├── view/
│   ├── Login.view.xml
│   ├── LivroLista.view.xml
│   ├── LivroCadastro.view.xml
│   ├── Emprestimo.view.xml
│   ├── Relatorio.view.xml
├── test/
│   ├── integration/
│   │   ├── arrangements/
│   │   │   ├── Startup.js
│   │   ├── pages/
│   │   │   ├── Login.js
│   │   │   ├── LivroLista.js
│   │   │   ├── LivroCadastro.js
│   │   │   ├── Emprestimo.js
│   │   │   ├── Relatorio.js
│   │   ├── AllJourneys.js
│   │   ├── opaTests.qunit.html
│   │   ├── opaTests.qunit.js
├── i18n/
│   ├── i18n.properties
├── index.html
├── manifest.json
```

---

## 6. Nomes e Convenções

- **Namespace SAPUI5**: `ui5.gerenciadorbiblioteca`
- **Rotas**:
  - `login`: Tela de login.
  - `listaLivros`: Listagem de livros.
  - `cadastroLivro`: Cadastro/edição de livros.
  - `emprestimoLivro`: Empréstimo de livros.
  - `relatorioEmprestados`: Relatório de livros emprestados.
- **Modelos**:
  - `livros`: Dados da lista de livros.
  - `emprestimos`: Dados de empréstimos.
  - `auth`: Token de autenticação.
- **Page Objects**:
  - `Login.js`
  - `LivroLista.js`
  - `LivroCadastro.js`
  - `Emprestimo.js`
  - `Relatorio.js`

---

## 7. Dicas de Estudo

- **SAPUI5**:
  - Explore controles avançados como `sap.ui.table.Table` (relatórios) e `sap.m.Dialog` (confirmações).
  - Pratique roteamento com `sap.ui.core.routing.Router`.
  - Use `sap.ui.model.json.JSONModel` para gerenciar estados.

- **Testes OPA5**:
  - Crie testes para ações (ex.: clique em botões) e validações (ex.: tabela com itens).
  - Use matchers como `AggregationLengthEquals` e `I18NText`.
  - Simule erros de API no `mockFetch` (ex.: erro 500).

- **Frontend (JavaScript)**:
  - Pratique `fetch` com `async/await` para chamadas HTTP.
  - Gerencie tokens JWT com `localStorage`.

- **Backend (ASP.NET Core)**:
  - Configure JWT com `Microsoft.AspNetCore.Authentication.JwtBearer`.
  - Use FluentValidation para validações.
  - Escreva testes com xUnit, mockando com Moq.

- **RavenDB**:
  - Estude índices para consultas otimizadas.
  - Use sessões para operações transacionais.

---

## 8. Cronograma Sugerido

- **Semana 1**: Configurar backend com Onion e RavenDB, implementar endpoints básicos e Swagger.
- **Semana 2**: Desenvolver frontend (login e listagem), configurar roteamento e testes OPA5.
- **Semana 3**: Implementar telas de cadastro e empréstimo com testes OPA5.
- **Semana 4**: Adicionar autenticação JWT, tela de relatório e testes unitários no backend.
- **Semana 5**: Testar integração completa, corrigir erros e documentar.

---

## 9. Benefícios do Projeto

- **Reforço de Conhecimentos**: Reutiliza conceitos de listagem, cadastro, testes OPA5 e mock de APIs.
- **Novos Desafios**: Autenticação JWT, filtros avançados, relatórios e uso do RavenDB.
- **Práticas Profissionais**: Arquitetura Onion, testes abrangentes, documentação Swagger e autenticação segura.
- **Alinhamento com Trabalho**: Estrutura de testes (AllJourneys.js, Startup.js), backend com CORS e FluentValidation, frontend SAPUI5 com i18n.