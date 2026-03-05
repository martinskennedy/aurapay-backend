using AuraPay.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.IntegrationTests.Config
{
    // WebApplicationFactory<Program> sobe a API AuraPay real em memória
    public class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly AuraPayDbContext _context;
        protected readonly IServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;

        public BaseIntegrationTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // 1. Remove a configuração original
                    services.RemoveAll(typeof(DbContextOptions<AuraPayDbContext>));

                    // 2. Cria um provedor de serviços interno ISOLADO para o EF não misturar Npgsql com InMemory
                    var internalServiceProvider = new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider();

                    // 3. Adiciona o contexto usando esse provedor isolado
                    services.AddDbContext<AuraPayDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("AuraPayIntegrationTests")
                               .UseInternalServiceProvider(internalServiceProvider);
                    });

                    // 4.Sobrescreve a Autenticação para usar o esquema de teste
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                        options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.AuthenticationScheme, options => { });
            });
        });

            // Cria um escopo para obter o DbContext e garantir que ele seja descartado corretamente após os testes
            _scope = _factory.Services.CreateScope();
            _serviceProvider = _scope.ServiceProvider;
            _context = _serviceProvider.GetRequiredService<AuraPayDbContext>();

            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _scope.Dispose();
            _context.Dispose();
        }
    }
}