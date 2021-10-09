using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Metro
    {
        public int id { get; set; }
        public string name { get; set; }
        public string map { get; set; }
        public int region_id { get; set; }
        public int settlement_id { get; set; }
    }
}
