using System.Globalization;
using HotelManagement.HoaDon;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.PhieuThue;

public sealed class FormChiTietPhieuThue : Form
{
    private readonly string _maPhieuThue;
    private readonly Label _guest = new();
    private readonly Label _staff = new();
    private readonly Label _created = new();
    private readonly DataGridView _roomGrid = new();
    private readonly DataGridView _serviceGrid = new();
    private readonly Label _selectedRoom = new();

    public FormChiTietPhieuThue(string maPhieuThue)
    {
        _maPhieuThue = maPhieuThue;
        Text = "Chi tiết phiếu thuê";
        ClientSize = new Size(1120, 760);
        MinimumSize = new Size(900, 620);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Microsoft Sans Serif", 9.5F);
        BuildLayout();
        Load += (_, _) => ReloadDetails();
        _roomGrid.SelectionChanged += (_, _) => ReloadServices();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(12), Margin = Padding.Empty };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        Controls.Add(root);

        var title = new Label { Text = $"Chi tiết phiếu thuê {_maPhieuThue}", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(Font, FontStyle.Bold), ForeColor = Color.FromArgb(192, 64, 0) };
        root.Controls.Add(title, 0, 0);

        var info = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(8, 2, 8, 2) };
        for (var i = 0; i < 3; i++) info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        info.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _guest.Dock = _staff.Dock = _created.Dock = DockStyle.Fill;
        _guest.TextAlign = _staff.TextAlign = _created.TextAlign = ContentAlignment.MiddleLeft;
        _guest.Font = _staff.Font = _created.Font = new Font(Font, FontStyle.Bold);
        info.Controls.Add(_guest, 0, 0);
        info.Controls.Add(_staff, 1, 0);
        info.Controls.Add(_created, 2, 0);
        root.Controls.Add(info, 0, 1);

        ConfigureGrid(_roomGrid);
        _roomGrid.Columns.Add(TextColumn(nameof(StayRow.MaCtdp), "Mã chi tiết", 13));
        _roomGrid.Columns.Add(TextColumn(nameof(StayRow.MaPhong), "Phòng", 9));
        _roomGrid.Columns.Add(TextColumn(nameof(StayRow.LoaiPhong), "Loại phòng", 14));
        _roomGrid.Columns.Add(TextColumn(nameof(StayRow.CheckInText), "Nhận phòng", 17));
        _roomGrid.Columns.Add(TextColumn(nameof(StayRow.CheckOutText), "Trả phòng", 17));
        _roomGrid.Columns.Add(TextColumn(nameof(StayRow.TrangThai), "Trạng thái", 13));
        _roomGrid.Columns.Add(TextColumn(nameof(StayRow.ThanhTien), "Tiền phòng", 13, "N0"));
        root.Controls.Add(_roomGrid, 0, 2);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 4, 0, 0) };
        _selectedRoom.Text = "Dịch vụ sử dụng trong phòng";
        _selectedRoom.AutoSize = false;
        _selectedRoom.Width = 280;
        _selectedRoom.TextAlign = ContentAlignment.MiddleLeft;
        _selectedRoom.Font = new Font(Font, FontStyle.Bold);
        actions.Controls.Add(_selectedRoom);
        var status = ActionButton("Cập nhật trạng thái", Color.Moccasin);
        status.Click += (_, _) => UpdateStatus();
        var services = ActionButton("Quản lý dịch vụ", Color.LightPink);
        services.Click += (_, _) => OpenServices();
        var invoice = ActionButton("Hóa đơn", Color.LightSkyBlue);
        invoice.Click += (_, _) => OpenInvoice();
        actions.Controls.Add(status);
        actions.Controls.Add(services);
        actions.Controls.Add(invoice);
        root.Controls.Add(actions, 0, 3);

        ConfigureGrid(_serviceGrid);
        _serviceGrid.Columns.Add(TextColumn(nameof(ServiceRow.MaDv), "Mã DV", 12));
        _serviceGrid.Columns.Add(TextColumn(nameof(ServiceRow.TenDv), "Dịch vụ", 30));
        _serviceGrid.Columns.Add(TextColumn(nameof(ServiceRow.SoLuong), "Số lượng", 12));
        _serviceGrid.Columns.Add(TextColumn(nameof(ServiceRow.DonGia), "Đơn giá", 18, "N0"));
        _serviceGrid.Columns.Add(TextColumn(nameof(ServiceRow.ThanhTien), "Thành tiền", 20, "N0"));
        root.Controls.Add(_serviceGrid, 0, 4);
    }

    private static Button ActionButton(string text, Color color) => new() { Text = text, AutoSize = true, Height = 34, BackColor = color, UseVisualStyleBackColor = false, Margin = new Padding(5, 0, 5, 0) };

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.AutoGenerateColumns = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.DarkGray;
        grid.BorderStyle = BorderStyle.Fixed3D;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.ReadOnly = true;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.RowTemplate.Height = 32;
    }

    private static DataGridViewTextBoxColumn TextColumn(string property, string title, float fill, string? format = null) => new()
    {
        Name = property, DataPropertyName = property, HeaderText = title, FillWeight = fill,
        ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable,
        DefaultCellStyle = { Format = format ?? string.Empty }
    };

    private void ReloadDetails()
    {
        try
        {
            using var db = new HotelManagementContext();
            var ticket = db.PhieuThues.AsNoTracking()
                .Where(item => item.MaPt == _maPhieuThue && item.DaXoa != true)
                .Select(item => new { item.NgPt, Khach = item.MaKhNavigation.TenKh, NhanVien = item.MaNvNavigation.TenNv })
                .FirstOrDefault();
            if (ticket is null)
            {
                MessageBox.Show("Không tìm thấy phiếu thuê.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }
            _guest.Text = "Khách hàng: " + ticket.Khach;
            _staff.Text = "Nhân viên: " + ticket.NhanVien;
            _created.Text = "Ngày lập: " + ticket.NgPt.ToString("dd/MM/yyyy HH:mm");
            var rows = db.Ctdps.AsNoTracking().Where(item => item.MaPt == _maPhieuThue && item.DaXoa != true)
                .OrderBy(item => item.CheckIn)
                .Select(item => new StayRow
                {
                    MaCtdp = item.MaCtdp,
                    MaPhong = item.MaPh,
                    LoaiPhong = item.MaPhNavigation.MaLphNavigation.TenLph,
                    CheckIn = item.CheckIn,
                    CheckOut = item.CheckOut,
                    TrangThai = item.TrangThai,
                    ThanhTien = item.ThanhTien ?? 0
                }).ToList();
            foreach (var row in rows)
            {
                row.CheckInText = row.CheckIn.ToString("dd/MM/yyyy HH:mm");
                row.CheckOutText = row.CheckOut.ToString("dd/MM/yyyy HH:mm");
            }
            _roomGrid.DataSource = rows;
            if (_roomGrid.Rows.Count > 0 && _roomGrid.CurrentCell is null)
                _roomGrid.Rows[0].Selected = true;
            ReloadServices();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không tải được phiếu thuê.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string? SelectedDetailCode() => _roomGrid.CurrentRow?.DataBoundItem is StayRow row ? row.MaCtdp : null;

    private string? SelectedRoomCode() => _roomGrid.CurrentRow?.DataBoundItem is StayRow row ? row.MaPhong : null;

    private void ReloadServices()
    {
        var code = SelectedDetailCode();
        _selectedRoom.Text = code is null ? "Chọn một phòng để xem dịch vụ" : $"Dịch vụ sử dụng trong phòng {SelectedRoomCode()} ({code})";
        if (code is null)
        {
            _serviceGrid.DataSource = null;
            return;
        }
        try
        {
            using var db = new HotelManagementContext();
            _serviceGrid.DataSource = db.Ctdvs.AsNoTracking().Where(line => line.MaCtdp == code && line.DaXoa != true)
                .OrderBy(line => line.MaDv)
                .Select(line => new ServiceRow
                {
                    MaDv = line.MaDv,
                    TenDv = line.MaDvNavigation.TenDv,
                    SoLuong = line.Sl,
                    DonGia = line.DonGia ?? line.MaDvNavigation.DonGia,
                    ThanhTien = line.ThanhTien ?? 0
                }).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không tải được dịch vụ đã dùng.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateStatus()
    {
        var code = SelectedDetailCode();
        if (code is null) return;
        using var dialog = new FormSuaChiTietPhieuThue(code);
        if (dialog.ShowDialog(this) == DialogResult.OK) ReloadDetails();
    }

    private void OpenServices()
    {
        var code = SelectedDetailCode();
        var room = SelectedRoomCode();
        if (code is null || room is null)
        {
            MessageBox.Show("Chọn một phòng trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new FormDanhSachDichVuPhongDaDung(code, room);
        dialog.ShowDialog(this);
        ReloadServices();
    }

    private void OpenInvoice()
    {
        var code = SelectedDetailCode();
        if (code is null)
        {
            MessageBox.Show("Chọn một phòng để lập hóa đơn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = FormHoaDon.ForStay(code);
        dialog.ShowDialog(this);
        ReloadDetails();
    }

    private sealed class StayRow
    {
        public string MaCtdp { get; init; } = "";
        public string MaPhong { get; init; } = "";
        public string LoaiPhong { get; init; } = "";
        public DateTime CheckIn { get; init; }
        public DateTime CheckOut { get; init; }
        public string CheckInText { get; set; } = "";
        public string CheckOutText { get; set; } = "";
        public string TrangThai { get; init; } = "";
        public decimal ThanhTien { get; init; }
    }

    private sealed class ServiceRow
    {
        public string MaDv { get; init; } = "";
        public string TenDv { get; init; } = "";
        public int SoLuong { get; init; }
        public decimal DonGia { get; init; }
        public decimal ThanhTien { get; init; }
    }
}

public sealed class FormSuaChiTietPhieuThue : Form
{
    private static readonly string[] Statuses = ["Đã đặt", "Đang thuê", "Đã xong", "Đã hủy"];
    private readonly string _maCtdp;
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };

    public FormSuaChiTietPhieuThue(string maCtdp)
    {
        _maCtdp = maCtdp;
        Text = "Cập nhật chi tiết phiếu thuê";
        ClientSize = new Size(580, 235);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = MinimizeBox = false;
        _status.Items.AddRange(Statuses);
        BuildLayout();
        Load += LoadCurrentStatus;
    }

    private void BuildLayout()
    {
        var title = new Label { Text = $"Cập nhật trạng thái chi tiết phiếu thuê {_maCtdp}", Dock = DockStyle.Top, Height = 55, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(192, 64, 0) };
        var field = new TableLayoutPanel { Dock = DockStyle.Top, Height = 54, ColumnCount = 2, Padding = new Padding(55, 10, 55, 0) };
        field.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        field.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        field.Controls.Add(new Label { Text = "Trạng thái", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
        field.Controls.Add(_status, 1, 0);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 66, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(20, 8, 35, 8) };
        var close = new Button { Text = "Thoát", Width = 112, Height = 39, BackColor = SystemColors.GradientActiveCaption, DialogResult = DialogResult.Cancel };
        var save = new Button { Text = "Cập nhật", Width = 125, Height = 39, BackColor = Color.Moccasin, UseVisualStyleBackColor = false };
        save.Click += SaveStatus;
        actions.Controls.Add(close);
        actions.Controls.Add(save);
        Controls.Add(field);
        Controls.Add(title);
        Controls.Add(actions);
        CancelButton = close;
        AcceptButton = save;
    }

    private void LoadCurrentStatus(object? sender, EventArgs e)
    {
        try
        {
            using var db = new HotelManagementContext();
            var detail = db.Ctdps.AsNoTracking().FirstOrDefault(item => item.MaCtdp == _maCtdp && item.DaXoa != true);
            if (detail is null) { MessageBox.Show("Không tìm thấy chi tiết phiếu thuê.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); Close(); return; }
            if (!Statuses.Contains(detail.TrangThai)) _status.Items.Add(detail.TrangThai);
            _status.SelectedItem = detail.TrangThai;
        }
        catch (Exception ex) { MessageBox.Show($"Không tải được dữ liệu.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error); Close(); }
    }

    private void SaveStatus(object? sender, EventArgs e)
    {
        if (_status.SelectedItem is not string newStatus) return;
        try
        {
            using var db = new HotelManagementContext();
            var detail = db.Ctdps.FirstOrDefault(item => item.MaCtdp == _maCtdp && item.DaXoa != true);
            if (detail is null) throw new InvalidOperationException("Không tìm thấy chi tiết phiếu thuê.");
            var hasPaidInvoice = db.HoaDons.Any(item => item.MaCtdp == _maCtdp && item.DaXoa != true && item.TrangThai == "Đã thanh toán");
            if (hasPaidInvoice && newStatus == "Đã hủy") throw new InvalidOperationException("Chi tiết đã thanh toán nên không thể hủy.");
            detail.TrangThai = newStatus;
            var room = db.Phongs.FirstOrDefault(item => item.MaPh == detail.MaPh);
            if (room is not null)
            {
                if (newStatus == "Đã xong") room.Ttdd = "Chưa dọn dẹp";
                if (newStatus == "Đang thuê") room.Ttph = "Đang thuê";
                else if (newStatus == "Đã đặt") room.Ttph = "Đã đặt";
                else
                {
                    var otherStays = db.Ctdps.AsNoTracking()
                        .Where(item => item.MaPh == detail.MaPh && item.MaCtdp != detail.MaCtdp && item.DaXoa != true)
                        .Select(item => new { item.TrangThai, item.CheckIn, item.CheckOut }).ToList();
                    if (otherStays.Any(item => item.TrangThai == "Đang thuê" && item.CheckIn <= DateTime.Now && item.CheckOut > DateTime.Now))
                        room.Ttph = "Đang thuê";
                    else if (otherStays.Any(item => item.TrangThai == "Đã đặt"))
                        room.Ttph = "Đã đặt";
                    else if (room.Ttph != "Đang sửa chữa")
                        room.Ttph = "Bình thường";
                }
            }
            db.SaveChanges();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { MessageBox.Show($"Không cập nhật được trạng thái.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

public sealed class FormDanhSachDichVuPhongDaDung : Form
{
    private readonly string _maCtdp;
    private readonly string _maPhong;
    private readonly DataGridView _grid = new();

    public FormDanhSachDichVuPhongDaDung(string maCtdp, string maPhong)
    {
        _maCtdp = maCtdp;
        _maPhong = maPhong;
        Text = "Dịch vụ phòng đã sử dụng";
        ClientSize = new Size(780, 560);
        MinimumSize = new Size(680, 460);
        StartPosition = FormStartPosition.CenterParent;
        BuildLayout();
        Load += (_, _) => ReloadRows();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        var title = new Label { Text = $"Dịch vụ sử dụng - Phòng {_maPhong} / {_maCtdp}", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(192, 64, 0) };
        root.Controls.Add(title, 0, 0);
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.ReadOnly = true;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.BackgroundColor = Color.DarkGray;
        AddColumn(nameof(UsageRow.MaDv), "Mã DV", 14);
        AddColumn(nameof(UsageRow.TenDv), "Tên dịch vụ", 31);
        AddColumn(nameof(UsageRow.SoLuong), "Số lượng", 14);
        AddColumn(nameof(UsageRow.DonGia), "Đơn giá", 18, "N0");
        AddColumn(nameof(UsageRow.ThanhTien), "Thành tiền", 21, "N0");
        root.Controls.Add(_grid, 0, 1);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 7, 0, 0) };
        var close = new Button { Text = "Đóng", Width = 100, Height = 34, DialogResult = DialogResult.Cancel };
        var remove = new Button { Text = "Xóa dịch vụ", Width = 126, Height = 34, BackColor = Color.MistyRose, UseVisualStyleBackColor = false };
        var edit = new Button { Text = "Sửa số lượng", Width = 132, Height = 34, BackColor = Color.Moccasin, UseVisualStyleBackColor = false };
        var add = new Button { Text = "Thêm dịch vụ", Width = 132, Height = 34, BackColor = Color.LightPink, UseVisualStyleBackColor = false };
        add.Click += (_, _) => AddUsage();
        edit.Click += (_, _) => EditUsage();
        remove.Click += (_, _) => DeleteUsage();
        actions.Controls.Add(close);
        actions.Controls.Add(remove);
        actions.Controls.Add(edit);
        actions.Controls.Add(add);
        root.Controls.Add(actions, 0, 2);
        Controls.Add(root);
        CancelButton = close;
    }

    private void AddColumn(string property, string title, float fill, string? format = null) => _grid.Columns.Add(new DataGridViewTextBoxColumn
    {
        Name = property, DataPropertyName = property, HeaderText = title, FillWeight = fill, ReadOnly = true,
        SortMode = DataGridViewColumnSortMode.NotSortable, DefaultCellStyle = { Format = format ?? string.Empty }
    });

    private UsageRow? SelectedRow => _grid.CurrentRow?.DataBoundItem as UsageRow;

    private void ReloadRows()
    {
        try
        {
            using var db = new HotelManagementContext();
            _grid.DataSource = db.Ctdvs.AsNoTracking().Where(item => item.MaCtdp == _maCtdp && item.DaXoa != true)
                .OrderBy(item => item.MaDv)
                .Select(item => new UsageRow { MaDv = item.MaDv, TenDv = item.MaDvNavigation.TenDv, SoLuong = item.Sl, DonGia = item.DonGia ?? item.MaDvNavigation.DonGia, ThanhTien = item.ThanhTien ?? 0 })
                .ToList();
        }
        catch (Exception ex) { MessageBox.Show($"Không tải được dịch vụ.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void AddUsage()
    {
        using var dialog = new FormThemDichVuPhongDaDung(_maCtdp, _maPhong);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.ServiceCode is null) return;
        try { SaveUsage(dialog.ServiceCode, dialog.Quantity, increment: true); ReloadRows(); }
        catch (Exception ex) { MessageBox.Show($"Không thêm được dịch vụ.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void EditUsage()
    {
        if (SelectedRow is not { } row) return;
        using var dialog = new QuantityDialog(row.SoLuong, "Sửa số lượng dịch vụ");
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { SaveUsage(row.MaDv, dialog.Quantity, increment: false); ReloadRows(); }
        catch (Exception ex) { MessageBox.Show($"Không cập nhật được dịch vụ.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void DeleteUsage()
    {
        if (SelectedRow is not { } row) return;
        if (MessageBox.Show($"Xóa dịch vụ {row.TenDv} khỏi phòng { _maPhong }?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            using var db = new HotelManagementContext();
            EnsureServiceEditable(db);
            var line = db.Ctdvs.FirstOrDefault(item => item.MaCtdp == _maCtdp && item.MaDv == row.MaDv);
            var service = db.DichVus.FirstOrDefault(item => item.MaDv == row.MaDv);
            if (line is null) return;
            if (service?.SlconLai is >= 0) service.SlconLai += line.Sl;
            db.Ctdvs.Remove(line);
            db.SaveChanges();
            RefreshDraftInvoice(db);
            ReloadRows();
        }
        catch (Exception ex) { MessageBox.Show($"Không xóa được dịch vụ.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void SaveUsage(string serviceCode, int quantity, bool increment)
    {
        if (quantity < 1 || quantity > 100000) throw new InvalidOperationException("Số lượng phải từ 1 đến 100.000.");
        using var db = new HotelManagementContext();
        EnsureServiceEditable(db);
        var service = db.DichVus.FirstOrDefault(item => item.MaDv == serviceCode && item.DaXoa != true)
            ?? throw new InvalidOperationException("Dịch vụ đã bị ngừng hoặc không tồn tại.");
        var line = db.Ctdvs.FirstOrDefault(item => item.MaCtdp == _maCtdp && item.MaDv == serviceCode);
        var oldQuantity = line is null || line.DaXoa == true ? 0 : line.Sl;
        var newQuantity = increment ? checked(oldQuantity + quantity) : quantity;
        var delta = newQuantity - oldQuantity;
        if (service.SlconLai is >= 0 && delta > service.SlconLai.Value)
            throw new InvalidOperationException($"Số lượng tồn kho chỉ còn {service.SlconLai.Value}.");
        if (service.SlconLai is >= 0) service.SlconLai -= delta;
        if (line is null)
        {
            line = new Ctdv { MaCtdp = _maCtdp, MaDv = serviceCode, Sl = newQuantity, DonGia = service.DonGia, ThanhTien = service.DonGia * newQuantity, DaXoa = false };
            db.Ctdvs.Add(line);
        }
        else
        {
            line.Sl = newQuantity;
            line.DonGia = service.DonGia;
            line.ThanhTien = service.DonGia * newQuantity;
            line.DaXoa = false;
        }
        db.SaveChanges();
        RefreshDraftInvoice(db);
    }

    private void EnsureServiceEditable(HotelManagementContext db)
    {
        var detail = db.Ctdps.AsNoTracking().FirstOrDefault(item => item.MaCtdp == _maCtdp && item.DaXoa != true)
            ?? throw new InvalidOperationException("Không tìm thấy chi tiết phiếu thuê.");
        if (detail.TrangThai is "Đã xong" or "Đã hủy") throw new InvalidOperationException("Không thể sửa dịch vụ của chi tiết đã kết thúc hoặc đã hủy.");
        if (db.HoaDons.Any(item => item.MaCtdp == _maCtdp && item.DaXoa != true && item.TrangThai == "Đã thanh toán"))
            throw new InvalidOperationException("Hóa đơn đã thanh toán nên không thể sửa dịch vụ.");
    }

    private void RefreshDraftInvoice(HotelManagementContext db)
    {
        var invoice = db.HoaDons.FirstOrDefault(item => item.MaCtdp == _maCtdp && item.DaXoa != true && item.TrangThai != "Đã thanh toán");
        if (invoice is null) return;
        var roomAmount = db.Ctdps.Where(item => item.MaCtdp == _maCtdp).Select(item => item.ThanhTien ?? 0).FirstOrDefault();
        var serviceAmount = db.Ctdvs.Where(item => item.MaCtdp == _maCtdp).Select(item => item.ThanhTien ?? 0).Sum();
        invoice.TriGia = roomAmount + serviceAmount;
        db.SaveChanges();
    }

    private sealed class UsageRow
    {
        public string MaDv { get; init; } = "";
        public string TenDv { get; init; } = "";
        public int SoLuong { get; init; }
        public decimal DonGia { get; init; }
        public decimal ThanhTien { get; init; }
    }
}

public sealed class FormThemDichVuPhongDaDung : Form
{
    private readonly string _maCtdp;
    private readonly string _maPhong;
    private readonly ComboBox _service = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly TextBox _code = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly NumericUpDown _quantity = new() { Minimum = 1, Maximum = 100000, Value = 1, Dock = DockStyle.Fill };
    private readonly TextBox _price = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly Label _total = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Red, Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold) };
    private List<ServiceOption> _options = [];

    public string? ServiceCode => (_service.SelectedItem as ServiceOption)?.MaDv;
    public int Quantity => decimal.ToInt32(_quantity.Value);

    public FormThemDichVuPhongDaDung(string maCtdp, string maPhong)
    {
        _maCtdp = maCtdp;
        _maPhong = maPhong;
        Text = "Thêm dịch vụ đã dùng theo phòng";
        ClientSize = new Size(700, 480);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = MinimizeBox = false;
        BuildLayout();
        Load += LoadServices;
        _service.SelectedIndexChanged += (_, _) => UpdatePreview();
        _quantity.ValueChanged += (_, _) => UpdatePreview();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(22) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        var title = new Label { Text = "Thêm dịch vụ phòng", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Microsoft Sans Serif", 17F, FontStyle.Bold), ForeColor = Color.Red };
        root.Controls.Add(title, 0, 0);

        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4, Padding = new Padding(12), CellBorderStyle = TableLayoutPanelCellBorderStyle.Single };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
        for (var i = 0; i < 4; i++) fields.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        AddField(fields, "Phòng", new Label { Text = _maPhong, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.DarkOrange, Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold) }, 0, 0);
        AddField(fields, "Chi tiết phiếu", new Label { Text = _maCtdp, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.Green, Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold) }, 2, 0);
        AddField(fields, "Mã dịch vụ", _code, 0, 1);
        AddField(fields, "Tên dịch vụ", _service, 2, 1);
        AddField(fields, "Số lượng", _quantity, 0, 2);
        AddField(fields, "Đơn giá", _price, 2, 2);
        fields.Controls.Add(_total, 0, 3);
        fields.SetColumnSpan(_total, 4);
        root.Controls.Add(fields, 0, 1);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 12, 0, 0) };
        var cancel = new Button { Text = "OK", Width = 110, Height = 42, BackColor = SystemColors.GradientActiveCaption, DialogResult = DialogResult.Cancel };
        var add = new Button { Text = "Thêm dịch vụ", Width = 155, Height = 42, BackColor = Color.Orchid, UseVisualStyleBackColor = false };
        add.Click += (_, _) => ValidateSelection();
        actions.Controls.Add(cancel);
        actions.Controls.Add(add);
        root.Controls.Add(actions, 0, 2);
        Controls.Add(root);
        CancelButton = cancel;
        AcceptButton = add;
    }

    private static void AddField(TableLayoutPanel layout, string caption, Control input, int column, int row)
    {
        var label = new Label { Text = caption, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(4) };
        input.Margin = new Padding(4, 9, 4, 9);
        layout.Controls.Add(label, column, row);
        layout.Controls.Add(input, column + 1, row);
    }

    private void LoadServices(object? sender, EventArgs e)
    {
        try
        {
            using var db = new HotelManagementContext();
            _options = db.DichVus.AsNoTracking().Where(item => item.DaXoa != true && (item.SlconLai == null || item.SlconLai < 0 || item.SlconLai > 0))
                .OrderBy(item => item.TenDv)
                .Select(item => new ServiceOption { MaDv = item.MaDv, TenDv = item.TenDv, DonGia = item.DonGia, TonKho = item.SlconLai })
                .ToList();
            _service.Items.AddRange(_options.Cast<object>().ToArray());
            if (_service.Items.Count > 0) _service.SelectedIndex = 0;
            else MessageBox.Show("Hiện không có dịch vụ còn tồn kho để thêm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Không tải được danh sách dịch vụ.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void UpdatePreview()
    {
        if (_service.SelectedItem is not ServiceOption item) return;
        _code.Text = item.MaDv;
        _price.Text = item.DonGia.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
        _total.Text = "Thành tiền: " + (item.DonGia * Quantity).ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
    }

    private void ValidateSelection()
    {
        if (_service.SelectedItem is not ServiceOption) { MessageBox.Show("Chọn dịch vụ cần thêm.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        DialogResult = DialogResult.OK;
        Close();
    }

    private sealed class ServiceOption
    {
        public string MaDv { get; init; } = "";
        public string TenDv { get; init; } = "";
        public decimal DonGia { get; init; }
        public int? TonKho { get; init; }
        public override string ToString() => $"{TenDv}  ({MaDv})";
    }
}

internal sealed class QuantityDialog : Form
{
    private readonly NumericUpDown _quantity = new()
    {
        Minimum = 1,
        Maximum = 100000,
        TextAlign = HorizontalAlignment.Center,
        ForeColor = Color.Black,
        BackColor = Color.White
    };
    public int Quantity => decimal.ToInt32(_quantity.Value);

    public QuantityDialog(int current, string title)
    {
        Text = title;
        ClientSize = new Size(380, 150);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = MinimizeBox = false;
        _quantity.Value = Math.Clamp(current, 1, 100000);
        var label = new Label { Text = "Số lượng", Location = new Point(20, 27), Size = new Size(95, 30), TextAlign = ContentAlignment.MiddleRight };
        _quantity.Location = new Point(125, 27);
        _quantity.Size = new Size(205, 30);
        var save = new Button { Text = "Lưu", Location = new Point(125, 78), Size = new Size(95, 34), DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Hủy", Location = new Point(235, 78), Size = new Size(95, 34), DialogResult = DialogResult.Cancel };
        Controls.Add(label); Controls.Add(_quantity); Controls.Add(save); Controls.Add(cancel);
        AcceptButton = save; CancelButton = cancel;
    }
}
