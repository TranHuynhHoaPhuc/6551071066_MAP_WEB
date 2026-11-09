using HotelManagement.Models;
using HotelManagement.UI;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.KhachHang;

public sealed class FormDanhSachKhachHang : EntityGridForm<FormDanhSachKhachHang.CustomerRow>
{
    private static readonly EditField[] EditFields =
    [
        new("MaKh", "Mã khách hàng", MaxLength: 5),
        new("TenKh", "Họ và tên", MaxLength: 50),
        new("CccdPassport", "CCCD / Passport", MaxLength: 12),
        new("Sdt", "Số điện thoại", EditFieldType.Text, 10),
        new("QuocTich", "Quốc tịch", MaxLength: 30),
        new("GioiTinh", "Giới tính", EditFieldType.Choice, Choices: ["Nam", "Nữ"])
    ];

    public FormDanhSachKhachHang() : base(
        "Quản lý thông tin khách hàng",
        "Danh sách khách hàng",
        [
            new(nameof(CustomerRow.MaKh), "Mã khách", 10),
            new(nameof(CustomerRow.TenKh), "Họ và tên", 22),
            new(nameof(CustomerRow.Sdt), "Số điện thoại", 15),
            new(nameof(CustomerRow.CccdPassport), "CCCD / Passport", 18),
            new(nameof(CustomerRow.QuocTich), "Quốc tịch", 15),
            new(nameof(CustomerRow.GioiTinh), "Giới tính", 10)
        ],
        LoadRows,
        row => row.MaKh,
        EditCustomer,
        DeleteCustomer,
        Properties.Resources.KhachHang,
        Properties.Resources.edit,
        Properties.Resources.delete)
    {
    }

    private static List<CustomerRow> LoadRows(string keyword)
    {
        using var db = new HotelManagementContext();
        var query = db.KhachHangs.AsNoTracking().Where(item => item.DaXoa != true);
        if (keyword.Length > 0)
            query = query.Where(item => item.MaKh.Contains(keyword) || item.TenKh.Contains(keyword) ||
                (item.Sdt != null && item.Sdt.Contains(keyword)) || item.CccdPassport.Contains(keyword));
        return query.OrderBy(item => item.MaKh)
            .Select(item => new CustomerRow
            {
                MaKh = item.MaKh,
                TenKh = item.TenKh,
                Sdt = item.Sdt ?? "",
                CccdPassport = item.CccdPassport,
                QuocTich = item.QuocTich,
                GioiTinh = item.GioiTinh
            }).ToList();
    }

    private static bool EditCustomer(string? code)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        if (code is null)
            values["MaKh"] = NextCustomerCode();
        if (code is not null)
        {
            using var db = new HotelManagementContext();
            var item = db.KhachHangs.AsNoTracking().FirstOrDefault(customer => customer.MaKh == code && customer.DaXoa != true);
            if (item is null)
            {
                MessageBox.Show("Không tìm thấy khách hàng cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            values["MaKh"] = item.MaKh;
            values["TenKh"] = item.TenKh;
            values["CccdPassport"] = item.CccdPassport;
            values["Sdt"] = item.Sdt ?? "";
            values["QuocTich"] = item.QuocTich;
            values["GioiTinh"] = item.GioiTinh;
        }
        var fields = EditFields.Select(field => field with { ReadOnly = field.Name == "MaKh" }).ToArray();
        using var dialog = new EntityEditDialog(
            code is null ? "FormThemKhachHang" : "FormSuaKhachHang",
            code is null ? "Thêm thông tin khách hàng" : "Cập nhật thông tin khách hàng",
            fields,
            values,
            input => SaveCustomer(code, input),
            code is null ? new Size(608, 645) : new Size(628, 620),
            saveCaption: code is null ? "Thêm mới" : "Cập nhật");
        return dialog.ShowDialog() == DialogResult.OK;
    }

    private static bool SaveCustomer(string? originalCode, IReadOnlyDictionary<string, string> input)
    {
        var code = input["MaKh"].Trim().ToUpperInvariant();
        var identity = input["CccdPassport"].Trim();
        var phone = input["Sdt"].Trim();
        if (phone.Length > 0 && (phone.Length > 10 || phone.Any(character => !char.IsDigit(character))))
            return Warn("Số điện thoại chỉ gồm tối đa 10 chữ số.");
        using var db = new HotelManagementContext();
        if (db.KhachHangs.Any(customer => customer.CccdPassport == identity && customer.MaKh != originalCode))
            return Warn("CCCD / Passport này đã được đăng ký cho khách hàng khác.");

        var item = originalCode is null
            ? db.KhachHangs.FirstOrDefault(customer => customer.MaKh == code)
            : db.KhachHangs.FirstOrDefault(customer => customer.MaKh == originalCode && customer.DaXoa != true);
        if (originalCode is null && item is not null)
        {
            code = NextCustomerCode();
            item = db.KhachHangs.FirstOrDefault(customer => customer.MaKh == code);
            if (item is not null)
                return Warn("Không tạo được mã khách hàng mới. Vui lòng thử lại.");
        }
        if (originalCode is not null && item is null)
            return Warn("Không tìm thấy khách hàng cần cập nhật.");
        if (item is null)
        {
            item = new HotelManagement.Models.KhachHang { MaKh = code, DaXoa = false };
            db.KhachHangs.Add(item);
        }
        item.TenKh = input["TenKh"].Trim();
        item.Sdt = phone.Length == 0 ? null : phone;
        item.CccdPassport = identity;
        item.QuocTich = input["QuocTich"].Trim();
        item.GioiTinh = input["GioiTinh"];
        db.SaveChanges();
        return true;
    }

    private static void DeleteCustomer(string code)
    {
        using var db = new HotelManagementContext();
        var item = db.KhachHangs.FirstOrDefault(customer => customer.MaKh == code && customer.DaXoa != true);
        if (item is null)
            return;
        item.DaXoa = true;
        db.SaveChanges();
    }

    private static bool Warn(string message)
    {
        MessageBox.Show(message, "Thông tin chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
    }

    private static string NextCustomerCode()
    {
        using var db = new HotelManagementContext();
        var max = db.KhachHangs.AsNoTracking().Select(item => item.MaKh).AsEnumerable()
            .Where(code => code.StartsWith("KH", StringComparison.OrdinalIgnoreCase))
            .Select(code => int.TryParse(code[2..], out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max();
        return "KH" + (max + 1).ToString("D3", System.Globalization.CultureInfo.InvariantCulture);
    }

    public sealed class CustomerRow
    {
        public string MaKh { get; init; } = "";
        public string TenKh { get; init; } = "";
        public string Sdt { get; init; } = "";
        public string CccdPassport { get; init; } = "";
        public string QuocTich { get; init; } = "";
        public string GioiTinh { get; init; } = "";
    }
}
