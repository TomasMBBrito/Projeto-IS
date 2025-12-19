namespace Aplicação_cliente
{
    partial class EncomendaForm
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
            this.textBoxProduto = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAddToCart = new System.Windows.Forms.Button();
            this.listBoxProdutos = new System.Windows.Forms.ListBox();
            this.btnCreateOrder = new System.Windows.Forms.Button();
            this.product_quantity = new System.Windows.Forms.NumericUpDown();
            this.listBoxEncomendas = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSubscribeToOrder = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.product_quantity)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxProduto
            // 
            this.textBoxProduto.Location = new System.Drawing.Point(34, 46);
            this.textBoxProduto.Name = "textBoxProduto";
            this.textBoxProduto.Size = new System.Drawing.Size(155, 22);
            this.textBoxProduto.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Nome do Produto";
            // 
            // btnAddToCart
            // 
            this.btnAddToCart.Location = new System.Drawing.Point(34, 132);
            this.btnAddToCart.Name = "btnAddToCart";
            this.btnAddToCart.Size = new System.Drawing.Size(155, 40);
            this.btnAddToCart.TabIndex = 2;
            this.btnAddToCart.Text = "Adicionar ao Carrinho";
            this.btnAddToCart.UseVisualStyleBackColor = true;
            this.btnAddToCart.Click += new System.EventHandler(this.btnAddToCart_Click);
            // 
            // listBoxProdutos
            // 
            this.listBoxProdutos.FormattingEnabled = true;
            this.listBoxProdutos.ItemHeight = 16;
            this.listBoxProdutos.Location = new System.Drawing.Point(213, 43);
            this.listBoxProdutos.Name = "listBoxProdutos";
            this.listBoxProdutos.Size = new System.Drawing.Size(213, 132);
            this.listBoxProdutos.TabIndex = 3;
            // 
            // btnCreateOrder
            // 
            this.btnCreateOrder.Location = new System.Drawing.Point(34, 199);
            this.btnCreateOrder.Name = "btnCreateOrder";
            this.btnCreateOrder.Size = new System.Drawing.Size(392, 43);
            this.btnCreateOrder.TabIndex = 4;
            this.btnCreateOrder.Text = "Criar Encomenda";
            this.btnCreateOrder.UseVisualStyleBackColor = true;
            this.btnCreateOrder.Click += new System.EventHandler(this.btnCreateOrder_Click);
            // 
            // product_quantity
            // 
            this.product_quantity.Location = new System.Drawing.Point(34, 88);
            this.product_quantity.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.product_quantity.Name = "product_quantity";
            this.product_quantity.Size = new System.Drawing.Size(155, 22);
            this.product_quantity.TabIndex = 5;
            this.product_quantity.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // listBoxEncomendas
            // 
            this.listBoxEncomendas.FormattingEnabled = true;
            this.listBoxEncomendas.ItemHeight = 16;
            this.listBoxEncomendas.Location = new System.Drawing.Point(488, 46);
            this.listBoxEncomendas.Name = "listBoxEncomendas";
            this.listBoxEncomendas.Size = new System.Drawing.Size(280, 372);
            this.listBoxEncomendas.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(485, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 16);
            this.label2.TabIndex = 7;
            this.label2.Text = "Encomendas";
            // 
            // btnSubscribeToOrder
            // 
            this.btnSubscribeToOrder.Location = new System.Drawing.Point(34, 269);
            this.btnSubscribeToOrder.Name = "btnSubscribeToOrder";
            this.btnSubscribeToOrder.Size = new System.Drawing.Size(392, 43);
            this.btnSubscribeToOrder.TabIndex = 8;
            this.btnSubscribeToOrder.Text = "Subscrever a uma encomenda";
            this.btnSubscribeToOrder.UseVisualStyleBackColor = true;
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 16;
            this.listBox1.Location = new System.Drawing.Point(34, 355);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(392, 324);
            this.listBox1.TabIndex = 9;
            // 
            // EncomendaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 707);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.btnSubscribeToOrder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listBoxEncomendas);
            this.Controls.Add(this.product_quantity);
            this.Controls.Add(this.btnCreateOrder);
            this.Controls.Add(this.listBoxProdutos);
            this.Controls.Add(this.btnAddToCart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxProduto);
            this.Name = "EncomendaForm";
            this.Text = "EncomendaForm";
            this.Load += new System.EventHandler(this.EncomendaForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.product_quantity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxProduto;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAddToCart;
        private System.Windows.Forms.ListBox listBoxProdutos;
        private System.Windows.Forms.Button btnCreateOrder;
        private System.Windows.Forms.NumericUpDown product_quantity;
        private System.Windows.Forms.ListBox listBoxEncomendas;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSubscribeToOrder;
        private System.Windows.Forms.ListBox listBox1;
    }
}

