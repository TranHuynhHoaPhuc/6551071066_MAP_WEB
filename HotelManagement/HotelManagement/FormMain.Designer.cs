
using System.Drawing;
using System.Windows.Forms;

namespace HotelManagement
{
    partial class FormMain
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
            panel1 = new Panel();
            labelTenKhachSan = new Label();
            pictureBox1 = new PictureBox();
            panel2 = new Panel();
            lblTenNguoiDung = new Label();
            pictureBox2 = new PictureBox();
            linkLabel1 = new LinkLabel();
            panelMain = new Panel();
            panel4 = new Panel();
            button13 = new Button();
            btnTrangChu = new Button();
            button5 = new Button();
            button4 = new Button();
            button7 = new Button();
            button6 = new Button();
            btnQuanLyPhong = new Button();
            button12 = new Button();
            button11 = new Button();
            btnSoDoPhong = new Button();
            button9 = new Button();
            button10 = new Button();

            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel4.SuspendLayout();
            SuspendLayout();

            // 
            // panel1
            // 
            panel1.BackColor = Color.SeaGreen;
            panel1.Controls.Add(labelTenKhachSan);
            panel1.Controls.Add(pictureBox1);
            panel1.Location = new Point(1, 4);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(1834, 92);
            panel1.TabIndex = 0;

            // 
            // labelTenKhachSan
            // 
            labelTenKhachSan.AutoSize = true;
            labelTenKhachSan.BackColor = Color.Transparent;
            labelTenKhachSan.Font = new Font(
                "Segoe UI Semibold",
                18F,
                FontStyle.Bold,
                GraphicsUnit.Point,
                0);
            labelTenKhachSan.ForeColor = Color.LightSalmon;
            labelTenKhachSan.Location = new Point(66, 19);
            labelTenKhachSan.Name = "labelTenKhachSan";
            labelTenKhachSan.Size = new Size(285, 41);
            labelTenKhachSan.TabIndex = 3;
            labelTenKhachSan.Text = "Hotel Management";

            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.Menu;
            pictureBox1.Location = new Point(3, 4);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(48, 78);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;

            // 
            // panel2
            // 
            panel2.BackColor = Color.MediumSeaGreen;
            panel2.Controls.Add(lblTenNguoiDung);
            panel2.Controls.Add(pictureBox2);
            panel2.Controls.Add(linkLabel1);
            panel2.Location = new Point(1, 94);
            panel2.Margin = new Padding(3, 4, 3, 4);
            panel2.Name = "panel2";
            panel2.Size = new Size(304, 198);
            panel2.TabIndex = 1;

            // 
            // lblTenNguoiDung
            // 
            lblTenNguoiDung.AutoSize = true;
            lblTenNguoiDung.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Bold,
                GraphicsUnit.Point,
                0);
            lblTenNguoiDung.Location = new Point(28, 159);
            lblTenNguoiDung.Name = "lblTenNguoiDung";
            lblTenNguoiDung.Size = new Size(76, 26);
            lblTenNguoiDung.TabIndex = 2;
            lblTenNguoiDung.Text = "label1";
            lblTenNguoiDung.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // pictureBox2
            // 
            pictureBox2.BackgroundImageLayout = ImageLayout.None;
            pictureBox2.Image = ResourceToImage(Properties.Resources.user);
            pictureBox2.Location = new Point(101, 48);
            pictureBox2.Margin = new Padding(3, 4, 3, 4);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(87, 108);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;

            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            linkLabel1.ForeColor = Color.MidnightBlue;
            linkLabel1.Location = new Point(4, 11);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(111, 26);
            linkLabel1.TabIndex = 0;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Đăng xuất";

            // 
            // panelMain
            // 
            panelMain.AutoScroll = true;
            panelMain.BackColor = Color.Honeydew;
            panelMain.Location = new Point(312, 105);
            panelMain.Margin = new Padding(3, 4, 3, 4);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(1647, 1349);
            panelMain.TabIndex = 2;

            // 
            // panel4
            // 
            panel4.BackColor = Color.DarkCyan;
            panel4.Controls.Add(button13);
            panel4.Controls.Add(btnTrangChu);
            panel4.Controls.Add(button5);
            panel4.Controls.Add(button4);
            panel4.Controls.Add(button7);
            panel4.Controls.Add(button6);
            panel4.Controls.Add(btnQuanLyPhong);
            panel4.Controls.Add(button12);
            panel4.Controls.Add(button11);
            panel4.Controls.Add(btnSoDoPhong);
            panel4.Controls.Add(button9);
            panel4.Controls.Add(button10);
            panel4.Location = new Point(1, 289);
            panel4.Margin = new Padding(3, 4, 3, 4);
            panel4.Name = "panel4";
            panel4.Size = new Size(304, 1161);
            panel4.TabIndex = 4;

            // 
            // button13
            // 
            button13.BackColor = Color.DarkCyan;
            button13.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            button13.ForeColor = SystemColors.ButtonHighlight;
            button13.Image = Properties.Resources.MenuThongKe;
            button13.ImageAlign = ContentAlignment.MiddleLeft;
            button13.Location = new Point(-1, 662);
            button13.Margin = new Padding(3, 4, 3, 4);
            button13.Name = "button13";
            button13.Size = new Size(307, 51);
            button13.TabIndex = 16;
            button13.Text = "Thống kê";
            button13.UseVisualStyleBackColor = false;

            // 
            // btnTrangChu
            // 
            btnTrangChu.BackColor = Color.DarkCyan;
            btnTrangChu.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            btnTrangChu.ForeColor = SystemColors.ButtonHighlight;
            btnTrangChu.Image = Properties.Resources.MenuTrangChu;
            btnTrangChu.ImageAlign = ContentAlignment.MiddleLeft;
            btnTrangChu.Location = new Point(0, 0);
            btnTrangChu.Margin = new Padding(3, 4, 3, 4);
            btnTrangChu.Name = "btnTrangChu";
            btnTrangChu.Size = new Size(304, 69);
            btnTrangChu.TabIndex = 3;
            btnTrangChu.Text = "Trang chủ";
            btnTrangChu.UseVisualStyleBackColor = false;

            // 
            // button5
            // 
            button5.BackColor = Color.DarkCyan;
            button5.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            button5.ForeColor = SystemColors.ButtonHighlight;
            button5.Image = Properties.Resources.MenuQuanLyTaiKhoan;
            button5.ImageAlign = ContentAlignment.MiddleLeft;
            button5.Location = new Point(-2, 605);
            button5.Margin = new Padding(3, 4, 3, 4);
            button5.Name = "button5";
            button5.Size = new Size(307, 51);
            button5.TabIndex = 8;
            button5.Text = "Danh sách tài khoản";
            button5.UseVisualStyleBackColor = false;

            // 
            // button4
            // 
            button4.BackColor = Color.DarkCyan;
            button4.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            button4.ForeColor = SystemColors.ButtonHighlight;
            button4.Image = Properties.Resources.MenuLoaiPhong;
            button4.ImageAlign = ContentAlignment.MiddleLeft;
            button4.Location = new Point(-2, 194);
            button4.Margin = new Padding(3, 4, 3, 4);
            button4.Name = "button4";
            button4.Size = new Size(307, 51);
            button4.TabIndex = 7;
            button4.Text = "Loại phòng";
            button4.UseVisualStyleBackColor = false;

            // 
            // button7
            // 
            button7.BackColor = Color.DarkCyan;
            button7.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            button7.ForeColor = SystemColors.ButtonHighlight;
            button7.Image = Properties.Resources.MenuQuanLyNhanVien;
            button7.ImageAlign = ContentAlignment.MiddleLeft;
            button7.Location = new Point(0, 546);
            button7.Margin = new Padding(3, 4, 3, 4);
            button7.Name = "button7";
            button7.Size = new Size(301, 51);
            button7.TabIndex = 10;
            button7.Text = "Danh sách nhân viên";
            button7.UseVisualStyleBackColor = false;

            // 
            // button6
            // 
            button6.BackColor = Color.DarkCyan;
            button6.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            button6.ForeColor = SystemColors.ButtonHighlight;
            button6.Image = Properties.Resources.MenuQuanLyKhachHang;
            button6.ImageAlign = ContentAlignment.MiddleLeft;
            button6.Location = new Point(-2, 488);
            button6.Margin = new Padding(3, 4, 3, 4);
            button6.Name = "button6";
            button6.Size = new Size(307, 51);
            button6.TabIndex = 9;
            button6.Text = "Danh sách khách hàng";
            button6.UseVisualStyleBackColor = false;

            // 
            // btnQuanLyPhong
            // 
            btnQuanLyPhong.BackColor = Color.DarkCyan;
            btnQuanLyPhong.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            btnQuanLyPhong.ForeColor = SystemColors.ButtonHighlight;
            btnQuanLyPhong.Image = Properties.Resources.MenuPhong;
            btnQuanLyPhong.ImageAlign = ContentAlignment.MiddleLeft;
            btnQuanLyPhong.Location = new Point(0, 135);
            btnQuanLyPhong.Margin = new Padding(3, 4, 3, 4);
            btnQuanLyPhong.Name = "btnQuanLyPhong";
            btnQuanLyPhong.Size = new Size(304, 51);
            btnQuanLyPhong.TabIndex = 6;
            btnQuanLyPhong.Text = "Phòng";
            btnQuanLyPhong.UseVisualStyleBackColor = false;

            // 
            // button12
            // 
            button12.BackColor = Color.DarkCyan;
            button12.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            button12.ForeColor = SystemColors.ButtonHighlight;
            button12.Image = Properties.Resources.MenuTienNghi;
            button12.ImageAlign = ContentAlignment.MiddleLeft;
            button12.Location = new Point(-2, 429);
            button12.Margin = new Padding(3, 4, 3, 4);
            button12.Name = "button12";
            button12.Size = new Size(306, 51);
            button12.TabIndex = 15;
            button12.Text = "Danh sách tiện nghi";
            button12.UseVisualStyleBackColor = false;

            // 
            // button11
            // 
            button11.BackColor = Color.DarkCyan;
            button11.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            button11.ForeColor = SystemColors.ButtonHighlight;
            button11.Image = Properties.Resources.MenuQuanLyDichVu;
            button11.ImageAlign = ContentAlignment.MiddleLeft;
            button11.Location = new Point(-2, 370);
            button11.Margin = new Padding(3, 4, 3, 4);
            button11.Name = "button11";
            button11.Size = new Size(305, 51);
            button11.TabIndex = 14;
            button11.Text = "Danh sách dịch vụ";
            button11.UseVisualStyleBackColor = false;

            // 
            // btnSoDoPhong
            // 
            btnSoDoPhong.AllowDrop = true;
            btnSoDoPhong.BackColor = Color.DarkCyan;
            btnSoDoPhong.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            btnSoDoPhong.ForeColor = SystemColors.ButtonHighlight;
            btnSoDoPhong.Image = Properties.Resources.MenuSoDoPhong;
            btnSoDoPhong.ImageAlign = ContentAlignment.MiddleLeft;
            btnSoDoPhong.Location = new Point(0, 76);
            btnSoDoPhong.Margin = new Padding(3, 4, 3, 4);
            btnSoDoPhong.Name = "btnSoDoPhong";
            btnSoDoPhong.Size = new Size(304, 51);
            btnSoDoPhong.TabIndex = 5;
            btnSoDoPhong.Text = "Sơ đồ phòng";
            btnSoDoPhong.UseVisualStyleBackColor = false;

            // 
            // button9
            // 
            button9.BackColor = Color.DarkCyan;
            button9.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            button9.ForeColor = SystemColors.ButtonHighlight;
            button9.Image = Properties.Resources.MenuDatPhong;
            button9.ImageAlign = ContentAlignment.MiddleLeft;
            button9.Location = new Point(-1, 252);
            button9.Margin = new Padding(3, 4, 3, 4);
            button9.Name = "button9";
            button9.Size = new Size(305, 51);
            button9.TabIndex = 12;
            button9.Text = "Danh sách đặt phòng";
            button9.UseVisualStyleBackColor = false;

            // 
            // button10
            // 
            button10.BackColor = Color.DarkCyan;
            button10.Font = new Font(
                "Microsoft Sans Serif",
                13F,
                FontStyle.Regular,
                GraphicsUnit.Point,
                0);
            button10.ForeColor = SystemColors.ButtonHighlight;
            button10.Image = Properties.Resources.MenuDanhSachHoaDon;
            button10.ImageAlign = ContentAlignment.MiddleLeft;
            button10.Location = new Point(-1, 311);
            button10.Margin = new Padding(3, 4, 3, 4);
            button10.Name = "button10";
            button10.Size = new Size(306, 51);
            button10.TabIndex = 13;
            button10.Text = "Danh sách hóa đơn";
            button10.UseVisualStyleBackColor = false;

            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BackColor = Color.White;
            BackgroundImageLayout = ImageLayout.None;
            ClientSize = new Size(1745, 1055);
            Controls.Add(panel4);
            Controls.Add(panelMain);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;

            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel4.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelTenKhachSan;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label lblTenNguoiDung;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Button btnTrangChu;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button btnQuanLyPhong;
        private System.Windows.Forms.Button btnSoDoPhong;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button13;
    }
}
