using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Monitramento.GarbageCollector;
using Newtonsoft.Json.Linq;
using Newtonsoft;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Newtonsoft.Json;
using System.Net.Mime;
using Monitramento.SqlCheck;
using Monitramento.SelfCheck;

namespace Monitramento
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

            services.AddControllersWithViews();
            services.AddHealthChecks()
                .AddGCInfoCheck("Memoria")
                 .AddDiskStorageHealthCheck(s => s.AddDrive("C:\\", 1024))
                 .AddProcessAllocatedMemoryHealthCheck(512)
                .AddCheck<SelfHealthCheck>("Self");
            // alterar a connectionString
            //.AddCheck("Sql", new SqlConnectionHealthCheck("Data Source=Server;Initial Catalog=DataBase;User ID=User;Password=Password"));
            services.AddHealthChecksUI();
                
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
                
            });

            app.UseHealthChecks("/status",
               new HealthCheckOptions()
               {
                   ResponseWriter = async (context, report) =>
                   {
                       var result = JsonConvert.SerializeObject(
                           new
                           {
                               statusApplication = report.Status.ToString(),
                               healthChecks = report.Entries.Select(e => new
                               {
                                   check = e.Key,
                                   ErrorMessage = e.Value.Exception?.Message,
                                   status = Enum.GetName(typeof(HealthStatus), e.Value.Status)
                               })
                           });
                       context.Response.ContentType = MediaTypeNames.Application.Json;
                       await context.Response.WriteAsync(result);
                   }
               });

            // Gera o endpoint que retornará os dados utilizados no dashboard
            app.UseHealthChecks("/status-ui", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            
            // Ativa o dashboard para a visualização da situação de cada Health Check
            app.UseHealthChecksUI(opt => {
                opt.UIPath = "/dashboard";
                opt.AddCustomStylesheet("dotnet.css");
            });
        }
    }
}
