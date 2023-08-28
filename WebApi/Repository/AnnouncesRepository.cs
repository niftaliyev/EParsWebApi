using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.ViewModels;

namespace WebApi.Repository
{
    public class AnnouncesRepository : IRepository<Announce>
    {
        private readonly MySqlConnection connection;
        public AnnouncesRepository(MySqlConnection connection)
        {
            this.connection = connection;
        }
        public IEnumerable<Announce> GetAll()
        {
            var query = "select * from announces";
            return connection.Query<Announce>(query);
        }
        public async Task<int> CreateAsync(Announce announce)
        {
            string query = "INSERT INTO announces (original_id, mobile,name" +
                ",price,cover,parser_site" +
                ",images,logo_images,room_count" +
                ",rent_type,property_type,announce_type" +
                ",region_id,settlement_id,metro_id " +
                ",apartment_id,mark,address,google_map," +
                "floor_count,current_floor,space,document" +
                ",communal,text,view_count,announcer,announce_date,original_date,repair,city_id,number_checked,kredit)"

                + "VALUES(@original_id,@mobile,@name" +
                ",@price,@cover,@parser_site" +
                ",@images,@logo_images,@room_count" +
                ",@rent_type,@property_type,@announce_type " +
                ",@region_id,@settlement_id,@metro_id" +
                ",@apartment_id,@mark,@address,@google_map" +
                ",@floor_count,@current_floor,@space,@document" +
                ",@communal,@text,@view_count,@announcer,@announce_date,@original_date,@repair,@city_id,@number_checked,@kredit); SELECT last_insert_id();";

            var lastId = await connection.ExecuteScalarAsync<long>(query, announce);
            return ((int)lastId);
        }
        public Announce GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(AnnounceImageUpdateViewModel updateViewModel)
        {
            await Task.Run(() =>
            {
                string uQuery = "UPDATE announces SET images = @Images WHERE id = @LastId";
                connection.Execute(uQuery, updateViewModel);
            });

        }

        public async Task<IEnumerable<int>> GetAnnouncesByMobileListAsync(string[] mobileList)
        {
            
            StringBuilder sb = new();
            for (int i = 0; i < mobileList.Length; i++)
            {
                if (mobileList.Length > 1 && mobileList.Length - 1 != i)
                {
                    sb.AppendLine($"'{mobileList[i]}',");
                }
                else
                {
                    sb.AppendLine($"'{mobileList[i]}'");
                }
            }

            string query = $"SELECT id FROM announces WHERE mobile in ({sb}) and announcer = 1";

            var result = await connection.QueryAsync<int>(query);

            return result;
        } 

        public async Task UpdateAnnouncerAsync(AnnounceAnnouncerUpdateViewModel updateViewModel)
        {
            string uQuery = "UPDATE announces SET announcer = @Announcer , number_checked = true WHERE id = @AnnounceId";

            await connection.ExecuteAsync(uQuery, updateViewModel);
        }

        public Task ArendaAzUpdateAnnouncerAsync(ArendaAzAnnouncerUpdateVM updateViewModel)
        {
            return Task.Run(() =>
            {
                string uQuery = "UPDATE announces SET announcer = @Announcer , number_checked = true WHERE id = @Id";
                connection.Execute(uQuery, updateViewModel);
            });
        }

        public async Task<bool> IsAnnounceValidAsync(AnnounceSearchViewModel searchVM)
        {
            string query = "SELECT COUNT(*) FROM announces WHERE original_id = @OriginalId and parser_site = @ParserSite";
            var count = await connection.ExecuteScalarAsync<long>(query, searchVM);

            return count == 0 ? false : true;
        }
    }
}
