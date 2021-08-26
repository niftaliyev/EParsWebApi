using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Models;

namespace WebApi.Repository
{
    public class ParserAnnounceRepository : IRepository<ParserAnnounce>
    {
        private readonly MySqlConnection connection;
        public ParserAnnounceRepository(MySqlConnection connection)
        {
            this.connection = connection;
        }
        public IEnumerable<ParserAnnounce> GetAll()
        {
            var query = "select * from parser_announces";

            return connection.Query<ParserAnnounce>(query);
        }
        public ParserAnnounce GetById(int id)
        {
            throw new NotImplementedException();
        }
        public ParserAnnounce GetBySiteName(string siteName)
        {
            var query = "select * from parser_announces where site = @siteName";

            return connection.Query<ParserAnnounce>(query, new { siteName }).First();
        }
        public void Update(ParserAnnounce parserAnnounce)
        {
            string uQuery = "UPDATE parser_announces SET last_id = @last_id , isActive = @isActive WHERE id = @id";
            connection.Execute(uQuery, parserAnnounce);

        }
    }
}
