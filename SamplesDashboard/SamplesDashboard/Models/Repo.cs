using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SamplesDashboard.Models
{
    public class Repo
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
        public List<string> Language { get; set; }
        public int PullRequests { get; set; }
        public int Issues { get; set; }
        public int Stars{ get; set; }
        public List<string> FeatureArea{ get; set; }
        public int SecurityAlerts { get; set; }

    }
}
