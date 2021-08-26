﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    [Table(name: "owner")]
    public class Owner
    {
        [Column(name: "id")]
        public int Id { get; set; }

        [Column(name: "phone")]
        public string Phone { get; set; }
    }
}
