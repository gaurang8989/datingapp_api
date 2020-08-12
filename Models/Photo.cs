using System;

namespace WebApplication1.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }

        public string PublicId { get; set; }
        public User user { get; set; }
        public int UserId { get; set; }
    }
} 