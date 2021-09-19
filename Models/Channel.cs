using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeParser.Models
{
    public class Channel
    {
        private string telegram;

        public int Number { get; set; }
        public string Link { get; set; }
        public string Name { get; set; }
        public int SubcribersCount { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Telegram
        {
            get
            {
                try
                {
                    return Description.Split().Where(x => x.Contains("https://t.me"))?.FirstOrDefault();
                }
                catch { return telegram; }
            }
            set { telegram = value; }
        }
        public string Description { get; set; }
        public string RegistrationDate { get; set; }
        public int ViewsCount { get; set; }
    }
}
