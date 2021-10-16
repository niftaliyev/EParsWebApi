using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Repository
{
    public class MarksRepository
    {
        private readonly MySqlConnection connection;
        public MarksRepository(MySqlConnection connection)
        {
            this.connection = connection;
        }
        public void Create(AnnounceMark announceMark)
        {
            string uQuery = "INSERT INTO announces_marks (id, announce_id,mark_id)"

                + "VALUES(@id,@announce_id,@mark_id);";
            connection.Execute(uQuery, announceMark);

        }
        public IEnumerable<Mark> GetMarks(string map)
        {
            var query = $"select id,region_id,settlement_id, ST_DISTANCE_SPHERE(map, POINT({map})) as distance from marks HAVING distance <= 500 order by distance";
            return connection.Query<Mark>(query);
        }
    }
}
