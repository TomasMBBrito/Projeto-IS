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

namespace Aplicação_cliente
{
    public partial class EncomendaForm : Form
    {
        string baseURI = @"http://localhost:61331/";
        string app_name = "FnacClient_Vecna";
        public EncomendaForm()
        {
            InitializeComponent();
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
            if (response.StatusCode == HttpStatusCode.Created)
            {
                MessageBox.Show("Application created.");
                if (response.Content != null)
                {
                    MessageBox.Show("Response Content: " + response.Content);
                }

            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                MessageBox.Show("Application already exists. Procede");
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
            listBoxProdutos.Items.Clear();

            var encomenda = new
            {
                name = "container_" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
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
            if (response.StatusCode == HttpStatusCode.Created)
            {
                MessageBox.Show("Order created.");
                if (response.Content != null)
                {
                    //MessageBox.Show("Response Content: " + response.Content);
                }

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
                Endpoint = baseURI
            };

            string jsonSubBody = JsonConvert.SerializeObject(subscription);
            request_subscription.AddParameter("application/json", jsonSubBody, ParameterType.RequestBody);
            var resposta_sub = client_rest.Execute(request_subscription);
            if (resposta_sub.StatusCode == HttpStatusCode.Created)
            {
                MessageBox.Show("Notifications about order activated.");
                if (resposta_sub.Content != null)
                {
                    //MessageBox.Show("Response Content: " + resposta_sub.Content);
                }

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
    }
}
