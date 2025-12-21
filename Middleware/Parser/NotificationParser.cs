using System;
using System.Xml.Linq;

namespace Aplicação_cliente
{
    /// <summary>
    /// Helper class to parse XML notifications
    /// </summary>
    public static class NotificationParser
    {
        /// <summary>
        /// Parse XML notification and extract order name and status
        /// </summary>
        public static (string orderName, string status, bool success) ParseNotification(string xmlContent)
        {
            try
            {
                // Load XML
                XDocument doc = XDocument.Parse(xmlContent);
                XNamespace ns = "http://schemas.somiod.com/notification";

                // Get Resource element
                var resourceElement = doc.Root?.Element(ns + "Resource");
                if (resourceElement == null)
                {
                    return (null, null, false);
                }

                // Check if it's a ContentInstance
                var contentInstance = resourceElement.Element(ns + "ContentInstance");
                if (contentInstance != null)
                {
                    string content = contentInstance.Element(ns + "Content")?.Value;

                    if (!string.IsNullOrEmpty(content))
                    {
                        // Parse the inner XML content (OrderUpdate)
                        var orderDoc = XDocument.Parse(content);
                        string orderName = orderDoc.Root?.Element("OrderName")?.Value;
                        string status = orderDoc.Root?.Element("Status")?.Value;

                        if (!string.IsNullOrEmpty(orderName) && !string.IsNullOrEmpty(status))
                        {
                            return (orderName, status, true);
                        }
                    }
                }

                return (null, null, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing notification: {ex.Message}");
                return (null, null, false);
            }
        }

        /// <summary>
        /// Get event type from notification
        /// </summary>
        public static string GetEventType(string xmlContent)
        {
            try
            {
                XDocument doc = XDocument.Parse(xmlContent);
                XNamespace ns = "http://schemas.somiod.com/notification";
                return doc.Root?.Element(ns + "EventType")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get timestamp from notification
        /// </summary>
        public static string GetTimestamp(string xmlContent)
        {
            try
            {
                XDocument doc = XDocument.Parse(xmlContent);
                XNamespace ns = "http://schemas.somiod.com/notification";
                return doc.Root?.Element(ns + "Timestamp")?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
