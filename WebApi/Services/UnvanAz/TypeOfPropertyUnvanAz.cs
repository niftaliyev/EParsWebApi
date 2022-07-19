using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.UnvanAz
{
    public class TypeOfPropertyUnvanAz : ITypeOfPropertyUnvanAz
    {
        public int GetTitleOfProperty(string type)
        {
            if (type.StartsWith("Kirayəyə verirəm"))
                return 1;
           
            else if (type.StartsWith("Satıram"))
                return 2;

            return 0;
        }

        public int GetTypeOfProperty(string type)
        {
            if (type.EndsWith("Yeni bina evi"))
                return 1;
            else if (type.EndsWith("Köhnə bina evi"))
                return 2;
            else if (type.EndsWith("Həyət evi / Villa"))
                return 5;
            else if (type.EndsWith("Obyekt / Ofis"))
                return 7;
            else if (type.EndsWith("Torpaq sahəsi"))
                return 9;


            return 0;
        }
    }
}
