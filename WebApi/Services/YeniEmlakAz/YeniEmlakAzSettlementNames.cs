using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.YeniEmlakAz
{
    public class YeniEmlakAzSettlementNames
    {
        Dictionary<string, int> settlementNames = new Dictionary<string, int>();

        public YeniEmlakAzSettlementNames()
        {
            settlementNames.Add("28 may qəs.",18);
            settlementNames.Add("6-cı mikrorayon",19);
            settlementNames.Add("7-ci mikrorayon",20);
            settlementNames.Add("8-ci mikrorayon",21);
            settlementNames.Add("9-cu mikrorayon",22);
            settlementNames.Add("Biləcəri qəs.",23);
            settlementNames.Add("Binəqədi qəs.",24);
            settlementNames.Add("Çiçək qəs.",2);
            settlementNames.Add("Rəsulzadə qəs.",100);
            settlementNames.Add("Sulutəpə qəs.",28);
            settlementNames.Add("Xocəsən qəs.",25);
            settlementNames.Add("Dərnəgül",101);
        }

        public Dictionary<string, int> GetSettlementNamesAll()
        {
            return settlementNames;
        }
    }
}
