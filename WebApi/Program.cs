using Application.Interfaces;
using Application.Vouchers.Commands;
using Domain.Entities;
using Elastic.Clients.Elasticsearch;
using Infrastructure.Persistence.Elastic;
using Infrastructure.Persistence.Mongo;
using MongoDB.Driver;
using WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateVoucherCommand).Assembly));

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb");
var mongoDbName = builder.Configuration["MongoDbSettings:DatabaseName"];

var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(mongoDbName);
builder.Services.AddSingleton(mongoDatabase);

builder.Services.AddScoped<IMongoRepository<Promotion>>(sp => new MongoRepository<Promotion>(sp.GetRequiredService<IMongoDatabase>(), "Promotions"));
builder.Services.AddScoped<IMongoRepository<Voucher>>(sp => new MongoRepository<Voucher>(sp.GetRequiredService<IMongoDatabase>(), "Vouchers"));

var elasticConnectionString = builder.Configuration.GetConnectionString("Elasticsearch");
var esSettings = new ElasticsearchClientSettings(new Uri(elasticConnectionString!));
builder.Services.AddSingleton(new ElasticsearchClient(esSettings));

builder.Services.AddScoped<IElasticSearchService<Voucher>, ElasticVoucherService>();
builder.Services.AddScoped<IElasticSearchService<Promotion>, ElasticPromotionService>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthorization();

app.MapControllers();
try
{
    await WebApi.Extensions.DataSeeder.SeedDataAsync(app.Services);
}
catch (Exception ex)
{
    Console.WriteLine($"Errors during data seeding: {ex.Message}");
}

app.Run();
