using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.UnvanAz
{
    public interface ITypeOfPropertyUnvanAz
    {
        int GetTypeOfProperty(string type);
        int GetTitleOfProperty(string type);
    }
}
