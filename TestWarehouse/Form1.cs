using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestWarehouse
{
    public partial class Form1 : Form
    {
        private SomiodApiService _somiodService;
        private List<Order> _orders;
        private Order _selectedOrder;

        // UI Controls
        private TextBox txtOrderId;
        private TextBox txtCustomerId;
        private ComboBox cmbStatus;
        private TextBox txtLocation;
        private TextBox txtItems;
        private Button btnCreateOrder;
        private Button btnUpdateStatus;
        private Button btnDeleteOrder;
        private Button btnRefresh;
        private ListBox lstOrders;
        private RichTextBox rtbLog;
        private GroupBox grpOrderDetails;
        private GroupBox grpActiveOrders;
        private Label lblOrderId;
        private Label lblCustomerId;
        private Label lblStatus;
        private Label lblLocation;
        private Label lblItems;

        public Form1()
        {
            InitializeComponents();
            InitializeCustomComponents();
            _somiodService = new SomiodApiService();
            _orders = new List<Order>();
            InitializeApplication();
        }

        private void InitializeComponents()
        {
            this.Text = "Warehouse Manager - Order Tracking System";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);
        }

        private void InitializeCustomComponents()
        {
            // Order Details Group
            grpOrderDetails = new GroupBox
            {
                Text = "Order Details",
                Location = new Point(20, 20),
                Size = new Size(520, 280)
            };

            lblOrderId = new Label { Text = "Order ID:", Location = new Point(20, 30), Size = new Size(100, 20) };
            txtOrderId = new TextBox { Location = new Point(130, 28), Size = new Size(350, 25) };
            txtOrderId.Text = "e.g., ORD001";

            lblCustomerId = new Label { Text = "Customer ID:", Location = new Point(20, 65), Size = new Size(100, 20) };
            txtCustomerId = new TextBox { Location = new Point(130, 63), Size = new Size(350, 25) };
            txtCustomerId.Text = "e.g., CUST001";

            lblStatus = new Label { Text = "Status:", Location = new Point(20, 100), Size = new Size(100, 20) };
            cmbStatus = new ComboBox
            {
                Location = new Point(130, 98),
                Size = new Size(350, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(Enum.GetNames(typeof(OrderStatus)));
            cmbStatus.SelectedIndex = 0;

            lblLocation = new Label { Text = "Location:", Location = new Point(20, 135), Size = new Size(100, 20) };
            txtLocation = new TextBox { Location = new Point(130, 133), Size = new Size(350, 25) };
            txtLocation.Text = "e.g., Warehouse A";

            lblItems = new Label { Text = "Items:", Location = new Point(20, 170), Size = new Size(100, 20) };
            txtItems = new TextBox
            {
                Location = new Point(130, 168),
                Size = new Size(350, 50),
                Multiline = true
            };
            txtItems.Text = "e.g., Laptop, Mouse, Keyboard";

            btnCreateOrder = new Button
            {
                Text = "📦 Create Order",
                Location = new Point(130, 230),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnCreateOrder.Click += BtnCreateOrder_Click;

            btnUpdateStatus = new Button
            {
                Text = "🔄 Update",
                Location = new Point(250, 230),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Enabled = false
            };
            btnUpdateStatus.Click += BtnUpdateStatus_Click;

            btnDeleteOrder = new Button
            {
                Text = "🗑️ Delete",
                Location = new Point(370, 230),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Enabled = false
            };
            btnDeleteOrder.Click += BtnDeleteOrder_Click;

            grpOrderDetails.Controls.AddRange(new Control[] {
                lblOrderId, txtOrderId,
                lblCustomerId, txtCustomerId,
                lblStatus, cmbStatus,
                lblLocation, txtLocation,
                lblItems, txtItems,
                btnCreateOrder, btnUpdateStatus, btnDeleteOrder
            });

            // Active Orders Group
            grpActiveOrders = new GroupBox
            {
                Text = "Active Orders",
                Location = new Point(560, 20),
                Size = new Size(310, 280)
            };

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(200, 25),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += BtnRefresh_Click;

            lstOrders = new ListBox
            {
                Location = new Point(10, 60),
                Size = new Size(290, 210),
                Font = new Font("Consolas", 9F)
            };
            lstOrders.SelectedIndexChanged += LstOrders_SelectedIndexChanged;

            grpActiveOrders.Controls.AddRange(new Control[] { btnRefresh, lstOrders });

            // Log Group
            var grpLog = new GroupBox
            {
                Text = "Activity Log",
                Location = new Point(20, 320),
                Size = new Size(850, 320)
            };

            rtbLog = new RichTextBox
            {
                Location = new Point(10, 25),
                Size = new Size(830, 285),
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Consolas", 9F)
            };

            grpLog.Controls.Add(rtbLog);

            // Add all to form
            this.Controls.AddRange(new Control[] { grpOrderDetails, grpActiveOrders, grpLog });
        }

        private async void InitializeApplication()
        {
            LogMessage("Initializing Warehouse Manager...", Color.Cyan);
            bool initialized = await _somiodService.InitializeWarehouseApplication();

            if (initialized)
            {
                LogMessage("✓ Connected to SOMIOD Middleware", Color.LightGreen);
                LogMessage("✓ Warehouse application ready", Color.LightGreen);
            }
            else
            {
                LogMessage("✗ Failed to connect to SOMIOD", Color.Red);
                MessageBox.Show("Failed to connect to SOMIOD Middleware. Please ensure the API is running.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnCreateOrder_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOrderId.Text))
            {
                MessageBox.Show("Please enter an Order ID", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCustomerId.Text))
            {
                MessageBox.Show("Please enter a Customer ID", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnCreateOrder.Enabled = false;
                LogMessage($"Creating order {txtOrderId.Text}...", Color.Yellow);

                var order = new Order
                {
                    OrderId = txtOrderId.Text.Trim(),
                    CustomerId = txtCustomerId.Text.Trim(),
                    Status = (OrderStatus)Enum.Parse(typeof(OrderStatus), cmbStatus.SelectedItem.ToString()),
                    Location = txtLocation.Text.Trim(),
                    Items = txtItems.Text.Trim()
                };

                bool success = await _somiodService.CreateNewOrder(order);

                if (success)
                {
                    _orders.Add(order);
                    lstOrders.Items.Add(order.ToString());
                    LogMessage($"✓ Order {order.OrderId} created successfully!", Color.LightGreen);
                    ClearInputs();
                }
                else
                {
                    LogMessage($"✗ Failed to create order {order.OrderId}", Color.Red);
                    MessageBox.Show("Failed to create order. Check the log for details.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"✗ Error: {ex.Message}", Color.Red);
                MessageBox.Show($"Error creating order: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCreateOrder.Enabled = true;
            }
        }

        private async void BtnUpdateStatus_Click(object sender, EventArgs e)
        {
            if (_selectedOrder == null) return;

            try
            {
                btnUpdateStatus.Enabled = false;
                LogMessage($"Updating order {_selectedOrder.OrderId}...", Color.Yellow);

                var newStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), cmbStatus.SelectedItem.ToString());
                _selectedOrder.UpdateStatus(newStatus, txtLocation.Text.Trim());

                bool success = await _somiodService.UpdateOrderStatus(_selectedOrder);

                if (success)
                {
                    // Update list
                    int index = lstOrders.SelectedIndex;
                    lstOrders.Items[index] = _selectedOrder.ToString();

                    LogMessage($"✓ Order {_selectedOrder.OrderId} updated to {newStatus}", Color.LightGreen);
                }
                else
                {
                    LogMessage($"✗ Failed to update order {_selectedOrder.OrderId}", Color.Red);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"✗ Error: {ex.Message}", Color.Red);
            }
            finally
            {
                btnUpdateStatus.Enabled = true;
            }
        }

        private async void BtnDeleteOrder_Click(object sender, EventArgs e)
        {
            if (_selectedOrder == null) return;

            // Confirm deletion
            var result = MessageBox.Show(
                $"Are you sure you want to delete order {_selectedOrder.OrderId}?\n\n" +
                $"Customer: {_selectedOrder.CustomerId}\n" +
                $"Status: {_selectedOrder.Status}\n\n" +
                "This action cannot be undone and will remove all order history.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                btnDeleteOrder.Enabled = false;
                btnUpdateStatus.Enabled = false;
                LogMessage($"Deleting order {_selectedOrder.OrderId}...", Color.Yellow);

                bool success = await _somiodService.DeleteOrder(_selectedOrder.OrderId);

                if (success)
                {
                    // Remove from list
                    int index = lstOrders.SelectedIndex;
                    _orders.RemoveAt(index);
                    lstOrders.Items.RemoveAt(index);

                    LogMessage($"✓ Order {_selectedOrder.OrderId} deleted successfully!", Color.LightGreen);

                    ClearInputs();

                    MessageBox.Show(
                        $"Order {_selectedOrder.OrderId} has been deleted.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    LogMessage($"✗ Failed to delete order {_selectedOrder.OrderId}", Color.Red);
                    MessageBox.Show(
                        "Failed to delete order. Check the log for details.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    btnDeleteOrder.Enabled = true;
                    btnUpdateStatus.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"✗ Error: {ex.Message}", Color.Red);
                MessageBox.Show(
                    $"Error deleting order: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                btnDeleteOrder.Enabled = true;
                btnUpdateStatus.Enabled = true;
            }
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                LogMessage("Refreshing order list...", Color.Yellow);
                var containers = await _somiodService.DiscoverAllOrders();
                LogMessage($"Found {containers.Count} orders in system", Color.Cyan);
            }
            catch (Exception ex)
            {
                LogMessage($"✗ Error refreshing: {ex.Message}", Color.Red);
            }
        }

        private void LstOrders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstOrders.SelectedIndex >= 0 && lstOrders.SelectedIndex < _orders.Count)
            {
                _selectedOrder = _orders[lstOrders.SelectedIndex];

                // Load order details
                txtOrderId.Text = _selectedOrder.OrderId;
                txtCustomerId.Text = _selectedOrder.CustomerId;
                cmbStatus.SelectedItem = _selectedOrder.Status.ToString();
                txtLocation.Text = _selectedOrder.Location;
                txtItems.Text = _selectedOrder.Items;

                // Enable update and delete buttons
                btnUpdateStatus.Enabled = true;
                btnDeleteOrder.Enabled = true;
                btnCreateOrder.Enabled = false;

                LogMessage($"Selected order: {_selectedOrder.OrderId}", Color.Cyan);
            }
        }

        private void ClearInputs()
        {
            txtOrderId.Clear();
            txtCustomerId.Clear();
            cmbStatus.SelectedIndex = 0;
            txtLocation.Clear();
            txtItems.Clear();
            _selectedOrder = null;
            btnUpdateStatus.Enabled = false;
            btnDeleteOrder.Enabled = false;
            btnCreateOrder.Enabled = true;
        }

        private void LogMessage(string message, Color color)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => LogMessage(message, color)));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = Color.Gray;
            rtbLog.AppendText($"[{timestamp}] ");
            rtbLog.SelectionColor = color;
            rtbLog.AppendText($"{message}\n");
            rtbLog.SelectionColor = rtbLog.ForeColor;
            rtbLog.ScrollToCaret();
        }
    }
}
