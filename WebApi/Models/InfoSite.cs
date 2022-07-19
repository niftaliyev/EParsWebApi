using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class InfoSite
    {
        public int id { get; set; }
        public int last_index { get; set; }
        public int last_page { get; set; }
        public string parser_site { get; set; }
    }
}
