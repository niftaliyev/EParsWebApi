using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Repository
{
    interface IRepository<T>
    {
        T GetById(int id);
        IEnumerable<T> GetAll();
    }
}
