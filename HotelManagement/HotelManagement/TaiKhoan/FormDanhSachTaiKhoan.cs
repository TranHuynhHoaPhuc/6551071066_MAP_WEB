using HotelManagement.DangNhap;
using HotelManagement.Models;
using HotelManagement.UI;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.TaiKhoan;

public sealed class FormDanhSachTaiKhoan : EntityGridForm<FormDanhSachTaiKhoan.AccountRow>
{
    public FormDanhSachTaiKhoan() : base(
        "Quản lý tài khoản và quyền đăng nhập",
        "Danh sách tài khoản",
        [
            new(nameof(AccountRow.TenTk), "Tên tài khoản", 22),
            new(nameof(AccountRow.MaNv), "Mã nhân viên", 14),
            new(nameof(AccountRow.NhanVien), "Nhân viên", 27),
            new(nameof(AccountRow.Quyen), "Cấp quyền", 20)
        ],
        LoadRows,
        row => row.TenTk,
        EditAccount,
        DeleteAccount,
        Properties.Resources.MenuQuanLyTaiKhoan,
        Properties.Resources.edit,
        Properties.Resources.delete,
        addCaption: "Thêm tài khoản",
        editCaption: "Cập nhật",
        deleteCaption: "Ngưng tài khoản")
    {
    }

    private static List<AccountRow> LoadRows(string keyword)
    {
        using var db = new HotelManagementContext();
        var query = db.TaiKhoans.AsNoTracking().Where(item => item.DaXoa != true);
        if (keyword.Length > 0)
            query = query.Where(item => item.TenTk.Contains(keyword) || item.MaNv.Contains(keyword) || item.MaNvNavigation.TenNv.Contains(keyword));
        return query.OrderBy(item => item.TenTk).Select(item => new AccountRow
        {
            TenTk = item.TenTk,
            MaNv = item.MaNv,
            NhanVien = item.MaNvNavigation.TenNv,
            Quyen = item.CapDoQuyen == 3 ? "3 - Quản trị viên" : item.CapDoQuyen == 2 ? "2 - Quản lý" : "1 - Nhân viên"
        }).ToList();
    }

    private static bool EditAccount(string? username)
    {
        if (!IsAdministrator()) return false;
        using var dialog = new AccountEditorDialog(username);
        return dialog.ShowDialog() == DialogResult.OK;
    }

    private static void DeleteAccount(string username)
    {
        if (!IsAdministrator()) throw new InvalidOperationException("Chỉ quản trị viên mới được quản lý tài khoản.");
        using var db = new HotelManagementContext();
        var account = db.TaiKhoans.FirstOrDefault(item => item.TenTk == username && item.DaXoa != true);
        if (account is null) return;
        if (account.MaNv == FormDangNhap.MaNV_DangNhap)
            throw new InvalidOperationException("Không thể ngưng tài khoản đang đăng nhập.");
        if (account.CapDoQuyen >= 3 && db.TaiKhoans.Count(item => item.DaXoa != true && item.CapDoQuyen >= 3) <= 1)
            throw new InvalidOperationException("Cần giữ lại ít nhất một tài khoản quản trị viên đang hoạt động.");
        account.DaXoa = true;
        db.SaveChanges();
    }

    private static bool IsAdministrator() => FormDangNhap.CapDoQuyen_DangNhap >= 3;

    public sealed class AccountRow
    {
        public string TenTk { get; init; } = "";
        public string MaNv { get; init; } = "";
        public string NhanVien { get; init; } = "";
        public string Quyen { get; init; } = "";
    }
}

internal sealed class AccountEditorDialog : Form
{
    private readonly string? _originalUsername;
    private readonly TextBox _username = new() { MaxLength = 50, Dock = DockStyle.Fill };
    private readonly TextBox _password = new() { MaxLength = 1000, UseSystemPasswordChar = true, Dock = DockStyle.Fill };
    private readonly ComboBox _employee = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly ComboBox _level = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly Label _passwordHelp = new() { Text = "Bỏ trống để giữ mật khẩu hiện tại", Dock = DockStyle.Fill, ForeColor = Color.DimGray };

    public AccountEditorDialog(string? username)
    {
        _originalUsername = username;
        Text = username is null ? "Thêm tài khoản" : "Cập nhật tài khoản";
        ClientSize = new Size(560, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = MinimizeBox = false;
        BuildLayout();
        Load += LoadChoices;
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(20, 18, 20, 8) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 154));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var title = new Label { Text = _originalUsername is null ? "Thêm tài khoản đăng nhập" : "Cập nhật tài khoản đăng nhập", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold), ForeColor = Color.Red };
        root.Controls.Add(title, 0, 0); root.SetColumnSpan(title, 2);
        AddField(root, "Tên tài khoản", _username, 1);
        AddField(root, "Mật khẩu", _password, 2);
        root.Controls.Add(_passwordHelp, 1, 3);
        AddField(root, "Nhân viên", _employee, 4);
        AddField(root, "Cấp quyền", _level, 5);
        _level.Items.AddRange(["1 - Nhân viên", "2 - Quản lý", "3 - Quản trị viên"]);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var cancel = new Button { Text = "Thoát", Width = 100, Height = 36, DialogResult = DialogResult.Cancel, BackColor = SystemColors.GradientActiveCaption };
        var save = new Button { Text = _originalUsername is null ? "Thêm mới" : "Cập nhật", Width = 125, Height = 36, BackColor = Color.Moccasin, UseVisualStyleBackColor = false };
        save.Click += SaveAccount;
        actions.Controls.Add(cancel); actions.Controls.Add(save);
        Controls.Add(root); Controls.Add(actions);
        CancelButton = cancel; AcceptButton = save;
    }

    private static void AddField(TableLayoutPanel root, string caption, Control input, int row)
    {
        var label = new Label { Text = caption, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(4) };
        input.Margin = new Padding(8, 10, 4, 8);
        root.Controls.Add(label, 0, row); root.Controls.Add(input, 1, row);
    }

    private void LoadChoices(object? sender, EventArgs e)
    {
        if (FormDangNhap.CapDoQuyen_DangNhap < 3)
        {
            MessageBox.Show("Chỉ quản trị viên mới được quản lý tài khoản.", "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Close(); return;
        }
        try
        {
            using var db = new HotelManagementContext();
            var current = _originalUsername is null ? null : db.TaiKhoans.AsNoTracking().FirstOrDefault(item => item.TenTk == _originalUsername && item.DaXoa != true);
            if (_originalUsername is not null && current is null) throw new InvalidOperationException("Không tìm thấy tài khoản.");
            _username.Text = current?.TenTk ?? "";
            _username.ReadOnly = current is not null;
            _passwordHelp.Visible = current is not null;

            var employees = db.NhanViens.AsNoTracking()
                .Where(item => item.DaXoa != true && !item.TaiKhoans.Any(account => account.DaXoa != true && account.TenTk != _originalUsername))
                .OrderBy(item => item.MaNv)
                .Select(item => new EmployeeOption { MaNv = item.MaNv, TenNv = item.TenNv })
                .ToList();
            _employee.DataSource = employees;
            _employee.DisplayMember = nameof(EmployeeOption.Caption);
            _employee.ValueMember = nameof(EmployeeOption.MaNv);
            if (current is not null) _employee.SelectedValue = current.MaNv;
            var level = current?.CapDoQuyen ?? 1;
            _level.SelectedIndex = Math.Clamp(level, 1, 3) - 1;
            if (current?.MaNv == FormDangNhap.MaNV_DangNhap)
            {
                _employee.Enabled = false;
                _level.Enabled = false;
            }
        }
        catch (Exception ex) { MessageBox.Show($"Không tải được thông tin tài khoản.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error); Close(); }
    }

    private void SaveAccount(object? sender, EventArgs e)
    {
        if (_employee.SelectedItem is not EmployeeOption employee || _level.SelectedIndex < 0)
        {
            MessageBox.Show("Chọn nhân viên và cấp quyền.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        var username = _username.Text.Trim();
        var password = _password.Text;
        var level = _level.SelectedIndex + 1;
        if (username.Length == 0) { MessageBox.Show("Nhập tên tài khoản.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); _username.Focus(); return; }
        if (_originalUsername is null && password.Length == 0) { MessageBox.Show("Nhập mật khẩu cho tài khoản mới.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); _password.Focus(); return; }
        if (password.Length > 1000) { MessageBox.Show("Mật khẩu vượt quá độ dài cho phép.", "Thông tin chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        try
        {
            using var db = new HotelManagementContext();
            if (_originalUsername is null && db.TaiKhoans.Any(item => item.TenTk == username))
                throw new InvalidOperationException("Tên tài khoản đã tồn tại.");
            var account = _originalUsername is null ? null : db.TaiKhoans.FirstOrDefault(item => item.TenTk == _originalUsername && item.DaXoa != true);
            if (_originalUsername is not null && account is null) throw new InvalidOperationException("Không tìm thấy tài khoản.");
            if (account is not null && account.MaNv == FormDangNhap.MaNV_DangNhap && level != 3)
                throw new InvalidOperationException("Không thể hạ quyền của tài khoản đang đăng nhập.");
            if (db.TaiKhoans.Any(item => item.DaXoa != true && item.MaNv == employee.MaNv && item.TenTk != _originalUsername))
                throw new InvalidOperationException("Nhân viên này đã được gán cho một tài khoản khác.");
            if (account is null)
            {
                account = new HotelManagement.Models.TaiKhoan { TenTk = username, MaNv = employee.MaNv, Password = password, CapDoQuyen = level, DaXoa = false };
                db.TaiKhoans.Add(account);
            }
            else
            {
                account.MaNv = employee.MaNv;
                account.CapDoQuyen = level;
                if (password.Length > 0) account.Password = password;
            }
            db.SaveChanges();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { MessageBox.Show($"Không lưu được tài khoản.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private sealed class EmployeeOption
    {
        public string MaNv { get; init; } = "";
        public string TenNv { get; init; } = "";
        public string Caption => $"{MaNv} - {TenNv}";
    }
}
