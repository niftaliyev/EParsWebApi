using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.TapAz.Interfaces;

namespace WebApi.Services.TapAz
{
    public class TypeOfPropertyTapAz : ITypeOfPropertyTapAz
    {
        public int GetTitleOfProperty(string type)
        {

            if (type.EndsWith("Yeni tikili"))
                return 1;
            if (type.EndsWith("Köhnə tikili"))
                return 2;
            if (type.EndsWith("Həyət evi"))
                return 3;
            if (type.EndsWith("Bağ evi"))
                return 4;
            if (type.EndsWith("Villa"))
                return 5;
            if (type.EndsWith("Ofis"))
                return 6;
            if (type.EndsWith("Obyekt"))
                return 7;
            if (type.EndsWith("Magaza"))
                return 8;
            if (type.EndsWith("Torpaq"))
                return 9;
            if (type.EndsWith("Qaraj"))
                return 10;

            return 0;


        }

        public int GetTypeOfProperty(string type)
        {
            if (type.StartsWith("Kirayə verilir"))
                return 1;
            if (type.StartsWith("İcarəyə verilir"))
                return 1;
            if (type.StartsWith("Satılır"))
                return 2;
            if (type.StartsWith("Kirayə"))
                return 1;
            if (type.StartsWith("Satış"))
                return 2;
            return 0;
        }
    }
}
