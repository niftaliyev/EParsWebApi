using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WebApi.Helpers;
using WebApi.Jobs;
using WebApi.Repository;
using WebApi.Services;
using WebApi.Services.TapAz;

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
            //services.AddTransient(x => new UnitOfWork("Server=localhost;Port=3306;Uid=weatherapi;Pwd=1n@pFmGImnXiQdTWw;Database=weatherapi;SslMode = none;"));
            services.AddTransient(x => new UnitOfWork("Server=localhost;Port=3306;Uid=root;Pwd='';Database=emlaksoon;SslMode = none;"));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EParseWebApi", Version = "v1" });
            });


            services.AddHttpClient();

            services.AddTransient<FileUploadHelper>();
            services.AddTransient<TapAzParser>();
            services.AddTransient<EmlakBaza>();
            services.AddTransient<TapAzImageUploader>();
            services.AddTransient<HttpClientCreater>();


            services.AddTransient<RecordsJob>();
            services.AddHostedService<RecordsScheduleService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                //c.RoutePrefix = string.Empty;
            });


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
