using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public  async Task CreateAsync(Rieltor rieltor)
        {
                string uQuery = "INSERT INTO rieltor (phone)"
                + "VALUES(@Phone); ";
           await connection.ExecuteAsync(uQuery, rieltor);
        }

        public async Task BulkInsertAsync(List<Rieltor> rieltors)
        {
            string query = "INSERT INTO rieltor (phone) VALUES ";
            var phones = rieltors.Select(x => $"('{x.Phone}')");
            query += string.Join(",", phones);

            await connection.ExecuteAsync(query);
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
