
namespace HotelManagement.DangNhap
{
    partial class FormDangNhap
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">
        /// true if managed resources should be disposed;
        /// otherwise, false.
        /// </param>
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
            panel2 = new Panel();
            label1 = new Label();
            txtMatKhau = new TextBox();
            txtTenDangNhap = new TextBox();
            pictureBox2 = new PictureBox();
            btnDangNhap = new Button();
            linkLabel1 = new LinkLabel();
            label2 = new Label();
            panel1 = new Panel();
            pictureBox1 = new PictureBox();

            panel2.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();

            SuspendLayout();

            // 
            // panel2
            // 
            panel2.Controls.Add(label1);
            panel2.Controls.Add(txtMatKhau);
            panel2.Controls.Add(txtTenDangNhap);
            panel2.Controls.Add(pictureBox2);
            panel2.Controls.Add(btnDangNhap);
            panel2.Controls.Add(linkLabel1);
            panel2.Controls.Add(label2);
            panel2.Location = new Point(448, 16);
            panel2.Margin = new Padding(3, 4, 3, 4);
            panel2.Name = "panel2";
            panel2.Size = new Size(395, 531);
            panel2.TabIndex = 4;

            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font(
                "Microsoft Sans Serif",
                11F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            label1.ForeColor = Color.FromArgb(192, 0, 0);
            label1.Location = new Point(19, 182);
            label1.Name = "label1";
            label1.Size = new Size(141, 24);
            label1.TabIndex = 9;
            label1.Text = "Tên đăng nhập";

            // 
            // txtMatKhau
            // 
            txtMatKhau.Font = new Font(
                "Microsoft Sans Serif",
                14F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            txtMatKhau.Location = new Point(177, 255);
            txtMatKhau.Margin = new Padding(3, 4, 3, 4);
            txtMatKhau.Name = "txtMatKhau";
            txtMatKhau.PasswordChar = '*';
            txtMatKhau.Size = new Size(192, 34);
            txtMatKhau.TabIndex = 8;
            txtMatKhau.KeyDown += txtMatKhau_KeyDown;

            // 
            // txtTenDangNhap
            // 
            txtTenDangNhap.Font = new Font(
                "Microsoft Sans Serif",
                14F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            txtTenDangNhap.Location = new Point(177, 170);
            txtTenDangNhap.Margin = new Padding(3, 4, 3, 4);
            txtTenDangNhap.Name = "txtTenDangNhap";
            txtTenDangNhap.Size = new Size(192, 34);
            txtTenDangNhap.TabIndex = 14;

            // 
            // pictureBox2
            // 
            pictureBox2.Image = Properties.Resources.DangNhap;
            pictureBox2.Location = new Point(97, 44);
            pictureBox2.Margin = new Padding(3, 4, 3, 4);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(203, 75);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 6;
            pictureBox2.TabStop = false;

            // 
            // btnDangNhap
            // 
            btnDangNhap.BackColor = Color.FromArgb(192, 192, 255);
            btnDangNhap.Font = new Font(
                "Microsoft Sans Serif",
                11F,
                FontStyle.Bold,
                GraphicsUnit.Point,
                0);
            btnDangNhap.Location = new Point(117, 380);
            btnDangNhap.Margin = new Padding(3, 4, 3, 4);
            btnDangNhap.Name = "btnDangNhap";
            btnDangNhap.Size = new Size(134, 66);
            btnDangNhap.TabIndex = 5;
            btnDangNhap.Text = "Đăng nhập";
            btnDangNhap.UseVisualStyleBackColor = false;
            btnDangNhap.Click += btnDangNhap_Click;

            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Italic,
                GraphicsUnit.Point,
                0);
            linkLabel1.Location = new Point(222, 328);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(161, 26);
            linkLabel1.TabIndex = 4;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Quên mật khẩu";

            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font(
                "Microsoft Sans Serif",
                11F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            label2.ForeColor = Color.FromArgb(192, 0, 0);
            label2.Location = new Point(62, 265);
            label2.Name = "label2";
            label2.Size = new Size(86, 24);
            label2.TabIndex = 2;
            label2.Text = "Mật khẩu";

            // 
            // panel1
            // 
            panel1.Location = new Point(-22, 16);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(414, 531);
            panel1.TabIndex = 3;

            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.pictureIcon;
            pictureBox1.Location = new Point(-16, 20);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(458, 524);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;

            // 
            // FormDangNhap
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Beige;
            ClientSize = new Size(855, 562);
            Controls.Add(pictureBox1);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormDangNhap";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FormDangNhap";

            panel2.ResumeLayout(false);
            panel2.PerformLayout();

            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();

            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button btnDangNhap;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtMatKhau;
        private System.Windows.Forms.TextBox txtTenDangNhap;
        private System.Windows.Forms.Label label1;
    }
}
