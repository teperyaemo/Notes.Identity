using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Notes.Identity;
using Notes.Identity.Data;
using Notes.Identity.Models;
using System.Security.Cryptography.X509Certificates;

public class Program
{

    private static void Main(string[] args)
    {        
        var builder = WebApplication.CreateBuilder(args);;

        var connectionString = builder.Configuration["DbConnection"];
        builder.Services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

        builder.Services.AddIdentity<AppUser, IdentityRole>(config =>
        {
            config.Password.RequiredLength = 4;
            config.Password.RequireDigit = false;
            config.Password.RequireNonAlphanumeric = false;
            config.Password.RequireUppercase = false;
        })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddIdentityServer()
        .AddAspNetIdentity<AppUser>()
        .AddInMemoryApiResources(Configuration.ApiResources)
        .AddInMemoryIdentityResources(Configuration.IdentityResources)
        .AddInMemoryApiScopes(Configuration.ApiScopes)
        .AddInMemoryClients(Configuration.Clients)
        .AddDeveloperSigningCredential();

        builder.Services.ConfigureApplicationCookie(config =>
        {
            config.Cookie.Name = "Notes.Identity.Cookie";
            config.LoginPath = "/Auth/Login";
            config.LogoutPath = "/Auth/Logout";
        })

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;

            try
            {
                var context = serviceProvider.GetRequiredService<AuthDbContext>();
                DbInitializer.Initialize(context);
            }
            catch (Exception exception)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(exception, "An error occured while app initialization");
            }
        }

        app.UseIdentityServer();
        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}
