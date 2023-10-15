using backend.Authorization;
using backend.Helpers;
using database.Repository;
using backend.Services;
using database.Repository.Base;
using database.Repository.History;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Добавляем нашу типизацию настроек
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddControllers();

// Инициализируем приставку для переменных среды
DataRepositoryEnvironment.VariablePrefix = "BACKEND";

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtUtils, JwtUtils>();

#region Добавляем зависимости для Repository
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddSingleton<IUserToUserRoleRepository, UserToUserRoleRepository>();
builder.Services.AddSingleton<IUserToRefreshTokenRepository, UserToRefreshTokenRepository>();
builder.Services.AddSingleton<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddSingleton<IMetricRepository, MetricRepository>();
#endregion


var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
if (isDevelopment)
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Test01", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."

        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>(); // свои исключения
app.UseMiddleware<JwtMiddleware>(); // проверка ключей

app.MapControllers();

app.Run();