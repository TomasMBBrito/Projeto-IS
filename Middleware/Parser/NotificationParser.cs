using System;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace Aplicação_cliente
{
    public static class NotificationParser
    {
        public static (string orderName, string status, bool success) ParseNotification(string xmlContent)
        {
            try
            {
                // Load XML
                XDocument doc = XDocument.Parse(xmlContent);
                XNamespace ns = "http://schemas.somiod.com/notification";

                //Check root element
                if (doc.Root == null)
                {
                    Console.WriteLine("DEBUG: Root is NULL", "Parser Debug");
                    return (null, null, false);
                }

                var resourceElement = doc.Root.Element(ns + "Resource");

                // Check Resource
                if (resourceElement == null)
                {
                    Console.WriteLine("DEBUG: Resource element is NULL");
                    return (null, null, false);
                }

                // Check if it's a ContentInstance
                var contentInstance = resourceElement.Element(ns + "ContentInstance");
                if (contentInstance == null)
                {
                    Console.WriteLine("DEBUG: ContentInstance is NULL");
                    return (null, null, false);
                }

                string content = contentInstance.Element(ns + "Content")?.Value;
                if (string.IsNullOrEmpty(content))
                {
                    Console.WriteLine("DEBUG: Content is NULL or empty");
                    return (null, null, false);
                }              
                //content = WebUtility.HtmlDecode(content).Trim();

                // Parse the inner XML content (OrderUpdate)
                var orderDoc = XDocument.Parse(content);

                //Check orderDoc root
                if (orderDoc.Root == null)
                {
                    Console.WriteLine("DEBUG: OrderDoc Root is NULL", "Parser Debug");
                    return (null, null, false);
                }

                string orderName = orderDoc.Root.Element("OrderName")?.Value;
                string status = orderDoc.Root.Element("Status")?.Value;
               

                if (!string.IsNullOrEmpty(orderName) && !string.IsNullOrEmpty(status))
                {
                    return (orderName, status, true);
                }

                return (null, null, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG EXCEPTION:\n{ex.Message}\n\nStack:\n{ex.StackTrace}", "Parser Exception");
                return (null, null, false);
            }
        }

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