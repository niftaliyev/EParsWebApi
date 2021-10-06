using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Repository
{
    public interface ICheckNumber
    {
        int CheckNumberForRieltor(params string[] numbers);
        int CheckNumberForOwner(params string[] numbers);
    }
}
