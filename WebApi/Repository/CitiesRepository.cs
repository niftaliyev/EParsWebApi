using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Repository
{
    public class CitiesRepository : IRepository<Cities>
    {
        private readonly MySqlConnection connection;
        public CitiesRepository(MySqlConnection connection)
        {
            this.connection = connection;
        }
        public IEnumerable<Cities> GetAll()
        {
            var query = "select * from cities";

            return connection.Query<Cities>(query);
        }

        public IEnumerable<Region> GetAllRegions()
        {
            var query = "select * from regions";

            return connection.Query<Region>(query);
        }
        public IEnumerable<Settlement> GetAllSettlement()
        {
            var query = "select * from settlement";

            return connection.Query<Settlement>(query);
        }
        public Cities GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
