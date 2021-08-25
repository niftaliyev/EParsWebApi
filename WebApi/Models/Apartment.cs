using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    [Table(name: "apartments")]
    public class Apartment
    {
        [Column(name: "id")]
        public int Id { get; set; }

        [Column(name: "title")]
        public string Title { get; set; }

        [Column(name: "description")]
        public string Description { get; set; }

        [Column(name: "keywords")]
        public string Keywords { get; set; }

        [Column(name: "about_company")]
        public string Aboutcompany { get; set; }

        [Column(name: "image")]
        public string Image { get; set; }

        [Column(name: "logo")]
        public string Logo { get; set; }

        [Column(name: "slug")]
        public string Slug { get; set; }

        [Column(name: "position")]
        public int Position { get; set; }

        [Column(name: "status")]
        public int Status { get; set; }

        [Column(name: "view_count")]
        public int ViewCount { get; set; }
    }
}
