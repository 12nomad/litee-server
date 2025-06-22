using Litee.Api;
using Litee.Application;
using Litee.Domain;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddCustomCors();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDatabase(configuration);
builder.Services.AddJwtAuthentication(configuration);
builder.Services.AddApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
