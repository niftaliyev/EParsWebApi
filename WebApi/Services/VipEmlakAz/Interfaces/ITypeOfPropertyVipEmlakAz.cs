using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.VipEmlakAz
{
    public interface ITypeOfPropertyVipEmlakAz
    {
        int GetTypeOfProperty(string type);
        int GetTitleOfProperty(string type);
    }
}
