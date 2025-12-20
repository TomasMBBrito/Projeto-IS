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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnDiscoverOrders = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBoxApplications
            // 
            this.listBoxApplications.FormattingEnabled = true;
            this.listBoxApplications.ItemHeight = 16;
            this.listBoxApplications.Location = new System.Drawing.Point(12, 39);
            this.listBoxApplications.Name = "listBoxApplications";
            this.listBoxApplications.Size = new System.Drawing.Size(241, 324);
            this.listBoxApplications.TabIndex = 0;
            // 
            // listBoxContainers
            // 
            this.listBoxContainers.FormattingEnabled = true;
            this.listBoxContainers.ItemHeight = 16;
            this.listBoxContainers.Location = new System.Drawing.Point(459, 39);
            this.listBoxContainers.Name = "listBoxContainers";
            this.listBoxContainers.Size = new System.Drawing.Size(241, 324);
            this.listBoxContainers.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Aplicacoes";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(456, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(145, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Containers associados";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(16, 647);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(167, 50);
            this.button1.TabIndex = 4;
            this.button1.Text = "Start Order processing";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(215, 647);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(167, 50);
            this.button2.TabIndex = 5;
            this.button2.Text = "Update Order Status";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(16, 597);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(167, 22);
            this.textBox1.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 569);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 16);
            this.label3.TabIndex = 8;
            this.label3.Text = "Order Status";
            // 
            // btnDiscoverOrders
            // 
            this.btnDiscoverOrders.Location = new System.Drawing.Point(282, 104);
            this.btnDiscoverOrders.Name = "btnDiscoverOrders";
            this.btnDiscoverOrders.Size = new System.Drawing.Size(155, 105);
            this.btnDiscoverOrders.TabIndex = 9;
            this.btnDiscoverOrders.Text = "Descobrir Encomendas";
            this.btnDiscoverOrders.UseVisualStyleBackColor = true;
            this.btnDiscoverOrders.Click += new System.EventHandler(this.btnDiscoverOrders_Click);
            // 
            // GestorEncomendaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 730);
            this.Controls.Add(this.btnDiscoverOrders);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBoxContainers);
            this.Controls.Add(this.listBoxApplications);
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnDiscoverOrders;
    }
}

