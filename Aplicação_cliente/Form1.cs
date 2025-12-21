using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Aplicação_cliente
{
    public partial class EncomendaForm : Form
    {
        string baseURI = @"http://localhost:61331/";
        string app_name = "FnacClient_Vecna";

        private MqttClient client;
        private List<string> subscribed_topics = new List<string>();
        private Dictionary<string, string> orderStatuses = new Dictionary<string, string>();
        public EncomendaForm()
        {
            InitializeComponent();
        }

        private void InitializeMqtt()
        {
            try
            {
                client = new MqttClient("localhost");

                // Registar o evento ANTES de conectar
                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

                // Conectar ao broker
                client.Connect(Guid.NewGuid().ToString());

                if (!client.IsConnected)
                {
                    MessageBox.Show("Error: Could not connect to MQTT broker");
                    client = null; // Garantir que está null se falhar
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to broker: " + ex.Message);
                client = null; // Garantir que está null em caso de exceção
            }
        }

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    string xmlMessage = Encoding.UTF8.GetString(e.Message);
                    string topic = e.Topic;

                    // Parse the XML notification
                    var (orderName, status, success) = NotificationParser.ParseNotification(xmlMessage);

                    if (success && !string.IsNullOrEmpty(orderName) && !string.IsNullOrEmpty(status))
                    {
                        // Update the dictionary
                        orderStatuses[orderName] = status;

                        // Update the ListBox
                        UpdateOrderStatusInListBox(orderName, status);

                        // Get additional info
                        string eventType = NotificationParser.GetEventType(xmlMessage);
                        string timestamp = NotificationParser.GetTimestamp(xmlMessage);

                        MessageBox.Show($"Order Update Received!\n\n" +
                                      $"Order: {orderName}\n" +
                                      $"Status: {status}\n" +
                                      $"Event: {eventType}\n" +
                                      $"Time: {timestamp}");
                    }
                    else
                    {
                        // Fallback: show raw notification
                        MessageBox.Show($"Notification received!\nTopic: {topic}\nMessage: {xmlMessage}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing notification: {ex.Message}");
                }
            });
        }

        private string ExtractXmlValue(string xml, string tagName)
        {
            string startTag = $"<{tagName}>";
            string endTag = $"</{tagName}>";

            int startIndex = xml.IndexOf(startTag);
            int endIndex = xml.IndexOf(endTag);

            if (startIndex >= 0 && endIndex > startIndex)
            {
                startIndex += startTag.Length;
                return xml.Substring(startIndex, endIndex - startIndex);
            }

            return null;
        }

        // Update the ListBox to show status next to order
        private void UpdateOrderStatusInListBox(string orderName, string status)
        {
            for (int i = 0; i < listBoxEncomendas.Items.Count; i++)
            {
                string item = listBoxEncomendas.Items[i].ToString();

                // Check if this item contains the order name
                if (item.Contains(orderName))
                {
                    // Extract the creation date part
                    int createdIndex = item.IndexOf("Criada em :");
                    string createdPart = createdIndex >= 0 ? item.Substring(createdIndex) : "";

                    // Rebuild the item with status
                    string updatedItem = $"{orderName} - Status: {status} | {createdPart}";
                    listBoxEncomendas.Items[i] = updatedItem;
                    break;
                }
            }
        }

        private void EncomendaForm_Load(object sender, EventArgs e)
        {

            var client_rest = new RestClient(baseURI);
            var request = new RestRequest("api/somiod", Method.Post);
            //request.AddHeader("content-type", "application/json");
            var application = new CreateResourceRequest()
            {
                ResType = "application",
                ResourceName = app_name
                //ResType = "application"
            };

            Console.WriteLine(application.ResType);


            string jsonBody = JsonConvert.SerializeObject(application);
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            Console.WriteLine("JSON sendo enviado:");
            Console.WriteLine(jsonBody);
            var response = client_rest.Execute(request);
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                //MessageBox.Show("Application already exists. Procede");
                return;
            }
            else
            {
                MessageBox.Show("Error creating application: " + response.Content);
                return;
            }
        }

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
                if (textBoxProduto.Text.Length <= 0) 
            {
                MessageBox.Show("Por favor insira o nome de um produto");
                return;
            }

            string produto = textBoxProduto.Text;

            var invalid_char = new[]
            {
                '!', '@', '#', '$', '%', '^', '&', '*', '(', ')',
                '-', '+', '=', '{', '}', '[', ']', '|', '\\', ':',
                ';', '"', '\'', '<', '>', ',', '.', '?', '/', '~', '`'
            };

            foreach (char c in invalid_char) { 
                if (produto.Contains(c))
                {
                    MessageBox.Show("O nome do produto nao pode conter caracteres especiais");
                    return; 
                }
            }
            

            if ((int)product_quantity.Value < 1 || (int) product_quantity.Value > 50)
            {
                MessageBox.Show("Por favor insira uma quantidade valida");
                return;
            }


            int quantidade = (int)product_quantity.Value;

            listBoxProdutos.Items.Add($"{produto} - Quantidade: {quantidade}" + Environment.NewLine);

        }

        private void btnCreateOrder_Click(object sender, EventArgs e)
        {
            if(listBoxProdutos.Items.Count == 0)
            {
                MessageBox.Show("Por favor adicione produtos à encomenda antes de criar.");
                return;
            }

            InitializeMqtt();
            if (client == null || !client.IsConnected)
            {
                MessageBox.Show("MQTT client is not connected. Please restart the application.");
                return;
            }

            listBoxProdutos.Items.Clear();

            var encomenda = new
            {
                name = "encomenda_" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                created_at = DateTime.UtcNow.ToString("yyyy-MM-dd")
            };

            listBoxEncomendas.Items.Add(encomenda.name + "... " + " Criada em : " + encomenda.created_at + Environment.NewLine);

            var client_rest = new RestClient(baseURI);
            var request = new RestRequest($"api/somiod/{app_name}", Method.Post);
            var container = new CreateResourceRequest()
            {
                ResType = "container",
                ResourceName = encomenda.name
            };

            string jsonBody = JsonConvert.SerializeObject(container);
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            var response = client_rest.Execute(request);
            if(response.StatusCode == HttpStatusCode.Created)
            {
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                MessageBox.Show("Order already made");
                return;
            }
            else
            {
                MessageBox.Show("Error creating order: " + response.Content);
                return;
            }

            var request_subscription = new RestRequest($"api/somiod/{app_name}/{encomenda.name}", Method.Post);
            var subscription = new CreateResourceRequest()
            {
                ResourceName = "sub_" + encomenda.name,
                ResType = "subscription",
                Evt = 1,
                Endpoint = "mqtt://localhost:1883"
            };

            string jsonSubBody = JsonConvert.SerializeObject(subscription);
            request_subscription.AddParameter("application/json", jsonSubBody, ParameterType.RequestBody);
            var resposta_sub = client_rest.Execute(request_subscription);
            if (resposta_sub.StatusCode == HttpStatusCode.Created)
            {

                string topic =  $"{app_name}/{encomenda.name}";



                client.Subscribe(new string[] { topic },
                 new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

                subscribed_topics.Add(topic);

            }
            else if (resposta_sub.StatusCode == HttpStatusCode.Conflict)
            {
                MessageBox.Show("Notification about order already made");
                return;
            }
            else
            {
                MessageBox.Show("Error on asking to notify: " + resposta_sub.Content);
                return;
            }
        }

        private void EncomendaForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(client != null && client.IsConnected)
            {
                client.Disconnect();
            }
            //base.OnFormClosing(e);
        }

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
}
