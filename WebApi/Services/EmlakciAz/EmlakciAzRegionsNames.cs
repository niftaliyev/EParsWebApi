using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.EmlakciAz
{
    public class EmlakciAzRegionsNames
    {
        Dictionary<string, int> citiesNames = new Dictionary<string, int>();

        public EmlakciAzRegionsNames()
        {
            citiesNames.Add("Binəqədi r.", 2);
            citiesNames.Add("Sabunçu r.", 10);
            citiesNames.Add("Nərimanov r.", 6);
            citiesNames.Add("Yasamal r.", 13);
            citiesNames.Add("Xətai r.", 3);
            citiesNames.Add("Qaradağ r.", 5);
            citiesNames.Add("Abşeron r.", 1);
            citiesNames.Add("Nizami r.", 8);
            citiesNames.Add("Nəsimi r.", 7);
            citiesNames.Add("Səbail r.", 11);
            citiesNames.Add("Suraxanı r.", 12);
            citiesNames.Add("Xəzər r.", 4);
            citiesNames.Add("Pirallahı", 9);
        }

        public Dictionary<string, int> GetRegionsNamesAll()
        {
            return citiesNames;
        }
    }
}
