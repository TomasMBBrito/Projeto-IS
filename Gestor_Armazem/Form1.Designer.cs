namespace Gestor_Armazem
{
    partial class GestorEncomendaForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBoxApplications = new System.Windows.Forms.ListBox();
            this.listBoxContainers = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelOrderStatus = new System.Windows.Forms.Label();
            this.btnDiscoverOrders = new System.Windows.Forms.Button();
            this.btnProcessOrder = new System.Windows.Forms.Button();
            this.btnShipOrder = new System.Windows.Forms.Button();
            this.btnDeliverOrder = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBoxApplications
            // 
            this.listBoxApplications.FormattingEnabled = true;
            this.listBoxApplications.ItemHeight = 20;
            this.listBoxApplications.Location = new System.Drawing.Point(14, 49);
            this.listBoxApplications.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listBoxApplications.Name = "listBoxApplications";
            this.listBoxApplications.Size = new System.Drawing.Size(271, 404);
            this.listBoxApplications.TabIndex = 0;
            // 
            // listBoxContainers
            // 
            this.listBoxContainers.FormattingEnabled = true;
            this.listBoxContainers.ItemHeight = 20;
            this.listBoxContainers.Location = new System.Drawing.Point(516, 49);
            this.listBoxContainers.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listBoxContainers.Name = "listBoxContainers";
            this.listBoxContainers.Size = new System.Drawing.Size(271, 404);
            this.listBoxContainers.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Clientes";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(512, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Encomendas";
            // 
            // labelOrderStatus
            // 
            this.labelOrderStatus.AutoSize = true;
            this.labelOrderStatus.Location = new System.Drawing.Point(15, 711);
            this.labelOrderStatus.Name = "labelOrderStatus";
            this.labelOrderStatus.Size = new System.Drawing.Size(112, 20);
            this.labelOrderStatus.TabIndex = 8;
            this.labelOrderStatus.Text = "Order Status : ";
            // 
            // btnDiscoverOrders
            // 
            this.btnDiscoverOrders.Location = new System.Drawing.Point(317, 130);
            this.btnDiscoverOrders.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnDiscoverOrders.Name = "btnDiscoverOrders";
            this.btnDiscoverOrders.Size = new System.Drawing.Size(174, 131);
            this.btnDiscoverOrders.TabIndex = 9;
            this.btnDiscoverOrders.Text = "Descobrir Encomendas";
            this.btnDiscoverOrders.UseVisualStyleBackColor = true;
            this.btnDiscoverOrders.Click += new System.EventHandler(this.btnDiscoverOrders_Click);
            // 
            // btnProcessOrder
            // 
            this.btnProcessOrder.Location = new System.Drawing.Point(18, 749);
            this.btnProcessOrder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnProcessOrder.Name = "btnProcessOrder";
            this.btnProcessOrder.Size = new System.Drawing.Size(159, 64);
            this.btnProcessOrder.TabIndex = 10;
            this.btnProcessOrder.Text = "Process Order";
            this.btnProcessOrder.UseVisualStyleBackColor = true;
            this.btnProcessOrder.Click += new System.EventHandler(this.btnProcessOrder_Click);
            // 
            // btnShipOrder
            // 
            this.btnShipOrder.Location = new System.Drawing.Point(209, 749);
            this.btnShipOrder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnShipOrder.Name = "btnShipOrder";
            this.btnShipOrder.Size = new System.Drawing.Size(159, 64);
            this.btnShipOrder.TabIndex = 11;
            this.btnShipOrder.Text = "Ship Order";
            this.btnShipOrder.UseVisualStyleBackColor = true;
            this.btnShipOrder.Click += new System.EventHandler(this.btnShipOrder_Click);
            // 
            // btnDeliverOrder
            // 
            this.btnDeliverOrder.Location = new System.Drawing.Point(396, 749);
            this.btnDeliverOrder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnDeliverOrder.Name = "btnDeliverOrder";
            this.btnDeliverOrder.Size = new System.Drawing.Size(159, 64);
            this.btnDeliverOrder.TabIndex = 12;
            this.btnDeliverOrder.Text = "Deliver Order";
            this.btnDeliverOrder.UseVisualStyleBackColor = true;
            this.btnDeliverOrder.Click += new System.EventHandler(this.btnDeliverOrder_Click);
            // 
            // GestorEncomendaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 912);
            this.Controls.Add(this.btnDeliverOrder);
            this.Controls.Add(this.btnShipOrder);
            this.Controls.Add(this.btnProcessOrder);
            this.Controls.Add(this.btnDiscoverOrders);
            this.Controls.Add(this.labelOrderStatus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBoxContainers);
            this.Controls.Add(this.listBoxApplications);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "GestorEncomendaForm";
            this.Text = "GestorEncomendasForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GestorEncomendaForm_FormClosing);
            this.Load += new System.EventHandler(this.GestorEncomendaForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxApplications;
        private System.Windows.Forms.ListBox listBoxContainers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelOrderStatus;
        private System.Windows.Forms.Button btnDiscoverOrders;
        private System.Windows.Forms.Button btnProcessOrder;
        private System.Windows.Forms.Button btnShipOrder;
        private System.Windows.Forms.Button btnDeliverOrder;
    }
}

