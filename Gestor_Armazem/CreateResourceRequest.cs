using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestor_Armazem
{
    public class CreateResourceRequest
    {
        [JsonProperty("res-type")]
        public string ResType { get; set; }

        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }
    }
}
