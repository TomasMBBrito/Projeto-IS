using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TestWarehouse
{
    /// <summary>
    /// Service to interact with SOMIOD Middleware API
    /// </summary>
    public class SomiodApiService
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private const string APP_NAME = "warehouse";

        public SomiodApiService(string baseUrl = "http://localhost:61331/api/somiod")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            //_httpClient.DefaultRequestHeaders.Add("content-type", "application/json");
        }

        #region Application Management

        /// <summary>
        /// Creates the warehouse application if it doesn't exist
        /// </summary>
        public async Task<bool> InitializeWarehouseApplication()
        {
            try
            {
                Console.WriteLine($"[DEBUG] Connecting to: {_baseUrl}");
                var request = new
                {
                    res_type = "application",
                    resource_name = APP_NAME
                };

                string json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl, content);

                // 201 Created or 409 Conflict (already exists) are both OK
                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing application: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Container Management

        /// <summary>
        /// Creates a container for a specific order
        /// </summary>
        public async Task<bool> CreateOrderContainer(string orderId)
        {
            try
            {
                string containerName = $"order-{orderId}";

                var request = new
                {
                    res_type = "container",
                    resource_name = containerName
                };

                string json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string url = $"{_baseUrl}/{APP_NAME}";
                var response = await _httpClient.PostAsync(url, content);

                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating container: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Content Instance Management

        /// <summary>
        /// Creates or updates order status as content-instance
        /// </summary>
        public async Task<bool> UpdateOrderStatus(Order order)
        {
            try
            {
                string containerName = $"order-{order.OrderId}";

                var request = new
                {
                    res_type = "content-instance",
                    resource_name = $"status-{DateTime.Now:yyyyMMddHHmmss}",
                    content_type = "application/json",
                    content = order.ToJson()
                };

                string json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string url = $"{_baseUrl}/{APP_NAME}/{containerName}";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Order {order.OrderId} status updated to {order.Status}");
                    return true;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error updating order: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets all content instances for an order
        /// </summary>
        public async Task<List<string>> GetOrderHistory(string orderId)
        {
            try
            {
                string containerName = $"order-{orderId}";
                string url = $"{_baseUrl}/{APP_NAME}/{containerName}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("somiod-discovery", "content-instance");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<string>>(jsonResponse);
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting order history: {ex.Message}");
                return new List<string>();
            }
        }

        #endregion

        #region Subscription Management

        /// <summary>
        /// Creates a subscription for order updates (for customer notifications)
        /// </summary>
        public async Task<bool> CreateOrderSubscription(string orderId, string endpoint)
        {
            try
            {
                string containerName = $"order-{orderId}";

                var request = new
                {
                    res_type = "subscription",
                    resource_name = $"sub-{orderId}",
                    evt = 1, // Creation events (new status updates)
                    endpoint = endpoint
                };

                string json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string url = $"{_baseUrl}/{APP_NAME}/{containerName}";
                var response = await _httpClient.PostAsync(url, content);

                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating subscription: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Discovery

        /// <summary>
        /// Discovers all order containers in the warehouse application
        /// </summary>
        public async Task<List<string>> DiscoverAllOrders()
        {
            try
            {
                string url = $"{_baseUrl}/{APP_NAME}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("somiod-discovery", "container");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<string>>(jsonResponse);
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error discovering orders: {ex.Message}");
                return new List<string>();
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes an order container (removes entire order from system)
        /// </summary>
        public async Task<bool> DeleteOrder(string orderId)
        {
            try
            {
                string containerName = $"order-{orderId}";
                string url = $"{_baseUrl}/{APP_NAME}/{containerName}";

                var response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Order {orderId} deleted successfully");
                    return true;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error deleting order: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting order: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Complete Order Workflow

        /// <summary>
        /// Creates a new order with container and initial status
        /// </summary>
        public async Task<bool> CreateNewOrder(Order order, string notificationEndpoint = null)
        {
            try
            {
                // 1. Ensure warehouse application exists
                await InitializeWarehouseApplication();

                // 2. Create container for the order
                bool containerCreated = await CreateOrderContainer(order.OrderId);
                if (!containerCreated)
                {
                    Console.WriteLine("Failed to create order container");
                    return false;
                }

                // 3. Create initial status content-instance
                bool statusCreated = await UpdateOrderStatus(order);
                if (!statusCreated)
                {
                    Console.WriteLine("Failed to create initial order status");
                    return false;
                }

                // 4. Create subscription if endpoint provided
                if (!string.IsNullOrEmpty(notificationEndpoint))
                {
                    await CreateOrderSubscription(order.OrderId, notificationEndpoint);
                }

                Console.WriteLine($"Order {order.OrderId} created successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateNewOrder: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}