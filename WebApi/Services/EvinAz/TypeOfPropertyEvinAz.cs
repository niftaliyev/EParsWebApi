using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.EvinAz.Interfaces;

namespace WebApi.Services.EvinAz
{
    public class TypeOfPropertyEvinAz : ITypeOfPropertyEvinAz
    {
        public int GetTitleOfProperty(string type)
        {

            if (type.EndsWith("Yeni tikili"))
                return 1;
            else if (type.EndsWith("Köhnə tikili"))
                return 2;
            else if (type.EndsWith("Ev/villa"))
                return 5;
            else if (type.EndsWith("Bağ evi"))
                return 4;
            else if (type.EndsWith("Ofis"))
                return 6;
            else if (type.EndsWith("Obyekt"))
                return 7;
            else if (type.EndsWith("Magaza"))
                return 8;
            else if (type.EndsWith("Torpaq sahəsi"))
                return 9;
            else if (type.EndsWith("Qaraj"))
                return 10;

            return 0;


        }

        public int GetTypeOfProperty(string type)
        {

            if (type.Contains("İcarə"))
                return 1;
            else if (type.Contains("Satılır"))
                return 2;


            return 0;
        }
    }
}
