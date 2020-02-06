using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SamplesDashboard.Models
{
    public class Repo
    {
        public string Name { get; set; }
        public string NameWithOwner { get; set; }
        public string Status { get; set; }
        public List<string> Language { get; set; }
        public PullRequests pullRequests { get; set; }
        public Issues issues { get; set; }
        public Stars stargazers { get; set; }
        public List<string> FeatureArea{ get; set; }
        public SecurityAlerts vulnerabilityAlerts { get; set; }

    }

    public class Issues
    {
        public int totalCount;
    }
    public class Stars 
    {
        public int totalCount;
    }
    public class PullRequests 
    {
        public int totalCount;
    }
    public class SecurityAlerts 
    {
        public int totalCount;
    }
}
