using System;
using System.Xml.Serialization;

namespace Middleware.Models
{
    /// <summary>
    /// Root notification model for XML serialization
    /// </summary>
    [XmlRoot("Notification", Namespace = "http://schemas.somiod.com/notification")]
    public class NotificationXml
    {
        [XmlElement("Evt")]
        public int Evt { get; set; }

        [XmlElement("EventType")]
        public string EventType { get; set; }

        [XmlElement("Timestamp")]
        public string Timestamp { get; set; }

        [XmlElement("Resource")]
        public ResourceXml Resource { get; set; }
    }

    /// <summary>
    /// Resource wrapper for polymorphic content
    /// </summary>
    public class ResourceXml
    {
        [XmlElement("ContentInstance", Type = typeof(ContentInstanceXml))]
        [XmlElement("Subscription", Type = typeof(SubscriptionXml))]
        public object Data { get; set; }
    }

    /// <summary>
    /// Content Instance for XML serialization
    /// </summary>
    public class ContentInstanceXml
    {
        [XmlElement("ResourceName")]
        public string ResourceName { get; set; }

        [XmlElement("ContentType")]
        public string ContentType { get; set; }

        [XmlElement("Content")]
        public string Content { get; set; }

        [XmlElement("CreationDatetime")]
        public string CreationDatetime { get; set; }
    }

    /// <summary>
    /// Subscription for XML serialization
    /// </summary>
    public class SubscriptionXml
    {
        [XmlElement("ResourceName")]
        public string ResourceName { get; set; }

        [XmlElement("Evt")]
        public int Evt { get; set; }

        [XmlElement("Endpoint")]
        public string Endpoint { get; set; }

        [XmlElement("CreationDatetime")]
        public string CreationDatetime { get; set; }
    }
}