using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Net.Http;
using WebApi.Extensions;
using WebApi.Helpers;
using WebApi.Jobs;
using WebApi.Proxy;
using WebApi.Repository;
using WebApi.Services;
using WebApi.Services.EmlakAz;
using WebApi.Services.EmlakAz.Interfaces;
using WebApi.Services.TapAz;
using WebApi.Services.TapAz.Interfaces;
using WebApi.Services.YeniEmlakAz;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddProxyServerConfigurations(options =>
            {
                options.Username = Configuration["ProxyServer:UsernameProxyServer"];
                options.Password = Configuration["ProxyServer:PasswordProxyServer"];
            });

             //services.AddTransient(x => new UnitOfWork("Server=localhost;Port=3306;Uid=root;Pwd='';Database=emlakcrawler;SslMode = none;"));
            services.AddTransient(x => new UnitOfWork($"Server={Configuration["ConnectionStrings:server"]};Port=3306;Uid={Configuration["ConnectionStrings:username"]};Pwd={Configuration["ConnectionStrings:password"]};Database={Configuration["ConnectionStrings:dbname"]};SslMode = none;"));
            //services.AddTransient(x => new UnitOfWork($"Server=localhost;Port=3306;Uid=emlakcrawler;Pwd=elgun123;Database=emlakcrawler;SslMode = none;"));
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EParseWebApi", Version = "v1" });
            });
            services.AddHttpClient();
            services.AddTransient<FileUploadHelper>();
            services.AddTransient<HttpClientCreater>();
            services.AddTransient<TapAzMetrosNames>();
            services.AddTransient<TapAzRegionsNames>();
            services.AddTransient<TapAzSettlementsNames>();
            services.AddTransient<EmlakAzMetrosNames>();
            services.AddTransient<EmlakAzMarksNames>();
            services.AddTransient<EmlakAzRegionsNames>();
            services.AddTransient<EmlakAzSettlementNames>();
            services.AddTransient<YeniEmlakAzRegionsNames>();

            services.AddTransient<ITypeOfProperty, TypeOfProperty>();
            services.AddTransient<ITypeOfPropertyTapAz, TypeOfPropertyTapAz>();
            services.AddParsers();
            services.AddJobs();
            services.AddHttpClient<ProxysHttpClient>().
                ConfigurePrimaryHttpMessageHandler((c => new HttpClientHandler()
                {
                    Proxy = new WebProxy(Configuration["ProxyOptions:Address"])
                    {
                        Credentials = new NetworkCredential { UserName = Configuration["ProxyServer:Username"], Password = Configuration["ProxyServer:Password"] }
                    }
                }));
            services.AddCors(); // добавляем сервисы CORS
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();// For the wwwroot folder

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(builder => builder.AllowAnyOrigin());
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });
            
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
