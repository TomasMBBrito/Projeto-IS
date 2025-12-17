using Middleware.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Middleware.Services
{
    public class NotificationService
    {
        private readonly string _connectionString;
        private static readonly HttpClient _httpClient = new HttpClient();

        public NotificationService(string connectionString)
        {
            _connectionString = connectionString;
        }

        ///<summary>
        /// Trigger notifications to subscriptions of the container
        /// </summary>
        public async Task TriggerNotifications(int container_id, int event_type, object resource_data, string container_path)
        {
            try
            {
                List<Subscription> subscriptions = GetSubscriptionsForEvent(container_id, event_type);

                if (subscriptions.Count == 0)
                {
                    return;
                }

                // Preparar payload da notificação
                var notificationPayload = new
                {
                    evt = event_type,
                    eventType = event_type == 1 ? "creation" : "deletion",
                    resource = resource_data,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    containerPath = container_path
                };

                string jsonPayload = JsonConvert.SerializeObject(notificationPayload, Formatting.Indented);

                // Disparar notificações para cada subscription
                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        if (IsHttpEndpoint(subscription.Endpoint))
                        {
                            await SendHttpNotification(subscription.Endpoint, jsonPayload);
                        }
                        else if (IsMqttEndpoint(subscription.Endpoint))
                        {
                            await SendMqttNotification(subscription.Endpoint, container_path, jsonPayload);
                        }
                        else
                        {
                            Console.WriteLine($"[WARNING] Unknown endpoint type: {subscription.Endpoint}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Failed to send notification to {subscription.Endpoint}: {ex.Message}");
                        // Continuar a enviar para outras subscriptions mesmo que uma falhe
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TriggerNotifications failed: {ex.Message}");
                throw;
            }
        }
        

        private List<Subscription> GetSubscriptionsForEvent(int container_id,int eventype) {
            List<Subscription> subscriptions = new List<Subscription>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(@"Select Id,Name, ContainerId, Evt, Endpoint,Created_at FROM Subscriptions Where ContainerId = @containerId AND (Evt = @eventtype)", con);
                cmd.Parameters.AddWithValue("@containerId", container_id);
                cmd.Parameters.AddWithValue("@eventtype", eventype);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subscriptions.Add(new Subscription()
                        {
                            Id = (int)reader["Id"],
                            Name = (string)reader["Name"],
                            ContainerId = (int)reader["ContainerId"],
                            Evt = (int)reader["Evt"],
                            Endpoint = (string)reader["Endpoint"],
                            Created_at = (DateTime)reader["Created_at"]

                        });
                    }
                }
            }

                return subscriptions;
        }

        /// <summary>
        /// Envia notificação via HTTP POST
        /// </summary>
        private async Task SendHttpNotification(string endpoint, string jsonPayload)
        {
            try
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

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
        private async Task SendMqttNotification(string brokerEndpoint, string containerPath, string jsonPayload)
        {
            MqttClient client = null;

            try
            {
                // Parse broker endpoint (ex: "mqtt://localhost:1883" ou apenas "localhost")
                string broker = ParseMqttBroker(brokerEndpoint);
                int port = ParseMqttPort(brokerEndpoint);

                client = new MqttClient(broker, port, false, null, null, MqttSslProtocols.None);

                // Conectar ao broker
                string clientId = Guid.NewGuid().ToString();

                await Task.Run(() => client.Connect(clientId))
                          .ConfigureAwait(false);

                if (!client.IsConnected)
                {
                    throw new Exception("Failed to connect to MQTT broker");
                }

                // O canal/topic é o path do container
                string topic = containerPath;

                // Publicar mensagem
                await Task.Run(() =>
                    client.Publish(
                        topic,
                        Encoding.UTF8.GetBytes(jsonPayload),
                        MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
                        false))
                    .ConfigureAwait(false);

                Console.WriteLine($"[SUCCESS] MQTT notification sent to {broker}:{port} on topic {topic}");
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
                    await Task.Run(() => client.Disconnect())
                              .ConfigureAwait(false);
                }
            }
        }


        /// <summary>
        /// Verify if endpoint is HTTP
        /// </summary>
        private bool IsHttpEndpoint(string endpoint)
        {
            return endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verify if endpoint is MQTT
        /// </summary>
        private bool IsMqttEndpoint(string endpoint)
        {
            return endpoint.StartsWith("mqtt://", StringComparison.OrdinalIgnoreCase) ||
                   (!endpoint.Contains("http://") && !endpoint.Contains("https://"));
        }

        /// <summary>
        /// Extract  broker from endpoint MQTT
        /// </summary>
        private string ParseMqttBroker(string endpoint)
        {
            // Remove "mqtt://" se existir
            string broker = endpoint.Replace("mqtt://", "");

            // Remove porta se existir
            if (broker.Contains(":"))
            {
                broker = broker.Split(':')[0];
            }

            return broker;
        }

        /// <summary>
        /// Extract port from endpoint MQTT (default: 1883)
        /// </summary>
        private int ParseMqttPort(string endpoint)
        {
            if (endpoint.Contains(":"))
            {
                string[] parts = endpoint.Replace("mqtt://", "").Split(':');
                if (parts.Length > 1 && int.TryParse(parts[1], out int port))
                {
                    return port;
                }
            }

            return 1883; // Port default MQTT
        }
    }
}
