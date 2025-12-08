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

        // ============================================
        // APPLICATION ENDPOINTS
        // ============================================

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetDiscover()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();

                    // Check if somiod-discover header exists
                    IEnumerable<string> headerValues;
                    if (Request.Headers.TryGetValues("somiod-discover", out headerValues))
                    {
                        string discoveryType = headerValues.FirstOrDefault();
                        List<string> paths = new List<string>();

                        if (discoveryType == "container")
                        {
                            SqlCommand cmd = new SqlCommand(@"
                                SELECT c.Name AS ContainerName, a.Name AS AppName
                                FROM Containers c
                                INNER JOIN Applications a ON c.ApplicationId = a.Id
                            ", con);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string containerName = (string)reader["ContainerName"];
                                    string appName = (string)reader["AppName"];

                                    paths.Add($"/api/somiod/{appName}/{containerName}");
                                }
                            }
                        }
                        else if (discoveryType == "content-instance")
                        {
                            SqlCommand ciCmd = new SqlCommand(@"
                                SELECT c.Name, ci.Name,a.Name 
                                FROM Content_Instances ci
                                INNER JOIN Containers c ON ci.ContainerId = c.Id
                                INNER JOIN Applications a ON c.ApplicationId = a.Id"
                                , con);
                            //ciCmd.Parameters.AddWithValue("@AppId", app.Id);

                            SqlDataReader ciReader = ciCmd.ExecuteReader();
                            while (ciReader.Read())
                            {
                                string appName = ciReader.GetString(2);
                                string containerName = ciReader.GetString(0);
                                string ciName = ciReader.GetString(1);
                                paths.Add($"/api/somiod/{appName}/{containerName}/{ciName}");
                            }
                            ciReader.Close();
                        }
                        else if (discoveryType == "subscription")
                        {
                            SqlCommand ciCmd = new SqlCommand(@"
                                SELECT c.Name, s.Name,a.Name 
                                FROM Subscriptions s
                                INNER JOIN Containers c ON s.ContainerId = c.Id
                                INNER JOIN Applications a ON c.ApplicationId = a.Id"
                                , con);
                            //ciCmd.Parameters.AddWithValue("@AppId", app.Id);

                            SqlDataReader ciReader = ciCmd.ExecuteReader();
                            while (ciReader.Read())
                            {
                                string appName = ciReader.GetString(2);
                                string containerName = ciReader.GetString(0);
                                string subsName = ciReader.GetString(1);
                                paths.Add($"/api/somiod/{appName}/{containerName}/{subsName}");
                            }
                            ciReader.Close();
                        }
                        else if (discoveryType == "application")
                        {
                            SqlCommand appCmd = new SqlCommand("SELECT Id, Name, Created_at FROM Applications", con);

                            SqlDataReader appReader = appCmd.ExecuteReader();

                            while (appReader.Read())
                            {
                                string appName = (string)appReader["Name"];
                                paths.Add($"/api/somiod/{appName}");
                            }
                            appReader.Close();                          
                        }
                        else
                        {
                            return BadRequest("Invalid somiod-discover header.");
                        }

                        return Ok(paths);
                    }
                    else
                    {
                        // No header - return string
                        return Ok("Nothing to discover");
                    }
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }
        }

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
            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO Applications VALUES (@Name, @Created_at); SELECT CAST(SCOPE_IDENTITY() AS INT);", con);
                    cmd.Parameters.AddWithValue("@Name", request.ResourceName);
                    cmd.Parameters.AddWithValue("@Created_at", DateTime.UtcNow);

                    int newId = (int)cmd.ExecuteScalar();

                    var app = new Application
                    {
                        Id = newId,
                        Name = request.ResourceName,
                        Created_at = DateTime.UtcNow
                    };

                    return Created($"api/somiod/{app.Name}", app);
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // Violation of unique constraint - Resource-name
            {
                return Conflict();
                //MELHORAR CATCH
            }
        }

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
            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();

                    // First, get the application
                    SqlCommand appCmd = new SqlCommand("SELECT Id, Name, Created_at FROM Applications WHERE Name = @Name", con);
                    appCmd.Parameters.AddWithValue("@Name", appName);

                    SqlDataReader appReader = appCmd.ExecuteReader();

                    if (!appReader.Read())
                    {
                        appReader.Close();
                        return NotFound();
                    }

                    var app = new Application
                    {
                        Id = (int)appReader["Id"],
                        Name = (string)appReader["Name"],
                        Created_at = (DateTime)appReader["Created_at"]
                    };

                    appReader.Close();

                    // Check if somiod-discover header exists
                    IEnumerable<string> headerValues;
                    if (Request.Headers.TryGetValues("somiod-discover", out headerValues))
                    {
                        string discoveryType = headerValues.FirstOrDefault();
                        List<string> paths = new List<string>();

                        if (discoveryType == "container")
                        {
                            SqlCommand containerCmd = new SqlCommand(
                                "SELECT Name FROM Containers WHERE ApplicationId = @AppId", con);
                            containerCmd.Parameters.AddWithValue("@AppId", app.Id);

                            SqlDataReader containerReader = containerCmd.ExecuteReader();
                            while (containerReader.Read())
                            {
                                string containerName = containerReader.GetString(0);
                                paths.Add($"/api/somiod/{appName}/{containerName}");
                            }
                            containerReader.Close();
                        }
                        else if (discoveryType == "content-instance")
                        {
                            SqlCommand ciCmd = new SqlCommand(@"
                                SELECT c.Name, ci.Name 
                                FROM Content_Instances ci
                                INNER JOIN Containers c ON ci.ContainerId = c.Id
                                WHERE c.ApplicationId = @AppId", con);
                            ciCmd.Parameters.AddWithValue("@AppId", app.Id);

                            SqlDataReader ciReader = ciCmd.ExecuteReader();
                            while (ciReader.Read())
                            {
                                string containerName = ciReader.GetString(0);
                                string ciName = ciReader.GetString(1);
                                paths.Add($"/api/somiod/{appName}/{containerName}/{ciName}");
                            }
                            ciReader.Close();
                        }
                        else if (discoveryType == "subscription")
                        {
                            SqlCommand subCmd = new SqlCommand(@"
                                SELECT c.Name, s.Name 
                                FROM Subscriptions s
                                INNER JOIN Containers c ON s.ContainerId = c.Id
                                WHERE c.ApplicationId = @AppId", con);
                            subCmd.Parameters.AddWithValue("@AppId", app.Id);

                            SqlDataReader subReader = subCmd.ExecuteReader();
                            while (subReader.Read())
                            {
                                string containerName = subReader.GetString(0);
                                string subName = subReader.GetString(1);
                                paths.Add($"/api/somiod/{appName}/{containerName}/{subName}");
                            }
                            subReader.Close();
                        }
                        else
                        {
                            return BadRequest("Invalid somiod-discover header.");
                        }

                        return Ok(paths);
                    }
                    else
                    {
                        // No header - return the application
                        return Ok(app);
                    }
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }
        }

        /*

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
        */



        [HttpPut]
        [Route("{appName}")]
        public IHttpActionResult UpdateApplication(string appName, [FromBody] CreateResourceRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();

                    // First, get the application
                    SqlCommand appCmd = new SqlCommand("SELECT Id, Name, Created_at FROM Applications WHERE Name = @Name", con);
                    appCmd.Parameters.AddWithValue("@Name", appName);

                    SqlDataReader appReader = appCmd.ExecuteReader();

                    if (!appReader.Read())
                    {
                        appReader.Close();
                        return NotFound();
                    }

                    appReader.Close();

                    SqlCommand cmd = new SqlCommand("UPDATE Applications SET Name = @Name WHere Name = @appName", con);
                    cmd.Parameters.AddWithValue("@Name", request.ResourceName);
                    cmd.Parameters.AddWithValue("@appName", appName);
                    int n_rows = cmd.ExecuteNonQuery();
                    if (n_rows > 0)
                    {
                        //PUT NEW APP INSIDE
                        return Ok();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // Violation of unique constraint - Resource-name
            {
                return Conflict();
                //MELHORAR CATCH
            }
        }


        [HttpDelete]
        [Route("{appName}")]
        public IHttpActionResult DeleteApplication(string appName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();

                    // First, get the application
                    SqlCommand appCmd = new SqlCommand("SELECT Id, Name, Created_at FROM Applications WHERE Name = @Name", con);
                    appCmd.Parameters.AddWithValue("@Name", appName);

                    SqlDataReader appReader = appCmd.ExecuteReader();

                    if (!appReader.Read())
                    {
                        appReader.Close();
                        return NotFound();
                    }

                    appReader.Close();

                    SqlCommand cmd = new SqlCommand("DELETE FROM Applications WHERE Name = @name", con);
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
            catch (SqlException ex)
            {
                return InternalServerError(ex);
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
            if(request == null)
            {
                return BadRequest("Body is required");
            }

            if(request.ResType?.ToLower() != "container")
            {
                return BadRequest("Invalid resource type. Must be container");
            }

            if (string.IsNullOrWhiteSpace(request.ResourceName) || string.IsNullOrWhiteSpace(appName))
            {
                return BadRequest("Resource name or application name is required");
            }

            try
            {
                Application app = null;
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Applications WHERE Name = @name",con);
                    cmd.Parameters.AddWithValue("@name", appName);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            app = new Application
                            {
                                Id = (int)reader["Id"],
                                Name = (string)reader["Name"]
                            };
                        }
                    }

                    if (app == null)
                    {
                        return NotFound();
                    }

                    cmd = new SqlCommand("INSERT INTO Containers VALUES (@name,@app_id,@Created_at) ; SELECT CAST(SCOPE_IDENTITY() AS INT);", con);
                    cmd.Parameters.AddWithValue("@name", request.ResourceName);
                    cmd.Parameters.AddWithValue("@app_id", app.Id);
                    cmd.Parameters.AddWithValue("@Created_at", DateTime.UtcNow);
                    int new_id = (int) cmd.ExecuteScalar();
                    Container container = new Container
                    {
                        Id = new_id,
                        Name = request.ResourceName,
                        AplicationId = app.Id,
                        Created_at = DateTime.UtcNow,
                    };
                    String container_name = container.Name;
                    return Created($"api/somiod/{appName}/{container_name}",container);
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Conflict();
            }                 
        }

        //[HttpGet]
        //[Route("{appName}/{containerName}")]
        public IHttpActionResult GetContainer(string appName, string containerName)
        {
            if (string.IsNullOrWhiteSpace(appName) || string.IsNullOrWhiteSpace(containerName))
            {
                return BadRequest("Application name or container name is required");
            }

            Container container = null;
            Application app = null;
            using (SqlConnection con = new SqlConnection(Connectstring))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Applications WHERE Name = @name",con);
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

                //con.Open();
                cmd = new SqlCommand("SELECT * FROM Containers WHERE ApplicationId = @id AND Name = @Name", con);
                cmd.Parameters.AddWithValue("@Name", containerName);
                cmd.Parameters.AddWithValue("@id", app.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        container = new Container();
                        container.Name = (string)reader["Name"];
                        container.Id = (int)reader["Id"];
                        container.AplicationId = (int)reader["ApplicationId"];
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

        [HttpGet]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult GetContainerOrDiscoverRelatedTo(string appName, string containerName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();

                    // First, get the application
                    SqlCommand appCmd = new SqlCommand("SELECT Id, Name, Created_at FROM Applications WHERE Name = @Name", con);
                    appCmd.Parameters.AddWithValue("@Name", appName);

                    SqlDataReader appReader = appCmd.ExecuteReader();

                    if (!appReader.Read())
                    {
                        appReader.Close();
                        return NotFound();
                    }

                    var app = new Application
                    {
                        Id = (int)appReader["Id"],
                        Name = (string)appReader["Name"],
                        Created_at = (DateTime)appReader["Created_at"]
                    };

                    appReader.Close();

                    SqlCommand contCmd = new SqlCommand("SELECT * FROM Containers WHERE Name = @Name AND ApplicationId = @appId", con);
                    contCmd.Parameters.AddWithValue("@Name", containerName);
                    contCmd.Parameters.AddWithValue("@appId", app.Id);

                    SqlDataReader contReader = contCmd.ExecuteReader();
                    if (!contReader.Read())
                    {
                        contReader.Close();
                        return NotFound();
                    }

                    Container container = new Container
                    {
                        Id = (int)contReader["Id"],
                        Name = (string)contReader["Name"],
                        AplicationId = (int)contReader["ApplicationId"],
                        Created_at = (DateTime)contReader["Created_at"]
                    };

                    contReader.Close();

                    // Check if somiod-discover header exists
                    IEnumerable<string> headerValues;
                    if (Request.Headers.TryGetValues("somiod-discover", out headerValues))
                    {
                        string discoveryType = headerValues.FirstOrDefault();
                        List<string> paths = new List<string>();

                        if (discoveryType == "content-instance")
                        {
                            SqlCommand ciCmd = new SqlCommand(@"
                                SELECT c.Name, ci.Name 
                                FROM Content_Instances ci
                                INNER JOIN Containers c ON ci.ContainerId = c.Id
                                WHERE c.ApplicationId = @AppId", con);
                            ciCmd.Parameters.AddWithValue("@AppId", app.Id);

                            SqlDataReader ciReader = ciCmd.ExecuteReader();
                            while (ciReader.Read())
                            {
                                //containerName = ciReader.GetString(0);
                                string ciName = ciReader.GetString(1);
                                paths.Add($"/api/somiod/{appName}/{containerName}/{ciName}");
                            }
                            ciReader.Close();
                        }
                        else if (discoveryType == "subscription")
                        {
                            SqlCommand subCmd = new SqlCommand(@"
                                SELECT c.Name, s.Name 
                                FROM Subscriptions s
                                INNER JOIN Containers c ON s.ContainerId = c.Id
                                WHERE c.ApplicationId = @AppId", con);
                            subCmd.Parameters.AddWithValue("@AppId", app.Id);

                            SqlDataReader subReader = subCmd.ExecuteReader();
                            while (subReader.Read())
                            {
                                //containerName = subReader.GetString(0);
                                string subName = subReader.GetString(1);
                                paths.Add($"/api/somiod/{appName}/{containerName}/{subName}");
                            }
                            subReader.Close();
                        }
                        else
                        {
                            return BadRequest("Invalid somiod-discover header.");
                        }

                        return Ok(paths);
                    }
                    else
                    {
                        // No header - return the container
                        return Ok(container);
                    }
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult UpdateContainer(string appName, string containerName, [FromBody] CreateResourceRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            if (string.IsNullOrWhiteSpace(request.ResourceName) || string.IsNullOrWhiteSpace(appName))
            {
                return BadRequest("Application name or container name is required");
            }
            Application app = null;
            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();
                    SqlCommand cmd_app = new SqlCommand("Select * FROM Applications WHERE Name = @Name", con);
                    cmd_app.Parameters.AddWithValue("@Name", appName);
                    SqlDataReader reader = cmd_app.ExecuteReader();
                    if (reader.Read())
                    {
                        app = new Application
                        {
                            Id = (int)reader["Id"],
                            Name = (string)reader["Name"]
                        };
                    }

                    reader.Close();

                    SqlCommand cmd = new SqlCommand("UPDATE Containers SET Name = @Name WHere Name = @contName AND ApplicationId = @appid", con);
                    cmd.Parameters.AddWithValue("@Name", request.ResourceName);
                    cmd.Parameters.AddWithValue("@contName", containerName);
                    cmd.Parameters.AddWithValue("@appid", app.Id);
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
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Conflict();
            }
        }

        [HttpDelete]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult DeleteContainer(string appName, string containerName)
        {
            if (string.IsNullOrWhiteSpace(appName) || string.IsNullOrWhiteSpace(containerName))
            {
                return BadRequest("Application name or container name is required");
            }
            Application app = null;

            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();
                    SqlCommand cmd_app = new SqlCommand("Select * FROM Applications WHERE Name = @Name", con);
                    cmd_app.Parameters.AddWithValue("@Name", appName);

                    SqlDataReader reader = cmd_app.ExecuteReader();
                    if (reader.Read())
                    {
                        app = new Application
                        {
                            Id = (int)reader["Id"],
                            Name = (string)reader["Name"]
                        };
                    }

                    reader.Close();

                    SqlCommand cmd = new SqlCommand("DELETE FROM Containers WHERE Name = @name AND ApplicationId = @appid",con);
                    cmd.Parameters.AddWithValue("@name", containerName);
                    cmd.Parameters.AddWithValue("@appid", app.Id);
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
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }         
        }
    }
}
