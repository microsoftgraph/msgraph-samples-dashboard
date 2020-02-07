using System.Collections.Generic;

namespace SamplesDashboard.Models
{
    public class Repo
    {
        public string Name { get; set; }
        public Owner Owner { get; set; }
        public string Status { get; set; }
        public List<string> Language { get; set; }
        public PullRequests pullRequests { get; set; }
        public Issues issues { get; set; }
        public Stargazer Stargazers { get; set; }
        public List<string> FeatureArea { get; set; }
        public SecurityAlerts vulnerabilityAlerts { get; set; }
        public int starsCount
        {
            get => Stargazers?.totalCount ?? -1;
            set
            {
                if (Stargazers is null)
                    Stargazers = new Stargazer { totalCount = value };
                else
                    Stargazers.totalCount = value;
            }
        }

        public int issueCount
        {
            get => issues?.totalCount ?? -1;
            set
            {
                if (issues is null)
                    issues = new Issues { totalCount = value };
                else
                    issues.totalCount = value;
            }
        }

        public int pullRequestCount
        {
            get => pullRequests?.totalCount ?? -1;
            set
            {
                if (pullRequests is null)
                    pullRequests = new PullRequests { totalCount = value };
                else
                    pullRequests.totalCount = value;
            }
        }

        public int vulnerabilityAlertsCount
        {
            get => vulnerabilityAlerts?.totalCount ?? -1;
            set
            {
                if (vulnerabilityAlerts is null)
                    vulnerabilityAlerts = new SecurityAlerts { totalCount = value };
                else
                    vulnerabilityAlerts.totalCount = value;
            }
        }

        public string login
        {
            get => Owner?.login ?? "";
            set
            {
                if (Owner is null)
                    Owner = new Owner { login = value };
                else
                    Owner.login = value;
            }
        }
    }

    public class Issues
    {
        public int totalCount;
    }
    public class Stargazer
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

    public class Owner
    {
        public string login;
    }
}