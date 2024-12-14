using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SNR_BGC.Models;
using System.Net.Http;
using SNR_BGC.Controllers;
using Polly;
using Infrastructure.External.ShopeeWebApi;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SNR_BGC.DataAccess;
using SNR_BGC.Utilities;
using SNR_BGC.Hubs;
using SNR_BGC.Interface;
using SNR_BGC.Services;

namespace SNR_BGC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration; 
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddLogging(builder =>
            {
                builder.AddSerilog(Log.Logger
                    = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: Configuration["Application:Environment:Paths:ErrorLogs"],
                        restrictedToMinimumLevel: LogEventLevel.Error,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        rollingInterval: RollingInterval.Day)
                    .WriteTo.Logger(logger =>
                    {
                        logger.WriteTo.File(
                           path: Configuration["Application:Environment:Paths:ShopeeApiLogs"],
                           restrictedToMinimumLevel: LogEventLevel.Verbose,
                           outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                           rollingInterval: RollingInterval.Day);
                        logger.Filter.ByIncludingOnly(logEvent => logEvent.Properties.TryGetValue(key: "Scope", out LogEventPropertyValue? value) && value.ToString() == "Shopee Api");
                    })
                    .CreateLogger());
            });

            services.AddTransient<IAsyncPolicy>(
               s =>
               {

                   return Policy.Handle<Exception>(ex => ex.GetType() != typeof(Infrastructure.External.ShopeeWebApi.ShopeeApiException))
                   .WaitAndRetryAsync(
                       retryCount: 3,
                       sleepDurationProvider: retryCount => TimeSpan.FromMilliseconds(500));
               });

            services.AddDbContext<UserClass>(options => options.UseSqlServer(Configuration.GetConnectionString("Myconnection"), sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure();
                // You can also configure other options here if needed
            }));

            //services.AddSingleton<IAuthenthicationTokenFlowManager>(
            //   s => new DatabaseStoreAuthenthicationTokenFlowManager(
            //       configuration: s.GetRequiredService<IConfiguration>(),
            //       logger: s.GetRequiredService<ILogger<DatabaseStoreAuthenthicationTokenFlowManager>>(),
            //       httpClientFactory: s.GetRequiredService<IHttpClientFactory>(),
            //       policy: s.GetRequiredService<IAsyncPolicy>(),
            //       userInfoConn: s.GetRequiredService<SNR_BGC.Models.UserClass>()));

            services.AddSingleton<IAuthenthicationTokenFlowManager>(
               s => {
                   using var serviceScope = s.CreateScope();

                   var sp = serviceScope.ServiceProvider;

                   return new DatabaseStoreAuthenthicationTokenFlowManager(
               configuration: sp.GetRequiredService<IConfiguration>(),
               logger: sp.GetRequiredService<ILogger<DatabaseStoreAuthenthicationTokenFlowManager>>(),
               httpClientFactory: sp.GetRequiredService<IHttpClientFactory>(),
               policy: sp.GetRequiredService<IAsyncPolicy>(),
               userInfoConn: sp.GetRequiredService<SNR_BGC.Models.UserClass>());
                   });

            services.AddControllersWithViews().AddRazorRuntimeCompilation();



            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => {
                    options.LoginPath = "/login";
                    /* options.Events = new CookieAuthenticationEvents()
                     {
                          OnSigningOut = async context =>
                         {
                             await Task.CompletedTask;
                         }

                     };*/
                });

            services.AddSingleton<IAuthenthicationTokenProvider, AuthenthicationTokenProvider>();
            services.AddSingleton<IDbAccess, DbAccess>();
            services.AddScoped<IDataRepository, DataRepository>();
            services.AddScoped<IWaybillPrinting, WaybillPrinting>();
            services.AddRazorPages();

            //services.AddHostedService<BackgroundWorkerService>();

            services.AddDistributedMemoryCache();

            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(10);//You can set Time   
            });
            services.AddSignalR();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapHub<ChatHub>("/chatHub");
            });


        }
    }
}
