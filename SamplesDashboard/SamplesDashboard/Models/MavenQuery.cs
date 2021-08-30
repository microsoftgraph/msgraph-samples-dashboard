// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

/*
{
  ...
  "response": {
    "numFound": 77,
    "docs": [
      {
        "id": "com.squareup.okhttp3:okhttp:5.0.0-alpha.2",
        "g": "com.squareup.okhttp3",
        "a": "okhttp",
        "v": "5.0.0-alpha.2",
        "p": "jar",
        ...
      },
      ...
    ]
  }
}
 */

namespace SamplesDashboard.Models
{
    public class MavenQuery
    {
        public MavenResponse Response { get; set; }

    }
    public class MavenResponse
    {
        public int NumFound { get; set; }
        public List<MavenPackageInfo> Docs { get; set; }
    }

    public class MavenPackageInfo
    {
        public string Id { get; set; }

        [JsonProperty("v")]
        public string Version { get; set; }
    }
}
