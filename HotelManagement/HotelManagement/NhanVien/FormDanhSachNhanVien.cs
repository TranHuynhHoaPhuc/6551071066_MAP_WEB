using System.Globalization;
using HotelManagement.Models;
using HotelManagement.UI;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.NhanVien;

public sealed class FormDanhSachNhanVien : EntityGridForm<FormDanhSachNhanVien.EmployeeRow>
{
    private static readonly EditField[] EditFields =
    [
        new("TenNv", "Họ và tên", MaxLength: 50),
        new("ChucVu", "Chức vụ", EditFieldType.Choice, Choices: ["Người quản trị hệ thống", "Quản lý khách sạn", "Nhân viên"]),
        new("Luong", "Lương", EditFieldType.Decimal),
        new("Sdt", "Số điện thoại", EditFieldType.Text, 10),
        new("GioiTinh", "Giới tính", EditFieldType.Choice, Choices: ["Nam", "Nữ"]),
        new("NgaySinh", "Ngày sinh", EditFieldType.Date),
        new("Cccd", "CCCD", MaxLength: 12),
        new("DiaChi", "Địa chỉ", MaxLength: 100),
        new("Email", "Email", EditFieldType.Text, 100, Required: false),
        new("MaNv", "Mã nhân viên", MaxLength: 5)
    ];

    public FormDanhSachNhanVien() : base(
        "Quản lý thông tin nhân viên",
        "Danh sách nhân viên",
        [
            new(nameof(EmployeeRow.MaNv), "Mã NV", 8),
            new(nameof(EmployeeRow.TenNv), "Họ và tên", 16),
            new(nameof(EmployeeRow.ChucVu), "Chức vụ", 13),
            new(nameof(EmployeeRow.Luong), "Lương", 12, "N0"),
            new(nameof(EmployeeRow.Sdt), "Điện thoại", 12),
            new(nameof(EmployeeRow.Cccd), "CCCD", 12),
            new(nameof(EmployeeRow.NgaySinhText), "Ngày sinh", 11),
            new(nameof(EmployeeRow.Email), "Email", 18)
        ],
        LoadRows,
        row => row.MaNv,
        EditEmployee,
        DeleteEmployee,
        Properties.Resources.NhanVien,
        Properties.Resources.edit,
        Properties.Resources.delete)
    {
    }

    private static List<EmployeeRow> LoadRows(string keyword)
    {
        using var db = new HotelManagementContext();
        var query = db.NhanViens.AsNoTracking().Where(item => item.DaXoa != true);
        if (keyword.Length > 0)
            query = query.Where(item => item.MaNv.Contains(keyword) || item.TenNv.Contains(keyword) ||
                item.ChucVu.Contains(keyword) || item.Sdt.Contains(keyword) || item.Cccd.Contains(keyword));
        return query.OrderBy(item => item.MaNv)
            .AsEnumerable()
            .Select(item => new EmployeeRow
            {
                MaNv = item.MaNv,
                TenNv = item.TenNv,
                ChucVu = item.ChucVu,
                Luong = item.Luong,
                Sdt = item.Sdt,
                Cccd = item.Cccd,
                NgaySinhText = item.NgaySinh.ToString("dd/MM/yyyy"),
                Email = item.Email ?? ""
            }).ToList();
    }

    private static bool EditEmployee(string? code)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["NgaySinh"] = DateTime.Today.AddYears(-18).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["Luong"] = "0",
            ["GioiTinh"] = "Nam"
        };
        if (code is not null)
        {
            using var db = new HotelManagementContext();
            var item = db.NhanViens.AsNoTracking().FirstOrDefault(employee => employee.MaNv == code && employee.DaXoa != true);
            if (item is null)
            {
                MessageBox.Show("Không tìm thấy nhân viên cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            values["MaNv"] = item.MaNv;
            values["TenNv"] = item.TenNv;
            values["ChucVu"] = item.ChucVu;
            values["Luong"] = item.Luong.ToString(CultureInfo.CurrentCulture);
            values["Sdt"] = item.Sdt;
            values["Cccd"] = item.Cccd;
            values["NgaySinh"] = item.NgaySinh.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            values["GioiTinh"] = item.GioiTinh;
            values["DiaChi"] = item.DiaChi;
            values["Email"] = item.Email ?? "";
        }
        var fields = EditFields.Select(field => field with
        {
            ReadOnly = code is not null && field.Name == "MaNv",
            Choices = field.Name == "ChucVu" && values.TryGetValue("ChucVu", out var currentRole)
                ? new[] { "Người quản trị hệ thống", "Quản lý khách sạn", "Nhân viên", currentRole }.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
                : field.Choices
        }).ToArray();
        using var dialog = new EntityEditDialog(
            code is null ? "FormThemNhanVien" : "FormSuaNhanVien",
            code is null ? "Thêm thông tin nhân viên" : "Cập nhật thông tin nhân viên",
            fields,
            values,
            input => SaveEmployee(code, input),
            code is null ? new Size(1076, 558) : new Size(1053, 559),
            twoColumns: true,
            saveCaption: code is null ? "Thêm mới" : "Cập nhật");
        return dialog.ShowDialog() == DialogResult.OK;
    }

    private static bool SaveEmployee(string? originalCode, IReadOnlyDictionary<string, string> input)
    {
        var code = input["MaNv"].Trim().ToUpperInvariant();
        var cccd = input["Cccd"].Trim();
        var phone = input["Sdt"].Trim();
        var email = input["Email"].Trim();
        if (!TryDecimal(input["Luong"], out var salary) || salary < 0)
            return Warn("Lương phải là số không âm.");
        if (!DateOnly.TryParse(input["NgaySinh"], out var birthDate) || birthDate >= DateOnly.FromDateTime(DateTime.Today))
            return Warn("Ngày sinh phải nhỏ hơn ngày hiện tại.");
        if (phone.Length != 10 || phone.Any(character => !char.IsDigit(character)))
            return Warn("Số điện thoại nhân viên phải gồm đúng 10 chữ số.");
        if (cccd.Length != 12 || cccd.Any(character => !char.IsDigit(character)))
            return Warn("CCCD nhân viên phải gồm đúng 12 chữ số.");
        if (email.Length > 0 && !email.Contains('@'))
            return Warn("Email chưa đúng định dạng.");

        using var db = new HotelManagementContext();
        if (db.NhanViens.Any(employee => employee.MaNv != originalCode && employee.Cccd == cccd))
            return Warn("CCCD đã được sử dụng bởi nhân viên khác.");
        if (db.NhanViens.Any(employee => employee.MaNv != originalCode && employee.Sdt == phone))
            return Warn("Số điện thoại đã được sử dụng bởi nhân viên khác.");
        if (email.Length > 0 && db.NhanViens.Any(employee => employee.MaNv != originalCode && employee.Email == email))
            return Warn("Email đã được sử dụng bởi nhân viên khác.");

        var item = originalCode is null
            ? db.NhanViens.FirstOrDefault(employee => employee.MaNv == code)
            : db.NhanViens.FirstOrDefault(employee => employee.MaNv == originalCode && employee.DaXoa != true);
        if (originalCode is null && item is not null)
            return Warn("Mã nhân viên đã tồn tại. Vui lòng nhập mã khác.");
        if (originalCode is not null && item is null)
            return Warn("Không tìm thấy nhân viên cần cập nhật.");
        if (item is null)
        {
            item = new HotelManagement.Models.NhanVien { MaNv = code, DaXoa = false };
            db.NhanViens.Add(item);
        }
        item.TenNv = input["TenNv"].Trim();
        item.ChucVu = input["ChucVu"].Trim();
        item.Luong = salary;
        item.Sdt = phone;
        item.Cccd = cccd;
        item.NgaySinh = birthDate;
        item.GioiTinh = input["GioiTinh"];
        item.DiaChi = input["DiaChi"].Trim();
        item.Email = email.Length == 0 ? null : email;
        db.SaveChanges();
        return true;
    }

    private static void DeleteEmployee(string code)
    {
        using var db = new HotelManagementContext();
        var item = db.NhanViens.FirstOrDefault(employee => employee.MaNv == code && employee.DaXoa != true);
        if (item is null)
            return;
        if (code == HotelManagement.DangNhap.FormDangNhap.MaNV_DangNhap)
            throw new InvalidOperationException("Không thể ẩn tài khoản nhân viên đang đăng nhập.");
        item.DaXoa = true;
        db.SaveChanges();
    }

    private static bool TryDecimal(string text, out decimal value) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value) ||
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    private static bool Warn(string message)
    {
        MessageBox.Show(message, "Thông tin chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
    }

    public sealed class EmployeeRow
    {
        public string MaNv { get; init; } = "";
        public string TenNv { get; init; } = "";
        public string ChucVu { get; init; } = "";
        public decimal Luong { get; init; }
        public string Sdt { get; init; } = "";
        public string Cccd { get; init; } = "";
        public string NgaySinhText { get; init; } = "";
        public string Email { get; init; } = "";
    }
}
