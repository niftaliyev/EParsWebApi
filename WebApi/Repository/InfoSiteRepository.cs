using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Repository
{
    public class InfoSiteRepository : IRepository<InfoSite>
    {
        private readonly MySqlConnection connection;
        public InfoSiteRepository(MySqlConnection connection)
        {
            this.connection = connection;
        }
        public IEnumerable<InfoSite> GetAll()
        {
            throw new NotImplementedException();
        }

        public InfoSite GetById(int id)
        {
            throw new NotImplementedException();
        }

        public InfoSite GetBySiteName(string site)
        {
            var query = "select * from info_site where parser_site = @site";
            try
            {
                return connection.Query<InfoSite>(query, new { site }).First();
            }
            catch (Exception e)
            {

                throw;
            }


        }

        public void Update(InfoSite model)
        {
            var q = "UPDATE info_site SET last_index = @last_index , last_page = @last_page WHERE id = @id";
            
            try
            {
                connection.Execute(q, model);
            }
            catch (Exception e)
            {

                throw;
            }


        }

        public void UpdatePage(InfoSite model)
        {
            var q = "UPDATE info_site SET last_page = @last_page WHERE id = @id";

            try
            {
                connection.Execute(q, model);
            }
            catch (Exception e)
            {

                throw;
            }


        }
    }
}
