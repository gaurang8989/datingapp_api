using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Like
    {
        public int LikerId { get; set; } // Collection of the User Entity
        public int LikeeId { get; set; }
        public User Liker { get; set; }  //User Properties
        public User Likee { get; set; }
    }
}
