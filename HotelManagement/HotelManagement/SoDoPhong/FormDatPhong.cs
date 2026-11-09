using System.Data;
using System.Globalization;
using HotelManagement.DangNhap;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.SoDoPhong;

public sealed class FormDatPhong : Form
{
    private static readonly string[] TimeChoices =
    [
        "01:00", "01:30", "02:00", "02:30", "03:00", "03:30",
        "04:00", "04:30", "05:00", "05:30", "06:00", "06:30",
        "07:00", "07:30", "08:00", "08:30", "09:00", "09:30",
        "10:00", "10:30", "11:00", "11:30", "12:00", "12:30"
    ];

    private readonly string? _initialRoomCode;
    private readonly TextBox _guestName = new() { MaxLength = 50 };
    private readonly TextBox _identity = new() { MaxLength = 12 };
    private readonly TextBox _phone = new() { MaxLength = 10 };
    private readonly TextBox _nationality = new() { MaxLength = 30, Text = "Việt Nam" };
    private readonly ComboBox _gender = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _checkInDate = new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd/MM/yyyy",
        MinDate = DateTime.Today,
        Value = DateTime.Today
    };
    private readonly DateTimePicker _checkOutDate = new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd/MM/yyyy",
        Value = DateTime.Today.AddDays(1)
    };
    private readonly ComboBox _checkInTime = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _checkInAmPm = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _checkOutTime = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _checkOutAmPm = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _availableGrid = new();
    private readonly DataGridView _selectedGrid = new();
    private readonly List<SelectedRoom> _selectedRooms = [];
    private bool _initialRoomHandled;
    private bool _updatingCustomerFields;

    public FormDatPhong(string? initialRoomCode = null)
    {
        _initialRoomCode = initialRoomCode;
        Text = "Đặt phòng trước";
        ClientSize = new Size(1291, 575);
        MinimumSize = new Size(1040, 520);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Font = new Font("Microsoft Sans Serif", 9.5F);

        _gender.Items.AddRange(["Nam", "Nữ"]);
        foreach (var time in TimeChoices)
        {
            _checkInTime.Items.Add(time);
            _checkOutTime.Items.Add(time);
        }
        _checkInAmPm.Items.AddRange(["AM", "PM"]);
        _checkOutAmPm.Items.AddRange(["AM", "PM"]);
        _checkInTime.SelectedItem = "02:00";
        _checkInAmPm.SelectedItem = "PM";
        _checkOutTime.SelectedItem = "12:00";
        _checkOutAmPm.SelectedItem = "PM";

        BuildLayout();
        WireEvents();
        Load += (_, _) => LoadAvailableRooms();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12, 4, 12, 8),
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var title = new Label
        {
            Text = "Đặt phòng",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft Sans Serif", 15F, FontStyle.Bold),
            ForeColor = Color.FromArgb(192, 0, 0)
        };
        root.Controls.Add(title, 0, 0);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37F));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63F));
        root.Controls.Add(content, 0, 1);

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 0, 6, 0),
            Margin = Padding.Empty
        };
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        content.Controls.Add(left, 0, 0);

        var guestPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = Padding.Empty,
            Padding = new Padding(8)
        };
        left.Controls.Add(guestPanel, 0, 0);
        BuildGuestPanel(guestPanel);
        BuildGuestActions(left);

        var roomPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(6, 0, 0, 0),
            Padding = new Padding(5)
        };
        content.Controls.Add(roomPanel, 1, 0);
        BuildRoomPanel(roomPanel);

        AcceptButton = Controls.Find("btnDatPhong", true).FirstOrDefault() as Button;
        CancelButton = Controls.Find("btnHuy", true).FirstOrDefault() as Button;
    }

    private void BuildGuestPanel(Panel host)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Margin = Padding.Empty,
            Padding = new Padding(4)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        for (var i = 1; i < 6; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        host.Controls.Add(layout);

        var heading = new Label
        {
            Text = "Thông tin khách hàng",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft Sans Serif", 15F, FontStyle.Bold),
            ForeColor = Color.FromArgb(192, 0, 0)
        };
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 2);
        AddGuestField(layout, "Họ và tên", _guestName, 1);
        AddGuestField(layout, "Số CCCD/Passport", _identity, 2);
        AddGuestField(layout, "Số điện thoại", _phone, 3);
        AddGuestField(layout, "Quốc tịch", _nationality, 4);
        AddGuestField(layout, "Giới tính", _gender, 5);
        _gender.SelectedIndex = 0;
    }

    private static void AddGuestField(TableLayoutPanel layout, string caption, Control input, int row)
    {
        var label = new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Microsoft Sans Serif", 11F),
            Margin = new Padding(2, 4, 8, 4)
        };
        input.Dock = DockStyle.Fill;
        input.Font = new Font("Microsoft Sans Serif", 11F);
        input.Margin = new Padding(2, 8, 6, 8);
        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(input, 1, row);
    }

    private void BuildGuestActions(TableLayoutPanel left)
    {
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Anchor = AnchorStyles.None,
            Padding = new Padding(24, 10, 0, 0),
            Margin = Padding.Empty
        };
        var book = new Button
        {
            Name = "btnDatPhong",
            Text = "Đặt trước",
            Size = new Size(167, 42),
            BackColor = SystemColors.ActiveCaption,
            ForeColor = Color.ForestGreen,
            Font = new Font("Microsoft Sans Serif", 13F, FontStyle.Bold),
            UseVisualStyleBackColor = false
        };
        book.Click += SaveBooking_Click;
        var cancel = new Button
        {
            Name = "btnHuy",
            Text = "Hủy",
            Size = new Size(129, 42),
            DialogResult = DialogResult.Cancel,
            Font = new Font("Microsoft Sans Serif", 13F)
        };
        cancel.Click += (_, _) => Close();
        actions.Controls.Add(book);
        actions.Controls.Add(cancel);
        left.Controls.Add(actions, 0, 1);
    }

    private void BuildRoomPanel(Panel host)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.Controls.Add(layout);

        var heading = new Label
        {
            Text = "Thông tin phòng",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft Sans Serif", 15F, FontStyle.Bold),
            ForeColor = Color.FromArgb(192, 0, 0)
        };
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 2);

        var schedule = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = new Padding(5, 4, 5, 2)
        };
        schedule.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        schedule.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        schedule.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74));
        schedule.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94));
        schedule.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
        schedule.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        schedule.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        schedule.Controls.Add(ScheduleLabel("Ngày bắt đầu"), 0, 0);
        schedule.Controls.Add(_checkInDate, 1, 0);
        schedule.Controls.Add(ScheduleLabel("Giờ đến"), 2, 0);
        schedule.Controls.Add(_checkInTime, 3, 0);
        schedule.Controls.Add(_checkInAmPm, 4, 0);
        schedule.Controls.Add(ScheduleLabel("Ngày kết thúc"), 0, 1);
        schedule.Controls.Add(_checkOutDate, 1, 1);
        schedule.Controls.Add(ScheduleLabel("Giờ đi"), 2, 1);
        schedule.Controls.Add(_checkOutTime, 3, 1);
        schedule.Controls.Add(_checkOutAmPm, 4, 1);
        foreach (Control control in schedule.Controls)
        {
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(3, 6, 4, 6);
            control.Font = new Font("Microsoft Sans Serif", 10F);
        }
        layout.Controls.Add(schedule, 0, 1);
        layout.SetColumnSpan(schedule, 2);

        var availableHeading = RoomHeading("Danh sách phòng trống");
        var selectedHeading = RoomHeading("Danh sách phòng đã chọn");
        layout.Controls.Add(availableHeading, 0, 2);
        layout.Controls.Add(selectedHeading, 1, 2);
        ConfigureAvailableGrid();
        ConfigureSelectedGrid();
        layout.Controls.Add(_availableGrid, 0, 3);
        layout.Controls.Add(_selectedGrid, 1, 3);
    }

    private static Label ScheduleLabel(string caption) => new()
    {
        Text = caption,
        TextAlign = ContentAlignment.MiddleRight,
        Margin = Padding.Empty
    };

    private static Label RoomHeading(string caption) => new()
    {
        Text = caption,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Microsoft Sans Serif", 13F, FontStyle.Bold),
        ForeColor = Color.Green
    };

    private void WireEvents()
    {
        _checkInDate.ValueChanged += (_, _) =>
        {
            UpdateCheckOutMinimum();
            LoadAvailableRooms();
        };
        _checkOutDate.ValueChanged += (_, _) => LoadAvailableRooms();
        _checkInTime.SelectedIndexChanged += (_, _) => LoadAvailableRooms();
        _checkInAmPm.SelectedIndexChanged += (_, _) => LoadAvailableRooms();
        _checkOutTime.SelectedIndexChanged += (_, _) => LoadAvailableRooms();
        _checkOutAmPm.SelectedIndexChanged += (_, _) => LoadAvailableRooms();
        _identity.TextChanged += Identity_TextChanged;
        _phone.KeyPress += (_, e) =>
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        };
        _availableGrid.CellClick += AvailableGrid_CellClick;
        _selectedGrid.CellClick += SelectedGrid_CellClick;
    }

    private void ConfigureAvailableGrid()
    {
        ConfigureGrid(_availableGrid);
        _availableGrid.Columns.Add(TextColumn("MaPH", "Mã phòng", 25));
        _availableGrid.Columns.Add(TextColumn("TenLPH", "Loại phòng", 45));
        var capacity = TextColumn("SoNguoiToiDa", "Số người tối đa", 20);
        capacity.Visible = false;
        _availableGrid.Columns.Add(capacity);
        _availableGrid.Columns.Add(ImageColumn("Them", "Thêm", Properties.Resources.Add, 50));
    }

    private void ConfigureSelectedGrid()
    {
        ConfigureGrid(_selectedGrid);
        _selectedGrid.Columns.Add(TextColumn("MaPhong", "Mã phòng", 14));
        _selectedGrid.Columns.Add(TextColumn("SoNguoiToiDa", "Số người tối đa", 13));
        var start = TextColumn("NgayBatDau", "Ngày bắt đầu", 25);
        start.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        _selectedGrid.Columns.Add(start);
        var end = TextColumn("NgayKetThuc", "Ngày kết thúc", 25);
        end.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        _selectedGrid.Columns.Add(end);
        _selectedGrid.Columns.Add(ImageColumn("Huy", "Hủy", Properties.Resources.delete, 45));
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.AutoGenerateColumns = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.DarkGray;
        grid.BorderStyle = BorderStyle.Fixed3D;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        grid.GridColor = SystemColors.ControlDark;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.RowTemplate.Height = 32;
        grid.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 8.5F);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 8.5F, FontStyle.Regular);
    }

    private static DataGridViewTextBoxColumn TextColumn(string property, string title, float fill) => new()
    {
        Name = property,
        DataPropertyName = property,
        HeaderText = title,
        FillWeight = fill,
        ReadOnly = true,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };

    private static DataGridViewImageColumn ImageColumn(string name, string title, Image image, int width) => new()
    {
        Name = name,
        HeaderText = title,
        Image = image,
        ImageLayout = DataGridViewImageCellLayout.Zoom,
        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
        Width = width,
        Resizable = DataGridViewTriState.False,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };

    private void UpdateCheckOutMinimum()
    {
        var minimum = _checkInDate.Value.Date.AddDays(1);
        if (_checkOutDate.Value.Date < minimum)
            _checkOutDate.Value = minimum;
        _checkOutDate.MinDate = minimum;
    }

    private void LoadAvailableRooms()
    {
        if (!IsHandleCreated || IsDisposed)
            return;
        if (!TryGetPeriod(out var checkIn, out var checkOut))
        {
            _availableGrid.Rows.Clear();
            return;
        }

        try
        {
            using var db = new HotelManagementContext();
            var rooms = db.Phongs.AsNoTracking()
                .Where(room => room.DaXoa != true && room.Ttph == "Bình thường" &&
                    !room.Ctdps.Any(detail => detail.DaXoa != true && detail.CheckIn < checkOut && detail.CheckOut > checkIn))
                .OrderBy(room => room.MaPh)
                .Select(room => new AvailableRoom
                {
                    MaPh = room.MaPh,
                    TenLph = room.MaLphNavigation.TenLph,
                    SoNguoiToiDa = room.MaLphNavigation.SoNguoiToiDa
                })
                .ToList()
                .Where(room => _selectedRooms.All(selected => selected.MaPh != room.MaPh))
                .ToList();

            if (!_initialRoomHandled)
            {
                _initialRoomHandled = true;
                var initial = rooms.FirstOrDefault(room => room.MaPh == _initialRoomCode);
                if (initial is not null)
                {
                    _selectedRooms.Add(CreateSelected(initial, checkIn, checkOut));
                    rooms.Remove(initial);
                    RefreshSelectedRooms();
                }
            }

            _availableGrid.Rows.Clear();
            foreach (var room in rooms)
                _availableGrid.Rows.Add(room.MaPh, room.TenLph, room.SoNguoiToiDa, Properties.Resources.Add);
            _availableGrid.CurrentCell = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải danh sách phòng trống: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AvailableGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _availableGrid.Columns[e.ColumnIndex].Name != "Them")
            return;
        var row = _availableGrid.Rows[e.RowIndex];
        var roomCode = row.Cells["MaPH"].Value?.ToString();
        if (string.IsNullOrWhiteSpace(roomCode))
            return;
        if (_selectedRooms.Any(item => string.Equals(item.MaPh, roomCode, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show($"Phòng {roomCode} đã có trong danh sách phòng đã chọn.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!TryGetPeriod(out var checkIn, out var checkOut))
        {
            MessageBox.Show("Ngày giờ trả phòng phải sau ngày giờ nhận phòng.", "Thông tin chưa hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var room = new AvailableRoom
        {
            MaPh = roomCode,
            TenLph = row.Cells["TenLPH"].Value?.ToString() ?? "",
            SoNguoiToiDa = Convert.ToInt32(row.Cells["SoNguoiToiDa"].Value, CultureInfo.InvariantCulture)
        };
        _selectedRooms.Add(CreateSelected(room, checkIn, checkOut));
        LoadAvailableRooms();
        RefreshSelectedRooms();
    }

    private void SelectedGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _selectedGrid.Columns[e.ColumnIndex].Name != "Huy")
            return;
        _selectedRooms.RemoveAt(e.RowIndex);
        RefreshSelectedRooms();
        LoadAvailableRooms();
    }

    private static SelectedRoom CreateSelected(AvailableRoom room, DateTime checkIn, DateTime checkOut) => new()
    {
        MaPh = room.MaPh,
        SoNguoiToiDa = room.SoNguoiToiDa,
        CheckIn = checkIn,
        CheckOut = checkOut
    };

    private void RefreshSelectedRooms()
    {
        _selectedGrid.Rows.Clear();
        foreach (var room in _selectedRooms)
            _selectedGrid.Rows.Add(room.MaPh, room.SoNguoiToiDa, room.CheckIn, room.CheckOut, Properties.Resources.delete);
        _selectedGrid.CurrentCell = null;
    }

    private bool TryGetPeriod(out DateTime checkIn, out DateTime checkOut)
    {
        checkIn = GetTime(_checkInDate, _checkInTime, _checkInAmPm) ?? default;
        checkOut = GetTime(_checkOutDate, _checkOutTime, _checkOutAmPm) ?? default;
        return checkIn != default && checkOut != default && checkOut > checkIn;
    }

    private static DateTime? GetTime(DateTimePicker date, ComboBox time, ComboBox amPm)
    {
        var value = time.SelectedItem?.ToString();
        var period = amPm.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(period))
            return null;
        var parts = value.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var hour) || !int.TryParse(parts[1], out var minute))
            return null;
        if (period == "PM" && hour < 12)
            hour += 12;
        if (period == "AM" && hour == 12)
            hour = 0;
        return date.Value.Date.AddHours(hour).AddMinutes(minute);
    }

    private void Identity_TextChanged(object? sender, EventArgs e)
    {
        if (_updatingCustomerFields || _guestName.Enabled)
            return;
        _updatingCustomerFields = true;
        _guestName.Clear();
        _phone.Clear();
        _nationality.Clear();
        _gender.SelectedIndex = -1;
        _guestName.Enabled = true;
        _phone.Enabled = true;
        _nationality.Enabled = true;
        _gender.Enabled = true;
        _updatingCustomerFields = false;
    }

    private bool TryFillExistingCustomer(string identity)
    {
        if (identity.Length == 0)
            return false;
        try
        {
            using var db = new HotelManagementContext();
            var customer = db.KhachHangs.AsNoTracking()
                .Where(item => item.CccdPassport == identity)
                .Select(item => new { item.TenKh, item.Sdt, item.QuocTich, item.GioiTinh })
                .FirstOrDefault();
            if (customer is null)
                return false;
            _updatingCustomerFields = true;
            _guestName.Text = customer.TenKh ?? "";
            _phone.Text = customer.Sdt ?? "";
            _nationality.Text = customer.QuocTich ?? "";
            _gender.SelectedIndex = _gender.FindStringExact(customer.GioiTinh ?? "");
            _guestName.Enabled = false;
            _phone.Enabled = false;
            _nationality.Enabled = false;
            _gender.Enabled = false;
            _updatingCustomerFields = false;
            return true;
        }
        catch (Exception ex)
        {
            _updatingCustomerFields = false;
            MessageBox.Show($"Lỗi kiểm tra thông tin khách hàng: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void SaveBooking_Click(object? sender, EventArgs e)
    {
        var identity = _identity.Text.Trim();
        var customerExists = TryFillExistingCustomer(identity);
        if (customerExists && MessageBox.Show(
                "Khách hàng đã tồn tại trong hệ thống. Thông tin sẽ được tự động điền và không thể sửa.\nBạn có muốn tiếp tục đặt phòng không?",
                "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
            return;

        if (string.IsNullOrWhiteSpace(_guestName.Text))
        {
            ShowValidation("Vui lòng nhập họ và tên khách hàng!", _guestName);
            return;
        }
        if (identity.Length == 0)
        {
            ShowValidation("Vui lòng nhập số CCCD/Passport!", _identity);
            return;
        }
        var phone = _phone.Text.Trim();
        if (phone.Length == 0 || phone.Any(character => !char.IsDigit(character)))
        {
            ShowValidation("Vui lòng nhập số điện thoại hợp lệ (chỉ gồm chữ số)!", _phone);
            return;
        }
        if (string.IsNullOrWhiteSpace(_nationality.Text))
        {
            ShowValidation("Vui lòng nhập quốc tịch!", _nationality);
            return;
        }
        if (_gender.SelectedIndex < 0)
        {
            ShowValidation("Vui lòng chọn giới tính!", _gender);
            return;
        }
        if (_selectedRooms.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn ít nhất 1 phòng để đặt!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_selectedRooms.Any(room => room.CheckIn < DateTime.Now))
        {
            MessageBox.Show("Ngày giờ nhận phòng không thể nằm trong quá khứ.", "Thông tin chưa hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!SaveRentalTicket())
            return;
        MessageBox.Show("Đặt phòng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }

    private static void ShowValidation(string message, Control control)
    {
        MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        control.Focus();
    }

    private bool SaveRentalTicket()
    {
        var employeeCode = FormDangNhap.MaNV_DangNhap;
        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            MessageBox.Show("Chưa xác định được nhân viên đang đăng nhập. Vui lòng đăng nhập lại.", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        try
        {
            using var db = new HotelManagementContext();
            if (!db.NhanViens.Any(employee => employee.MaNv == employeeCode && employee.DaXoa != true))
            {
                MessageBox.Show("Không tìm thấy nhân viên đang đăng nhập để lập phiếu đặt phòng.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
            var identity = _identity.Text.Trim();
            var customer = db.KhachHangs.FirstOrDefault(item => item.CccdPassport == identity);
            if (customer is null)
            {
                var customerCode = NextCode(db.KhachHangs.Select(item => item.MaKh).ToList(), "KH", 3, 5);
                customer = new HotelManagement.Models.KhachHang { MaKh = customerCode };
                db.KhachHangs.Add(customer);
            }
            customer.TenKh = _guestName.Text.Trim();
            customer.Sdt = _phone.Text.Trim();
            customer.CccdPassport = identity;
            customer.QuocTich = _nationality.Text.Trim();
            customer.GioiTinh = _gender.SelectedItem!.ToString()!;
            customer.DaXoa = false;

            var ticketCode = NextCode(db.PhieuThues.Select(item => item.MaPt).ToList(), "PT", 3, 5);
            db.PhieuThues.Add(new HotelManagement.Models.PhieuThue
            {
                MaPt = ticketCode,
                NgPt = DateTime.Now,
                MaKh = customer.MaKh,
                MaNv = employeeCode,
                DaXoa = false
            });
            db.SaveChanges();

            var detailCodes = db.Ctdps.Select(item => item.MaCtdp).ToList();
            foreach (var selected in _selectedRooms)
            {
                var room = db.Phongs.Include(item => item.MaLphNavigation)
                    .FirstOrDefault(item => item.MaPh == selected.MaPh && item.DaXoa != true);
                if (room is null)
                    throw new InvalidOperationException($"Không tìm thấy phòng {selected.MaPh}.");
                if (room.Ttph != "Bình thường")
                    throw new InvalidOperationException($"Phòng {selected.MaPh} hiện ở trạng thái \"{room.Ttph}\" nên không thể đặt.");
                if (db.Ctdps.Any(item => item.MaPh == selected.MaPh && item.DaXoa != true &&
                                         item.CheckIn < selected.CheckOut && item.CheckOut > selected.CheckIn))
                    throw new InvalidOperationException($"Phòng {selected.MaPh} đã có người đặt trong khoảng thời gian này.");

                var nightlyRate = room.MaLphNavigation.GiaNgay;
                var nights = Math.Max(1, (selected.CheckOut.Date - selected.CheckIn.Date).Days);
                var detailCode = NextCode(detailCodes, "CTDP", 3, 7);
                detailCodes.Add(detailCode);
                db.Ctdps.Add(new Ctdp
                {
                    MaCtdp = detailCode,
                    SoNguoi = selected.SoNguoiToiDa,
                    MaPt = ticketCode,
                    MaPh = selected.MaPh,
                    CheckIn = selected.CheckIn,
                    CheckOut = selected.CheckOut,
                    TrangThai = "Đã đặt",
                    DonGia = nightlyRate,
                    ThanhTien = nightlyRate * nights,
                    DaXoa = false,
                    TheoGio = false
                });
                room.Ttph = "Đã đặt";
                // Trigger CapNhatGiaCTDP in Database.sql reads inserted into scalar variables.
                // Persist each detail separately so its inserted pseudo-table contains one row.
                db.SaveChanges();
            }

            transaction.Commit();
            return true;
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Có lỗi xảy ra khi lưu phiếu thuê: {ex.InnerException?.Message ?? ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Có lỗi xảy ra khi lưu phiếu thuê: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private static string NextCode(IEnumerable<string> codes, string prefix, int numericDigits, int maxLength)
    {
        var largest = codes
            .Where(code => code.Length > prefix.Length && code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(code => code[prefix.Length..])
            .Where(suffix => int.TryParse(suffix, NumberStyles.None, CultureInfo.InvariantCulture, out _))
            .Select(suffix => int.Parse(suffix, CultureInfo.InvariantCulture))
            .DefaultIfEmpty(0)
            .Max();
        var next = prefix + (largest + 1).ToString($"D{numericDigits}", CultureInfo.InvariantCulture);
        if (next.Length > maxLength)
            throw new InvalidOperationException($"Đã hết mã có tiền tố {prefix} (tối đa {maxLength} ký tự).");
        return next;
    }

    private sealed class AvailableRoom
    {
        public string MaPh { get; init; } = "";
        public string TenLph { get; init; } = "";
        public int SoNguoiToiDa { get; init; }
    }

    private sealed class SelectedRoom
    {
        public string MaPh { get; init; } = "";
        public int SoNguoiToiDa { get; init; }
        public DateTime CheckIn { get; init; }
        public DateTime CheckOut { get; init; }
    }
}
