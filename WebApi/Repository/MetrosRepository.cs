using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Repository
{
    public class MetrosRepository : IRepository<Metro>
    {
        private readonly MySqlConnection connection;
        public MetrosRepository(MySqlConnection connection)
        {
            this.connection = connection;
        }
        public IEnumerable<Metro> GetAll()
        {
            var query = "select * from metros";
            return connection.Query<Metro>(query);
        }

        public Metro GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
