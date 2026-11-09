using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.SoDoPhong;

public sealed class FormSoDoPhong : Form
{
    private readonly RadioButton rdbPhongDangSuaChua = new() { Text = "Phòng đang sửa chữa" };
    private readonly RadioButton rdbPhongDangThue = new() { Text = "Phòng đang thuê" };
    private readonly RadioButton rdbPhongDaDat = new() { Text = "Phòng đã đặt" };
    private readonly RadioButton rdbPhongTrong = new() { Text = "Phòng trống" };
    private readonly RadioButton rdbTatCaTrangThaiPhong = new() { Text = "Tất cả phòng" };

    private readonly RadioButton rdbPhongChuaDon = new() { Text = "Chưa dọn dẹp" };
    private readonly RadioButton rdbPhongDaDonDep = new() { Text = "Đã dọn dẹp" };
    private readonly RadioButton rdbTatCaPhong = new() { Text = "Tất cả phòng" };

    private readonly RadioButton rdbPhongVipDoi = new() { Text = "Phòng VIP đôi" };
    private readonly RadioButton rdbPhongVipDon = new() { Text = "Phòng VIP đơn" };
    private readonly RadioButton rdbPhongThuongDoi = new() { Text = "Phòng thường đôi" };
    private readonly RadioButton rdbPhongThuongDon = new() { Text = "Phòng thường đơn" };
    private readonly RadioButton rdbTatCaLoaiPhong = new() { Text = "Tất cả phòng" };

    private readonly DateTimePicker dtNgayChon = new()
    {
        Name = "dtNgayChon",
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dddd, dd MMMM",
        ShowCheckBox = true,
        Checked = false,
        Width = 204,
        Font = new Font("Microsoft Sans Serif", 9F)
    };

    private readonly DateTimePicker dateTimePicker2 = new()
    {
        Name = "dateTimePicker2",
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dddd, dd MMMM",
        Width = 186,
        Font = new Font("Microsoft Sans Serif", 9F)
    };

    private readonly TextBox txtSearchMaPhong = new()
    {
        Name = "txtSearchMaPhong",
        Width = 150,
        Font = new Font("Microsoft Sans Serif", 10F)
    };

    private readonly DataGridView dataGridViewPhong = new();
    private bool _dangKhoiTao;

    public FormSoDoPhong()
    {
        Text = "Sơ đồ phòng";
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        BackColor = SystemColors.Control;
        BuildLayout();
        WireEvents();
        Load += (_, _) => InitializeFilters();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 267));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var left = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.GreenYellow,
            Margin = Padding.Empty,
            Padding = new Padding(4)
        };
        root.Controls.Add(left, 0, 0);

        var filterLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.GreenYellow,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        filterLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 39F));
        filterLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 36F));
        filterLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        filterLayout.Controls.Add(CreateFilterPanel(
            "Trạng thái phòng",
            rdbPhongTrong, rdbPhongDaDat, rdbPhongDangThue, rdbPhongDangSuaChua, rdbTatCaTrangThaiPhong), 0, 0);
        filterLayout.Controls.Add(CreateFilterPanel(
            "Loại phòng",
            rdbPhongThuongDon, rdbPhongThuongDoi, rdbPhongVipDon, rdbPhongVipDoi, rdbTatCaLoaiPhong), 0, 1);
        filterLayout.Controls.Add(CreateFilterPanel(
            "Tình trạng dọn dẹp",
            rdbPhongDaDonDep, rdbPhongChuaDon, rdbTatCaPhong), 0, 2);
        left.Controls.Add(filterLayout);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(right, 1, 0);

        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 2,
            BackColor = SystemColors.Control,
            Padding = new Padding(8, 2, 8, 2),
            Margin = Padding.Empty
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        toolbar.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        toolbar.Controls.Add(CreateToolbarLabel("Chọn ngày"), 0, 0);
        toolbar.Controls.Add(CreateToolbarLabel("Chọn giờ"), 1, 0);
        dtNgayChon.Dock = DockStyle.Fill;
        dtNgayChon.Margin = new Padding(3, 2, 5, 3);
        dateTimePicker2.Dock = DockStyle.Fill;
        dateTimePicker2.Margin = new Padding(3, 2, 5, 3);
        toolbar.Controls.Add(dtNgayChon, 0, 1);
        toolbar.Controls.Add(dateTimePicker2, 1, 1);

        var searchIcon = new PictureBox
        {
            Image = HotelManagement.UI.ResourceImages.FromBytes(Properties.Resources.search),
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.Fixed3D,
            Margin = new Padding(3, 2, 6, 3),
            TabStop = false
        };
        toolbar.Controls.Add(searchIcon, 2, 1);
        txtSearchMaPhong.Dock = DockStyle.Fill;
        txtSearchMaPhong.Margin = new Padding(3, 2, 6, 3);
        toolbar.Controls.Add(txtSearchMaPhong, 3, 1);

        var btnDatPhong = new Button
        {
            Name = "btnDatPhong",
            Text = "Đặt phòng",
            Dock = DockStyle.Fill,
            Margin = new Padding(3, 0, 3, 0),
            BackColor = SystemColors.GradientActiveCaption,
            ForeColor = Color.Blue,
            Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
            UseVisualStyleBackColor = false,
        };
        btnDatPhong.Click += BtnDatPhong_Click;
        toolbar.Controls.Add(btnDatPhong, 4, 1);
        right.Controls.Add(toolbar, 0, 0);

        ConfigureGrid();
        right.Controls.Add(dataGridViewPhong, 0, 1);
    }

    private static Label CreateToolbarLabel(string caption) => new()
    {
        Text = caption,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Microsoft Sans Serif", 9F),
        Margin = Padding.Empty
    };

    private static Panel CreateFilterPanel(string title, params RadioButton[] buttons)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.GreenYellow,
            Margin = new Padding(1, 1, 1, 3),
            Padding = Padding.Empty
        };
        var label = new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
            Location = new Point(10, 8)
        };
        panel.Controls.Add(label);

        var y = 43;
        foreach (var radio in buttons)
        {
            radio.AutoSize = true;
            radio.Font = new Font("Microsoft Sans Serif", 11F);
            radio.Location = new Point(10, y);
            radio.Margin = Padding.Empty;
            radio.UseVisualStyleBackColor = true;
            panel.Controls.Add(radio);
            y += 35;
        }

        return panel;
    }

    private void ConfigureGrid()
    {
        dataGridViewPhong.Name = "dataGridViewPhong";
        dataGridViewPhong.Dock = DockStyle.Fill;
        dataGridViewPhong.AutoGenerateColumns = false;
        dataGridViewPhong.BackgroundColor = Color.DarkGray;
        dataGridViewPhong.BorderStyle = BorderStyle.Fixed3D;
        dataGridViewPhong.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        dataGridViewPhong.GridColor = SystemColors.ControlDark;
        dataGridViewPhong.AllowUserToAddRows = false;
        dataGridViewPhong.AllowUserToDeleteRows = false;
        dataGridViewPhong.AllowUserToResizeRows = false;
        dataGridViewPhong.ReadOnly = true;
        dataGridViewPhong.MultiSelect = false;
        dataGridViewPhong.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewPhong.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridViewPhong.RowHeadersVisible = false;
        dataGridViewPhong.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 9F);
        dataGridViewPhong.Columns.Add(TextColumn("MaPH", "Mã phòng", 12));
        dataGridViewPhong.Columns.Add(TextColumn("Tang", "Tầng", 7));
        dataGridViewPhong.Columns.Add(TextColumn("TenLPH", "Loại phòng", 20));
        dataGridViewPhong.Columns.Add(TextColumn("TTPH", "Trạng thái phòng", 20));
        dataGridViewPhong.Columns.Add(TextColumn("TTDD", "Tình trạng dọn dẹp", 20));
        dataGridViewPhong.Columns.Add(TextColumn("GhiChu", "Ghi chú", 21));
    }

    private static DataGridViewTextBoxColumn TextColumn(string property, string title, float weight) => new()
    {
        DataPropertyName = property,
        Name = property,
        HeaderText = title,
        FillWeight = weight,
        ReadOnly = true,
        SortMode = DataGridViewColumnSortMode.Automatic
    };

    private void WireEvents()
    {
        foreach (var radio in new[]
        {
            rdbPhongDangSuaChua, rdbPhongDangThue, rdbPhongDaDat, rdbPhongTrong, rdbTatCaTrangThaiPhong,
            rdbPhongChuaDon, rdbPhongDaDonDep, rdbTatCaPhong,
            rdbPhongVipDoi, rdbPhongVipDon, rdbPhongThuongDoi, rdbPhongThuongDon, rdbTatCaLoaiPhong
        })
        {
            radio.CheckedChanged += (_, _) => LocTheoRadio(radio);
        }

        txtSearchMaPhong.TextChanged += (_, _) =>
        {
            if (!_dangKhoiTao)
                TaiLaiDanhSach();
        };
        dtNgayChon.ValueChanged += (_, _) =>
        {
            if (!_dangKhoiTao)
                TaiLaiDanhSach();
        };
    }

    private void InitializeFilters()
    {
        _dangKhoiTao = true;
        rdbTatCaTrangThaiPhong.Checked = true;
        rdbTatCaLoaiPhong.Checked = true;
        rdbTatCaPhong.Checked = true;
        dtNgayChon.ShowCheckBox = true;
        dtNgayChon.Checked = false;
        dateTimePicker2.Value = DateTime.Today;
        ToolTip toolTip = new();
        toolTip.SetToolTip(txtSearchMaPhong, "Ví dụ: 101, P20...");

        _dangKhoiTao = false;
        TaiLaiDanhSach();
    }

    private void LocTheoRadio(RadioButton radio)
    {
        if (_dangKhoiTao || !radio.Checked)
            return;
        TaiLaiDanhSach();
    }

    private string? TrangThaiPhongDangChon()
    {
        if (rdbPhongTrong.Checked) return "Bình thường";
        if (rdbPhongDaDat.Checked) return "Đã đặt";
        if (rdbPhongDangThue.Checked) return "Đang thuê";
        if (rdbPhongDangSuaChua.Checked) return "Đang sửa chữa";
        return null;
    }

    private string? MaLoaiPhongDangChon()
    {
        if (rdbPhongThuongDon.Checked) return "NOR01";
        if (rdbPhongThuongDoi.Checked) return "NOR02";
        if (rdbPhongVipDon.Checked) return "VIP01";
        if (rdbPhongVipDoi.Checked) return "VIP02";
        return null;
    }

    private string? TinhTrangDonDepDangChon()
    {
        if (rdbPhongDaDonDep.Checked) return "Đã dọn dẹp";
        if (rdbPhongChuaDon.Checked) return "Chưa dọn dẹp";
        return null;
    }

    private string? MaPhongDangChon() =>
        dataGridViewPhong.CurrentRow?.Cells["MaPH"].Value?.ToString();

    private void TaiLaiDanhSach()
    {
        if (_dangKhoiTao || IsDisposed)
            return;

        try
        {
            var ttph = TrangThaiPhongDangChon();
            var maLph = MaLoaiPhongDangChon();
            var ttdd = TinhTrangDonDepDangChon();
            var maPhong = txtSearchMaPhong.Text.Trim();

            using var db = new HotelManagementContext();
            var query = db.Phongs.AsNoTracking().Where(p => p.DaXoa != true);
            if (ttph != null) query = query.Where(p => p.Ttph == ttph);
            if (maLph != null) query = query.Where(p => p.MaLph == maLph);
            if (ttdd != null) query = query.Where(p => p.Ttdd == ttdd);
            if (maPhong.Length > 0) query = query.Where(p => p.MaPh.Contains(maPhong));

            if (dtNgayChon.Checked)
            {
                var ngayChon = dtNgayChon.Value.Date;
                query = query.Where(p => p.Ttph == "Bình thường" &&
                    !p.Ctdps.Any(c => c.DaXoa != true &&
                        ngayChon >= c.CheckIn.Date &&
                        ngayChon < c.CheckOut.Date));
            }

            var dsPhong = query
                .OrderBy(p => p.MaPh)
                .Select(p => new
                {
                    p.MaPh,
                    TenLph = p.MaLphNavigation.TenLph,
                    p.Ttph,
                    p.Ttdd,
                    p.GhiChu
                })
                .ToList()
                .Select(p => new RoomMapRow
                {
                    MaPH = p.MaPh,
                    Tang = p.MaPh.Length > 1 ? p.MaPh.Substring(1, 1) : "",
                    TenLPH = p.TenLph,
                    TTPH = p.Ttph,
                    TTDD = p.Ttdd,
                    GhiChu = p.GhiChu ?? ""
                })
                .ToList();

            dataGridViewPhong.DataSource = dsPhong;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải danh sách phòng: " + ex.Message,
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnDatPhong_Click(object? sender, EventArgs e)
    {
        var maPhong = MaPhongDangChon();
        using var dialog = new FormDatPhong(maPhong);
        if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            TaiLaiDanhSach();
    }

    private sealed class RoomMapRow
    {
        public string MaPH { get; init; } = "";
        public string Tang { get; init; } = "";
        public string TenLPH { get; init; } = "";
        public string TTPH { get; init; } = "";
        public string TTDD { get; init; } = "";
        public string GhiChu { get; init; } = "";
    }
}
