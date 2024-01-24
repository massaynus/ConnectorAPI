using ConnectorAPI.DbContexts;
using ConnectorAPI.Repositories;
using ConnectorAPI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using ConnectorAPI.DbContexts.ConnectorDb;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddMemoryCache();
        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddControllers()
            .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                }
            );
        builder.Services.AddDbContext<ConnectorDbContext>(
            options => options
            .UseSqlServer(builder.Configuration.GetConnectionString("ConnectorContext"))
        );

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<ConnectionManagerService>();
        builder.Services.AddScoped<AccessRepository>();

        //Auth Section
        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);
        builder.Services.AddAuthorizationBuilder();
        builder.Services.AddIdentityCore<User>()
            .AddEntityFrameworkStores<ConnectorDbContext>()
            .AddApiEndpoints();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}