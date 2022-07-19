using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.UcuzTapAz
{
    public class TypeOfPropertyUcuzTapAz
    {
        public int GetTitleOfProperty(string type)
        {

            if (type.EndsWith("Yeni tikili"))
                return 1;
            else if (type.EndsWith("Köhnə tikili"))
                return 2;
            else if (type.EndsWith("Həyət evi"))
                return 3;
            else if (type.EndsWith("Bağ evi"))
                return 4;
            else if (type.EndsWith("Villa"))
                return 5;
            else if (type.EndsWith("Ofis"))
                return 6;
            else if (type.EndsWith("Obyekt"))
                return 7;
            else if (type.EndsWith("Magaza"))
                return 8;
            else if (type.EndsWith("Torpaq"))
                return 9;
            else if (type.EndsWith("Qarajlar"))
                return 10;

            return 0;


        }

        public int GetTypeOfProperty(string type)
        {
            if (type.Contains("Kirayə"))
                return 1;
            else if (type.Contains("İcarəyə"))
                return 1;
            else if (type.Contains("Sat"))
                return 2;


            return 0;
        }
    }
}
