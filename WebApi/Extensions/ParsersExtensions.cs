using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Repository;
using WebApi.Services;
using WebApi.Services.ArendaAz;
using WebApi.Services.BinaAz;
using WebApi.Services.EmlakAz;
using WebApi.Services.EmlakciAz;
using WebApi.Services.LalafoAz;
using WebApi.Services.TapAz;
using WebApi.Services.YeniEmlakAz;

namespace WebApi.Extensions
{
    public static class ParsersExtensions
    {
        public static IServiceCollection AddParsers(this IServiceCollection services)
        {
            services.AddTransient<TapAzParser>();
            services.AddTransient<EmlakBaza>();
            services.AddTransient<EmlakBazaWithProxy>();
            services.AddTransient<TapAzImageUploader>();
            services.AddTransient<EmlakAzParser>();
            services.AddTransient<EmlakAzImageUploader>();
            services.AddTransient<YeniEmlakAzParser>();
            services.AddTransient<YeniEmlakAzParserImageUploader>();
            services.AddTransient<BinaAzParser>();
            services.AddTransient<BinaAzParserImageUploader>();
            services.AddTransient<EmlakciAzParser>();
            services.AddTransient<EmlakAzImageUploader>();
            services.AddTransient<EmlakciAzImageUploader>();
            services.AddTransient<LalafoAzParser>();
            services.AddTransient<LalafoImageUploader>();


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
            services.AddTransient<YeniEmlakAzMetrosNames>();
            services.AddTransient<YeniEmlakAzCountryNames>();
            services.AddTransient<YeniEmlakAzSettlementNames>();
            services.AddTransient<EmlakciAzCountryNames>();
            services.AddTransient<EmlakciAzMetrosNames>();
            services.AddTransient<EmlakciAzRegionsNames>();
            services.AddTransient<EmlakciAzSettlementNames>();
            services.AddTransient<LalafoCountryNames>();
            services.AddTransient<LalafoSettlementsName>();
            return services;
        }
    }
}
