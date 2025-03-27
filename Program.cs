using Azure.Storage.Blobs;
using BlobStorageAPI.Interfaces;
using BlobStorageAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Подключение к Azure Blob Storage
string? connectionString = builder.Configuration.GetConnectionString("AzureBlobConnection");
builder.Services.AddSingleton(x => new BlobServiceClient(connectionString));

// ✅ Регистрация репозитория
builder.Services.AddScoped<IBlobRepository, BlobRepository>();

var app = builder.Build();

// ✅ Включаем Swagger не только в Development, но и в Production (например, на Azure)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blob Storage API v1");
        c.RoutePrefix = "swagger"; // Теперь Swagger будет доступен по /swagger
    });
}


// Включаем HTTPS редирект и контроллеры
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
