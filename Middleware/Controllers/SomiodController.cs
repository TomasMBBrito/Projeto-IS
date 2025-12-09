using Middleware.DTOs;
using Middleware.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Application = Middleware.Models.Application;
using Container = Middleware.Models.Container;

namespace Middleware.Controllers
{
    [RoutePrefix("api/somiod")]
    public class SomiodController : ApiController
    {
        string Connectstring = Properties.Settings.Default.connectstr;


        // HARDCODED DATA FOR TESTING PURPOSES
        //private static List<Application> applications = new List<Application>()
        //{
        //    new Application(){ Id=1, Name="App1", Created_at=DateTime.UtcNow },
        //    new Application(){ Id=2, Name="App2", Created_at=DateTime.UtcNow },
        //    new Application(){ Id=3, Name="App3", Created_at=DateTime.UtcNow }
        //};

        //private static List<Container> containers = new List<Container>()
        //{
        //    new Container(){ Id=1, Name="Container1", AplicationId=1, Created_at=DateTime.UtcNow },
        //    new Container(){ Id=2, Name="Container2", AplicationId=2, Created_at=DateTime.UtcNow },
        //    new Container(){ Id=3, Name="Container3", AplicationId=1, Created_at=DateTime.UtcNow }
        //};

        //private static List<ContentInstance> contentInstances = new List<ContentInstance>()
        //{
        //    new ContentInstance(){ Id=1, Name="Content1", ContainerId=1, Created_at=DateTime.UtcNow },
        //    new ContentInstance(){ Id=2, Name="Content2", ContainerId=2, Created_at=DateTime.UtcNow },
        //    new ContentInstance(){ Id=3, Name="Content3", ContainerId=1, Created_at=DateTime.UtcNow }
        //};

        //private static List<Subscription> subscriptions = new List<Subscription>()
        //{
        //    new Subscription(){ Id=1, Name="Subscription1", ContainerId=1, Created_at=DateTime.UtcNow },
        //    new Subscription(){ Id=2, Name="Subscription2", ContainerId=2, Created_at=DateTime.UtcNow },
        //    new Subscription(){ Id=3, Name="Subscription3", ContainerId=1, Created_at=DateTime.UtcNow }
        //};

        // ============================================
        // APPLICATION ENDPOINTS
        // ============================================

        // POST api/somiod - Create Application
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateApplication([FromBody] CreateResourceRequest request)
        {
            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Applications VALUES (@Name,@Created_at)", con);
                cmd.Parameters.AddWithValue("@Name", request.ResourceName);
                cmd.Parameters.AddWithValue("@Created_at", DateTime.UtcNow);

                int n_rows = cmd.ExecuteNonQuery();
                if (n_rows > 0)
                {
                    return Created($"api/somiod/{request.ResourceName}", "Application"); ;
                }
                else
                {
                    return BadRequest();
                }
            }


            //if (request == null)
            //    return BadRequest("Request body is required.");

            //if (request.ResType?.ToLower() != "application")
            //    return BadRequest("Invalid resource type. Expected 'application'.");

            //if (string.IsNullOrWhiteSpace(request.ResourceName))
            //    return BadRequest("Resource name is required.");

            //if (applications.Any(a => a.Name.Equals(request.ResourceName, StringComparison.OrdinalIgnoreCase)))
            //    return Conflict();

            //var app = new Application
            //{
            //    Id = applications.Any() ? applications.Max(a => a.Id) + 1 : 1,
            //    Name = request.ResourceName,
            //    Created_at = DateTime.UtcNow
            //};

            //applications.Add(app);
            //return Created($"api/somiod/{app.Name}", app);
        }

        [HttpGet]
        [Route("test/test/test")]
        public IHttpActionResult Gettestapp()
        {
            Application app = null;


            using (SqlConnection con = new SqlConnection(Connectstring))
            {

                SqlCommand cmd = new SqlCommand("SELECT * FROM Applications WHERE ID = 2", con);
                cmd.Connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        app = new Application();
                        app.Id = (int)reader["Id"];
                        app.Name = (string)reader["Name"];
                    }
                }
            }
            if (app == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(app);
            }
        }

        //GET APPLICATION OR DISCOVER RELATED TO APPLICATIONS

        [HttpGet]
        [Route("{appName}")]
        public IHttpActionResult GetApplicationOrDiscoverRelatedTo(string appName)
        {
            IEnumerable<string> headerValues;

            // Load application from DB
            Application app = null;
            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Applications WHERE Name = @name", con);
                cmd.Parameters.AddWithValue("@name", appName);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        app = new Application
                        {
                            Id = (int)reader["Id"],
                            Name = (string)reader["Name"]
                            // optionally set Created_at if needed:
                            // Created_at = (DateTime)reader["Created_at"]
                        };
                    }
                }
            }

            if (app == null)
                return NotFound();

            // If discovery header present, return paths of requested type
            if (Request.Headers.TryGetValues("somiod-discover", out headerValues))
            {
                string discoveryType = headerValues.FirstOrDefault();
                var paths = new List<string>();

                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();

                    if (discoveryType == "container")
                    {
                        SqlCommand cmd = new SqlCommand("SELECT Name FROM Containers WHERE AplicationId = @appId", con);
                        cmd.Parameters.AddWithValue("@appId", app.Id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                paths.Add($"/api/somiod/{appName}/{(string)reader["Name"]}");
                            }
                        }
                    }
                    else if (discoveryType == "content-instance")
                    {
                        SqlCommand cmd = new SqlCommand(
                            "SELECT c.Name AS ContainerName, ci.Name AS ContentName " +
                            "FROM Containers c JOIN ContentInstances ci ON c.Id = ci.ContainerId " +
                            "WHERE c.AplicationId = @appId", con);
                        cmd.Parameters.AddWithValue("@appId", app.Id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                paths.Add($"/api/somiod/{appName}/{(string)reader["ContainerName"]}/{(string)reader["ContentName"]}");
                            }
                        }
                    }
                    else if (discoveryType == "subscription")
                    {
                        SqlCommand cmd = new SqlCommand(
                            "SELECT c.Name AS ContainerName, s.Name AS SubscriptionName " +
                            "FROM Containers c JOIN Subscriptions s ON c.Id = s.ContainerId " +
                            "WHERE c.AplicationId = @appId", con);
                        cmd.Parameters.AddWithValue("@appId", app.Id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                paths.Add($"/api/somiod/{appName}/{(string)reader["ContainerName"]}/{(string)reader["SubscriptionName"]}");
                            }
                        }
                    }
                }

                return Ok(paths);
            }
            else
            {
                // Regular GET application
                return Ok(app);
            }

            /*
            CODIGO ANTIGO QUE NÃO DAVA BUILD!!

            NÃO APAGUEI PORQUE PODE ESTAR BEM MAS COM UM ERROZITO

            O CODIGO ACIMA É UM REPLACE TEMPORARIO DO COPILOT

            SE ESTIVER TUDO BEM PODEM APAGAR ESTE CODIGO TODO COMENTADO

            OU CONCERTA LO E SUBSTITUIR PELO CODIGO DO COPILOT
            
            var app = applications.FirstOrDefault(a => a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));
            if (app == null)
                return NotFound();

            // Check if header exists
            if (Request.Headers.TryGetValues("somiod-discover", out headerValues))
            {
                // Header exists - get the value
                string discoveryType = headerValues.FirstOrDefault();
                List<string> paths = new List<string>();

                if (discoveryType == "container")
                {
                    paths = containers
                       .Where(c => c.AplicationId == app.Id)
                       .Select(c => $"/api/somiod/{appName}/{c.Name}")
                   .ToList();
                }

                if (discoveryType == "content-instance")
                {
                    // find containers of the application
                    var appContainers = containers
                        .Where(c => c.AplicationId == app.Id)
                        .ToList();

                    // find content instances inside those containers
                    paths = appContainers
                        .SelectMany(container => contentInstances
                            .Where(ci => ci.ContainerId == container.Id)
                            .Select(ci => $"/api/somiod/{appName}/{container.Name}/{ci.Name}")
                        )
                        .ToList();
                }

                if (discoveryType == "subscription")
                {
                    // find containers of the application
                    var appContainers = containers
                        .Where(c => c.AplicationId == app.Id)
                        .ToList();

                    // find content instances inside those containers
                    paths = appContainers
                        .SelectMany(container => subscriptions
                            .Where(s => s.ContainerId == container.Id)
                            .Select(s => $"/api/somiod/{appName}/{container.Name}/{s.Name}")
                        )
                        .ToList();
                }

                return Ok(paths);
            }
            else
            {
                // Header doesn't exist - regular GET - GET APPLICATION
                return Ok(app);
            }*/
        }

        [HttpPut]
        [Route("{appName}")]
        public IHttpActionResult UpdateApplication(string appName, [FromBody] CreateResourceRequest request)
        {
            //if (request == null)
            //{
            //    return BadRequest("Request body is required.");
            //}

            //var app = applications.FirstOrDefault(a => a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));

            //if (app == null)
            //{
            //    return NotFound();
            //}

            ////REVER
            //if (!appName.Equals(request.ResourceName, StringComparison.OrdinalIgnoreCase) &&
            //    applications.Any(a => a.Name.Equals(request.ResourceName, StringComparison.OrdinalIgnoreCase)))
            //{
            //    return Conflict();
            //}


            //app.Name = request.ResourceName;
            //return Ok(app);

            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Applications SET Name = @Name WHere Name = @appName", con);
                cmd.Parameters.AddWithValue("@Name", request.ResourceName);
                cmd.Parameters.AddWithValue("@appName", appName);
                int n_rows = cmd.ExecuteNonQuery();
                if (n_rows > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        [HttpDelete]
        [Route("{appName}")]
        public IHttpActionResult DeleteApplication(string appName)
        {

            //var app = applications.FirstOrDefault(a => a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));
            //if (app == null)
            //{
            //    return NotFound();
            //}
            //applications.Remove(app);
            //return Ok();

            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Applications WHERE Name = @name");
                cmd.Parameters.AddWithValue("@name", appName);
                int n_rows = cmd.ExecuteNonQuery();
                if (n_rows > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        // ============================================
        // CONTAINER ENDPOINTS
        // ============================================

        // POST api/somiod/{appName} - Create Container
        [HttpPost]
        [Route("{appName}")]
        public IHttpActionResult CreateContainer(string appName, [FromBody] CreateResourceRequest request)
        {
            //if (container == null || string.IsNullOrEmpty(appName) || string.IsNullOrEmpty(container.Name))
            //{
            //    return BadRequest("Invalid data");
            //}

            //Application app = applications.FirstOrDefault(a => a.Name == appName);
            //if (app == null)
            //{
            //    return NotFound();
            //}

            //if (containers.Any(c => c.Name == container.Name && c.AplicationId == app.Id))
            //{
            //    return Conflict();
            //}

            //container.Id = containers.Max(c => c.Id) + 1;
            //container.AplicationId = app.Id;
            //container.Created_at = DateTime.Now;

            //containers.Add(container);

            //return Created($"api/somiod/{appName}/{container.Name}", container);
            Application app = null;
            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Applications WHERE Name = @name");
                cmd.Parameters.AddWithValue("@name", appName);
                cmd.Connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        app = new Application();
                        app.Id = (int)reader["Id"];
                        app.Name = (string)reader["Name"];
                    }
                }

                if (app == null)
                {
                    return NotFound();
                }

                cmd = new SqlCommand("INSERT INTO Containers VALUES (@name,@app_id,@Created_at)", con);
                cmd.Parameters.AddWithValue("@name", request.ResourceName);
                cmd.Parameters.AddWithValue("@app_id", app.Id);
                cmd.Parameters.AddWithValue("@Created_at", DateTime.UtcNow);
                int n_rows = cmd.ExecuteNonQuery();
                if (n_rows > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        [HttpGet]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult GetContainer(string appName, string containerName)
        {
            //Models.Application app = applications.FirstOrDefault(a => a.Name == appName);
            //if (app == null)
            //{
            //    return NotFound();
            //}

            //Container container = containers.FirstOrDefault(c => c.Name == containerName && c.AplicationId == app.Id);
            //if (container == null)
            //{
            //    return NotFound();
            //}

            //return Ok(container);
            //Application app = null;
            Container container = null;
            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                //SqlCommand cmd = new SqlCommand("SELECT * FROM Applications WHERE Name = @name");
                //cmd.Parameters.AddWithValue("@name", appName);
                //cmd.Connection.Open();
                //using (SqlDataReader reader = cmd.ExecuteReader())
                //{
                //    if (reader.Read())
                //    {
                //        app = new Application();
                //        app.Id = (int)reader["Id"];
                //        app.Name = (string)reader["Name"];
                //    }
                //}

                //if(app == null)
                //{
                //    return NotFound();
                //}


                SqlCommand cmd = new SqlCommand("SELECT * FROM Containers WHERE Name = @Name", con);
                cmd.Parameters.AddWithValue("@name", containerName);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        container = new Container();
                        container.Name = (string)reader["Name"];
                        container.Id = (int)reader["Id"];
                    }
                }

                if (container == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(container);
                }
            }
        }

        [HttpPut]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult UpdateContainer(string appName, string containerName, [FromBody] CreateResourceRequest request)
        {
            //if(request == null)
            //{
            //    return BadRequest();
            //}

            //Application app = applications.FirstOrDefault(a => a.Name.Equals(appName,StringComparison.OrdinalIgnoreCase));
            //if (app == null)
            //{
            //    return NotFound();
            //}

            //Container container = containers.FirstOrDefault(c => c.Name.Equals(containerName,StringComparison.OrdinalIgnoreCase) && c.AplicationId == app.Id);
            //if (container == null)
            //{
            //    return NotFound();
            //}

            //if (containers.Any(c => c.Name == request.ResourceName && c.AplicationId == app.Id) && !containerName.Equals(request.ResourceName,StringComparison.OrdinalIgnoreCase))
            //{
            //    return Conflict();
            //}

            //container.Name = request.ResourceName;

            //return Ok(container);

            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Containers SET Name = @Name WHere Name = @contName", con);
                cmd.Parameters.AddWithValue("@Name", request.ResourceName);
                cmd.Parameters.AddWithValue("@contName", containerName);
                int n_rows = cmd.ExecuteNonQuery();
                if (n_rows > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        [HttpDelete]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult DeleteContainer(string appName, string containerName)
        {
            //Application app = applications.FirstOrDefault(a => a.Name == appName);
            //if (app == null)
            //{
            //    return NotFound();
            //}

            //Container container = containers.FirstOrDefault(c => c.Name == containerName && c.AplicationId == app.Id);
            //if (container == null)
            //{
            //    return NotFound();
            //}

            //containers.Remove(container);

            //return Ok();

            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Containers WHERE Name = @name");
                cmd.Parameters.AddWithValue("@name", containerName);
                int n_rows = cmd.ExecuteNonQuery();
                if (n_rows > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        // CREATE
        // Adicionar resource (content-instance)
        [HttpPost]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult CreateContentInstance(string appName, string containerName, [FromBody] CreateResourceRequest request)
        {
            Container container = null;

            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                con.Open();

                // GET ao container
                SqlCommand cmd = new SqlCommand("SELECT * FROM Containers WHERE Name = @name", con);
                cmd.Parameters.AddWithValue("@name", containerName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        container = new Container();
                        container.Id = (int)reader["Id"];
                        container.Name = (string)reader["Name"];
                    }
                }

                if (container == null)
                {
                    return NotFound();
                }

                // Insert do content-instance
                cmd = new SqlCommand("INSERT INTO ContentInstances VALUES (@name, @container_id, @Created_at)", con);
                cmd.Parameters.AddWithValue("@name", request.ResourceName);
                cmd.Parameters.AddWithValue("@container_id", container.Id);
                cmd.Parameters.AddWithValue("@Created_at", DateTime.UtcNow);

                int n_rows = cmd.ExecuteNonQuery();
                if (n_rows > 0)
                {
                    return Created($"api/somiod/{appName}/{containerName}/{request.ResourceName}", "ContentInstance");
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        // CREATE
        // Adicionar resource (subscription)
        [HttpPost]
        [Route("{appName}/{containerName}/subscription")]
        public IHttpActionResult CreateSubscription(string appName, string containerName, [FromBody] CreateResourceRequest request)
        {
            Container container = null;

            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                con.Open();

                // GET ao container
                SqlCommand cmd = new SqlCommand("SELECT * FROM Containers WHERE Name = @name", con);
                cmd.Parameters.AddWithValue("@name", containerName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        container = new Container();
                        container.Id = (int)reader["Id"];
                        container.Name = (string)reader["Name"];
                    }
                }

                if (container == null)
                {
                    return NotFound();
                }

                // Insert da subscription
                cmd = new SqlCommand("INSERT INTO Subscriptions VALUES (@name, @container_id, @Created_at)", con);
                cmd.Parameters.AddWithValue("@name", request.ResourceName);
                cmd.Parameters.AddWithValue("@container_id", container.Id);
                cmd.Parameters.AddWithValue("@Created_at", DateTime.UtcNow);

                int n_rows = cmd.ExecuteNonQuery();
                if (n_rows > 0)
                {
                    return Created($"api/somiod/{appName}/{containerName}/subscription/{request.ResourceName}", "Subscription");
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        // READ
        // Ver resource (content-instance) específico de um container de uma aplicação
        [HttpGet]
        [Route("{appName}/{containerName}/{contentInstanceName}")]
        public IHttpActionResult GetContentInstance(string appName, string containerName, string contentInstanceName)
        {
            ContentInstance contentInstance = null;

            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT * FROM ContentInstances WHERE Name = @name", con);
                cmd.Parameters.AddWithValue("@name", contentInstanceName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        contentInstance = new ContentInstance();
                        contentInstance.Id = (int)reader["Id"];
                        contentInstance.Name = (string)reader["Name"];
                        contentInstance.ContainerId = (int)reader["ContainerId"];
                        contentInstance.Created_at = (DateTime)reader["Created_at"];
                    }
                }

                if (contentInstance == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(contentInstance);
                }
            }
        }


        // READ
        // Ver um resource (subscription)  de um container de uma aplicação
        [HttpGet]
        [Route("{appName}/{containerName}/subscription/{subscriptionName}")]
        public IHttpActionResult GetSubscription(string appName, string containerName, string subscriptionName)
        {
            Subscription subscription = null;

            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT * FROM Subscriptions WHERE Name = @name", con);
                cmd.Parameters.AddWithValue("@name", subscriptionName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        subscription = new Subscription();
                        subscription.Id = (int)reader["Id"];
                        subscription.Name = (string)reader["Name"];
                        subscription.ContainerId = (int)reader["ContainerId"];
                        subscription.Created_at = (DateTime)reader["Created_at"];
                    }
                }

                if (subscription == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(subscription);
                }
            }
        }

        // DELETE
        // Eliminar (content-instance) de um container que pertencente a uma aplicação específica
        [HttpDelete]
        [Route("{appName}/{containerName}/{contentInstanceName}")]
        public IHttpActionResult DeleteContentInstance(string appName, string containerName, string contentInstanceName)
        {
            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("DELETE FROM ContentInstances WHERE Name = @name", con);
                cmd.Parameters.AddWithValue("@name", contentInstanceName);

                int n_rows = cmd.ExecuteNonQuery();
                if (n_rows > 0)
                {
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
        }

        // DELETE
        // Eliminar (subscription) de um container que pertencente a uma aplicação específica
        [HttpDelete]
        [Route("{appName}/{containerName}/subscription/{subscriptionName}")]
        public IHttpActionResult DeleteSubscription(string appName, string containerName, string subscriptionName)
        {
            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("DELETE FROM Subscriptions WHERE Name = @name", con);
                cmd.Parameters.AddWithValue("@name", subscriptionName);

                int n_rows = cmd.ExecuteNonQuery();
                if (n_rows > 0)
                {
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
        }
    }
}
