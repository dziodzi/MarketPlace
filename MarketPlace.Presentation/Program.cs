using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MarketPlace.BLL;
using MarketPlace.DAL.Interfaces;
using MarketPlace.DAL.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var configuration = builder.Configuration;

if (configuration["DataAccess:Type"] == "Database")
{
    builder.Services.AddDbContext<MarketPlaceDbContext>(options =>
        options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddScoped<IMarketPlaceRepository, DbMarketPlaceRepository>();
}
else
{
    builder.Services.AddScoped<IMarketPlaceRepository>(sp =>
        new FileMarketPlaceRepository(
            Path.Combine(AppContext.BaseDirectory, "MarketPlace.DAL", "products.csv"),
            Path.Combine(AppContext.BaseDirectory, "MarketPlace.DAL", "markets.csv"),
            Path.Combine(AppContext.BaseDirectory, "MarketPlace.DAL", "productsInMarkets.csv")
        ));
}

builder.Services.AddScoped<IMarketPlaceService, MarketPlaceService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MarketPlaceDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();