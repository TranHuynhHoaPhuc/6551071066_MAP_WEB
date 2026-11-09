using System.Globalization;
using ClosedXML.Excel;
using HotelManagement.DangNhap;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.HoaDon;

public sealed class FormDanhSachHoaDon : Form
{
    private readonly TextBox _search = new() { MaxLength = 100 };
    private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
    private readonly DataGridView _grid = new();

    public FormDanhSachHoaDon()
    {
        Text = "Danh sách hóa đơn";
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        BuildLayout();
        Load += (_, _) => ReloadRows();
        _search.TextChanged += (_, _) => ReloadRows();
        _date.ValueChanged += (_, _) => ReloadRows();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty, Padding = Padding.Empty };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(10, 12, 8, 7), AutoScroll = true };
        toolbar.Controls.Add(new Label { Text = "Tìm kiếm", AutoSize = true, Margin = new Padding(0, 7, 5, 0) });
        _search.Width = 190; _search.Font = new Font("Microsoft Sans Serif", 11F); toolbar.Controls.Add(_search);
        toolbar.Controls.Add(new Label { Text = "Ngày lập", AutoSize = true, Margin = new Padding(14, 7, 5, 0) });
        _date.Width = 150; toolbar.Controls.Add(_date);
        var export = ToolbarButton("Xuất Excel", Color.DarkTurquoise, 130);
        export.Click += (_, _) => ExportExcel();
        var create = ToolbarButton("Tạo hóa đơn", Color.LightPink, 140);
        create.Click += (_, _) => CreateInvoice();
        toolbar.Controls.Add(export); toolbar.Controls.Add(create);
        root.Controls.Add(toolbar, 0, 0);

        ConfigureGrid();
        root.Controls.Add(_grid, 0, 1);
    }

    private static Button ToolbarButton(string text, Color color, int width) => new()
    {
        Text = text, Width = width, Height = 34, BackColor = color, Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold),
        UseVisualStyleBackColor = false, Margin = new Padding(12, 0, 0, 0)
    };

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = Color.DarkGray;
        _grid.BorderStyle = BorderStyle.Fixed3D;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.ReadOnly = true;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.RowTemplate.Height = 38;
        AddColumn(nameof(InvoiceRow.MaHd), "Số hóa đơn", 11);
        AddColumn(nameof(InvoiceRow.KhachHang), "Khách hàng", 19);
        AddColumn(nameof(InvoiceRow.MaPh), "Phòng", 8);
        AddColumn(nameof(InvoiceRow.NgayLapText), "Ngày lập", 16);
        AddColumn(nameof(InvoiceRow.NhanVien), "Nhân viên", 17);
        AddColumn(nameof(InvoiceRow.TriGia), "Trị giá", 13, "N0");
        AddColumn(nameof(InvoiceRow.TrangThai), "Trạng thái", 14);
        var open = new DataGridViewButtonColumn { Name = "Mo", HeaderText = "Chi tiết", Text = "Xem / in", UseColumnTextForButtonValue = true, FillWeight = 12, ReadOnly = true, FlatStyle = FlatStyle.Popup };
        _grid.Columns.Add(open);
        _grid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Mo" || _grid.Rows[e.RowIndex].DataBoundItem is not InvoiceRow row) return;
            OpenInvoice(row.MaHd);
        };
    }

    private void AddColumn(string property, string title, float fill, string? format = null) => _grid.Columns.Add(new DataGridViewTextBoxColumn
    {
        Name = property, DataPropertyName = property, HeaderText = title, FillWeight = fill, ReadOnly = true,
        SortMode = DataGridViewColumnSortMode.NotSortable, DefaultCellStyle = { Format = format ?? string.Empty }
    });

    private void ReloadRows()
    {
        if (IsDisposed || !IsHandleCreated) return;
        try
        {
            var keyword = _search.Text.Trim();
            using var db = new HotelManagementContext();
            var query = db.HoaDons.AsNoTracking().Where(item => item.DaXoa != true);
            if (keyword.Length > 0)
                query = query.Where(item => item.MaHd.Contains(keyword) || item.MaCtdp.Contains(keyword) ||
                    item.MaCtdpNavigation.MaPtNavigation.MaKhNavigation.TenKh.Contains(keyword) || item.MaCtdpNavigation.MaPh.Contains(keyword));
            if (_date.Checked)
            {
                var day = _date.Value.Date;
                query = query.Where(item => item.NgHd >= day && item.NgHd < day.AddDays(1));
            }
            _grid.DataSource = query.OrderByDescending(item => item.NgHd)
                .Select(item => new InvoiceRow
                {
                    MaHd = item.MaHd,
                    MaCtdp = item.MaCtdp,
                    KhachHang = item.MaCtdpNavigation.MaPtNavigation.MaKhNavigation.TenKh,
                    MaPh = item.MaCtdpNavigation.MaPh,
                    NgayLap = item.NgHd,
                    NhanVien = item.MaNvNavigation == null ? "" : item.MaNvNavigation.TenNv,
                    TriGia = item.TriGia ?? 0,
                    TrangThai = item.TrangThai
                }).ToList().Select(row => row with { NgayLapText = row.NgayLap?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa thanh toán" }).ToList();
        }
        catch (Exception ex) { MessageBox.Show($"Không tải được danh sách hóa đơn.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void CreateInvoice()
    {
        try
        {
            using var selection = new StaySelectionDialog();
            if (selection.ShowDialog(this) != DialogResult.OK || selection.SelectedCode is null) return;
            OpenForStay(selection.SelectedCode);
        }
        catch (Exception ex) { MessageBox.Show($"Không mở được danh sách phòng cần lập hóa đơn.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void OpenForStay(string code)
    {
        using var form = FormHoaDon.ForStay(code);
        form.ShowDialog(this);
        ReloadRows();
    }

    private void OpenInvoice(string code)
    {
        using var form = new FormHoaDon(code);
        form.ShowDialog(this);
        ReloadRows();
    }

    private void ExportExcel()
    {
        if (_grid.Rows.Count == 0) { MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        using var dialog = new SaveFileDialog { Title = "Xuất danh sách hóa đơn", Filter = "Excel Workbook (*.xlsx)|*.xlsx", FileName = $"HoaDon_{DateTime.Now:yyyyMMdd_HHmm}.xlsx" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Danh sách hóa đơn");
            var columns = _grid.Columns.Cast<DataGridViewColumn>().Where(column => column.Visible && column.Name != "Mo").ToArray();
            for (var index = 0; index < columns.Length; index++) { sheet.Cell(1, index + 1).Value = columns[index].HeaderText; sheet.Cell(1, index + 1).Style.Font.Bold = true; }
            for (var rowIndex = 0; rowIndex < _grid.Rows.Count; rowIndex++)
                for (var columnIndex = 0; columnIndex < columns.Length; columnIndex++)
                    sheet.Cell(rowIndex + 2, columnIndex + 1).Value = _grid.Rows[rowIndex].Cells[columns[columnIndex].Index].FormattedValue?.ToString() ?? "";
            sheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);
            MessageBox.Show("Xuất Excel thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Không xuất được Excel.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private sealed record InvoiceRow
    {
        public string MaHd { get; init; } = "";
        public string MaCtdp { get; init; } = "";
        public string KhachHang { get; init; } = "";
        public string MaPh { get; init; } = "";
        public DateTime? NgayLap { get; init; }
        public string NgayLapText { get; init; } = "";
        public string NhanVien { get; init; } = "";
        public decimal TriGia { get; init; }
        public string TrangThai { get; init; } = "";
    }
}

public sealed class FormHoaDon : Form
{
    private readonly string? _maHoaDon;
    private readonly string? _maCtdp;
    private string _activeInvoiceId = "";
    private string _customerName = "";
    private string _staffName = "";
    private string _roomCode = "";
    private string _roomType = "";
    private string _status = "";
    private DateTime? _invoiceDate;
    private decimal _roomAmount;
    private decimal _serviceAmount;
    private int _nights;
    private readonly Label _customer = new();
    private readonly Label _room = new();
    private readonly Label _date = new();
    private readonly Label _invoiceNo = new();
    private readonly Label _staff = new();
    private readonly Label _roomTypeLabel = new();
    private readonly Label _nightsLabel = new();
    private readonly Label _roomAmountLabel = new();
    private readonly Label _total = new();
    private readonly Label _statusLabel = new();
    private readonly DataGridView _services = new();
    private readonly Button _paid = new();

    public FormHoaDon(string maHoaDon)
    {
        _maHoaDon = maHoaDon;
        Text = "Hóa đơn";
        ClientSize = new Size(790, 660);
        MinimumSize = new Size(710, 560);
        StartPosition = FormStartPosition.CenterParent;
        BuildLayout();
        Load += (_, _) => LoadInvoice();
    }

    private FormHoaDon(string maCtdp, bool createForStay)
    {
        _maCtdp = maCtdp;
        Text = "Hóa đơn";
        ClientSize = new Size(790, 660);
        MinimumSize = new Size(710, 560);
        StartPosition = FormStartPosition.CenterParent;
        BuildLayout();
        Load += (_, _) => CreateOrOpenInvoice();
    }

    public static FormHoaDon ForStay(string maCtdp) => new(maCtdp, createForStay: true);

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(20, 12, 20, 12) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        Controls.Add(root);
        var title = new Label { Text = "Hóa đơn", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold), ForeColor = Color.OliveDrab };
        root.Controls.Add(title, 0, 0);

        var info = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3, Padding = new Padding(2) };
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112)); info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47));
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88)); info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53));
        for (var i = 0; i < 3; i++) info.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        AddInfo(info, "Khách hàng", _customer, 0, 0);
        AddInfo(info, "Mã phòng", _room, 2, 0);
        AddInfo(info, "Ngày lập", _date, 0, 1);
        AddInfo(info, "Số hóa đơn", _invoiceNo, 2, 1);
        AddInfo(info, "Nhân viên", _staff, 0, 2);
        AddInfo(info, "Loại phòng", _roomTypeLabel, 2, 2);
        root.Controls.Add(info, 0, 1);

        var roomCharge = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(3, 5, 3, 5) };
        roomCharge.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45)); roomCharge.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25)); roomCharge.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        roomCharge.Controls.Add(new Label { Text = "Tiền thuê phòng", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        _nightsLabel.Dock = _roomAmountLabel.Dock = DockStyle.Fill;
        _nightsLabel.TextAlign = _roomAmountLabel.TextAlign = ContentAlignment.MiddleLeft;
        roomCharge.Controls.Add(_nightsLabel, 1, 0); roomCharge.Controls.Add(_roomAmountLabel, 2, 0);
        root.Controls.Add(roomCharge, 0, 2);

        _services.Dock = DockStyle.Fill;
        _services.AutoGenerateColumns = false;
        _services.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _services.BackgroundColor = Color.White;
        _services.AllowUserToAddRows = false;
        _services.AllowUserToDeleteRows = false;
        _services.ReadOnly = true;
        _services.MultiSelect = false;
        _services.RowHeadersVisible = false;
        _services.Columns.Add(InvoiceTextColumn(nameof(ServiceLine.TenDv), "Dịch vụ", 40));
        _services.Columns.Add(InvoiceTextColumn(nameof(ServiceLine.Sl), "Số lượng", 16));
        _services.Columns.Add(InvoiceTextColumn(nameof(ServiceLine.DonGia), "Đơn giá", 20, "N0"));
        _services.Columns.Add(InvoiceTextColumn(nameof(ServiceLine.ThanhTien), "Thành tiền", 24, "N0"));
        root.Controls.Add(_services, 0, 3);

        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(2, 0, 2, 0) };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        _statusLabel.Dock = _total.Dock = DockStyle.Fill;
        _statusLabel.TextAlign = _total.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.Font = new Font(Font, FontStyle.Bold);
        _total.Font = new Font("Microsoft Sans Serif", 13F, FontStyle.Bold);
        _total.ForeColor = Color.DarkGoldenrod;
        footer.Controls.Add(_statusLabel, 0, 0);
        footer.Controls.Add(_total, 1, 0);
        var close = new Button { Text = "Đóng", Size = new Size(90, 34), DialogResult = DialogResult.Cancel, Margin = new Padding(5) };
        var print = new Button { Text = "In / Xuất PDF", Size = new Size(145, 34), BackColor = Color.LightSkyBlue, UseVisualStyleBackColor = false, Margin = new Padding(5) };
        _paid.Text = "Xác nhận thu tiền"; _paid.Size = new Size(150, 34); _paid.BackColor = Color.Moccasin; _paid.UseVisualStyleBackColor = false; _paid.Margin = new Padding(5);
        print.Click += (_, _) => ExportPdf(); _paid.Click += (_, _) => MarkPaid();
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Margin = Padding.Empty, Padding = Padding.Empty };
        actions.Controls.Add(close);
        actions.Controls.Add(_paid);
        actions.Controls.Add(print);
        footer.Controls.Add(actions, 0, 1);
        footer.SetColumnSpan(actions, 2);
        root.Controls.Add(footer, 0, 4);
        CancelButton = close;
    }

    private static void AddInfo(TableLayoutPanel table, string caption, Label value, int column, int row)
    {
        table.Controls.Add(new Label { Text = caption + ":", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, column, row);
        value.Dock = DockStyle.Fill; value.TextAlign = ContentAlignment.MiddleLeft; value.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
        table.Controls.Add(value, column + 1, row);
    }

    private static DataGridViewTextBoxColumn InvoiceTextColumn(string property, string title, float fill, string? format = null) => new()
    {
        Name = property, DataPropertyName = property, HeaderText = title, FillWeight = fill, ReadOnly = true,
        SortMode = DataGridViewColumnSortMode.NotSortable, DefaultCellStyle = { Format = format ?? string.Empty }
    };

    private void CreateOrOpenInvoice()
    {
        try
        {
            using var db = new HotelManagementContext();
            var existing = db.HoaDons.AsNoTracking().Where(item => item.MaCtdp == _maCtdp && item.DaXoa != true).Select(item => item.MaHd).FirstOrDefault();
            if (existing is not null) { _activeInvoiceId = existing; }
            else
            {
                var detail = db.Ctdps.AsNoTracking().FirstOrDefault(item => item.MaCtdp == _maCtdp && item.DaXoa != true)
                    ?? throw new InvalidOperationException("Không tìm thấy chi tiết phòng cần lập hóa đơn.");
                if (detail.TrangThai == "Đã hủy") throw new InvalidOperationException("Không thể lập hóa đơn cho phòng đã hủy.");
                _activeInvoiceId = NextInvoiceCode(db);
                var invoice = new HotelManagement.Models.HoaDon
                {
                    MaHd = _activeInvoiceId,
                    MaCtdp = detail.MaCtdp,
                    NgHd = DateTime.Now,
                    MaNv = string.IsNullOrWhiteSpace(FormDangNhap.MaNV_DangNhap) ? null : FormDangNhap.MaNV_DangNhap,
                    TrangThai = "Chưa thanh toán",
                    TriGia = detail.ThanhTien ?? 0,
                    DaXoa = false
                };
                db.HoaDons.Add(invoice);
                db.SaveChanges();
            }
            LoadInvoice();
        }
        catch (Exception ex) { MessageBox.Show($"Không lập được hóa đơn.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); Close(); }
    }

    private static string NextInvoiceCode(HotelManagementContext db)
    {
        var used = db.HoaDons.AsNoTracking().Select(item => item.MaHd).ToHashSet(StringComparer.OrdinalIgnoreCase);
        for (var number = 1; number <= 999; number++)
        {
            var code = "HD" + number.ToString("000", CultureInfo.InvariantCulture);
            if (!used.Contains(code)) return code;
        }
        throw new InvalidOperationException("Đã sử dụng hết mã hóa đơn có thể lưu trong cơ sở dữ liệu.");
    }

    private void LoadInvoice()
    {
        var id = _maHoaDon ?? _activeInvoiceId;
        if (string.IsNullOrWhiteSpace(id)) return;
        try
        {
            using var db = new HotelManagementContext();
            var invoice = db.HoaDons.AsNoTracking().Where(item => item.MaHd == id && item.DaXoa != true)
                .Select(item => new InvoiceSnapshot
                {
                    MaHd = item.MaHd,
                    MaCtdp = item.MaCtdp,
                    NgHd = item.NgHd,
                    Status = item.TrangThai,
                    Customer = item.MaCtdpNavigation.MaPtNavigation.MaKhNavigation.TenKh,
                    Staff = item.MaNvNavigation == null ? "" : item.MaNvNavigation.TenNv,
                    Room = item.MaCtdpNavigation.MaPh,
                    RoomType = item.MaCtdpNavigation.MaPhNavigation.MaLphNavigation.TenLph,
                    CheckIn = item.MaCtdpNavigation.CheckIn,
                    CheckOut = item.MaCtdpNavigation.CheckOut,
                    RoomAmount = item.MaCtdpNavigation.ThanhTien ?? 0
                }).FirstOrDefault();
            if (invoice is null) throw new InvalidOperationException("Không tìm thấy hóa đơn.");
            var lines = db.Ctdvs.AsNoTracking().Where(line => line.MaCtdp == invoice.MaCtdp && line.DaXoa != true)
                .OrderBy(line => line.MaDv)
                .Select(line => new ServiceLine
                {
                    TenDv = line.MaDvNavigation.TenDv,
                    Sl = line.Sl,
                    DonGia = line.DonGia ?? line.MaDvNavigation.DonGia,
                    ThanhTien = line.ThanhTien ?? ((line.DonGia ?? line.MaDvNavigation.DonGia) * line.Sl)
                })
                .ToList();
            _activeInvoiceId = invoice.MaHd;
            _customerName = invoice.Customer;
            _staffName = invoice.Staff;
            _roomCode = invoice.Room;
            _roomType = invoice.RoomType;
            _status = invoice.Status;
            _invoiceDate = invoice.NgHd;
            _roomAmount = invoice.RoomAmount;
            _serviceAmount = lines.Sum(line => line.ThanhTien);
            _nights = Math.Max(0, (invoice.CheckOut.Date - invoice.CheckIn.Date).Days);
            _customer.Text = invoice.Customer;
            _staff.Text = string.IsNullOrWhiteSpace(invoice.Staff) ? "Chưa xác định" : invoice.Staff;
            _room.Text = invoice.Room;
            _date.Text = invoice.NgHd?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa thanh toán";
            _invoiceNo.Text = invoice.MaHd;
            _roomTypeLabel.Text = invoice.RoomType;
            _nightsLabel.Text = $"{_nights} ngày";
            _roomAmountLabel.Text = _roomAmount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " VND";
            _services.DataSource = lines;
            _total.Text = $"Tổng tiền: {(_roomAmount + _serviceAmount):N0} đ";
            _statusLabel.Text = "Trạng thái: " + invoice.Status;
            _paid.Enabled = invoice.Status != "Đã thanh toán";
            _paid.Text = invoice.Status == "Đã thanh toán" ? "Đã thanh toán" : "Xác nhận thu tiền";
            Text = $"Hóa đơn {invoice.MaHd}";
        }
        catch (Exception ex) { MessageBox.Show($"Không tải được hóa đơn.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); Close(); }
    }

    private void MarkPaid()
    {
        if (_status == "Đã thanh toán") return;
        if (MessageBox.Show($"Xác nhận khách hàng đã thanh toán {(_roomAmount + _serviceAmount):N0} VND?", "Xác nhận thanh toán", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            using var db = new HotelManagementContext();
            var invoice = db.HoaDons.FirstOrDefault(item => item.MaHd == _activeInvoiceId && item.DaXoa != true)
                ?? throw new InvalidOperationException("Không tìm thấy hóa đơn.");
            invoice.TrangThai = "Đã thanh toán";
            invoice.NgHd = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(FormDangNhap.MaNV_DangNhap)) invoice.MaNv = FormDangNhap.MaNV_DangNhap;
            invoice.TriGia = _roomAmount + _serviceAmount;
            db.SaveChanges();
            LoadInvoice();
        }
        catch (Exception ex) { MessageBox.Show($"Không cập nhật được thanh toán.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ExportPdf()
    {
        using var dialog = new SaveFileDialog { Title = "Xuất hóa đơn PDF", Filter = "PDF (*.pdf)|*.pdf", FileName = $"HoaDon_{_activeInvoiceId}.pdf" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var fontPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts) + "\\arial.ttf";
            var baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(fontPath, iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);
            var titleFont = new iTextSharp.text.Font(baseFont, 18, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(64, 105, 32));
            var normalFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            var boldFont = new iTextSharp.text.Font(baseFont, 11, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
            using var stream = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
            using var document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A5, 28, 28, 32, 30);
            iTextSharp.text.pdf.PdfWriter.GetInstance(document, stream);
            document.Open();
            document.Add(new iTextSharp.text.Paragraph("HÓA ĐƠN THANH TOÁN", titleFont) { Alignment = iTextSharp.text.Element.ALIGN_CENTER, SpacingAfter = 16 });
            document.Add(new iTextSharp.text.Paragraph($"Số hóa đơn: {_activeInvoiceId}     Ngày lập: {_invoiceDate:dd/MM/yyyy HH:mm}", normalFont));
            document.Add(new iTextSharp.text.Paragraph($"Khách hàng: {_customerName}     Phòng: {_roomCode} ({_roomType})", normalFont));
            document.Add(new iTextSharp.text.Paragraph($"Nhân viên: {_staffName}     Số ngày: {_nights}", normalFont) { SpacingAfter = 10 });
            var table = new iTextSharp.text.pdf.PdfPTable(4) { WidthPercentage = 100 };
            table.SetWidths([4, 1, 2, 2]);
            foreach (var heading in new[] { "Nội dung", "SL", "Đơn giá", "Thành tiền" })
                table.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(heading, boldFont)) { BackgroundColor = new iTextSharp.text.BaseColor(230, 238, 225), HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
            table.AddCell(new iTextSharp.text.Phrase($"Tiền phòng - {_roomCode}", normalFont));
            table.AddCell(new iTextSharp.text.Phrase("1", normalFont));
            table.AddCell(new iTextSharp.text.Phrase(_roomAmount.ToString("N0"), normalFont));
            table.AddCell(new iTextSharp.text.Phrase(_roomAmount.ToString("N0"), normalFont));
            foreach (DataGridViewRow row in _services.Rows)
            {
                table.AddCell(new iTextSharp.text.Phrase(row.Cells[nameof(ServiceLine.TenDv)].FormattedValue?.ToString() ?? "", normalFont));
                table.AddCell(new iTextSharp.text.Phrase(row.Cells[nameof(ServiceLine.Sl)].FormattedValue?.ToString() ?? "", normalFont));
                table.AddCell(new iTextSharp.text.Phrase(Convert.ToDecimal(row.Cells[nameof(ServiceLine.DonGia)].Value).ToString("N0"), normalFont));
                table.AddCell(new iTextSharp.text.Phrase(Convert.ToDecimal(row.Cells[nameof(ServiceLine.ThanhTien)].Value).ToString("N0"), normalFont));
            }
            document.Add(table);
            document.Add(new iTextSharp.text.Paragraph($"TỔNG TIỀN: {(_roomAmount + _serviceAmount):N0} VND", boldFont) { Alignment = iTextSharp.text.Element.ALIGN_RIGHT, SpacingBefore = 12 });
            document.Add(new iTextSharp.text.Paragraph($"Trạng thái: {_status}", normalFont) { SpacingBefore = 6 });
            document.Add(new iTextSharp.text.Paragraph("Cảm ơn quý khách!", boldFont) { Alignment = iTextSharp.text.Element.ALIGN_CENTER, SpacingBefore = 18 });
            document.Close();
            MessageBox.Show("Đã xuất hóa đơn PDF.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Không xuất được hóa đơn PDF.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private sealed class InvoiceSnapshot
    {
        public string MaHd { get; init; } = "";
        public string MaCtdp { get; init; } = "";
        public DateTime? NgHd { get; init; }
        public string Status { get; init; } = "";
        public string Customer { get; init; } = "";
        public string Staff { get; init; } = "";
        public string Room { get; init; } = "";
        public string RoomType { get; init; } = "";
        public DateTime CheckIn { get; init; }
        public DateTime CheckOut { get; init; }
        public decimal RoomAmount { get; init; }
    }

    private sealed class ServiceLine
    {
        public string TenDv { get; init; } = "";
        public int Sl { get; init; }
        public decimal DonGia { get; init; }
        public decimal ThanhTien { get; init; }
    }
}

internal sealed class StaySelectionDialog : Form
{
    private readonly DataGridView _grid = new();
    public string? SelectedCode => _grid.CurrentRow?.DataBoundItem is StayRow row ? row.MaCtdp : null;

    public StaySelectionDialog()
    {
        Text = "Chọn phòng lập hóa đơn";
        ClientSize = new Size(720, 480);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(10) };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        _grid.Dock = DockStyle.Fill; _grid.AutoGenerateColumns = false; _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.AllowUserToAddRows = false; _grid.ReadOnly = true; _grid.MultiSelect = false; _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; _grid.RowHeadersVisible = false;
        _grid.Columns.Add(Column(nameof(StayRow.MaCtdp), "Chi tiết", 13));
        _grid.Columns.Add(Column(nameof(StayRow.Khach), "Khách hàng", 24));
        _grid.Columns.Add(Column(nameof(StayRow.MaPh), "Phòng", 10));
        _grid.Columns.Add(Column(nameof(StayRow.CheckOutText), "Trả phòng", 20));
        _grid.Columns.Add(Column(nameof(StayRow.TrangThai), "Trạng thái", 18));
        root.Controls.Add(_grid, 0, 0);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(5, 5, 0, 0) };
        var cancel = new Button { Text = "Hủy", Width = 90, Height = 34, DialogResult = DialogResult.Cancel };
        var choose = new Button { Text = "Lập hóa đơn", Width = 125, Height = 34, BackColor = Color.Moccasin, UseVisualStyleBackColor = false };
        choose.Click += (_, _) => { if (SelectedCode is null) { MessageBox.Show("Chọn một phòng trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); return; } DialogResult = DialogResult.OK; Close(); };
        buttons.Controls.Add(cancel); buttons.Controls.Add(choose); root.Controls.Add(buttons, 0, 1); Controls.Add(root); CancelButton = cancel; AcceptButton = choose;
        Load += (_, _) => LoadRows();
    }

    private static DataGridViewTextBoxColumn Column(string prop, string title, float fill) => new() { Name = prop, DataPropertyName = prop, HeaderText = title, FillWeight = fill, ReadOnly = true };

    private void LoadRows()
    {
        try
        {
            using var db = new HotelManagementContext();
            _grid.DataSource = db.Ctdps.AsNoTracking().Where(item => item.DaXoa != true && item.TrangThai != "Đã hủy" &&
                    !item.HoaDons.Any(invoice => invoice.DaXoa != true))
                .OrderByDescending(item => item.CheckIn)
                .Select(item => new StayRow { MaCtdp = item.MaCtdp, Khach = item.MaPtNavigation.MaKhNavigation.TenKh, MaPh = item.MaPh, CheckOut = item.CheckOut, TrangThai = item.TrangThai })
                .ToList().Select(row => row with { CheckOutText = row.CheckOut.ToString("dd/MM/yyyy HH:mm") }).ToList();
        }
        catch (Exception ex) { MessageBox.Show($"Không tải được phòng.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private sealed record StayRow
    {
        public string MaCtdp { get; init; } = "";
        public string Khach { get; init; } = "";
        public string MaPh { get; init; } = "";
        public DateTime CheckOut { get; init; }
        public string CheckOutText { get; init; } = "";
        public string TrangThai { get; init; } = "";
    }
}
