﻿using ConnectorAPI.DbContexts;
using ConnectorAPI.Repositories;
using ConnectorAPI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using ConnectorAPI.DbContexts.ConnectorDb;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using ConnectorAPI.Extensions;
using ConnectorAPI;
using ConnectorAPI.Service;
using System.Net;

internal class Program
{
    private static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/logFile.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

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
        builder.Services.AddSingleton<TokenStorageService>();
        builder.Services.AddSingleton<CryptoService>();
        builder.Services.AddSingleton<UserIPStorageService>();

        builder.Services.AddScoped<AccessRepository>();
        builder.Services.AddScoped<TokenizerService>();
        builder.Services.AddHttpLogging(options =>
        {
            options.CombineLogs = true;
            options.LoggingFields = HttpLoggingFields.RequestMethod
                                    | HttpLoggingFields.RequestPath
                                    | HttpLoggingFields.RequestQuery
                                    | HttpLoggingFields.ResponseStatusCode
                                    | HttpLoggingFields.Duration;
        });

        //Auth Section
        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);
        builder.Services.AddAuthorizationBuilder();
        builder.Services.AddIdentityCore<User>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.SignIn.RequireConfirmedPhoneNumber = false;
            options.SignIn.RequireConfirmedAccount = false;
            options.SignIn.RequireConfirmedEmail = false;
        })
            .AddEntityFrameworkStores<ConnectorDbContext>()
            .AddApiEndpoints();

        var app = builder.Build();
        app.UseHttpLogging();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.Use(async (HttpContext context, RequestDelegate req) =>
        {
            var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                await req.Invoke(context);
                return;
            }

            var scp = app.Services.CreateScope();
            var ipStorageService = scp.ServiceProvider.GetRequiredService<UserIPStorageService>();
            var userManager = scp.ServiceProvider.GetRequiredService<UserManager<User>>();

            var username = userManager.GetUserName(context.User);
            if (username is null) context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            else
            {
                var reqIp = context.Connection.RemoteIpAddress?.ToString();
                var uIp = ipStorageService.GetIP(username);

                if (reqIp != uIp) context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                else await req.Invoke(context);
            }

        });

        app.MapCustomIdentityApi<User>();
        app.MapControllers()
            .RequireAuthorization();

        try
        {
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}