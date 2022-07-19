using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.DashinmazEmlak.Interfaces;

namespace WebApi.Services.DashinmazEmlak
{
    public class TypeOfPropertyDashinmazEmlak : ITypeOfPropertyDashinmazEmlak
    {
        public int GetTitleOfProperty(string type)
        {
            if (type.EndsWith("Yeni tikili"))
                return 1;
            else if (type.EndsWith("Köhnə tikili"))
                return 2;
            else if (type.EndsWith("Həyət evi"))
                return 3;
            else if (type.EndsWith("Bağ"))
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
            else if (type.EndsWith("Qaraj"))
                return 10;

            return 0;


        }

        public int GetTypeOfProperty(string type)
        {
            throw new NotImplementedException();
        }
    }
}
