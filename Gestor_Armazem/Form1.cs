using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Gestor_Armazem
{
    public partial class GestorEncomendaForm : Form
    {
        string baseURI = @"http://localhost:61331/api/somiod";
        string app_name = "FnacWarehouse_UpsideDown";

        private Thread searchThread;
        private bool isSearching = false;
        private int searchInterval = 5000; 
        public GestorEncomendaForm()
        {
            InitializeComponent();
        }

        private void UpdateApplicationsList(List<string> applications)
        {
            // Clear and repopulate only if the list has changed
            if (applications == null || applications.Count == 0)
            {
                if (listBoxApplications.Items.Count > 0)
                {
                    listBoxApplications.Items.Clear();
                }
                return;
            }

            // Get current items
            var currentItems = new HashSet<string>();
            foreach (string item in listBoxApplications.Items)
            {
                currentItems.Add(item);
            }

            // Get new items
            var newItems = new HashSet<string>(applications);

            // Remove items that are no longer in the list
            for (int i = listBoxApplications.Items.Count - 1; i >= 0; i--)
            {
                string item = listBoxApplications.Items[i].ToString();
                if (!newItems.Contains(item))
                {
                    listBoxApplications.Items.RemoveAt(i);
                }
            }

            // Add new items that aren't already in the list
            foreach (var app in applications)
            {
                if (!currentItems.Contains(app))
                {
                    listBoxApplications.Items.Add(app);
                }
            }
        }

        private string ExtractApplicationName(string path)
        {
            // Extract application name from path like "/api/somiod/FnacClient_Vecna"
            if (string.IsNullOrEmpty(path)) return null;

            var parts = path.Split('/');
            return parts.Length > 0 ? parts[parts.Length - 1] : null;
        }

        private string ExtractContainerName(string path)
        {
            // Extract container name from path like "/api/somiod/FnacClient_Vecna/container_ABC123"
            if (string.IsNullOrEmpty(path)) return null;

            var parts = path.Split('/');
            return parts.Length > 0 ? parts[parts.Length - 1] : null;
        }

        private void SearchApplicationsThreadMethod()
        {
            while (isSearching)
            {
                try
                {
                    var clientRest = new RestClient(baseURI);
                    var request = new RestRequest("", Method.Get);
                    request.AddHeader("somiod-discovery", "application");
                    request.RequestFormat = DataFormat.Json;

                    var response = clientRest.Execute(request);

                    if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                    {
                        var applications = JsonConvert.DeserializeObject<List<string>>(response.Content);

                        // Filter and extract FnacClient_ applications
                        var filteredApps = applications
                            .Select(path => ExtractApplicationName(path))
                            .Where(name => !string.IsNullOrEmpty(name) && name.StartsWith("FnacClient_"))
                            .ToList();

                        // Update UI on the UI thread
                        this.Invoke((MethodInvoker)delegate
                        {
                            UpdateApplicationsList(filteredApps);
                        });
                    }
                    else
                    {
                        Console.WriteLine("Error retrieving applications: " + response.Content);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception in search thread: " + ex.Message);
                }

                // Wait before next search
                Thread.Sleep(searchInterval);
            }
        }

        private void StartSearchThread()
        {
            if (searchThread == null || !searchThread.IsAlive)
            {
                isSearching = true;
                searchThread = new Thread(SearchApplicationsThreadMethod);
                searchThread.IsBackground = true; // Thread will stop when form closes
                searchThread.Start();
            }
        }

        private void StopSearchThread()
        {
            isSearching = false;
            if (searchThread != null && searchThread.IsAlive)
            {
                searchThread.Join(2000); // Wait up to 2 seconds for thread to finish
            }
        }

        private void CriarAplicacao()
        {
            var client_rest = new RestClient(baseURI);
            var request = new RestRequest("", Method.Post);
            //request.AddHeader("content-type", "application/json");
            var application = new CreateResourceRequest()
            {
                ResType = "application",
                ResourceName = app_name
            };

            Console.WriteLine(application.ResType);


            string jsonBody = JsonConvert.SerializeObject(application);
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            Console.WriteLine("JSON sendo enviado:");
            Console.WriteLine(jsonBody);
            var response = client_rest.Execute(request);
            if (response.StatusCode == HttpStatusCode.Created)
            {
                MessageBox.Show("Application created.");
                if (response.Content != null)
                {
                    //MessageBox.Show("Response Content: " + response.Content);
                }

            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
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

        private void GestorEncomendaForm_Load(object sender, EventArgs e)
        {
            //CriarAplicacao();
            listBoxApplications.Items.Clear();

            // Start the background search thread
            StartSearchThread();
        }

        private void btnDiscoverOrders_Click(object sender, EventArgs e)
        {
            // Verificar se há uma aplicação selecionada
            if (listBoxApplications.SelectedItem == null)
            {
                MessageBox.Show("Please select a client first.");
                return;
            }

            string selectedApp = listBoxApplications.SelectedItem.ToString();

            try
            {
                var clientRest = new RestClient(baseURI);
                var request = new RestRequest($"/{selectedApp}", Method.Get);
                request.AddHeader("somiod-discovery", "container");
                request.RequestFormat = DataFormat.Json;

                var response = clientRest.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                {
                    var containers = JsonConvert.DeserializeObject<List<string>>(response.Content);

                    listBoxContainers.Items.Clear();

                    if (containers == null || containers.Count == 0)
                    {
                        MessageBox.Show("Orders not found.");
                        return;
                    }

                    foreach (var containerPath in containers)
                    {
                        string containerName = ExtractContainerName(containerPath);
                        if (!string.IsNullOrEmpty(containerName))
                        {
                            listBoxContainers.Items.Add(containerName);
                        }
                    }

                    MessageBox.Show($"{containers.Count} order(s) found.");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    MessageBox.Show("App not found or without orders.");
                    listBoxContainers.Items.Clear();
                }
                else
                {
                    MessageBox.Show("Error on finding orders: " + response.Content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on finding orders: " + ex.Message);
            }
        }

        private void GestorEncomendaForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopSearchThread();
        }

        private void btnProcessOrder_Click(object sender, EventArgs e)
        {
            ProcessOrder("Processing");
        }

        private void btnShipOrder_Click(object sender, EventArgs e)
        {
            ProcessOrder("Sending");
        }

        private void btnDeliverOrder_Click(object sender, EventArgs e)
        {
            ProcessOrder("Delivered");
        }

        private void ProcessOrder(string status)
        {
            if (listBoxApplications.SelectedItem == null)
            {
                MessageBox.Show("Please,select a client first");
                return;
            }

            if (listBoxContainers.SelectedItem == null)
            {
                MessageBox.Show("Please, select an order first");
                return;
            }

            string selectedApp = listBoxApplications.SelectedItem.ToString();
            string selectedOrder = listBoxContainers.SelectedItem.ToString();

            try
            {
                var clientRest = new RestClient(baseURI);
                var request = new RestRequest($"/{selectedApp}/{selectedOrder}", Method.Post);

                // Criar o content-instance com informação do status
                var contentInstance = new CreateResourceRequest()
                {
                    ResType = "content-instance",
                    ResourceName = $"status_{DateTime.Now.Ticks}",
                    ContentType = "application/json",
                    Content = JsonConvert.SerializeObject(new
                    {
                        status = status,
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                        processedBy = app_name
                    })
                };

                string jsonBody = JsonConvert.SerializeObject(contentInstance);
                request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

                var response = clientRest.Execute(request);

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    MessageBox.Show($"Order updated to: {status}\n\nThe client will be notified via MQTT!");

                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    MessageBox.Show("Client or Order not found.");
                }
                else
                {
                    MessageBox.Show("Error on processing order: " + response.Content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on processing order: " + ex.Message);
            }
        }
    }
}
