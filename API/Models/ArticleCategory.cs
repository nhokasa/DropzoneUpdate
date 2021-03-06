using System;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class ArticleCategory
    {
        [Key]
        public string Article_Cate_ID { get; set; }
        public string Article_Cate_Name { get; set; }
        public bool Status { get; set; }
        public int Position { get; set; }
        public string Update_By { get; set; }
        public DateTime Update_Time { get; set; }
    }
}