using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Treynessen.Database.Entities
{
    public class Visitor
    {
        public int ID { get; set; }
        [Required]
        public string IPAdress { get; set; }
        public int IPStringHash { get; set; }
        [Required]
        public DateTime FirstVisit { get; set; }
        [Required]
        public DateTime LastVisit { get; set; }

        public List<VisitedPage> VisitedPages { get; set; }
    }
}