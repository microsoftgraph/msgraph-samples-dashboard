// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace SamplesDashboard
{
    using SamplesDashboard.Models;
    using System.Collections.Generic;

    public class Data
    {
        public Organization organization { get; set; }
    }

    public class Organization
    {
        public Repository repository { get; set; }
    }

    public class Repository
    {
        public DependencyGraphManifests dependencyGraphManifests { get; set; }
    }

    public class DependencyGraphManifests
    {
        public List<DependencyGraphManifestsNode> nodes { get; set; }
    }

    public class DependencyGraphManifestsNode
    {
        public string filename { get; set; }
        public Dependencies dependencies { get; set; }
    }

    public class Dependencies
    {
        public List<DependenciesNode> nodes { get; set; }
        public long totalCount { get; set; }
    }

    public class DependenciesNode
    {
        public string packageManager { get; set; }
        public string packageName { get; set; }
        public string requirements { get; set; }
    }

}