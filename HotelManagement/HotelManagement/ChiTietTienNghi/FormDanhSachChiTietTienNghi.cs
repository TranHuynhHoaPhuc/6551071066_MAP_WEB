using System.Globalization;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.ChiTietTienNghi;

public sealed class FormDanhSachChiTietTienNghi : Form
{
    private readonly string _roomTypeCode;
    private readonly string _roomTypeName;
    private readonly DataGridView _grid = new();
    private readonly Label _emptyState = new()
    {
        Text = "Loại phòng này chưa có tiện nghi. Bấm “Thêm mới” để thêm.",
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        ForeColor = Color.DimGray,
        BackColor = Color.DarkGray,
        Font = new Font("Microsoft Sans Serif", 11F),
        Visible = false
    };

    public FormDanhSachChiTietTienNghi(string roomTypeCode, string roomTypeName)
    {
        _roomTypeCode = roomTypeCode;
        _roomTypeName = roomTypeName;
        Text = "Quản lý các tiện nghi theo loại phòng";
        ClientSize = new Size(800, 656);
        MinimumSize = new Size(800, 560);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        ShowInTaskbar = false;
        BuildLayout();
        Load += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_roomTypeCode))
            {
                MessageBox.Show("Không xác định được loại phòng!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            LoadRows();
        };
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12, 12, 12, 10),
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        Controls.Add(root);

        var heading = new Label
        {
            Text = "DANH SÁCH TIỆN NGHI HIỆN CÓ",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft Sans Serif", 17F, FontStyle.Bold),
            ForeColor = Color.Red
        };
        root.Controls.Add(heading, 0, 0);

        var roomType = new Label
        {
            Text = $"Loại phòng: {_roomTypeName}",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft Sans Serif", 15F, FontStyle.Bold),
            ForeColor = Color.Blue
        };
        root.Controls.Add(roomType, 0, 1);

        ConfigureGrid();
        var gridHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.DarkGray,
            Margin = Padding.Empty
        };
        gridHost.Controls.Add(_grid);
        gridHost.Controls.Add(_emptyState);
        _emptyState.BringToFront();
        root.Controls.Add(gridHost, 0, 2);

        var actions = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(0, 10, 0, 0),
            Margin = Padding.Empty
        };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 143));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 143));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var add = new Button
        {
            Text = "Thêm mới",
            Size = new Size(137, 59),
            BackColor = Color.Moccasin,
            Font = new Font("Microsoft Sans Serif", 12F),
            UseVisualStyleBackColor = false
        };
        var close = new Button
        {
            Text = "Thoát",
            Size = new Size(137, 59),
            BackColor = SystemColors.GradientActiveCaption,
            Font = new Font("Microsoft Sans Serif", 12F),
            UseVisualStyleBackColor = false,
            DialogResult = DialogResult.Cancel
        };
        add.Click += (_, _) => AddAmenity();
        close.Click += (_, _) => Close();
        add.Dock = DockStyle.Fill;
        add.Margin = new Padding(3, 0, 3, 0);
        close.Dock = DockStyle.Fill;
        close.Margin = new Padding(3, 0, 3, 0);
        actions.Controls.Add(add, 1, 0);
        actions.Controls.Add(close, 2, 0);
        root.Controls.Add(actions, 0, 3);
        CancelButton = close;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = Color.DarkGray;
        _grid.BorderStyle = BorderStyle.Fixed3D;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        _grid.GridColor = SystemColors.ControlDark;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.ReadOnly = true;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.RowTemplate.Height = 36;

        var code = TextColumn("MaTN", "Mã tiện nghi", 8);
        code.Visible = false;
        _grid.Columns.Add(code);
        _grid.Columns.Add(TextColumn("TenTN", "Tên tiện nghi", 45));
        var quantity = TextColumn("SL", "Số lượng", 15);
        quantity.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _grid.Columns.Add(quantity);
        _grid.Columns.Add(TextColumn("GhiChu", "Ghi chú", 40));
        _grid.Columns.Add(ImageColumn("Sua", "Sửa", Properties.Resources.edit));
        _grid.Columns.Add(ImageColumn("Xoa", "Xóa", Properties.Resources.delete));
        _grid.CellClick += Grid_CellClick;
    }

    private static DataGridViewTextBoxColumn TextColumn(string name, string title, float fill) => new()
    {
        Name = name,
        HeaderText = title,
        FillWeight = fill,
        ReadOnly = true,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };

    private static DataGridViewImageColumn ImageColumn(string name, string title, Image image) => new()
    {
        Name = name,
        HeaderText = title,
        Image = image,
        ImageLayout = DataGridViewImageCellLayout.Zoom,
        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
        Width = 50,
        Resizable = DataGridViewTriState.False,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };

    private void LoadRows()
    {
        try
        {
            using var db = new HotelManagementContext();
            var amenities = db.Cttns.AsNoTracking()
                .Where(item => item.MaLph == _roomTypeCode && item.DaXoa != true && item.MaTnNavigation.DaXoa != true)
                .OrderBy(item => item.MaTn)
                .Select(item => new { item.MaTn, item.MaTnNavigation.TenTn, item.Sl })
                .ToList();

            _grid.Rows.Clear();
            foreach (var amenity in amenities)
            {
                var quantity = amenity.Sl ?? 0;
                var rowIndex = _grid.Rows.Add(
                    amenity.MaTn,
                    amenity.TenTn,
                    quantity,
                    quantity >= 1 ? "Có thể sử dụng tốt" : "Đang không sử dụng được",
                    Properties.Resources.edit,
                    Properties.Resources.delete);
                if (quantity < 1)
                    _grid.Rows[rowIndex].Cells["GhiChu"].Style.ForeColor = Color.Red;
            }
            _emptyState.Visible = amenities.Count == 0;
            _grid.ClearSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi load tiện nghi của loại phòng: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;
        var column = _grid.Columns[e.ColumnIndex].Name;
        if (column != "Sua" && column != "Xoa")
            return;
        var row = _grid.Rows[e.RowIndex];
        var code = row.Cells["MaTN"].Value?.ToString();
        var name = row.Cells["TenTN"].Value?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(code))
            return;

        if (column == "Sua")
        {
            var quantity = Convert.ToInt32(row.Cells["SL"].Value, System.Globalization.CultureInfo.InvariantCulture);
            using var dialog = new FormSuaChiTietTienNghiDialog(_roomTypeCode, code, name, quantity);
            if (dialog.ShowDialog(this) == DialogResult.OK)
                LoadRows();
            return;
        }

        if (MessageBox.Show($"Bạn có chắc muốn xóa tiện nghi '{name}'?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        try
        {
            using var db = new HotelManagementContext();
            var detail = db.Cttns.FirstOrDefault(item => item.MaLph == _roomTypeCode && item.MaTn == code);
            if (detail is null)
            {
                MessageBox.Show("Không tìm thấy tiện nghi cần xóa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                db.Cttns.Remove(detail);
                db.SaveChanges();
                MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            LoadRows();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xóa tiện nghi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddAmenity()
    {
        using var dialog = new FormThemChiTietTienNghiDialog(_roomTypeCode);
        if (dialog.ShowDialog(this) == DialogResult.OK)
            LoadRows();
    }
}

internal sealed class FormThemChiTietTienNghiDialog : Form
{
    private readonly string _roomTypeCode;
    private readonly ComboBox _amenity = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _quantity = new();

    public FormThemChiTietTienNghiDialog(string roomTypeCode)
    {
        _roomTypeCode = roomTypeCode;
        Text = "FormThemChiTietTienNghi";
        ClientSize = new Size(610, 445);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BuildLayout();
        Load += (_, _) => LoadAvailableAmenities();
    }

    private void BuildLayout()
    {
        Controls.Add(new Label
        {
            Text = "Thêm mới thông tin tiện nghi của loại phòng",
            Location = new Point(15, 32),
            Size = new Size(580, 42),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(192, 64, 0)
        });
        Controls.Add(new Label
        {
            Text = "Tên tiện nghi",
            Location = new Point(73, 148),
            Size = new Size(145, 32),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Microsoft Sans Serif", 13F)
        });
        _amenity.Location = new Point(242, 149);
        _amenity.Size = new Size(275, 34);
        _amenity.Font = new Font("Microsoft Sans Serif", 13F);
        Controls.Add(_amenity);
        AddQuantityField(new Point(115, 271), new Point(242, 271));

        var save = new Button
        {
            Text = "Thêm mới",
            Location = new Point(161, 371),
            Size = new Size(137, 59),
            BackColor = Color.Moccasin,
            Font = new Font("Microsoft Sans Serif", 12F),
            UseVisualStyleBackColor = false
        };
        save.Click += Save_Click;
        var close = new Button
        {
            Text = "Thoát",
            Location = new Point(361, 371),
            Size = new Size(137, 59),
            BackColor = SystemColors.GradientActiveCaption,
            Font = new Font("Microsoft Sans Serif", 12F),
            UseVisualStyleBackColor = false,
            DialogResult = DialogResult.Cancel
        };
        close.Click += (_, _) => Close();
        Controls.Add(save);
        Controls.Add(close);
        AcceptButton = save;
        CancelButton = close;
        ConfigureQuantityInput();
    }

    private void AddQuantityField(Point labelLocation, Point inputLocation)
    {
        Controls.Add(new Label
        {
            Text = "Số lượng",
            Location = labelLocation,
            Size = new Size(110, 32),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Microsoft Sans Serif", 13F)
        });
        _quantity.Location = inputLocation;
        _quantity.Size = new Size(275, 30);
        _quantity.Font = new Font("Microsoft Sans Serif", 12F);
        Controls.Add(_quantity);
    }

    private void ConfigureQuantityInput()
    {
        _quantity.MaxLength = 4;
        _quantity.KeyPress += (_, e) =>
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        };
    }

    private void LoadAvailableAmenities()
    {
        try
        {
            using var db = new HotelManagementContext();
            var used = db.Cttns.AsNoTracking()
                .Where(item => item.MaLph == _roomTypeCode && item.DaXoa != true)
                .Select(item => item.MaTn);
            var choices = db.TienNghis.AsNoTracking()
                .Where(item => item.DaXoa != true && !used.Contains(item.MaTn))
                .OrderBy(item => item.TenTn)
                .Select(item => new AmenityChoice { MaTn = item.MaTn, TenTn = item.TenTn })
                .ToList();
            if (choices.Count == 0)
            {
                MessageBox.Show("Loại phòng này đã có đầy đủ các tiện nghi, không còn tiện nghi nào để thêm!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            _amenity.DataSource = choices;
            _amenity.DisplayMember = nameof(AmenityChoice.TenTn);
            _amenity.ValueMember = nameof(AmenityChoice.MaTn);
            _amenity.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi load danh sách tiện nghi: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private void Save_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_quantity.Text) || _amenity.SelectedIndex < 0)
        {
            MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!int.TryParse(_quantity.Text.Trim(), out var quantity) || quantity < 0)
        {
            MessageBox.Show("Số lượng không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _quantity.Focus();
            return;
        }
        if (_amenity.SelectedValue?.ToString() is not { Length: > 0 } amenityCode)
        {
            MessageBox.Show("Vui lòng chọn tiện nghi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            using var db = new HotelManagementContext();
            var detail = db.Cttns.FirstOrDefault(item => item.MaLph == _roomTypeCode && item.MaTn == amenityCode);
            if (detail is null)
            {
                db.Cttns.Add(new Cttn { MaLph = _roomTypeCode, MaTn = amenityCode, Sl = quantity, DaXoa = false });
            }
            else if (detail.DaXoa == true)
            {
                detail.Sl = quantity;
                detail.DaXoa = false;
            }
            else
            {
                MessageBox.Show("Tiện nghi này đã có trong loại phòng!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadAvailableAmenities();
                return;
            }
            if (db.SaveChanges() > 0)
            {
                MessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Thêm thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi thêm tiện nghi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class AmenityChoice
    {
        public string MaTn { get; init; } = "";
        public string TenTn { get; init; } = "";
    }
}

internal sealed class FormSuaChiTietTienNghiDialog : Form
{
    private readonly string _roomTypeCode;
    private readonly string _amenityCode;
    private readonly string _amenityName;
    private readonly int _currentQuantity;
    private readonly TextBox _name = new() { ReadOnly = true };
    private readonly TextBox _quantity = new();

    public FormSuaChiTietTienNghiDialog(string roomTypeCode, string amenityCode, string amenityName, int quantity)
    {
        _roomTypeCode = roomTypeCode;
        _amenityCode = amenityCode;
        _amenityName = amenityName;
        _currentQuantity = quantity;
        Text = "FormSuaChiTietTienNghi";
        ClientSize = new Size(613, 576);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BuildLayout();
        Load += (_, _) =>
        {
            _name.Text = _amenityName;
            _quantity.Text = _currentQuantity.ToString(CultureInfo.InvariantCulture);
            _quantity.Focus();
            _quantity.SelectAll();
        };
    }

    private void BuildLayout()
    {
        Controls.Add(new Label
        {
            Text = "Cập nhật thông tin tiện nghi của loại phòng",
            Location = new Point(15, 35),
            Size = new Size(580, 42),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(192, 64, 0)
        });
        AddField("Tên tiện nghi", _name, 84, 170);
        AddField("Số lượng", _quantity, 126, 294);
        _quantity.MaxLength = 4;
        _quantity.KeyPress += (_, e) =>
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        };
        var save = new Button
        {
            Text = "Cập nhật",
            Location = new Point(127, 431),
            Size = new Size(137, 59),
            BackColor = Color.Moccasin,
            Font = new Font("Microsoft Sans Serif", 12F),
            UseVisualStyleBackColor = false
        };
        save.Click += Save_Click;
        var close = new Button
        {
            Text = "Thoát",
            Location = new Point(327, 431),
            Size = new Size(137, 59),
            BackColor = SystemColors.GradientActiveCaption,
            Font = new Font("Microsoft Sans Serif", 12F),
            UseVisualStyleBackColor = false,
            DialogResult = DialogResult.Cancel
        };
        close.Click += (_, _) => Close();
        Controls.Add(save);
        Controls.Add(close);
        AcceptButton = save;
        CancelButton = close;
    }

    private void AddField(string caption, TextBox input, int captionX, int y)
    {
        Controls.Add(new Label
        {
            Text = caption,
            Location = new Point(captionX, y),
            Size = new Size(112, 32),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Microsoft Sans Serif", 13F)
        });
        input.Location = new Point(253, y);
        input.Size = new Size(275, 30);
        input.Font = new Font("Microsoft Sans Serif", 12F);
        Controls.Add(input);
    }

    private void Save_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_quantity.Text))
        {
            MessageBox.Show("Vui lòng nhập số lượng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _quantity.Focus();
            return;
        }
        if (!int.TryParse(_quantity.Text.Trim(), out var quantity) || quantity < 0)
        {
            MessageBox.Show("Vui lòng nhập số hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _quantity.Focus();
            return;
        }
        try
        {
            using var db = new HotelManagementContext();
            var detail = db.Cttns.FirstOrDefault(item => item.MaLph == _roomTypeCode && item.MaTn == _amenityCode && item.DaXoa != true);
            if (detail is null)
            {
                MessageBox.Show("Không tìm thấy tiện nghi cần cập nhật!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            detail.Sl = quantity;
            if (db.SaveChanges() > 0)
            {
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Không có thay đổi nào để cập nhật!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi cập nhật tiện nghi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
