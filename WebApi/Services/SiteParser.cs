using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Services
{
    public abstract class SiteParser
    {
        public abstract Task ParseSite();
        protected abstract void CreateHttpClientByIpAddressFromProxyList(int count, ref int x);
        protected abstract Task<bool> IsAnnouncementAvailableInDbAsync(string siteUrl, int id);
        protected abstract Task PopulateDocumentWithResponseMessageAsync(HttpResponseMessage message);
        protected abstract Task HandleScrapingAsync(int announcementId);
        protected abstract void HandleText();
        protected abstract void HandleAddressRelatedStuff();
        protected abstract void HandlePropertySpecifications();
        protected abstract void HandleAnnouncementType();
        protected abstract string[] HandleMobile();
        protected abstract void HandleSellerName();
        protected abstract Task HandleImagesAsync(int id);
        protected abstract void HandleOriginalDate();
        protected abstract bool ShouldScrapingStop(int duration);
        protected abstract void StopScraping(int id);
    }
}
