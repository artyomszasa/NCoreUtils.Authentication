using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCoreUtils.Authentication;
using NCoreUtils.Data;
using NCoreUtils.OAuth2.Data;
using Newtonsoft.Json;

namespace NCoreUtils.OAuth2.WebService
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", reloadOnChange: false, optional: false)
                .Build();

            services
                .AddSingleton<IConfiguration>(configuration)
                .Configure<OAuth2Configuration>(configuration.GetSection("OAuth2"))
                .Configure<GoogleEncryptionConfiguration>(configuration.GetSection("Google"))
                // client application binding
                .AddScoped<ClientApplicationBinderService>()
                // http context access
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                // OAuth2 core functions
                .AddScoped<OAuth2Core>()
                // Add db context
                .AddOAuth2DbContext(opts => opts.UseNpgsql(configuration.GetConnectionString("Default"), b => b.MigrationsAssembly("NCoreUtils.OAuth2.WebService")))
                // Add data repositories
                .AddOAuth2DataRepositories()
                .AddDataEventHandlers()
                // Encryption
                .AddSingleton<IEncryptionProvider, GoogleEncryptionProvider>()
                // Azonosítás
                .AddLoginAuthenticator(b => b.AddPasswordAuthentication<OAuth2UserManager, int, OAuth2PasswordLogin>())
                // Session level client application id
                .AddScoped<CurrentClientApplication>()
                // MVC
                .AddMvcCore(opts => opts.RespectBrowserAcceptHeader = true)
                    .AddJsonFormatters(opts => opts.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
                    // .AddAuthorization(opts => opts.DefaultPolicy = new AuthorizationPolicyBuilder(WeLoveAuthenticationScheme.Name).RequireAuthenticatedUser().Build())
                    .AddCors();
            services
                .TryAddTransient<OAuth2UserManager>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IDataEventHandlers dataEventHandlers)
        {
            dataEventHandlers.AddImplicitObservers();

            // var pw = PasswordLogin.GeneratePaswordHash("xasdxasd");
            // Console.WriteLine($"Salt: {pw.Salt}");
            // Console.WriteLine($"Hash: {pw.Hash}");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowCredentials().AllowAnyMethod().WithExposedHeaders("X-Access-Token", "X-Total-Count", "Location", "X-Message"))
                .UseMvc(routes => {
                    routes.MapRoute(
                        "oauth2",
                        "{action}",
                        new { controller = "OAuth2" });
                })
                .Run(context => {
                    context.Response.StatusCode = 404;
                    return Task.CompletedTask;
                });
        }
    }
}
