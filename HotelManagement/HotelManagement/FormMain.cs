
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HotelManagement.BaoCao;
using HotelManagement.DichVu;
using HotelManagement.DangNhap;
using HotelManagement.HoaDon;
using HotelManagement.KhachHang;
using HotelManagement.LoaiPhong;
using HotelManagement.NhanVien;
using HotelManagement.PhieuThue;
using HotelManagement.TaiKhoan;
using HotelManagement.Phong;
using HotelManagement.SoDoPhong;
using HotelManagement.TienNghi;

namespace HotelManagement
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            ConfigureResponsiveLayout();
            Resize += (_, _) => LayoutResponsiveContent();

            HienThiThongTinNguoiDung();

            // Kết nối các sự kiện giao diện
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            button13.Click += button13_Click;
            btnTrangChu.Click += btnTrangChu_Click;
            button5.Click += button5_Click;
            button4.Click += button4_Click;
            button7.Click += button7_Click;
            button6.Click += button6_Click;
            btnQuanLyPhong.Click += btnQuanLyPhong_Click;
            button12.Click += button12_Click;
            button11.Click += button11_Click;
            btnSoDoPhong.Click += btnSoDoPhong_Click;
            button9.Click += button9_Click;
            button10.Click += button10_Click;
            Load += FormMain_Load;
        }

        // Chuyển tài nguyên byte[] thành Image.
        // Chỉ sử dụng hàm này khi Resource thực sự có kiểu byte[].
        private static Image ResourceToImage(Image image)
        {
            return image;
        }

        private static Image ResourceToImage(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var image = Image.FromStream(stream);
            return new Bitmap(image);
        }

        private void ConfigureResponsiveLayout()
        {
            AutoScroll = false;
            panel1.Dock = DockStyle.Top;
            panel1.Margin = Padding.Empty;
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            panel2.Margin = Padding.Empty;
            panel4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            panel4.AutoScroll = true;
            panel4.Margin = Padding.Empty;
            panelMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelMain.AutoScroll = false;
            panelMain.Margin = Padding.Empty;
            panelMain.Padding = Padding.Empty;

            var sidebarWidth = panel2.Width;
            foreach (Control control in panel4.Controls)
            {
                if (control is Button button)
                {
                    button.Left = 0;
                    button.Width = sidebarWidth;
                    button.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }
            }

            LayoutResponsiveContent();
        }

        private void LayoutResponsiveContent()
        {
            var sidebarWidth = panel2.Width;
            panel2.Location = new Point(0, panel1.Height);
            panel4.Location = new Point(0, panel2.Bottom);
            panel4.Width = sidebarWidth;
            panel4.Height = Math.Max(0, ClientSize.Height - panel4.Top);

            var contentLeft = sidebarWidth + 8;
            var contentTop = panel1.Height + 13;
            panelMain.Location = new Point(contentLeft, contentTop);
            panelMain.Size = new Size(
                Math.Max(0, ClientSize.Width - contentLeft - 8),
                Math.Max(0, ClientSize.Height - contentTop - 8));
        }

        private void linkLabel1_LinkClicked(
            object? sender,
            LinkLabelLinkClickedEventArgs e)
        {
            DialogResult ketQua = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ketQua != DialogResult.Yes)
                return;

            Hide();

            using (var formLogin = new FormDangNhap())
            {
                if (formLogin.ShowDialog() == DialogResult.OK)
                {
                    HienThiThongTinNguoiDung();
                    HienThiTrangChu();
                    Show();
                }
                else
                {
                    Close();
                }
            }
        }

        private void button13_Click(object? sender, EventArgs e)
        {
            if (FormDangNhap.CapDoQuyen_DangNhap < 2)
            {
                MessageBox.Show("Chức năng thống kê dành cho quản lý và quản trị viên.", "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoadFormCon(new FormBaoCaoKinhDoanh());
        }

        private void btnTrangChu_Click(object? sender, EventArgs e)
        {
            HienThiTrangChu();
        }

        private void button5_Click(object? sender, EventArgs e)
        {
            if (FormDangNhap.CapDoQuyen_DangNhap < 3)
            {
                MessageBox.Show("Chỉ quản trị viên mới được quản lý tài khoản.", "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoadFormCon(new FormDanhSachTaiKhoan());
        }

        private void button4_Click(object? sender, EventArgs e)
        {
            LoadFormCon(new FormDanhSachLoaiPhong());
        }

        private void button7_Click(object? sender, EventArgs e)
        {
            LoadFormCon(new FormDanhSachNhanVien());
        }

        private void button6_Click(object? sender, EventArgs e)
        {
            LoadFormCon(new FormDanhSachKhachHang());
        }

        private void btnQuanLyPhong_Click(object? sender, EventArgs e)
        {
            LoadFormCon(new FormDanhSachPhong());
        }

        private void button12_Click(object? sender, EventArgs e)
        {
            LoadFormCon(new FormDanhSachTienNghi());
        }

        private void button11_Click(object? sender, EventArgs e)
        {
            LoadFormCon(new FormDanhSachDichVu());
        }

        private void btnSoDoPhong_Click(object? sender, EventArgs e)
        {
            LoadFormCon(new FormSoDoPhong());
        }

        private void button9_Click(object? sender, EventArgs e)
        {
            LoadFormCon(new FormDanhSachPhieuThue());
        }

        private void button10_Click(object? sender, EventArgs e)
        {
            LoadFormCon(new FormDanhSachHoaDon());
        }

        private void FormMain_Load(object? sender, EventArgs e)
        {
            HienThiTrangChu();
        }

        private void HienThiThongTinNguoiDung()
        {
            lblTenNguoiDung.Text =
                FormDangNhap.TenNV_DangNhap;
            lblTenNguoiDung.Left = (panel2.Width - lblTenNguoiDung.PreferredWidth) / 2;
            button5.Visible = FormDangNhap.CapDoQuyen_DangNhap >= 3;
            button13.Visible = FormDangNhap.CapDoQuyen_DangNhap >= 2;
        }

        private void XoaNoiDungPanelMain()
        {
            while (panelMain.Controls.Count > 0)
            {
                Control control = panelMain.Controls[0];

                panelMain.Controls.RemoveAt(0);
                control.Dispose();
            }
        }

        private void LoadUserControl(UserControl uc)
        {
            XoaNoiDungPanelMain();

            uc.Dock = DockStyle.Fill;
            panelMain.Controls.Add(uc);
            uc.BringToFront();
        }

        private void LoadFormCon(Form formCon)
        {
            XoaNoiDungPanelMain();

            formCon.TopLevel = false;
            formCon.FormBorderStyle = FormBorderStyle.None;
            formCon.Dock = DockStyle.Fill;

            panelMain.Controls.Add(formCon);
            formCon.Show();
        }

        private void HienThiTrangChu()
        {
            LoadUserControl(new FormTrangChu());
        }

        private void HienThiDangPhatTrien(string tenChucNang)
        {
            XoaNoiDungPanelMain();

            Label lblThongBao = new Label
            {
                Dock = DockStyle.Fill,
                Text = tenChucNang +
                       Environment.NewLine +
                       "(chức năng đang được xây dựng)",
                Font = new Font(
                    "Microsoft Sans Serif",
                    16F,
                    FontStyle.Italic),
                ForeColor = Color.DarkCyan,
                TextAlign = ContentAlignment.MiddleCenter
            };

            panelMain.Controls.Add(lblThongBao);
        }
    }
}
