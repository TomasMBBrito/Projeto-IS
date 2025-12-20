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
            ((System.ComponentModel.ISupportInitialize)(this.product_quantity)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxProduto
            // 
            this.textBoxProduto.Location = new System.Drawing.Point(38, 58);
            this.textBoxProduto.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBoxProduto.Name = "textBoxProduto";
            this.textBoxProduto.Size = new System.Drawing.Size(174, 26);
            this.textBoxProduto.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Nome do Produto";
            // 
            // btnAddToCart
            // 
            this.btnAddToCart.Location = new System.Drawing.Point(38, 165);
            this.btnAddToCart.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAddToCart.Name = "btnAddToCart";
            this.btnAddToCart.Size = new System.Drawing.Size(174, 50);
            this.btnAddToCart.TabIndex = 2;
            this.btnAddToCart.Text = "Adicionar ao Carrinho";
            this.btnAddToCart.UseVisualStyleBackColor = true;
            this.btnAddToCart.Click += new System.EventHandler(this.btnAddToCart_Click);
            // 
            // listBoxProdutos
            // 
            this.listBoxProdutos.FormattingEnabled = true;
            this.listBoxProdutos.ItemHeight = 20;
            this.listBoxProdutos.Location = new System.Drawing.Point(240, 54);
            this.listBoxProdutos.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listBoxProdutos.Name = "listBoxProdutos";
            this.listBoxProdutos.Size = new System.Drawing.Size(239, 164);
            this.listBoxProdutos.TabIndex = 3;
            // 
            // btnCreateOrder
            // 
            this.btnCreateOrder.Location = new System.Drawing.Point(38, 249);
            this.btnCreateOrder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCreateOrder.Name = "btnCreateOrder";
            this.btnCreateOrder.Size = new System.Drawing.Size(441, 54);
            this.btnCreateOrder.TabIndex = 4;
            this.btnCreateOrder.Text = "Criar Encomenda";
            this.btnCreateOrder.UseVisualStyleBackColor = true;
            this.btnCreateOrder.Click += new System.EventHandler(this.btnCreateOrder_Click);
            // 
            // product_quantity
            // 
            this.product_quantity.Location = new System.Drawing.Point(38, 110);
            this.product_quantity.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.product_quantity.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.product_quantity.Name = "product_quantity";
            this.product_quantity.Size = new System.Drawing.Size(174, 26);
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
            this.listBoxEncomendas.ItemHeight = 20;
            this.listBoxEncomendas.Location = new System.Drawing.Point(485, 58);
            this.listBoxEncomendas.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listBoxEncomendas.Name = "listBoxEncomendas";
            this.listBoxEncomendas.Size = new System.Drawing.Size(409, 464);
            this.listBoxEncomendas.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(546, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "Encomendas";
            // 
            // EncomendaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 585);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listBoxEncomendas);
            this.Controls.Add(this.product_quantity);
            this.Controls.Add(this.btnCreateOrder);
            this.Controls.Add(this.listBoxProdutos);
            this.Controls.Add(this.btnAddToCart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxProduto);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "EncomendaForm";
            this.Text = "EncomendaForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EncomendaForm_FormClosing);
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
    }
}

