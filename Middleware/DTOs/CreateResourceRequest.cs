using Middleware.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Middleware.DTOs
{
    public class CreateResourceRequest
    {
        [JsonProperty("res-type")]
        public string ResType { get; set; }

        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        // For content-instance
        [JsonProperty("content-type")]
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        // For subscription
        [JsonProperty("evt")]
        public int? Evt { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

    }
}