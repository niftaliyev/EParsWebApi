using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Services.EmlaktapAz
{
    public class EmlaktapAzParser
    {
        private HttpClient _httpClient;
        private readonly EmlakBaza _emlakBaza;
        HttpResponseMessage header;
        private readonly UnitOfWork _unitOfWork;
        private readonly TypeOfPropertyEmlaktapAz _propertyType;
        private readonly EmlaktapAzMetrosNames _metrosNames;
        private readonly EmlaktapAzSettlementNames _settlementNames;
        private readonly EmlaktapAzMarksNames _marksNames;
        private readonly EmlaktapAzImageUploader _imageUploader;
        //static string[] proxies = SingletonProxyServersIp.Instance;
       // private readonly HttpClientCreater clientCreater;
        private static bool isActive = false;
        public const int maxRequest = 50;

        public EmlaktapAzParser(HttpClient httpClient,
                                UnitOfWork unitOfWork,
                                EmlakBaza emlakBaza,
                                TypeOfPropertyEmlaktapAz propertyType,
                                EmlaktapAzSettlementNames settlementNames,
                                EmlaktapAzMetrosNames metrosNames,
                                EmlaktapAzMarksNames marksNames,
                                EmlaktapAzImageUploader imageUploader
                               //HttpClientCreater httpClientCreater
                               )
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _emlakBaza = emlakBaza;
            _propertyType = propertyType;
            _settlementNames = settlementNames;
            _metrosNames = metrosNames;
            _marksNames = marksNames;
            _imageUploader = imageUploader;
            // clientCreater = httpClientCreater;
        }

        public async Task EmlaktapAzPars()
        {
            var model = _unitOfWork.ParserAnnounceRepository.GetBySiteName("http://emlaktap.az");

            
        }
    }
}