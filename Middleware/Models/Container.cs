using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Middleware.Models
{
    public class Container
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int AplicationId { get; set; }
        public DateTime Created_at { get; set; }
    }
}