using CM.API.Factories;
using CM.Core.Domain.Settings;
using CM.Data.Base;
using CM.Data.DBInitializer;
using CM.Services.Companies;
using CM.Services.Contacts;
using CM.Services.Schema;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

var mongoDbConfig = new MongoDatabaseConfig();
builder.Configuration.Bind(nameof(MongoDatabaseConfig), mongoDbConfig);
builder.Services.AddSingleton(mongoDbConfig);

builder.Services.AddSingleton(typeof(IMongoRepoistory<>), typeof(MongoRepoistory<>));
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IDocumentSchemaService, DocumentSchemaService>();
builder.Services.AddScoped<IDtoFactory, DtoFactory>();
builder.Services.AddScoped(typeof(MongoDbInitializer));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var monogoSeeder = scope.ServiceProvider.GetService<MongoDbInitializer>();

    await monogoSeeder.Seed();
}

app.Run();



