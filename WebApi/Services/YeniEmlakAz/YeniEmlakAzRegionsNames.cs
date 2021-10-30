using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.YeniEmlakAz
{
    public class YeniEmlakAzRegionsNames
    {
        Dictionary<string, int> regionNames = new Dictionary<string, int>();

        public YeniEmlakAzRegionsNames()
        {
            regionNames.Add("Binəqədi rayonu",2);
            regionNames.Add("Nizami rayonu",8);
            regionNames.Add("Nərimanov rayonu",6);
            regionNames.Add("Nəsimi rayonu",7);
            regionNames.Add("Qaradağ rayonu",5);
            regionNames.Add("Sabunçu rayonu",10);
            regionNames.Add("Suraxanı rayonu",12);
            regionNames.Add("Səbail rayonu",11);
            regionNames.Add("Xətai rayonu",3);
            regionNames.Add("Yasamal rayonu",13);
            regionNames.Add("Xəzər rayonu",4);
            regionNames.Add("Pirallahı rayonu",9);
        }

        public Dictionary<string, int> GetRegionsNamesAll()
        {
            return regionNames;
        }
    }
}
