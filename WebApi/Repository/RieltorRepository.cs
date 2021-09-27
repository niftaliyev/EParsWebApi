using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Repository
{
    public class RieltorRepository : IRepository<Rieltor>
    {
        private readonly MySqlConnection connection;
        public RieltorRepository(MySqlConnection connection)
        {
            this.connection = connection;
        }
        public void Create(Rieltor rieltor)
        {

            string uQuery = "INSERT INTO rieltor (phone)"

                + "VALUES(@phone); ";
            connection.Execute(uQuery, rieltor);
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
