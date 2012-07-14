using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;


namespace ProjectManager.Models
{
    public class News
    {
        public int      ID      { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string   Title   { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date    { get; set; }

        [Required(ErrorMessage = "News text is required")]
        [DataType(DataType.MultilineText)]
        public string   Text    { get; set; }
    }

    public class NewsDBContext : DbContext
    {
        public DbSet<News> News { get; set; }
    }
}