using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.DashinmazEmlak.Interfaces
{
    interface ITypeOfPropertyDashinmazEmlak
    {
        int GetTypeOfProperty(string type);
        int GetTitleOfProperty(string type);

    }
}
