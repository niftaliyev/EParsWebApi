using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Region
    {
        public int id { get; set; }
        public string name { get; set; }
        public int city_id { get; set; }
    }
}
