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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gestor_Armazem
{
    public partial class GestorEncomendaForm : Form
    {
        string baseURI = @"http://localhost:61331/";
        string app_name = "FnacWarehouse_UpsideDown";
        public GestorEncomendaForm()
        {
            InitializeComponent();
        }

        private void CriarAplicacao()
        {
            var client_rest = new RestClient(baseURI);
            var request = new RestRequest("api/somiod", Method.Post);
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
                MessageBox.Show("Application already exists. Procede");
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

            //var clientRest = new RestClient(baseURI);
            //var request = new RestRequest("", Method.Get);
            //request.AddHeader("somiod-discovery", "application");
            //request.RequestFormat = DataFormat.Json;

            //var response = clientRest.Execute(request);
            //if (response.StatusCode == HttpStatusCode.OK)
            //{
            //    var applications = JsonConvert.DeserializeObject<List<string>>(response.Content);
            //    foreach (var app in applications)
            //    {
            //        listBoxApplications.Items.Add(app);
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Error retrieving applications: " + response.Content);
            //}
        }
    }
}
