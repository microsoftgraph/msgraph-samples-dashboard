// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace SamplesDashboard.Models
{
    public class YamlHeaderExtensions
    {
        public string[] Services { get; set; }
    }
    public class YamlHeader
    {
        public string[] Languages { get; set; }
        public YamlHeaderExtensions Extensions { get; set; }
        public string DependencyFile { get; set; }
    }
}
