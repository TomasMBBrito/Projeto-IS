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

    public class ApplicationResponse
    {
        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        [JsonProperty("creation-datetime")]
        public string CreationDatetime { get; set; }
    }

    /// <summary>
    /// Response DTO for Container resource
    /// Only exposes user-facing properties (no internal IDs)
    /// </summary>
    public class ContainerResponse
    {
        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        [JsonProperty("creation-datetime")]
        public string CreationDatetime { get; set; }
    }

    /// <summary>
    /// Response DTO for Content-Instance resource
    /// Only exposes user-facing properties (no internal IDs)
    /// </summary>
    public class ContentInstanceResponse
    {
        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        [JsonProperty("content-type")]
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("creation-datetime")]
        public string CreationDatetime { get; set; }
    }

    /// <summary>
    /// Response DTO for Subscription resource
    /// Only exposes user-facing properties (no internal IDs)
    /// </summary>
    public class SubscriptionResponse
    {
        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        [JsonProperty("evt")]
        public int Evt { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("creation-datetime")]
        public string CreationDatetime { get; set; }
    }
}