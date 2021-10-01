using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.EmlakAz.Interfaces
{
    public interface ITypeOfProperty
    {
        int GetTypeOfProperty(string type);
        int GetTitleOfProperty(string type);
    }
}
