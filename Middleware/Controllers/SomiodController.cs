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
        private static List<Application> applications = new List<Application>()
        {
            new Application(){ Id=1, Name="App1", Created_at=DateTime.UtcNow },
            new Application(){ Id=2, Name="App2", Created_at=DateTime.UtcNow },
            new Application(){ Id=3, Name="App3", Created_at=DateTime.UtcNow }
        };

        private static List<Container> containers = new List<Container>()
        {
            new Container(){ Id=1, Name="Container1", AplicationId=1, Created_at=DateTime.UtcNow },
            new Container(){ Id=2, Name="Container2", AplicationId=2, Created_at=DateTime.UtcNow },
            new Container(){ Id=3, Name="Container3", AplicationId=1, Created_at=DateTime.UtcNow }
        };

        private static List<ContentInstance> contentInstances = new List<ContentInstance>()
        {
            new ContentInstance(){ Id=1, Name="Content1", ContainerId=1, Created_at=DateTime.UtcNow },
            new ContentInstance(){ Id=2, Name="Content2", ContainerId=2, Created_at=DateTime.UtcNow },
            new ContentInstance(){ Id=3, Name="Content3", ContainerId=1, Created_at=DateTime.UtcNow }
        };

        private static List<Subscription> subscriptions = new List<Subscription>()
        {
            new Subscription(){ Id=1, Name="Subscription1", ContainerId=1, Created_at=DateTime.UtcNow },
            new Subscription(){ Id=2, Name="Subscription2", ContainerId=2, Created_at=DateTime.UtcNow },
            new Subscription(){ Id=3, Name="Subscription3", ContainerId=1, Created_at=DateTime.UtcNow }
        };

        // ============================================
        // APPLICATION ENDPOINTS
        // ============================================

        // POST api/somiod - Create Application
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateApplication([FromBody] CreateResourceRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (request.ResType?.ToLower() != "application")
                return BadRequest("Invalid resource type. Expected 'application'.");

            if (string.IsNullOrWhiteSpace(request.ResourceName))
                return BadRequest("Resource name is required.");

            if (applications.Any(a => a.Name.Equals(request.ResourceName, StringComparison.OrdinalIgnoreCase)))
                return Conflict();

            var app = new Application
            {
                Id = applications.Any() ? applications.Max(a => a.Id) + 1 : 1,
                Name = request.ResourceName,
                Created_at = DateTime.UtcNow
            };

            applications.Add(app);
            return Created($"api/somiod/{app.Name}", app);
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

            var app = applications.FirstOrDefault(a => a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));
            if (app == null)
                return NotFound();

            // Check if header exists
            if (Request.Headers.TryGetValues("somiod-discover", out headerValues)) {
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
            }
        }

        [HttpPut]
        [Route("{appName}")]
        public IHttpActionResult UpdateApplication(string appName, [FromBody] CreateResourceRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            var app = applications.FirstOrDefault(a => a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));
            
            if (app == null)
            {
                return NotFound();
            }

            //REVER
            if (!appName.Equals(request.ResourceName, StringComparison.OrdinalIgnoreCase) &&
                applications.Any(a => a.Name.Equals(request.ResourceName, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict();
            }


            app.Name = request.ResourceName;
            return Ok(app);
        }

        [HttpDelete]
        [Route("{appName}")]
        public IHttpActionResult DeleteApplication(string appName)
        {
            var app = applications.FirstOrDefault(a => a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));
            if (app == null)
            {
                return NotFound();
            }
            applications.Remove(app);
            return Ok();
        }   

        // ============================================
        // CONTAINER ENDPOINTS
        // ============================================

        // POST api/somiod/{appName} - Create Container
        [HttpPost]
        [Route("{appName}")]
        public IHttpActionResult CreateContainer(string appName, [FromBody] Container container)
        {
            if (container == null || string.IsNullOrEmpty(appName) || string.IsNullOrEmpty(container.Name))
            {
                return BadRequest("Invalid data");
            }

            Application app = applications.FirstOrDefault(a => a.Name == appName);
            if (app == null)
            {
                return NotFound();
            }

            if (containers.Any(c => c.Name == container.Name && c.AplicationId == app.Id))
            {
                return Conflict();
            }

            container.Id = containers.Max(c => c.Id) + 1;
            container.AplicationId = app.Id;
            container.Created_at = DateTime.Now;

            containers.Add(container);

            return Created($"api/somiod/{appName}/{container.Name}", container);
        }

        [HttpGet]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult GetContainer(string appName, string containerName)
        {
            Models.Application app = applications.FirstOrDefault(a => a.Name == appName);
            if (app == null)
            {
                return NotFound();
            }

            Container container = containers.FirstOrDefault(c => c.Name == containerName && c.AplicationId == app.Id);
            if (container == null)
            {
                return NotFound();
            }

            return Ok(container);
        }

        [HttpPut]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult UpdateContainer(string appName, string containerName, [FromBody] CreateResourceRequest request)
        {
            if(request == null)
            {
                return BadRequest();
            }

            Application app = applications.FirstOrDefault(a => a.Name.Equals(appName,StringComparison.OrdinalIgnoreCase));
            if (app == null)
            {
                return NotFound();
            }

            Container container = containers.FirstOrDefault(c => c.Name.Equals(containerName,StringComparison.OrdinalIgnoreCase) && c.AplicationId == app.Id);
            if (container == null)
            {
                return NotFound();
            }

            if (containers.Any(c => c.Name == request.ResourceName && c.AplicationId == app.Id) && !containerName.Equals(request.ResourceName,StringComparison.OrdinalIgnoreCase))
            {
                return Conflict();
            }

            container.Name = request.ResourceName;

            return Ok(container);
        }

        [HttpDelete]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult DeleteContainer(string appName, string containerName)
        {
            Application app = applications.FirstOrDefault(a => a.Name == appName);
            if (app == null)
            {
                return NotFound();
            }

            Container container = containers.FirstOrDefault(c => c.Name == containerName && c.AplicationId == app.Id);
            if (container == null)
            {
                return NotFound();
            }

            containers.Remove(container);

            return Ok();
        }
    }
}
