using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();

// Добавляем Swagger/OpenAPI (для совместимости с шаблоном)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Разрешаем CORS для WinForms
builder.Services.AddCors();

var app = builder.Build();

// Swagger (нужен для шаблона)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS - разрешаем все запросы
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Маршрутизация
app.MapControllers();

// Запуск
app.Run();