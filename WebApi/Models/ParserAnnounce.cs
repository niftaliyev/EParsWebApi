using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class ParserAnnounce
    {
        public int id { get; set; }

        public string site { get; set; }
        public int last_id { get; set; }
        public bool isActive { get; set; }
    }
}
