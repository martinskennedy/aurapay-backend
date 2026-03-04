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

// 4. CONFIGURAÇÃO JWT
var supabaseUrl = "https://tgfipyvrglihoqwtfkug.supabase.co/auth/v1";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = supabaseUrl;
    options.RequireHttpsMetadata = false; // Em desenvolvimento pode ser false
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = supabaseUrl,
        ValidateAudience = true,
        ValidAudience = "authenticated", // Padrão do Supabase
        ValidateLifetime = true,
        NameClaimType = "sub" // Diz ao .NET que o "sub" do JWT é o identificador único do usuário
    };
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


// 5. CONFIGURAÇÃO SWAGGER PARA JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuraPay API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

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