using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.ViewModels;

namespace WebApi.Repository
{
    public class OwnerRepository : IRepository<Rieltor>
    {
        private readonly MySqlConnection connection;
        public OwnerRepository(MySqlConnection connection)
        {
            this.connection = connection;
        }
        public void CreateAsync(OwnerViewModel owner)
        {
           
                string uQuery = "INSERT INTO owner (phone)"

                + "VALUES(@Phone); ";
            connection.Execute(uQuery,owner);
            

        }
        public IEnumerable<Rieltor> GetAll()
        {
            throw new NotImplementedException();
        }

        public Rieltor GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
