using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using WebApplication1.Interfaces;
using WebApplication1.Models.Data;
using WebApplication1.Repositories;
using WebApplication1.Validators;

var builder = WebApplication.CreateBuilder(args);
SQLitePCL.Batteries_V2.Init();

const string localDbPassword = "Passw0rd1234!!";

var encryptedConnectionString = builder.Configuration
    .GetSection("ConnectionStringsEncrypt")
    .GetValue<string>("DefaultConnection")
    ?? throw new InvalidOperationException("Missing ConnectionStringsEncrypt:DefaultConnection configuration.");

var encryptedSqliteConnectionString = new SqliteConnectionStringBuilder(encryptedConnectionString)
{
    Mode = SqliteOpenMode.ReadWriteCreate,
    Password = localDbPassword
}.ToString();

// Add services to the container.

builder.Services.AddControllers();
// Learn about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddDbContext<EncryptedAppDbContext>(options =>
{
    options.UseSqlite(encryptedSqliteConnectionString);
});


builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Work Order" , Version = "v1" });
});

builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
builder.Services.AddSingleton<IWorkOrderValidator, WorkOrderValidator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var normalDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    normalDbContext.Database.EnsureCreated();

    //var encryptedDbContext = scope.ServiceProvider.GetRequiredService<EncryptedAppDbContext>();
    //encryptedDbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    //options.SwaggerEndpoint("v1/workorder.json", "Work Order V1");
});

app.Run();
