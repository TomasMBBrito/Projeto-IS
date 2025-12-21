using Middleware.Models;
using Middleware.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Middleware.Services
{
    public class NotificationService
    {
        private readonly string _connectionString;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _xsdPath;
        private XmlSchemaSet _schemaSet;

        public NotificationService(string connectionString, string xsdPath = null)
        {
            _connectionString = connectionString;
            _xsdPath = xsdPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NotificationSchema.xsd");

            // Load XSD schema
            LoadXsdSchema();
        }

        /// <summary>
        /// Load and compile the XSD schema
        /// </summary>
        private void LoadXsdSchema()
        {
            try
            {
                _schemaSet = new XmlSchemaSet();
                _schemaSet.Add("http://schemas.somiod.com/notification", _xsdPath);
                _schemaSet.Compile();
                Console.WriteLine("[INFO] XSD Schema loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Failed to load XSD schema: {ex.Message}");
                _schemaSet = null;
            }
        }

        /// <summary>
        /// Trigger notifications to subscriptions of the container
        /// </summary>
        public async Task TriggerNotifications(int container_id, int event_type, object resource_data, string container_path)
        {
            try
            {
                List<Subscription> subscriptions = GetSubscriptionsForEvent(container_id, event_type);

                System.Diagnostics.Debug.WriteLine($"[NOTIFY] Subscriptions found: {subscriptions.Count}");

                if (subscriptions.Count == 0)
                {
                    return;
                }

                // Convert resource_data to XML model
                NotificationXml notification = CreateNotificationXml(event_type, resource_data);

                // Serialize to XML
                string xmlPayload = SerializeToXml(notification);

                // Validate against XSD
                if (!ValidateXml(xmlPayload))
                {
                    Console.WriteLine("[ERROR] XML validation failed. Notification not sent.");
                    return;
                }

                Console.WriteLine("[SUCCESS] XML validated successfully");

                // Trigger notifications for each subscription
                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        if (IsHttpEndpoint(subscription.Endpoint))
                        {
                            await SendHttpNotification(subscription.Endpoint, xmlPayload);
                        }
                        else if (IsMqttEndpoint(subscription.Endpoint))
                        {
                            await SendMqttNotification(subscription.Endpoint, container_path, xmlPayload);
                        }
                        else
                        {
                            Console.WriteLine($"[WARNING] Unknown endpoint type: {subscription.Endpoint}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Failed to send notification to {subscription.Endpoint}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TriggerNotifications failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create NotificationXml object from resource data
        /// </summary>
        private NotificationXml CreateNotificationXml(int event_type, object resource_data)
        {
            var notification = new NotificationXml
            {
                Evt = event_type,
                EventType = event_type == 1 ? "creation" : "deletion",
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                Resource = new ResourceXml()
            };

            // Determine resource type and populate
            if (resource_data is ContentInstanceResponse ciResponse)
            {
                notification.Resource.Data = new ContentInstanceXml
                {
                    ResourceName = ciResponse.ResourceName,
                    ContentType = ciResponse.ContentType,
                    Content = ciResponse.Content,
                    CreationDatetime = ciResponse.CreationDatetime
                };
            }
            else if (resource_data is SubscriptionResponse subResponse)
            {
                notification.Resource.Data = new SubscriptionXml
                {
                    ResourceName = subResponse.ResourceName,
                    Evt = subResponse.Evt,
                    Endpoint = subResponse.Endpoint,
                    CreationDatetime = subResponse.CreationDatetime
                };
            }
            else
            {
                throw new ArgumentException("Invalid resource data type");
            }

            return notification;
        }

        /// <summary>
        /// Serialize NotificationXml to XML string
        /// </summary>
        private string SerializeToXml(NotificationXml notification)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(NotificationXml));
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    OmitXmlDeclaration = false,
                    Encoding = Encoding.UTF8
                };

                using (var stringWriter = new StringWriter())
                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    serializer.Serialize(xmlWriter, notification);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] XML serialization failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validate XML against XSD schema
        /// </summary>
        private bool ValidateXml(string xmlContent)
        {
            if (_schemaSet == null)
            {
                Console.WriteLine("[WARNING] XSD schema not loaded. Skipping validation.");
                return true; // Allow notification if schema not loaded
            }

            try
            {
                var settings = new XmlReaderSettings
                {
                    Schemas = _schemaSet,
                    ValidationType = ValidationType.Schema
                };

                bool isValid = true;
                settings.ValidationEventHandler += (sender, args) =>
                {
                    Console.WriteLine($"[VALIDATION ERROR] {args.Severity}: {args.Message}");
                    isValid = false;
                };

                using (var stringReader = new StringReader(xmlContent))
                using (var xmlReader = XmlReader.Create(stringReader, settings))
                {
                    while (xmlReader.Read()) { }
                }

                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] XML validation exception: {ex.Message}");
                return false;
            }
        }

        private List<Subscription> GetSubscriptionsForEvent(int container_id, int eventype)
        {
            List<Subscription> subscriptions = new List<Subscription>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(@"SELECT Id, Name, ContainerId, Evt, Endpoint, Created_at 
                                         FROM Subscriptions 
                                         WHERE ContainerId = @containerId AND Evt = @eventtype", con);
                cmd.Parameters.AddWithValue("@containerId", container_id);
                cmd.Parameters.AddWithValue("@eventtype", eventype);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subscriptions.Add(new Subscription()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                            ContainerId = reader.GetInt32(reader.GetOrdinal("ContainerId")),
                            Evt = reader.GetInt32(reader.GetOrdinal("Evt")),
                            Endpoint = reader.IsDBNull(reader.GetOrdinal("Endpoint")) ? string.Empty : reader.GetString(reader.GetOrdinal("Endpoint")),
                            Created_at = reader.GetDateTime(reader.GetOrdinal("Created_at"))
                        });
                    }
                }
            }

            return subscriptions;
        }

        /// <summary>
        /// Send notification via HTTP POST
        /// </summary>
        private async Task SendHttpNotification(string endpoint, string xmlPayload)
        {
            try
            {
                var content = new StringContent(xmlPayload, Encoding.UTF8, "application/xml");

                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[SUCCESS] HTTP notification sent to {endpoint}");
                }
                else
                {
                    Console.WriteLine($"[WARNING] HTTP notification failed with status {response.StatusCode} for {endpoint}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] HTTP notification exception for {endpoint}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Send notification via MQTT
        /// </summary>
        private async Task SendMqttNotification(string brokerEndpoint, string containerPath, string xmlPayload)
        {
            MqttClient client = null;

            try
            {
                string broker = ParseMqttBroker(brokerEndpoint);
                client = new MqttClient(broker);

                string clientId = Guid.NewGuid().ToString();

                await Task.Run(() => client.Connect(clientId))
                          .ConfigureAwait(false);

                if (!client.IsConnected)
                {
                    throw new Exception("Failed to connect to MQTT broker");
                }

                string topic = containerPath;
                System.Diagnostics.Debug.WriteLine(xmlPayload);

                await Task.Run(() =>
                    client.Publish(
                        topic,
                        Encoding.UTF8.GetBytes(xmlPayload),
                        MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
                        false))
                    .ConfigureAwait(false);

                await Task.Delay(1000);

                Console.WriteLine($"[SUCCESS] MQTT notification sent to {broker} on topic {topic}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] MQTT notification exception: {ex.Message}");
                throw;
            }
            finally
            {
                if (client != null && client.IsConnected)
                {
                    client.Disconnect();
                }
            }
        }

        private bool IsHttpEndpoint(string endpoint)
        {
            return endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsMqttEndpoint(string endpoint)
        {
            return endpoint.StartsWith("mqtt://", StringComparison.OrdinalIgnoreCase) ||
                   (!endpoint.Contains("http://") && !endpoint.Contains("https://"));
        }

        private string ParseMqttBroker(string endpoint)
        {
            string broker = endpoint.Replace("mqtt://", "");

            if (broker.Contains(":"))
            {
                broker = broker.Split(':')[0];
            }

            return broker;
        }
    }
}