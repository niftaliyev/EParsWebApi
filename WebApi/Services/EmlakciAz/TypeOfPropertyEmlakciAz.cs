using System;
using WebApi.Services.EmlakciAz.Interfaces;

namespace WebApi.Services.EmlakciAz
{
    public class TypeOfPropertyEmlakciAz : ITypeOfPropertyEmlakciAz
    {
        public int GetTitleOfProperty(string type)
        {
            if (type.Contains("Yeni tikili"))
                return 1;
            else if (type.Contains("Köhnə tikili"))
                return 2;
            else if (type.Contains("Həyət evi"))
                return 3;
            else if (type.Contains("Bağ"))
                return 4;
            else if (type.Contains("Ev / Villa"))
                return 5;
            else if (type.Contains("Ofis"))
                return 6;
            else if (type.Contains("Qaraj"))
                return 7;
            else if (type.Contains("Torpaq"))
                return 9;
            else if (type.Contains("Obyekt"))
                return 7;


            return 0;
        }

        public int GetTypeOfProperty(string type)
        {
            if (type.StartsWith("İCARƏ (Aylıq)"))
                return 1;
            else if (type.StartsWith("SATIŞ"))
                return 2;
            return 0;
        }
    }
}
