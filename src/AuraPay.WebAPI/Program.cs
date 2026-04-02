using AuraPay.Application.Interfaces;
using AuraPay.Application.Services;
using AuraPay.Application.Validators;
using AuraPay.Domain.Interfaces;
using AuraPay.Infrastructure.Data;
using AuraPay.Infrastructure.Repositories;
using AuraPay.WebAPI.Middlewares;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURAÇÃO DO SERILOG ---
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/aurapay-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(); // Diz ao .NET para usar Serilog

// 1. Configurar o Banco de Dados (PostgreSQL/Supabase)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuraPayDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Registrar Repositórios (Infrastructure)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();

// 3. Registrar Serviços (Application)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ICurrencyExchangeService, CurrencyExchangeService>();
builder.Services.AddScoped<IInternationalTransactionService, InternationalTransactionService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// 4. CONFIGURAÇÃO JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = builder.Configuration["JwtSettings:Secret"];

if (string.IsNullOrEmpty(secretKey))
    throw new Exception("JWT Secret Key não configurada!");

var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero, // Remove a tolerância de 5 min para expiração exata
        NameClaimType = ClaimTypes.NameIdentifier // Permite que o ClaimTypes.NameIdentifier seja usado para obter o UserId no BaseController
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AuraPayFrontPolicy", policy =>
    {
        policy.AllowAnyOrigin()   // Trocar pelo domínio do Vercel ex: .WithOrigins("https://seu-front.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


// 5. CONFIGURAÇÃO SWAGGER PARA JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuraPay API", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta maneira: Bearer {seu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 6. Adicionar FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<TransferRequestDtoValidator>();

// 7. Adicionar HttpClient para chamadas externas (ex: Supabase)
builder.Services.AddHttpClient();

try
{
    Log.Information("Iniciando AuraPay Web API...");

    var app = builder.Build();

    app.UseCors("AuraPayFrontPolicy");

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar!");
}
finally
{
    Log.CloseAndFlush();
}

// Para permitir que o WebApplicationFactory<Program> funcione nos testes de integração, precisamos tornar a classe Program pública e parcial
public partial class Program { }