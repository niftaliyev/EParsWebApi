using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.EmlaktapAz
{
    public class TypeOfPropertyEmlaktapAz
    {
        public int GetTitleOfProperty(string type)
        {
            if (type.Contains("Yeni Tikili"))
                return 1;
            else if (type.Contains("Köhnə Tikili"))
                return 2;
            else if (type.Contains("Ev / Villa"))
                return 5;
            else if (type.Contains("Bağ"))
                return 4;
            else if (type.Contains("Ofis"))
                return 6;
            else if (type.Contains("Obyekt"))
                return 7;
            else if (type.Contains("Magaza"))
                return 8;
            else if (type.Contains("Torpaq"))
                return 9;
            else if (type.Contains("Qaraj"))
                return 10;

            return 0;


        }

        public int GetTypeOfProperty(string type)
        {
            throw new NotImplementedException();
        }
    }
}
