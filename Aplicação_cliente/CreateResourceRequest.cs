using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Aplicação_cliente
{
    public class CreateResourceRequest
    {
        [JsonProperty("res-type")]
        public string ResType { get; set; }

        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }
    }
}
