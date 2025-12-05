using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using static System.Net.Mime.MediaTypeNames;
using Middleware.Models;

namespace Middleware.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ApplicationController : ApiController
    {
        //HARDCODED DATA FOR TESTING PURPOSES
        List<Aplication> applications = new List<Aplication>()
        {
            new Aplication(){ Id=1, Name="App1", Created_at=DateTime.Now },
            new Aplication(){ Id=2, Name="App2", Created_at=DateTime.Now },
            new Aplication(){ Id=3, Name="App3", Created_at=DateTime.Now }
        };

        //TEST ENDPOINT TO GET ALL APPLICATIONS
        [HttpGet]
        public IHttpActionResult GetApplications()
        {
            return Ok(applications);
        }

        [HttpPost]
        public IHttpActionResult AddApplication([FromBody] Aplication app)
        {
            if (app == null || string.IsNullOrEmpty(app.Name))
            {
                return BadRequest("Invalid application data.");
            }
            app.Id = applications.Max(a => a.Id) + 1;
            app.Created_at = DateTime.Now;
            applications.Add(app);
            return Ok("Valid application data");
        }

    }
}