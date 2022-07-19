using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.ArendaAz.Interfaces
{
    public interface ITypeOfPropertyArendaAz
    {
        int GetTypeOfProperty(string type);
        int GetTitleOfProperty(string type);
    }
}
