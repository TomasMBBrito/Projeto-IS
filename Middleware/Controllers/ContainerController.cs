using Middleware.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Http;
using static System.Net.Mime.MediaTypeNames;
using Container = Middleware.Models.Container;


namespace Middleware.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ContainerController : ApiController
    {
        List<Aplication> aplications = new List<Aplication>();
        List<Container> containers = new List<Container>()
        {
            new Container(){ Id=1, Name="Container1", AplicationId=1, Created_at=DateTime.Now },
            new Container(){ Id=2, Name="Container2", AplicationId=2, Created_at=DateTime.Now },
            new Container(){ Id=3, Name="Container3", AplicationId=1, Created_at=DateTime.Now }
        };

        [HttpPost]
        [Route("{appName}")]
        public IHttpActionResult CreateContainer(string appName, [FromBody] Container container)
        {
            if(container == null || string.IsNullOrEmpty(appName) || string.IsNullOrEmpty(container.Name))
            {
                return BadRequest("Invalid data");
            }

            Aplication app = aplications.FirstOrDefault(a => a.Name == appName);
            if(app == null)
            {
                return NotFound();
            }

            if(containers.Any(c => c.Name == container.Name && c.AplicationId == app.Id))
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
            Aplication app = aplications.FirstOrDefault(a => a.Name == appName);
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

        [HttpGet]
        [Route("")]
        [Route("{appName}")]
        public IHttpActionResult DiscoverContainers(string appName = null)
        {
            IEnumerable<String> header_values;
            if (!Request.Headers.TryGetValues("somiod-discovery", out header_values))
                return BadRequest("Missing somiod-discovery header");

            var discoveryType = header_values.FirstOrDefault();
            if (discoveryType != "container")
                return BadRequest("Invalid discovery type for containers");

            List<string> paths = new List<string>();

            // Se appName foi especificado, descobrir apenas containers dessa app
            if (!string.IsNullOrEmpty(appName))
            {
                var app = aplications.FirstOrDefault(a => a.Name == appName);
                if (app == null)
                    return NotFound();

                paths = containers
                    .Where(c => c.AplicationId == app.Id)
                    .Select(c => $"/api/somiod/{appName}/{c.Name}")
                    .ToList();
            }
            else
            {
                // Descobrir todos os containers (recursivamente)
                foreach (var app in aplications)
                {
                    var appContainers = containers
                        .Where(c => c.AplicationId == app.Id)
                        .Select(c => $"/api/somiod/{app.Name}/{c.Name}");

                    paths.AddRange(appContainers);
                }
            }

            return Ok(paths);

        }

        [HttpPut]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult UpdateContainer(string appName, string containerName, [FromBody] Container updatedContainer)
        {
            if(updatedContainer == null || string.IsNullOrEmpty(updatedContainer.Name))
            {
                return BadRequest("Invalid data");
            }

            Aplication app = aplications.FirstOrDefault(a => a.Name == appName);
            if (app == null)
            {
                return NotFound();
            }

            Container container = containers.FirstOrDefault(c => c.Name == containerName && c.AplicationId == app.Id);
            if (container == null)
            {
                return NotFound();
            }

            if(updatedContainer.Name != containerName &&
               containers.Any(c => c.Name == updatedContainer.Name && c.AplicationId == app.Id))
            {
                return Conflict();
            }

            container.Name = updatedContainer.Name;

            return Ok(container);
        }

        [HttpDelete]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult DeleteContainer(string appName, string containerName)
        {
            Aplication app = aplications.FirstOrDefault(a => a.Name == appName);
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