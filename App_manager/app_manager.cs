using System;
using RestSharp;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Web.Script.Serialization;

namespace App_manager
{
    public partial class app_manager : Form
    {

        string baseURI = @"http://localhost:61331/api/somiod/";
        public app_manager()
        {
            InitializeComponent();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            var client_rest = new RestClient(baseURI);
            var request = new RestRequest("",Method.Post);
            //request.AddBody(textBoxName.Text);

            var pedido = new Request_App()
            {
                ResourceName = textBoxName.Text,
                ResType = "application"
            };

            MessageBox.Show(pedido.ToString());

            request.AddBody(pedido);

            var item = client_rest.Execute(request);
            if(item.StatusCode == HttpStatusCode.Created)
            {
                MessageBox.Show($"Application created");
            }
            else if(item.StatusCode == HttpStatusCode.Conflict)
            {
                MessageBox.Show("Error creating application : Application with that name already exists");
            }
            else
            {
                MessageBox.Show("Unknown error : " + item.ErrorMessage);
            }
        }

        private void btnGetApp_Click(object sender, EventArgs e)
        {
            string nome = textBoxName.Text;
            if(nome.Trim().Length <= 0)
            {
                MessageBox.Show("Must insert an app name");
                return;
            }

            var client_rest = new RestClient(baseURI);
            var request = new RestRequest("{nome}", Method.Get);
            request.AddUrlSegment("nome", nome);

            var status = client_rest.Execute(request);
            if(status.StatusCode == HttpStatusCode.OK)
            {
                var serializer = new JavaScriptSerializer();
                Application_Res  app = serializer.Deserialize<Application_Res>(status.Content);
                MessageBox.Show($"Application name : {app.Name} - Created on {app.Created_at}");
            }
            else
            {
                MessageBox.Show("Internal error");
            }


        }

        private void btnDiscover_Click(object sender, EventArgs e)
        {
            listBoxApps.Items.Clear();
            var client_rest = new RestClient(baseURI);
            var request = new RestRequest("", Method.Get);
            request.AddHeader("somiod-discovery", "application");

            var status = client_rest.Execute(request);

            if(status.StatusCode == HttpStatusCode.OK)
            {
                var serializer = new JavaScriptSerializer();
                List<string> paths = serializer.Deserialize<List<string>>(status.Content);
                foreach (var path in paths)
                {
                    listBoxApps.Items.Add($"{path}" + Environment.NewLine);
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            var client_rest = new RestClient(baseURI);

            if(listBoxApps.SelectedItem == null){
                MessageBox.Show("Must select an app");
                return;
            }
            string app_name = listBoxApps.SelectedItem.ToString().Split('/')[3].ToLower();

            var request = new RestRequest("{appname}", Method.Put);
            request.AddUrlSegment("appname", app_name);
            //request.AddBody(textBoxName.Text);

            var pedido = new Request_App()
            {
                ResourceName = textBoxName.Text,
            };

            request.AddBody(pedido);

            var item = client_rest.Execute(request);
            if (item.StatusCode == HttpStatusCode.OK)
            {
                MessageBox.Show($"Application created");
            }
            else if(item.StatusCode == HttpStatusCode.Conflict)
            {
                MessageBox.Show("Error updating application : Application with that name already exists");
            }
            else
            {
                MessageBox.Show("Error updating the app : Bad request");
            }
            
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var client_rest = new RestClient(baseURI);

            if (listBoxApps.SelectedItem == null)
            {
                MessageBox.Show("Must select an app");
                return;
            }
            string app_name = listBoxApps.SelectedItem.ToString().Split('/')[3].ToLower();

            var request = new RestRequest("{appname}", Method.Delete);
            request.AddUrlSegment("appname", app_name);

            var item = client_rest.Execute(request);
            if (item.StatusCode == HttpStatusCode.OK)
            {
                MessageBox.Show($"Application created");
            }
            else if (item.StatusCode == HttpStatusCode.InternalServerError)
            {
                MessageBox.Show("Error updating application : Internal server error");
            }
            else
            {
                MessageBox.Show("Error updating the app : Bad request");
            }
        }
    }
}
