using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Announce
    {
        public int id { get; set; }
        public int original_id { get; set; }
        public string mobile { get; set; }
        public int price { get; set; }
        public string cover { get; set; }
        public string parser_site { get; set; }
        public string images { get; set; }
        public string logo_images { get; set; }
        public int room_count { get; set; }
        public int rent_type { get; set; }
        public int property_type { get; set; }
        public int announce_type { get; set; }
        public int region_id { get; set; }
        public int settlement_id { get; set; }
        public int metro_id { get; set; }
        public int apartment_id { get; set; }
        public int mark { get; set; }
        public string address { get; set; }
        public string google_map { get; set; }
        public int floor_count { get; set; }
        public int current_floor { get; set; }
        public string space { get; set; }
        public int document { get; set; }
        public string communal { get; set; }
        public string text { get; set; }
        public int view_count { get; set; }
        public int announcer { get; set; }
        public DateTime announce_date { get; set; }
        public string original_date { get; set; }
        public bool number_checked { get; set; }

    }
}
