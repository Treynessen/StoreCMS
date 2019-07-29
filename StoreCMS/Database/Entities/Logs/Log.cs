using System;
using System.ComponentModel.DataAnnotations;

namespace Treynessen.Database.Entities
{
    public abstract class Log
    {
        public int ID { get; set; }
        [Required]
        public DateTime Time { get; set; }
        [Required]
        public string Info { get; set; }
    }
}