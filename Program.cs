using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using WebApplication1.Interfaces;
using WebApplication1.Repositories;
using WebApplication1.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Work Order" , Version = "v1" });
});

builder.Services.AddSingleton<IWorkOrderRepository, WorkOrderRepository>();
builder.Services.AddSingleton<IWorkOrderValidator, WorkOrderValidator>();

var app = builder.Build();

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
