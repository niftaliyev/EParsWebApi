using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.TapAz.Interfaces
{
    public interface ITypeOfPropertyTapAz
    {
        int GetTypeOfProperty(string type);
        int GetTitleOfProperty(string type);
    }
}
