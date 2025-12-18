using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWarehouse
{
    public class Order
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public OrderStatus Status { get; set; }
        public string Location { get; set; }
        public string Items { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdate { get; set; }

        public Order()
        {
            CreatedAt = DateTime.Now;
            LastUpdate = DateTime.Now;
            Status = OrderStatus.Pending;
        }

        public Order(string orderId, string customerId, string location, string items)
        {
            OrderId = orderId;
            CustomerId = customerId;
            Location = location;
            Items = items;
            Status = OrderStatus.Pending;
            CreatedAt = DateTime.Now;
            LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Updates the order status and location
        /// </summary>
        public void UpdateStatus(OrderStatus newStatus, string newLocation = null)
        {
            Status = newStatus;
            if (!string.IsNullOrEmpty(newLocation))
            {
                Location = newLocation;
            }
            LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Converts order to JSON string for SOMIOD content
        /// </summary>
        public string ToJson()
        {
                        return $@"{{
                ""orderId"": ""{OrderId}"",
                ""customerId"": ""{CustomerId}"",
                ""status"": ""{Status}"",
                ""location"": ""{Location}"",
                ""items"": ""{Items}"",
                ""createdAt"": ""{CreatedAt:yyyy-MM-ddTHH:mm:ss}"",
                ""lastUpdate"": ""{LastUpdate:yyyy-MM-ddTHH:mm:ss}""
            }}";
        }

        public override string ToString()
        {
            return $"{OrderId} | {CustomerId} | {Status} | {Location}";
        }
    }

    /// <summary>
    /// Order status enumeration
    /// </summary>
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        InTransit,
        Delivered,
        Cancelled
    }
}
