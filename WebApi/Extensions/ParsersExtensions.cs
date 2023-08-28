using Microsoft.Extensions.DependencyInjection;
using WebApi.Helpers;
using WebApi.Services;
using WebApi.Services.ArendaAz;
using WebApi.Services.BinaAz;
using WebApi.Services.EmlakAz;
using WebApi.Services.EvinAz;
using WebApi.Services.LalafoAz;
using WebApi.Services.TapAz;
using WebApi.Services.UcuzTapAz;
using WebApi.Services.UnvanAz;
using WebApi.Services.VipEmlakAz;
using WebApi.Services.YeniEmlakAz;

namespace WebApi.Extensions
{
    public static class ParsersExtensions
    {
        public static IServiceCollection AddParsers(this IServiceCollection services)
        {
            services.AddTransient<TapAzParser>();
            services.AddTransient<TapAzImageUploader>();
            services.AddTransient<TapAzMetrosNames>();
            services.AddTransient<TapAzRegionsNames>();
            services.AddTransient<TapAzSettlementsNames>();

            services.AddTransient<UnvanAzParser>();
            services.AddTransient<UnvanAzImageUploader>();
            services.AddTransient<UnvanAzSettlementNames>();
            services.AddTransient<UnvanAzMetroNames>();
            services.AddTransient<EmlakBaza>();
            services.AddTransient<ArendaAzEmlakBazaWithProxy>();
            services.AddTransient<UcuzTapAzParser>();
            services.AddTransient<UcuzTapAzImageUploader>();
            services.AddTransient<UcuzTapAzSettlementsNames>();
            services.AddTransient<UcuzTapAzMetroNames>();
            services.AddTransient<UcuzTapAzMarksNames>();
            services.AddTransient<EmlakBazaWithProxy>();
          
            services.AddTransient<EmlakAzParser>();
            services.AddTransient<EmlakAzImageUploader>();
            services.AddTransient<YeniEmlakAzParser>();
            services.AddTransient<YeniEmlakAzParserImageUploader>();
            services.AddTransient<BinaAzParser>();
            services.AddTransient<BinaAzParserImageUploader>();
            //services.AddTransient<EmlakciAzParser>();
            services.AddTransient<EmlakAzImageUploader>();
            //services.AddTransient<EmlakciAzImageUploader>();
            services.AddTransient<LalafoAzParser>();
            services.AddTransient<LalafoImageUploader>();
            //services.AddTransient<DashinmazEmlakParser>();
            //services.AddTransient<DashinmazEmlakImageUploader>();
            services.AddTransient<EvinAzParser>();
            services.AddTransient<EvinAzImageUploader>();
            services.AddTransient<EvinAzMetroNames>();
            services.AddTransient<EvinAzSettlementNames>();





            services.AddTransient<FileUploadHelper>();
            services.AddTransient<EvinAzMetroNames>();
            services.AddTransient<EvinAzSettlementNames>();
            services.AddTransient<HttpClientCreater>();

            services.AddTransient<ArendaAzParser>();
            services.AddTransient<ArendaAzSettlementNames>();
            services.AddTransient<ArendaAzMetroNames>();
            services.AddTransient<ArendaAzImageUploader>();

            services.AddTransient<VipEmlakAzParser>();
            services.AddTransient<VipEmlakAzSettlementNames>();
            services.AddTransient<VipEmlakAzMetroNames>();
            services.AddTransient<VipEmlakAzImageUploader>();

            //services.AddTransient<EmlaktapAzImageUploader>();
            //services.AddTransient<EmlaktapAzParser>();
            //services.AddTransient<EmlaktapAzMarksNames>();
            //services.AddTransient<EmlaktapAzMetrosNames>();
            //services.AddTransient<EmlaktapAzSettlementNames>();
        
            services.AddTransient<EmlakAzMetrosNames>();
            services.AddTransient<EmlakAzMarksNames>();
            services.AddTransient<EmlakAzRegionsNames>();
            services.AddTransient<EmlakAzSettlementNames>();
            services.AddTransient<YeniEmlakAzRegionsNames>();
            services.AddTransient<YeniEmlakAzMetrosNames>();
            services.AddTransient<YeniEmlakAzCountryNames>();
            services.AddTransient<YeniEmlakAzSettlementNames>();
            //services.AddTransient<EmlakciAzCountryNames>();
            //services.AddTransient<EmlakciAzMetrosNames>();
            //services.AddTransient<EmlakciAzRegionsNames>();
            //services.AddTransient<EmlakciAzSettlementNames>();
            services.AddTransient<LalafoCountryNames>();
            services.AddTransient<LalafoSettlementsName>();
            services.AddTransient<LalalafoAzMetrosNames>();
            return services;
        }
    }
}
