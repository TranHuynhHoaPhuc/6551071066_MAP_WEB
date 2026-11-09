using ClosedXML.Excel;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Phong;

public sealed class FormDanhSachPhong : Form
{
    private readonly TextBox _search = new() { Name = "txtTimKiem", MaxLength = 80 };
    private readonly DataGridView _grid = new();

    public FormDanhSachPhong()
    {
        Text = "Quản lý thông tin phòng";
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        BackColor = SystemColors.Control;
        BuildLayout();
        Load += (_, _) => LoadRooms();
        _search.TextChanged += (_, _) => LoadRooms();
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
        exportButton.Location = new Point(-290, 12);
        exportButton.Click += (_, _) => ExportToExcel();
        var addButton = ToolbarButton("Thêm mới phòng", Color.LightPink, 148);
        addButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        addButton.Location = new Point(-148, 12);
        addButton.Click += (_, _) => AddRoom();
        toolbar.Controls.Add(pictureBox1);
        toolbar.Controls.Add(_search);
        toolbar.Controls.Add(exportButton);
        toolbar.Controls.Add(addButton);
        toolbar.Resize += (_, _) =>
        {
            exportButton.Location = new Point(toolbar.ClientSize.Width - 296, 12);
            addButton.Location = new Point(toolbar.ClientSize.Width - 154, 12);
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
        _grid.Columns.Add(ImageColumn("colIcon", "", Properties.Resources.Phong));
        _grid.Columns.Add(TextColumn(nameof(RoomRow.MaPh), "Mã phòng", 13, "MaPH"));
        _grid.Columns.Add(TextColumn(nameof(RoomRow.Ttph), "Tình trạng phòng", 21, "TTPH"));
        _grid.Columns.Add(TextColumn(nameof(RoomRow.Ttdd), "Dọn dẹp", 17, "TTDD"));
        _grid.Columns.Add(TextColumn(nameof(RoomRow.TenLph), "Loại phòng", 21, "TenLPH"));
        _grid.Columns.Add(ImageColumn("colSua", "Sửa", Properties.Resources.edit));
        _grid.Columns.Add(ImageColumn("colXoa", "Xóa", Properties.Resources.delete));
        _grid.CellClick += Grid_CellContentClick;
        _grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            var name = _grid.Columns[e.ColumnIndex].Name;
            if (name == "colIcon") e.Value = Properties.Resources.Phong;
            else if (name == "colSua") e.Value = Properties.Resources.edit;
            else if (name == "colXoa") e.Value = Properties.Resources.delete;
            else return;
            e.FormattingApplied = true;
        };
    }

    private static DataGridViewTextBoxColumn TextColumn(string property, string title, float fill, string name) => new()
    {
        DataPropertyName = property,
        Name = name,
        HeaderText = title,
        FillWeight = fill,
        ReadOnly = true
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

    private void LoadRooms()
    {
        if (IsDisposed)
            return;

        try
        {
            using var db = new HotelManagementContext();
            var query = db.Phongs.AsNoTracking().Where(p => p.DaXoa != true);
            var keyword = _search.Text.Trim();
            if (keyword.Length > 0)
                query = query.Where(p => p.MaPh.Contains(keyword));

            var rooms = query.OrderBy(p => p.MaPh)
                .Select(p => new RoomRow
                {
                    MaPh = p.MaPh,
                    Ttph = p.Ttph,
                    Ttdd = p.Ttdd,
                    TenLph = p.MaLphNavigation.TenLph,
                    GhiChu = p.GhiChu ?? ""
                })
                .ToList();
            _grid.DataSource = rooms;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không tải được danh sách phòng.\n{ex.Message}", "Lỗi CSDL",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddRoom()
    {
        using var dialog = new FormThemPhong();
        if (dialog.ShowDialog(this) == DialogResult.OK)
            LoadRooms();
    }

    private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Rows[e.RowIndex].DataBoundItem is not RoomRow room)
            return;

        switch (_grid.Columns[e.ColumnIndex].Name)
        {
            case "colSua":
                using (var dialog = new FormSuaPhong(room.MaPh))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                        LoadRooms();
                }
                break;
            case "colXoa":
                DeleteRoom(room);
                break;
        }
    }

    private void DeleteRoom(RoomRow room)
    {
        var answer = MessageBox.Show($"Bạn có chắc muốn ẩn phòng {room.MaPh} khỏi danh sách?",
            "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes)
            return;

        try
        {
            using var db = new HotelManagementContext();
            var entity = db.Phongs.FirstOrDefault(p => p.MaPh == room.MaPh && p.DaXoa != true);
            if (entity is null)
            {
                LoadRooms();
                return;
            }
            if (entity.Ttph is "Đang thuê" or "Đã đặt")
            {
                MessageBox.Show($"Phòng {room.MaPh} đang ở trạng thái \"{entity.Ttph}\" nên không thể xóa.",
                    "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            entity.DaXoa = true;
            db.SaveChanges();
            LoadRooms();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không xóa được phòng {room.MaPh}.\n{ex.Message}", "Lỗi CSDL",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportToExcel()
    {
        var rows = (_grid.DataSource as List<RoomRow>) ?? [];
        if (rows.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "Xuất danh sách phòng",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"DanhSachPhong_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        try
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Danh sách phòng");
            var headers = new[] { "Mã phòng", "Tình trạng phòng", "Dọn dẹp", "Loại phòng" };
            for (int column = 0; column < headers.Length; column++)
                sheet.Cell(1, column + 1).Value = headers[column];
            sheet.Row(1).Style.Font.Bold = true;

            for (int index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                sheet.Cell(index + 2, 1).Value = row.MaPh;
                sheet.Cell(index + 2, 2).Value = row.Ttph;
                sheet.Cell(index + 2, 3).Value = row.Ttdd;
                sheet.Cell(index + 2, 4).Value = row.TenLph;
            }
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

    private sealed class RoomRow
    {
        public string MaPh { get; init; } = "";
        public string Ttph { get; init; } = "";
        public string Ttdd { get; init; } = "";
        public string TenLph { get; init; } = "";
        public string GhiChu { get; init; } = "";
    }
}

internal class FormPhongDialog : Form
{
    private readonly string? _roomCode;
    private readonly TextBox _code = new() { MaxLength = 5 };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cleaning = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _note = new() { MaxLength = 100, Multiline = true };

    public FormPhongDialog(string? roomCode = null)
    {
        _roomCode = roomCode;
        Text = roomCode is null ? "Thêm phòng" : "Cập nhật phòng";
        ClientSize = roomCode is null ? new Size(410, 370) : new Size(604, 626);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        BackColor = SystemColors.Control;
        BuildLayout();
        Load += (_, _) => LoadData();
    }

    private void BuildLayout()
    {
        var isEdit = _roomCode is not null;
        Font = new Font("Microsoft Sans Serif", 11F);

        if (isEdit)
        {
            var title = new Label
            {
                Text = "Cập nhật thông tin phòng",
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(192, 64, 0),
                Location = new Point(113, 49)
            };
            Controls.Add(title);

            var panel = new Panel { Location = new Point(48, 121), Size = new Size(507, 375) };
            Controls.Add(panel);
            AddDialogField(panel, "Trạng thái phòng", _status, 40, 20, 211);
            AddDialogField(panel, "Loại phòng", _type, 90, 20, 211);
            AddDialogField(panel, "Tình trạng dọn dẹp", _cleaning, 140, 10, 211);
            AddDialogField(panel, "Ghi chú", _note, 190, 20, 211, 90);
            _note.ScrollBars = ScrollBars.Vertical;
            _code.ReadOnly = true;

            var update = DialogButton("Cập nhật", Color.Moccasin, new Point(136, 519));
            update.Click += Save_Click;
            var cancel = DialogButton("Thoát", SystemColors.GradientActiveCaption, new Point(336, 519));
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(update);
            Controls.Add(cancel);
            AcceptButton = update;
            CancelButton = cancel;
        }
        else
        {
            var panel = new Panel { Location = new Point(22, 12), Size = new Size(366, 260) };
            Controls.Add(panel);
            AddDialogField(panel, "Mã số phòng", _code, 28, 10, 170);
            AddDialogField(panel, "Trạng thái phòng", _status, 67, 10, 170);
            AddDialogField(panel, "Loại phòng", _type, 106, 10, 170);
            AddDialogField(panel, "Tình trạng dọn dẹp", _cleaning, 145, 0, 180);
            AddDialogField(panel, "Ghi chú", _note, 182, 10, 170, 58);
            _note.ScrollBars = ScrollBars.Vertical;

            var add = DialogButton("Thêm mới", Color.Moccasin, new Point(95, 292));
            add.Click += Save_Click;
            var cancel = DialogButton("Thoát", SystemColors.GradientActiveCaption, new Point(225, 292));
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(add);
            Controls.Add(cancel);
            AcceptButton = add;
            CancelButton = cancel;
        }
    }

    private static Button DialogButton(string text, Color color, Point location) => new()
    {
        Text = text,
        Location = location,
        Size = new Size(120, 48),
        BackColor = color,
        Font = new Font("Microsoft Sans Serif", 12F),
        UseVisualStyleBackColor = false
    };

    private static void AddDialogField(Panel panel, string labelText, Control editor, int y, int labelX, int editorX, int height = 30)
    {
        var label = new Label
        {
            Text = labelText,
            Location = new Point(labelX, y + (height - 26) / 2),
            Size = new Size(editorX - labelX - 12, 26),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Microsoft Sans Serif", 11F)
        };
        editor.Location = new Point(editorX, y);
        editor.Size = new Size(panel.ClientSize.Width - editorX - 12, height);
        editor.Font = new Font("Microsoft Sans Serif", 11F);
        panel.Controls.Add(label);
        panel.Controls.Add(editor);
    }

    private void LoadData()
    {
        try
        {
            using var db = new HotelManagementContext();
            _type.DisplayMember = nameof(RoomTypeOption.TenLph);
            _type.ValueMember = nameof(RoomTypeOption.MaLph);
            _type.DataSource = db.LoaiPhongs.AsNoTracking().OrderBy(l => l.MaLph)
                .Select(l => new RoomTypeOption { MaLph = l.MaLph, TenLph = l.TenLph }).ToList();
            _status.DataSource = new[] { "Bình thường", "Đã đặt", "Đang thuê", "Đang sửa chữa" };
            _cleaning.DataSource = new[] { "Đã dọn dẹp", "Chưa dọn dẹp" };

            if (_roomCode is null)
                return;

            var room = db.Phongs.AsNoTracking().FirstOrDefault(p => p.MaPh == _roomCode && p.DaXoa != true);
            if (room is null)
            {
                MessageBox.Show("Không tìm thấy phòng cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            _code.Text = room.MaPh;
            _code.ReadOnly = true;
            _type.SelectedValue = room.MaLph;
            _status.SelectedItem = room.Ttph;
            _cleaning.SelectedItem = room.Ttdd;
            _note.Text = room.GhiChu ?? "";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không tải được dữ liệu phòng.\n{ex.Message}", "Lỗi CSDL",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private void Save_Click(object? sender, EventArgs e)
    {
        var code = _code.Text.Trim().ToUpperInvariant();
        var typeId = _type.SelectedValue?.ToString();
        if (code.Length == 0)
        {
            Warn("Vui lòng nhập mã phòng.", _code);
            return;
        }
        if (string.IsNullOrWhiteSpace(typeId))
        {
            Warn("Vui lòng chọn loại phòng.", _type);
            return;
        }

        try
        {
            using var db = new HotelManagementContext();
            var existing = db.Phongs.FirstOrDefault(p => p.MaPh == code);
            if (_roomCode is null && existing is not null && existing.DaXoa != true)
            {
                Warn("Mã phòng đã tồn tại. Vui lòng nhập mã khác.", _code);
                return;
            }
            if (_roomCode is not null && (existing is null || existing.DaXoa == true))
            {
                MessageBox.Show("Phòng không còn tồn tại hoặc đã bị xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_roomCode is null && existing is not null)
            {
                if (MessageBox.Show($"Mã {code} thuộc phòng đã xóa. Khôi phục với thông tin vừa nhập?", "Khôi phục phòng",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            if (existing is null)
            {
                existing = new HotelManagement.Models.Phong { MaPh = code };
                db.Phongs.Add(existing);
            }
            existing.Ttph = _status.SelectedItem?.ToString() ?? "Bình thường";
            existing.Ttdd = _cleaning.SelectedItem?.ToString() ?? "Đã dọn dẹp";
            existing.GhiChu = string.IsNullOrWhiteSpace(_note.Text) ? null : _note.Text.Trim();
            existing.MaLph = typeId;
            existing.DaXoa = false;
            db.SaveChanges();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Không lưu được phòng.\n{ex.InnerException?.Message ?? ex.Message}", "Lỗi CSDL",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không lưu được phòng.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void Warn(string message, Control focus)
    {
        MessageBox.Show(message, "Thông tin chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        focus.Focus();
    }

    private sealed class RoomTypeOption
    {
        public string MaLph { get; init; } = "";
        public string TenLph { get; init; } = "";
    }
}

internal sealed class FormThemPhong : FormPhongDialog
{
    public FormThemPhong() : base() { Text = "FormThemPhong"; }
}

internal sealed class FormSuaPhong : FormPhongDialog
{
    public FormSuaPhong(string roomCode) : base(roomCode) { Text = "Cập nhật thông tin phòng"; }
}
