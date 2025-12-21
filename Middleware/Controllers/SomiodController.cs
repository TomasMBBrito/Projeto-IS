using Middleware.DTOs;
using Middleware.Models;
using Middleware.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Application = Middleware.Models.Application;
using Container = Middleware.Models.Container;

namespace Middleware.Controllers
{
    [RoutePrefix("api/somiod")]
    public class SomiodController : ApiController
    {
        string Connectstring = Properties.Settings.Default.connectstr;
        private readonly NotificationService _notificationService;

        // Constructor
        public SomiodController()
        {
            string xsdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NotificationSchema.xsd");
            _notificationService = new NotificationService(Connectstring, xsdPath);
        }

        // ============================================
        // APPLICATION ENDPOINTS
        // ============================================

        /// <summary>
        /// Get Discover
        /// </summary>
        /// <remarks>Gets the Discoveries</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
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
                    if (Request.Headers.TryGetValues("somiod-discovery", out headerValues))
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
                                paths.Add($"/api/somiod/{appName}/{containerName}/subs/{subsName}");
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

        /// <summary>
        /// Create Application
        /// </summary>
        /// <param name="request">Request</param>
        /// <remarks>Create a new application in the system</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="201">Created</response>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateApplication([FromBody] CreateResourceRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (!string.Equals(request.ResType, "application", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid resource type. Expected 'application'.");

            if (string.IsNullOrWhiteSpace(request.ResourceName))
                //return BadRequest("Resource name is required.");
                request.ResourceName = $"{request.ResType}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();
                    // Verificar se já existe
                    SqlCommand checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Applications WHERE Name = @Name", con);
                    checkCmd.Parameters.AddWithValue("@Name", request.ResourceName);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        // Aplicação já existe
                        return Conflict();
                    }

                    DateTime createdAt = DateTime.UtcNow;
                    SqlCommand cmd = new SqlCommand("INSERT INTO Applications VALUES (@Name, @Created_at); SELECT CAST(SCOPE_IDENTITY() AS INT);", con);
                    cmd.Parameters.AddWithValue("@Name", request.ResourceName);
                    cmd.Parameters.AddWithValue("@Created_at", createdAt);
                    cmd.ExecuteNonQuery();

                    var app = new ApplicationResponse
                    {
                        ResourceName = request.ResourceName,
                        CreationDatetime = createdAt.ToString("yyyy-MM-ddTHH:mm:ss")
                    };

                    return Created($"api/somiod/{app.ResourceName}", app);
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Application or Discover Related To
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <remarks>Gets the application or discovers related to application</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
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

                    DateTime createdAt = (DateTime)appReader["Created_at"];

                    int app_id = (int)appReader["Id"];

                    var app = new ApplicationResponse
                    {
                        ResourceName = (string)appReader["Name"],
                        CreationDatetime = createdAt.ToString("yyyy-MM-ddTHH:mm:ss")
                    };

                    appReader.Close();

                    // Check if somiod-discover header exists
                    IEnumerable<string> headerValues;
                    if (Request.Headers.TryGetValues("somiod-discovery", out headerValues))
                    {
                        string discoveryType = headerValues.FirstOrDefault();
                        List<string> paths = new List<string>();

                        if (discoveryType == "container")
                        {
                            SqlCommand containerCmd = new SqlCommand(
                                "SELECT Name FROM Containers WHERE ApplicationId = @AppId", con);
                            containerCmd.Parameters.AddWithValue("@AppId", app_id);

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
                            ciCmd.Parameters.AddWithValue("@AppId", app_id);

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
                            subCmd.Parameters.AddWithValue("@AppId", app_id);

                            SqlDataReader subReader = subCmd.ExecuteReader();
                            while (subReader.Read())
                            {
                                string containerName = subReader.GetString(0);
                                string subName = subReader.GetString(1);
                                paths.Add($"/api/somiod/{appName}/{containerName}/subs/{subName}");
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


        /// <summary>
        /// Update Application
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="request">Request</param>
        /// <remarks>Update an existing application in the system</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
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
                    DateTime createdAt = (DateTime)appReader["Created_at"];
                    appReader.Close();

                    SqlCommand cmd = new SqlCommand("UPDATE Applications SET Name = @Name WHere Name = @appName", con);
                    cmd.Parameters.AddWithValue("@Name", request.ResourceName);
                    cmd.Parameters.AddWithValue("@appName", appName);
                    int n_rows = cmd.ExecuteNonQuery();
                    if (n_rows > 0)
                    {
                        var app = new ApplicationResponse
                        {
                            ResourceName = request.ResourceName,
                            CreationDatetime = createdAt.ToString("yyyy-MM-ddTHH:mm:ss")
                        };
                        return Ok(app);
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

        /// <summary>
        /// Delete Application
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <remarks>Delete an existing application in the system</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
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

        /// <summary>
        /// Create Container
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="request">Request</param>
        /// <remarks> Creates a container in the selected application</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="201">Created</response>
        /// <returns></returns>
        // POST api/somiod/{appName}
        [HttpPost]
        [Route("{appName}")]
        public IHttpActionResult CreateContainer(string appName, [FromBody] CreateResourceRequest request)
        {
            if (request == null)
            {
                return BadRequest("Body is required");
            }

            if (request.ResType?.ToLower() != "container")
            {
                return BadRequest("Invalid resource type. Must be container");
            }

            if (string.IsNullOrWhiteSpace(appName))
            {
                return BadRequest("application name is required");
            }

            if (string.IsNullOrWhiteSpace(request.ResourceName))
            {
                request.ResourceName = $"{request.ResType}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }

            try
            {
                Application app = null;
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Applications WHERE Name = @name", con);
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

                    DateTime createdAt = DateTime.UtcNow;

                    cmd = new SqlCommand("INSERT INTO Containers VALUES (@name,@app_id,@Created_at) ; SELECT CAST(SCOPE_IDENTITY() AS INT);", con);
                    cmd.Parameters.AddWithValue("@name", request.ResourceName);
                    cmd.Parameters.AddWithValue("@app_id", app.Id);
                    cmd.Parameters.AddWithValue("@Created_at", createdAt);
                    cmd.ExecuteNonQuery();
                    var container = new ContainerResponse
                    {
                        ResourceName = request.ResourceName,
                        CreationDatetime = createdAt.ToString("yyyy-MM-ddTHH:mm:ss")
                    };

                    return Created($"api/somiod/{appName}/{container.ResourceName}", container);
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Conflict();
            }
        }

        /// <summary>
        /// Get Container
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="containerName">Container Name</param>
        /// <remarks>Gets the container in the selected application</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
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
                SqlCommand cmd = new SqlCommand("SELECT * FROM Applications WHERE Name = @name", con);
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

        /// <summary>
        /// Get container or discover related to
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="containerName"></param>
        /// <remarks>Gets the container in the specified app or discovers related to</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
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

                    Application app = new Application
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

                    DateTime createdAt = (DateTime)contReader["Created_at"];

                    int container_id = (int)contReader["Id"];

                    ContainerResponse container = new ContainerResponse
                    {
                        ResourceName = (string)contReader["Name"],
                        CreationDatetime = createdAt.ToString("yyyy-MM-ddTHH:mm:ss")
                    };

                    contReader.Close();

                    // Check if somiod-discover header exists
                    IEnumerable<string> headerValues;
                    if (Request.Headers.TryGetValues("somiod-discovery", out headerValues))
                    {
                        string discoveryType = headerValues.FirstOrDefault();
                        List<string> paths = new List<string>();

                        if (discoveryType == "content-instance")
                        {
                            SqlCommand ciCmd = new SqlCommand(@"
                                SELECT c.Name, ci.Name 
                                FROM Content_Instances ci
                                INNER JOIN Containers c ON ci.ContainerId = c.Id
                                WHERE c.ApplicationId = @AppId AND ci.ContainerId = @ContainerID", con);
                            ciCmd.Parameters.AddWithValue("@AppId", app.Id);
                            ciCmd.Parameters.AddWithValue("@ContainerID", container_id);

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
                                WHERE c.ApplicationId = @AppId AND s.ContainerId = @ContainerID", con);
                            subCmd.Parameters.AddWithValue("@AppId", app.Id);
                            subCmd.Parameters.AddWithValue("@ContainerID", container_id);


                            SqlDataReader subReader = subCmd.ExecuteReader();
                            while (subReader.Read())
                            {
                                //containerName = subReader.GetString(0);
                                string subName = subReader.GetString(1);
                                paths.Add($"/api/somiod/{appName}/{containerName}/subs/{subName}");
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

        /// <summary>
        /// Update Container
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="containerName">Container Name</param>
        /// <param name="request">Request</param>
        /// <remarks>Update the specified container in the specified app</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
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

                    // 2️⃣ Segundo: Buscar o Container e o seu Created_at ORIGINAL
                    SqlCommand containerCmd = new SqlCommand(@"
                        SELECT Id, Created_at 
                        FROM Containers 
                        WHERE Name = @ContainerName AND ApplicationId = @AppId", con);

                    containerCmd.Parameters.AddWithValue("@ContainerName", containerName);
                    containerCmd.Parameters.AddWithValue("@AppId", app.Id);

                    SqlDataReader containerReader = containerCmd.ExecuteReader();

                    if (!containerReader.Read())
                    {
                        containerReader.Close();
                        return NotFound();
                    }

                    DateTime createdAt = (DateTime)containerReader["Created_at"];
                    containerReader.Close();

                    SqlCommand cmd = new SqlCommand("UPDATE Containers SET Name = @Name WHere Name = @contName AND ApplicationId = @appid", con);
                    cmd.Parameters.AddWithValue("@Name", request.ResourceName);
                    cmd.Parameters.AddWithValue("@contName", containerName);
                    cmd.Parameters.AddWithValue("@appid", app.Id);
                    int n_rows = cmd.ExecuteNonQuery();
                    if (n_rows > 0)
                    {
                        var container = new ContainerResponse
                        {
                            ResourceName = request.ResourceName,
                            CreationDatetime = createdAt.ToString("yyyy-MM-ddTHH:mm:ss")
                        };
                        return Ok(container);
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

        /// <summary>
        /// Delete container
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="containerName">Container Name</param>
        /// <remarks>Deletes the container in the specified app</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
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

                    SqlCommand cmd = new SqlCommand("DELETE FROM Containers WHERE Name = @name AND ApplicationId = @appid", con);
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

        /// <summary>
        /// Create Content Instance or Subscription
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="containerName">Container Name</param>
        /// <param name="request">Request</param>
        /// <remarks>Creates Content Instance or Subscription in the specified container</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="201">Created</response>
        /// <returns></returns>
        [HttpPost]
        [Route("{appName}/{containerName}")]
        public async Task<IHttpActionResult> CreateContentInstanceOrSubscription(string appName, string containerName, [FromBody] CreateResourceRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if(request.ResType?.ToLower() != "content-instance" && request.ResType?.ToLower() != "subscription")
            {
                return BadRequest("Invalid resource type!");
            }

            String request_type = request.ResType.ToLower();

            if (request_type == "content-instance")
            {
                if (request.Content == null)
                {
                    return BadRequest("Content is required");
                }

                if (request.ContentType == null)
                {
                    return BadRequest("Content-type is required");
                }

                var valid_content_types = new[]
                {
                    "application/json",
                    "application/xml",
                    "application/plaintext"
                };

                if (!valid_content_types.Contains(request.ContentType.ToLower()))
                {
                    return BadRequest("Invalid content-type. Must be application/json, application/xml or application/plaintext");
                }



            }
            else if (request_type == "subscription")
            {
                if (request.Evt == null || (request.Evt != 1 && request.Evt != 2))
                {
                    return BadRequest("Evt is required or must be 1 (creation) or 2 (deletion)");
                }

                if (request.Endpoint == null)
                {
                    return BadRequest("Endpoint is required");
                }
            }

            if (string.IsNullOrWhiteSpace(appName) || string.IsNullOrWhiteSpace(containerName))
            {
                return BadRequest("Application name or contianer name is required");
            }

            if (string.IsNullOrWhiteSpace(request.ResourceName))
            {
                request.ResourceName = $"{request.ResType}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }

            try
            {
                //Application app = null;
                Container container = null;

                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();
                    SqlCommand app_cmd = new SqlCommand("SELECT c.id as ID FROM Containers c " +
                        "INNER JOIN APPLICATIONS a ON c.ApplicationId = a.id " +
                        "WHERE c.Name = @name AND a.name = @appname",con);
                    app_cmd.Parameters.AddWithValue("@name", containerName);
                    app_cmd.Parameters.AddWithValue("@appname", appName);

                    using(SqlDataReader reader = app_cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            container = new Container();
                            container.Id = (int)reader["ID"];
                        }
                    }

                    if(container == null)
                    {
                        return NotFound();
                    }

                    SqlCommand cmd = null;

                    DateTime createdAt = DateTime.UtcNow;

                    if (request_type == "content-instance")
                    {
                        cmd = new SqlCommand("INSERT INTO Content_Instances VALUES (@name,@contenttype,@container_id,@content,@created_at) ; SELECT CAST (SCOPE_IDENTITY() AS INT);", con);
                        cmd.Parameters.AddWithValue("@name", request.ResourceName);
                        cmd.Parameters.AddWithValue("@contenttype", request.ContentType);
                        cmd.Parameters.AddWithValue("@container_id", container.Id);
                        cmd.Parameters.AddWithValue("@content", request.Content);
                        cmd.Parameters.AddWithValue("@created_at", createdAt);
                        cmd.ExecuteNonQuery();

                        var content_instance = new ContentInstanceResponse
                        {
                            ResourceName = request.ResourceName,
                            ContentType = request.ContentType,
                            Content = request.Content,
                            CreationDatetime = createdAt.ToString("yyyy-MM-ddTHH:mm:ss")
                        };

                        string containerPath = $"{appName}/{containerName}";
                        await _notificationService.TriggerNotifications(
                            container.Id,
                            1,
                            content_instance,
                            containerPath
                        );

                        String content_name = content_instance.ResourceName;
                        return Created($"api/somiod/{appName}/{containerName}/{content_name}", content_instance);
                    }
                    else
                    {
                        cmd = new SqlCommand("INSERT INTO Subscriptions VALUES (@name,@container_id,@evt,@endpoint,@created_at) ; SELECT CAST (SCOPE_IDENTITY() AS INT);", con);
                        cmd.Parameters.AddWithValue("@name", request.ResourceName);
                        cmd.Parameters.AddWithValue("@container_id", container.Id);
                        cmd.Parameters.AddWithValue("@evt", request.Evt);
                        cmd.Parameters.AddWithValue("@endpoint", request.Endpoint);
                        cmd.Parameters.AddWithValue("@created_at", createdAt);
                        int new_id = (int)cmd.ExecuteScalar();

                        var subscription = new SubscriptionResponse
                        {
                            ResourceName = request.ResourceName,
                            Evt = request.Evt.Value,
                            Endpoint = request.Endpoint,
                            CreationDatetime = createdAt.ToString("yyyy-MM-ddTHH:mm:ss")
                        };

                        String sub_name = subscription.ResourceName;
                        return Created($"api/somiod/{appName}/{containerName}/subs/{sub_name}", subscription);
                    }                       
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Conflict();
            }
           
        }

        /// <summary>
        /// Get Content Instance
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="containerName">Container Name</param>
        /// <param name="ciName">Content Instance Name</param>
        /// <remarks>Gets the content instance in the specified container</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
        [HttpGet]
        [Route("{appName}/{containerName}/{ciName}")]
        public IHttpActionResult GetContentInstance(string appName, string containerName, string ciName)
        {
            if (string.IsNullOrEmpty(appName))
                return BadRequest("Nome de aplicação inválida");

            if (string.IsNullOrEmpty(containerName))
                return BadRequest("Nome de container inválido");

            if (string.IsNullOrEmpty(ciName))
                return BadRequest("Nome de content instance inválido");

            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand(@"
                        SELECT * 
                        FROM Content_Instances ci 
                        INNER JOIN Containers c ON ci.containerId = c.id 
                        INNER JOIN Applications a ON c.ApplicationId = a.id 
                        WHERE c.Name = @name 
                          AND a.name = @appname 
                          AND ci.name = @CiName", con);

                    cmd.Parameters.AddWithValue("@name", containerName);
                    cmd.Parameters.AddWithValue("@appname", appName);
                    cmd.Parameters.AddWithValue("@CiName", ciName);

                    SqlDataReader ciReader = cmd.ExecuteReader();

                    if (!ciReader.Read())
                        return NotFound();

                    var contentInstance = new ContentInstanceResponse
                    {
                        ResourceName = (string)ciReader["Name"],
                        ContentType = (string)ciReader["ContentType"],
                        Content = (string)ciReader["Content"],
                        CreationDatetime = ((DateTime)ciReader["Created_at"]).ToString("yyyy-MM-ddTHH:mm:ss")
                    };

                    return Ok(contentInstance);
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Subscription
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="containerName">Container Name</param>
        /// <param name="subName">Subscription Name</param>
        /// <remarks>Gets the subscription in the specified container</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
        [HttpGet]
        [Route("{appName}/{containerName}/subs/{subName}")]
        public IHttpActionResult GetSubscription(string appName, string containerName, string subName)
        {
            if (string.IsNullOrEmpty(appName))
                return BadRequest("Nome de aplicação inválida");

            if (string.IsNullOrEmpty(containerName))
                return BadRequest("Nome de container inválido");

            if (string.IsNullOrEmpty(subName))
                return BadRequest("Nome de subscription inválido");

            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand(@"
                SELECT *
                FROM Subscriptions s
                INNER JOIN Containers c ON s.ContainerId = c.Id
                INNER JOIN Applications a ON c.ApplicationId = a.Id
                WHERE c.Name = @containerName
                  AND a.Name = @appName
                  AND s.Name = @subName", con);

                    cmd.Parameters.AddWithValue("@containerName", containerName);
                    cmd.Parameters.AddWithValue("@appName", appName);
                    cmd.Parameters.AddWithValue("@subName", subName);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (!reader.Read())
                        return NotFound();

                    var subscription = new SubscriptionResponse
                    {
                        ResourceName = (string)reader["Name"],
                        Evt = (int)reader["Evt"],
                        Endpoint = (string)reader["Endpoint"],
                        CreationDatetime = ((DateTime)reader["Created_at"]).ToString("yyyy-MM-ddTHH:mm:ss")
                    };

                    return Ok(subscription);
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Delete Content Instance
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="containerName">Container Name</param>
        /// <param name="ciName">Content Instance Name</param>
        /// <remarks>Deletes the content instance in the specified container</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
        [HttpDelete]
        [Route("{appName}/{containerName}/{ciName}")]
        public async Task<IHttpActionResult> DeleteContentInstance(string appName, string containerName, string ciName)
        {
            if (string.IsNullOrEmpty(appName))
                return BadRequest("Nome de aplicação inválida");

            if (string.IsNullOrEmpty(containerName))
                return BadRequest("Nome de container inválido");

            if (string.IsNullOrEmpty(ciName))
                return BadRequest("Nome de content instance inválido");

            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();

                    SqlCommand checkCmd = new SqlCommand(@"
                SELECT ci.Id, ci.ContainerId, ci.Name, ci.ContentType, ci.Content, ci.Created_at
                FROM Content_Instances ci 
                INNER JOIN Containers c ON ci.ContainerId = c.Id 
                INNER JOIN Applications a ON c.ApplicationId = a.Id 
                WHERE c.Name = @containerName
                  AND a.Name = @appName
                  AND ci.Name = @ciName", con);

                    checkCmd.Parameters.AddWithValue("@containerName", containerName);
                    checkCmd.Parameters.AddWithValue("@appName", appName);
                    checkCmd.Parameters.AddWithValue("@ciName", ciName);

                    SqlDataReader reader = checkCmd.ExecuteReader();

                    if (!reader.Read())
                        return NotFound();

                    int contentInstanceId = (int)reader["Id"];
                    int containerId = (int)reader["ContainerId"];

                    var deletedContentInstance = new ContentInstanceResponse
                    {
                        ResourceName = (string)reader["Name"],
                        ContentType = (string)reader["ContentType"],
                        Content = (string)reader["Content"],
                        CreationDatetime = ((DateTime)reader["Created_at"]).ToString("yyyy-MM-ddTHH:mm:ss")
                    };


                    reader.Close();

                    // ----- DELETE THE RECORD -----

                    SqlCommand deleteCmd = new SqlCommand(
                        "DELETE FROM Content_Instances WHERE Id = @id", con);

                    deleteCmd.Parameters.AddWithValue("@id", contentInstanceId);
                    int rowsAffected = deleteCmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        return InternalServerError(new Exception("Erro ao apagar content instance."));

                    
                    string containerPath = $"{appName}/{containerName}";
                    await _notificationService.TriggerNotifications(
                        containerId,
                        2,
                        deletedContentInstance,
                        containerPath
                    );

                    return Ok();
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Delete Subscription
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="containerName">Container Name</param>
        /// <param name="subName">Subscription Name</param>
        /// <remarks>Deletes the subscription in the specified container</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200">OK</response>
        /// <returns></returns>
        [HttpDelete]
        [Route("{appName}/{containerName}/subs/{subName}")]
        public IHttpActionResult DeleteSubscription(string appName, string containerName, string subName)
        {
            if (string.IsNullOrEmpty(appName))
                return BadRequest("Nome de aplicação inválida");

            if (string.IsNullOrEmpty(containerName))
                return BadRequest("Nome de container inválido");

            if (string.IsNullOrEmpty(subName))
                return BadRequest("Nome de subscrição inválido");

            try
            {
                using (SqlConnection con = new SqlConnection(Connectstring))
                {
                    con.Open();

                    // -------- VALIDATE PATH --------

                    SqlCommand checkCmd = new SqlCommand(@"
                        SELECT s.Id
                        FROM Subscriptions s
                        INNER JOIN Containers c ON s.ContainerId = c.Id
                        INNER JOIN Applications a ON c.ApplicationId = a.Id
                        WHERE c.Name = @containerName
                          AND a.Name = @appName
                          AND s.Name = @subName", con);

                    checkCmd.Parameters.AddWithValue("@containerName", containerName);
                    checkCmd.Parameters.AddWithValue("@appName", appName);
                    checkCmd.Parameters.AddWithValue("@subName", subName);

                    SqlDataReader reader = checkCmd.ExecuteReader();

                    if (!reader.Read())
                        return NotFound();

                    int subscriptionId = (int)reader["Id"];
                    reader.Close();

                    // -------- DELETE SUBSCRIPTION --------

                    SqlCommand deleteCmd = new SqlCommand(
                        "DELETE FROM Subscriptions WHERE Id = @id", con);

                    deleteCmd.Parameters.AddWithValue("@id", subscriptionId);

                    int rowsAffected = deleteCmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        return InternalServerError(new Exception("Erro ao apagar a subscrição."));

                    return Ok();
                }
            }
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }
        }


    }
}
