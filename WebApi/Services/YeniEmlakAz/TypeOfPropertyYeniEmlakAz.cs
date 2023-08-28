using WebApi.Services.YeniEmlakAz.Interfaces;

namespace WebApi.Services.YeniEmlakAz
{
    public class TypeOfPropertyYeniEmlakAz : ITypeOfPropertyYeniEmlakAz
    {
        public int GetTitleOfProperty(string type)
        {
            throw new System.NotImplementedException();
        }

        public int GetTypeOfProperty(string type)
        {


            if (type.Contains("Yeni tikili"))
                return 1;
            else if (type.Contains("Köhnə tikili"))
                return 2;
            else if (type.Contains("Bağ evi"))
                return 4;
            else if (type.Contains("Həyət evi / Villa"))
                return 5;
            else if (type.Contains("Ofis"))
                return 6;
            else if (type.Contains("Obyekt"))
                return 7;
            else if (type.Contains("Magaza"))
                return 8;
            else if (type.Contains("Torpaq sahəsi"))
                return 9;
            else if (type.Contains("Qaraj"))
                return 10;

            return 0;
        }
    }
}
