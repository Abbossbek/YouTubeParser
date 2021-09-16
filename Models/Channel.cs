using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeParser.Models
{
    public class Channel
    {
        public string Id { get; set; }
        public string Link { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public int SubcribersCount { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Telegram { get; set; }
        public string Description { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int ViewsCount { get; set; }
    }
}
