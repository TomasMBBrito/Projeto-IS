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
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxUtilizador = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.product_quantity)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxProduto
            // 
            this.textBoxProduto.Enabled = false;
            this.textBoxProduto.Location = new System.Drawing.Point(23, 115);
            this.textBoxProduto.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBoxProduto.Name = "textBoxProduto";
            this.textBoxProduto.Size = new System.Drawing.Size(117, 20);
            this.textBoxProduto.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 91);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Nome do Produto";
            // 
            // btnAddToCart
            // 
            this.btnAddToCart.Enabled = false;
            this.btnAddToCart.Location = new System.Drawing.Point(23, 184);
            this.btnAddToCart.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnAddToCart.Name = "btnAddToCart";
            this.btnAddToCart.Size = new System.Drawing.Size(116, 32);
            this.btnAddToCart.TabIndex = 2;
            this.btnAddToCart.Text = "Adicionar ao Carrinho";
            this.btnAddToCart.UseVisualStyleBackColor = true;
            this.btnAddToCart.Click += new System.EventHandler(this.btnAddToCart_Click);
            // 
            // listBoxProdutos
            // 
            this.listBoxProdutos.Enabled = false;
            this.listBoxProdutos.FormattingEnabled = true;
            this.listBoxProdutos.Location = new System.Drawing.Point(158, 112);
            this.listBoxProdutos.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.listBoxProdutos.Name = "listBoxProdutos";
            this.listBoxProdutos.Size = new System.Drawing.Size(161, 108);
            this.listBoxProdutos.TabIndex = 3;
            // 
            // btnCreateOrder
            // 
            this.btnCreateOrder.Enabled = false;
            this.btnCreateOrder.Location = new System.Drawing.Point(23, 239);
            this.btnCreateOrder.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnCreateOrder.Name = "btnCreateOrder";
            this.btnCreateOrder.Size = new System.Drawing.Size(294, 35);
            this.btnCreateOrder.TabIndex = 4;
            this.btnCreateOrder.Text = "Criar Encomenda";
            this.btnCreateOrder.UseVisualStyleBackColor = true;
            this.btnCreateOrder.Click += new System.EventHandler(this.btnCreateOrder_Click);
            // 
            // product_quantity
            // 
            this.product_quantity.Enabled = false;
            this.product_quantity.Location = new System.Drawing.Point(23, 149);
            this.product_quantity.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.product_quantity.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.product_quantity.Name = "product_quantity";
            this.product_quantity.Size = new System.Drawing.Size(116, 20);
            this.product_quantity.TabIndex = 5;
            this.product_quantity.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // listBoxEncomendas
            // 
            this.listBoxEncomendas.Enabled = false;
            this.listBoxEncomendas.FormattingEnabled = true;
            this.listBoxEncomendas.Location = new System.Drawing.Point(323, 38);
            this.listBoxEncomendas.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.listBoxEncomendas.Name = "listBoxEncomendas";
            this.listBoxEncomendas.Size = new System.Drawing.Size(274, 303);
            this.listBoxEncomendas.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(364, 14);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Encomendas";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Utilizador";
            // 
            // textBoxUtilizador
            // 
            this.textBoxUtilizador.Location = new System.Drawing.Point(25, 54);
            this.textBoxUtilizador.Name = "textBoxUtilizador";
            this.textBoxUtilizador.Size = new System.Drawing.Size(177, 20);
            this.textBoxUtilizador.TabIndex = 9;
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(217, 54);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(100, 23);
            this.btnLogin.TabIndex = 10;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // EncomendaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 380);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.textBoxUtilizador);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listBoxEncomendas);
            this.Controls.Add(this.product_quantity);
            this.Controls.Add(this.btnCreateOrder);
            this.Controls.Add(this.listBoxProdutos);
            this.Controls.Add(this.btnAddToCart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxProduto);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
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
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxUtilizador;
        private System.Windows.Forms.Button btnLogin;
    }
}

