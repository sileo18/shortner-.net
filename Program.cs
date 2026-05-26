using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var redisConnection = builder.Configuration.GetConnectionString("Redis");

if (string.IsNullOrEmpty(redisConnection))
{
    throw new InvalidOperationException("Redis connection string is not configured.");
}

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnection)
);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
});
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddScoped<IUrlService, UrlService>(); 
builder.Services.AddScoped<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// ============================================================
// Configurar CORS para aceitar o frontend em portas diferentes
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .WithOrigins(
                "http://20.243.9.229:3000",      // Frontend em produção na VM
                "http://20.243.9.229",            // Frontend sem porta (fallback)
                "http://localhost:3000",          // Desenvolvimento local
                "http://localhost:5173",          // Vite dev server
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5173"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    );
});

var app = builder.Build();

// ============================================================
// Aplicar CORS PRIMEIRO (antes de qualquer outro middleware)
// ============================================================
app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
