using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Middleware.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int ContainerId { get; set; }
        public int Evt { get; set; }
        public string Content { get; set; }
        public DateTime Created_at { get; set; }
    }
}