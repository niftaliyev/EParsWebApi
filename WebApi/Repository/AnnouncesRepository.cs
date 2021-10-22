﻿using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public void Create(Announce announce)
        {
            string uQuery = "INSERT INTO announces (original_id, mobile" +
                ",price,cover,parser_site" +
                ",images,logo_images,room_count" +
                ",rent_type,property_type,announce_type" +
                ",region_id,settlement_id,metro_id " +
                ",apartment_id,mark,address,google_map," +
                "floor_count,current_floor,space,document" +
                ",communal,text,view_count,announcer,announce_date,original_date,repair,city_id)"

                + "VALUES(@original_id,@mobile" +
                ",@price,@cover,@parser_site" +
                ",@images,@logo_images,@room_count" +
                ",@rent_type,@property_type,@announce_type " +
                ",@region_id,@settlement_id,@metro_id" +
                ",@apartment_id,@mark,@address,@google_map" +
                ",@floor_count,@current_floor,@space,@document" +
                ",@communal,@text,@view_count,@announcer,@announce_date,@original_date,@repair,@city_id); ";
                connection.Execute(uQuery, announce);

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

        public Task UpdateAnnouncerAsync (AnnounceAnnouncerUpdateViewModel updateViewModel)
        {
            return Task.Run(() =>
            { 
                Console.WriteLine("STAR UPDATE");
                string uQuery = "UPDATE announces SET announcer = @Announcer , number_checked = true WHERE original_id = @OriginalId";
                Console.WriteLine("UPDATED");

                connection.Execute(uQuery, updateViewModel);
            });
        }
    }
}
