using ClosedXML.Excel;
using HotelManagement.ChiTietTienNghi;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.LoaiPhong;

public sealed class FormDanhSachLoaiPhong : Form
{
    private readonly TextBox _search = new() { Name = "txtTimKiem", MaxLength = 80 };
    private readonly DataGridView _grid = new();

    public FormDanhSachLoaiPhong()
    {
        Text = "Quản lý thông tin loại phòng";
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        BackColor = SystemColors.Control;
        BuildLayout();
        Load += (_, _) => LoadRoomTypes();
        _search.TextChanged += (_, _) => LoadRoomTypes();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var toolbar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = SystemColors.Control,
            Margin = Padding.Empty
        };
        var pictureBox1 = new PictureBox
        {
            Image = HotelManagement.UI.ResourceImages.FromBytes(Properties.Resources.search),
            Location = new Point(36, 14),
            Size = new Size(28, 28),
            SizeMode = PictureBoxSizeMode.StretchImage,
            BorderStyle = BorderStyle.Fixed3D,
            TabStop = false
        };
        _search.Location = new Point(69, 18);
        _search.Size = new Size(150, 25);
        _search.Font = new Font("Microsoft Sans Serif", 10F);
        var exportButton = ToolbarButton("Xuất file Excel", Color.Aqua, 132);
        exportButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        exportButton.Click += (_, _) => ExportToExcel();
        var addButton = ToolbarButton("Thêm mới", Color.LightPink, 116);
        addButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        addButton.Click += (_, _) => AddRoomType();
        toolbar.Controls.Add(pictureBox1);
        toolbar.Controls.Add(_search);
        toolbar.Controls.Add(exportButton);
        toolbar.Controls.Add(addButton);
        toolbar.Resize += (_, _) =>
        {
            exportButton.Location = new Point(toolbar.ClientSize.Width - 274, 12);
            addButton.Location = new Point(toolbar.ClientSize.Width - 134, 12);
        };
        root.Controls.Add(toolbar, 0, 0);

        ConfigureGrid();
        root.Controls.Add(_grid, 0, 1);
    }

    private static Button ToolbarButton(string text, Color color, int width) => new()
    {
        Text = text,
        Size = new Size(width, 38),
        BackColor = color,
        Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
        UseVisualStyleBackColor = false
    };

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.BackgroundColor = Color.DarkGray;
        _grid.BorderStyle = BorderStyle.Fixed3D;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        _grid.GridColor = SystemColors.ControlDark;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 9F);
        _grid.RowTemplate.Height = 40;
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Add(ImageColumn("colIcon", "", Properties.Resources.Giuong));
        _grid.Columns.Add(TextColumn(nameof(RoomTypeRow.MaLph), "Mã loại", 10));
        _grid.Columns.Add(TextColumn(nameof(RoomTypeRow.TenLph), "Tên loại", 16));
        _grid.Columns.Add(TextColumn(nameof(RoomTypeRow.SoGiuong), "Số giường", 9));
        _grid.Columns.Add(TextColumn(nameof(RoomTypeRow.SoNguoiToiDa), "Số người tối đa", 13));
        _grid.Columns.Add(TextColumn(nameof(RoomTypeRow.GiaNgay), "Giá ngày", 13, "N0"));
        _grid.Columns.Add(TextColumn(nameof(RoomTypeRow.GiaGio), "Giá giờ", 13, "N0"));
        _grid.Columns.Add(ImageColumn("colTienNghi", "Tiện nghi", Properties.Resources.details));
        _grid.Columns.Add(ImageColumn("colSua", "Cập nhật", Properties.Resources.edit));
        _grid.CellClick += Grid_CellContentClick;
        _grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            var name = _grid.Columns[e.ColumnIndex].Name;
            if (name == "colIcon") e.Value = Properties.Resources.Giuong;
            else if (name == "colTienNghi") e.Value = Properties.Resources.details;
            else if (name == "colSua") e.Value = Properties.Resources.edit;
            else return;
            e.FormattingApplied = true;
        };
    }

    private static DataGridViewTextBoxColumn TextColumn(string property, string title, float fill, string? format = null) => new()
    {
        DataPropertyName = property,
        Name = property,
        HeaderText = title,
        FillWeight = fill,
        ReadOnly = true,
        DefaultCellStyle = { Format = format ?? "" }
    };

    private static DataGridViewImageColumn ImageColumn(string name, string title, Image image) => new()
    {
        Name = name,
        HeaderText = title,
        Image = image,
        ImageLayout = DataGridViewImageCellLayout.Zoom,
        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
        Width = 50,
        FillWeight = 5,
        MinimumWidth = 40,
        ReadOnly = true,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };

    private void LoadRoomTypes()
    {
        if (IsDisposed)
            return;

        try
        {
            using var db = new HotelManagementContext();
            var query = db.LoaiPhongs.AsNoTracking();
            var keyword = _search.Text.Trim();
            if (keyword.Length > 0)
                query = query.Where(l => l.MaLph.Contains(keyword) || l.TenLph.Contains(keyword));

            _grid.DataSource = query.OrderBy(l => l.MaLph)
                .Select(l => new RoomTypeRow
                {
                    MaLph = l.MaLph,
                    TenLph = l.TenLph,
                    SoGiuong = l.SoGiuong,
                    SoNguoiToiDa = l.SoNguoiToiDa,
                    GiaNgay = l.GiaNgay,
                    GiaGio = l.GiaGio
                })
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không tải được danh sách loại phòng.\n{ex.Message}", "Lỗi CSDL",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddRoomType()
    {
        using var dialog = new FormThemLoaiPhong();
        if (dialog.ShowDialog(this) == DialogResult.OK)
            LoadRoomTypes();
    }

    private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Rows[e.RowIndex].DataBoundItem is not RoomTypeRow roomType)
            return;

        switch (_grid.Columns[e.ColumnIndex].Name)
        {
            case "colSua":
                using (var dialog = new FormSuaLoaiPhong(roomType.MaLph))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                        LoadRoomTypes();
                }
                break;
            case "colTienNghi":
                using (var dialog = new FormDanhSachChiTietTienNghi(roomType.MaLph, roomType.TenLph))
                    dialog.ShowDialog(FindForm());
                break;
        }
    }

    private void ExportToExcel()
    {
        var rows = (_grid.DataSource as List<RoomTypeRow>) ?? [];
        if (rows.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "Xuất danh sách loại phòng",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"DanhSachLoaiPhong_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        try
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Danh sách loại phòng");
            var headers = new[] { "Mã loại", "Tên loại phòng", "Số giường", "Số người tối đa", "Giá ngày", "Giá giờ" };
            for (int column = 0; column < headers.Length; column++)
                sheet.Cell(1, column + 1).Value = headers[column];
            sheet.Row(1).Style.Font.Bold = true;

            for (int index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                sheet.Cell(index + 2, 1).Value = row.MaLph;
                sheet.Cell(index + 2, 2).Value = row.TenLph;
                sheet.Cell(index + 2, 3).Value = row.SoGiuong;
                sheet.Cell(index + 2, 4).Value = row.SoNguoiToiDa;
                sheet.Cell(index + 2, 5).Value = row.GiaNgay;
                sheet.Cell(index + 2, 6).Value = row.GiaGio;
            }
            sheet.Column(5).Style.NumberFormat.Format = "#,##0";
            sheet.Column(6).Style.NumberFormat.Format = "#,##0";
            sheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);

            if (MessageBox.Show("Xuất Excel thành công. Mở tệp ngay?", "Thông báo", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information) == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName)
                {
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không xuất được Excel.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class RoomTypeRow
    {
        public string MaLph { get; init; } = "";
        public string TenLph { get; init; } = "";
        public int SoGiuong { get; init; }
        public int SoNguoiToiDa { get; init; }
        public decimal GiaNgay { get; init; }
        public decimal GiaGio { get; init; }
    }
}

internal class FormLoaiPhongDialog : Form
{
    private readonly string? _roomTypeCode;
    private readonly TextBox _code = new() { MaxLength = 5 };
    private readonly TextBox _name = new() { MaxLength = 20 };
    private readonly TextBox _beds = new() { MaxLength = 3 };
    private readonly TextBox _capacity = new() { MaxLength = 3 };
    private readonly TextBox _dayPrice = new() { MaxLength = 20 };
    private readonly TextBox _hourPrice = new() { MaxLength = 20 };

    public FormLoaiPhongDialog(string? roomTypeCode = null)
    {
        _roomTypeCode = roomTypeCode;
        Text = roomTypeCode is null ? "Thêm loại phòng" : "Cập nhật loại phòng";
        ClientSize = new Size(604, 626);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        BackColor = SystemColors.Control;
        BuildLayout();
        Load += (_, _) => LoadRoomType();
        _beds.KeyPress += OnlyDigits;
        _capacity.KeyPress += OnlyDigits;
    }

    private void BuildLayout()
    {
        var isEdit = _roomTypeCode is not null;
        Font = new Font("Microsoft Sans Serif", 11F);
        var title = new Label
        {
            Text = isEdit ? "Cập nhật thông tin loại phòng" : "Thêm mới loại phòng",
            AutoSize = true,
            Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(192, 64, 0),
            Location = new Point(isEdit ? 90 : 165, 49)
        };
        Controls.Add(title);

        var panel = new Panel { Location = new Point(48, 121), Size = new Size(507, 375) };
        Controls.Add(panel);
        var y = 34;
        if (!isEdit)
        {
            AddDialogField(panel, "Mã loại phòng", _code, y);
            y += 50;
        }
        else
        {
            _code.ReadOnly = true;
        }
        AddDialogField(panel, "Tên loại phòng", _name, y);
        AddDialogField(panel, "Số giường", _beds, y + 56);
        AddDialogField(panel, "Số người tối đa", _capacity, y + 112);
        AddDialogField(panel, "Giá ngày", _dayPrice, y + 168);
        AddDialogField(panel, "Giá giờ", _hourPrice, y + 224);

        var save = DialogButton(isEdit ? "Cập nhật" : "Thêm mới", Color.Moccasin, new Point(136, 519));
        save.Click += Save_Click;
        var cancel = DialogButton("Thoát", SystemColors.GradientActiveCaption, new Point(336, 519));
        cancel.DialogResult = DialogResult.Cancel;
        Controls.Add(save);
        Controls.Add(cancel);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private static Button DialogButton(string text, Color color, Point location) => new()
    {
        Text = text,
        Location = location,
        Size = new Size(137, 59),
        BackColor = color,
        Font = new Font("Microsoft Sans Serif", 12F),
        UseVisualStyleBackColor = false
    };

    private static void AddDialogField(Panel panel, string labelText, TextBox editor, int y)
    {
        var label = new Label
        {
            Text = labelText,
            Location = new Point(8, y),
            Size = new Size(185, 30),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Microsoft Sans Serif", 12F)
        };
        editor.Location = new Point(208, y);
        editor.Size = new Size(275, 30);
        editor.Font = new Font("Microsoft Sans Serif", 12F);
        panel.Controls.Add(label);
        panel.Controls.Add(editor);
    }

    private void LoadRoomType()
    {
        if (_roomTypeCode is null)
            return;

        try
        {
            using var db = new HotelManagementContext();
            var item = db.LoaiPhongs.AsNoTracking().FirstOrDefault(l => l.MaLph == _roomTypeCode);
            if (item is null)
            {
                MessageBox.Show("Không tìm thấy loại phòng cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            _code.Text = item.MaLph;
            _code.ReadOnly = true;
            _name.Text = item.TenLph;
            _beds.Text = item.SoGiuong.ToString();
            _capacity.Text = item.SoNguoiToiDa.ToString();
            _dayPrice.Text = item.GiaNgay.ToString("0.##", System.Globalization.CultureInfo.CurrentCulture);
            _hourPrice.Text = item.GiaGio.ToString("0.##", System.Globalization.CultureInfo.CurrentCulture);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không tải được loại phòng.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private void Save_Click(object? sender, EventArgs e)
    {
        var code = _code.Text.Trim().ToUpperInvariant();
        var name = _name.Text.Trim();
        if (code.Length == 0)
        {
            Warn("Vui lòng nhập mã loại phòng.", _code);
            return;
        }
        if (name.Length == 0)
        {
            Warn("Vui lòng nhập tên loại phòng.", _name);
            return;
        }
        if (!int.TryParse(_beds.Text.Trim(), out var beds) || beds <= 0)
        {
            Warn("Số giường phải là số nguyên lớn hơn 0.", _beds);
            return;
        }
        if (!int.TryParse(_capacity.Text.Trim(), out var capacity) || capacity <= 0)
        {
            Warn("Số người tối đa phải là số nguyên lớn hơn 0.", _capacity);
            return;
        }
        if (!TryPrice(_dayPrice.Text, out var dayPrice) || dayPrice <= 0)
        {
            Warn("Giá theo ngày phải lớn hơn 0.", _dayPrice);
            return;
        }
        if (!TryPrice(_hourPrice.Text, out var hourPrice) || hourPrice <= 0)
        {
            Warn("Giá theo giờ phải lớn hơn 0.", _hourPrice);
            return;
        }

        try
        {
            using var db = new HotelManagementContext();
            var item = db.LoaiPhongs.FirstOrDefault(l => l.MaLph == code);
            if (_roomTypeCode is null && item is not null)
            {
                Warn("Mã loại phòng đã tồn tại. Vui lòng nhập mã khác.", _code);
                return;
            }
            if (_roomTypeCode is not null && item is null)
            {
                MessageBox.Show("Không tìm thấy loại phòng cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (item is null)
            {
                item = new HotelManagement.Models.LoaiPhong { MaLph = code };
                db.LoaiPhongs.Add(item);
            }
            item.TenLph = name;
            item.SoGiuong = beds;
            item.SoNguoiToiDa = capacity;
            item.GiaNgay = dayPrice;
            item.GiaGio = hourPrice;
            db.SaveChanges();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Không lưu được loại phòng.\n{ex.InnerException?.Message ?? ex.Message}", "Lỗi CSDL",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không lưu được loại phòng.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static bool TryPrice(string text, out decimal price)
    {
        const System.Globalization.NumberStyles style = System.Globalization.NumberStyles.Number;
        return decimal.TryParse(text.Trim(), style, System.Globalization.CultureInfo.CurrentCulture, out price) ||
               decimal.TryParse(text.Trim(), style, System.Globalization.CultureInfo.InvariantCulture, out price);
    }

    private static void OnlyDigits(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            e.Handled = true;
    }

    private static void Warn(string message, Control focus)
    {
        MessageBox.Show(message, "Thông tin chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        focus.Focus();
    }
}

internal sealed class FormThemLoaiPhong : FormLoaiPhongDialog
{
    public FormThemLoaiPhong() : base() { Text = "FormThemLoaiPhong"; }
}

internal sealed class FormSuaLoaiPhong : FormLoaiPhongDialog
{
    public FormSuaLoaiPhong(string roomTypeCode) : base(roomTypeCode) { Text = "Cập nhật thông tin loại phòng"; }
}
