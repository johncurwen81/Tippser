using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tippser.Core
{
    public class ScrapeModel
    {
        

        public DateTime Kickoff { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
    }
}
