using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.EvinAz.Interfaces
{
    public interface ITypeOfPropertyEvinAz
    {
        int GetTypeOfProperty(string type);
        int GetTitleOfProperty(string type);
    }
}
