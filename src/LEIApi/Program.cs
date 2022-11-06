using Business.Workflow;
using Data.Model;
using DataAccess;
using DataAccess.AccessInterfaces;
using DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using Utils.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = builder.Configuration
        .GetSection(nameof(MongoDbSettings) + ":" + MongoDbSettings.ConnectionStringValue).Value;
    options.Database = builder.Configuration
        .GetSection(nameof(MongoDbSettings) + ":" + MongoDbSettings.DatabaseValue).Value;
});

builder.Services.AddSingleton<LeiRecordMongoAccess>();
builder.Services.AddSingleton<LeiRecordBusiness>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (Convert.ToBoolean(Environment.GetEnvironmentVariable("USE_SWAGGER")))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
