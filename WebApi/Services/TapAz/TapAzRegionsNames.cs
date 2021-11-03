using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.TapAz
{
    public class TapAzRegionsNames
    {
        Dictionary<string, int> regionNames = new Dictionary<string, int>();


        public TapAzRegionsNames()
        {
            regionNames.Add("Abşeron r.",1);
            regionNames.Add("Xətai ray.", 3);
            regionNames.Add("Xətai r.", 3);
            regionNames.Add("Nərimanov r.",6);
            regionNames.Add("Nərimanov rayonu", 6);
            regionNames.Add("Səbail r.",11);
            regionNames.Add("Xəzər ray.", 4);
            regionNames.Add("Xəzər r.", 4);
            regionNames.Add("Nəsimi r.",7);
            regionNames.Add("Nəsimi ray.", 7);
            regionNames.Add("Suraxanı r.",12);
            regionNames.Add("Binəqədi r.",2);
            regionNames.Add("Nizami r.",8);
            regionNames.Add("Pirallahı r.",9);
            regionNames.Add("Qaradağ r.",5);
            regionNames.Add("Sabunçu r.",10);
            regionNames.Add("Yasamal r.",13);
            regionNames.Add("Насиминский р.",7);
        }

        public Dictionary<string, int> GetRegionsNamesAll()
        {
            return regionNames;
        }
    }
}
