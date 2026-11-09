using System.Globalization;
using HotelManagement.Models;
using HotelManagement.UI;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.DichVu;

public sealed class FormDanhSachDichVu : EntityGridForm<FormDanhSachDichVu.ServiceRow>
{
    private static readonly EditField[] EditFields =
    [
        new("MaDv", "Mã dịch vụ", MaxLength: 5),
        new("TenDv", "Tên dịch vụ", MaxLength: 20),
        new("LoaiDv", "Loại dịch vụ", EditFieldType.Choice, Choices: ["Thức uống", "Đồ ăn", "Dịch vụ"]),
        new("DonGia", "Đơn giá", EditFieldType.Decimal),
        new("SlconLai", "Số lượng còn lại", EditFieldType.Integer, Required: false)
    ];

    public FormDanhSachDichVu() : base(
        "Quản lý thông tin dịch vụ",
        "Danh sách dịch vụ",
        [
            new(nameof(ServiceRow.MaDv), "Mã dịch vụ", 10),
            new(nameof(ServiceRow.TenDv), "Tên dịch vụ", 20),
            new(nameof(ServiceRow.DonGia), "Đơn giá", 14, "N0"),
            new(nameof(ServiceRow.SoLuong), "Số lượng còn lại", 17),
            new(nameof(ServiceRow.LoaiDv), "Loại dịch vụ", 20)
        ],
        LoadRows,
        row => row.MaDv,
        EditService,
        DeleteService,
        Properties.Resources.DichVu,
        Properties.Resources.edit,
        Properties.Resources.delete)
    {
    }

    private static List<ServiceRow> LoadRows(string keyword)
    {
        using var db = new HotelManagementContext();
        var query = db.DichVus.AsNoTracking().Where(item => item.DaXoa != true);
        if (keyword.Length > 0)
            query = query.Where(item => item.MaDv.Contains(keyword) || item.TenDv.Contains(keyword) || item.LoaiDv.Contains(keyword));

        var services = query.OrderBy(item => item.MaDv)
            .Select(item => new
            {
                MaDv = item.MaDv,
                TenDv = item.TenDv,
                DonGia = item.DonGia,
                item.SlconLai,
                LoaiDv = item.LoaiDv
            }).ToList();

        return services.Select(item => new ServiceRow
        {
            MaDv = item.MaDv,
            TenDv = item.TenDv,
            DonGia = item.DonGia,
            SoLuong = item.SlconLai == null || item.SlconLai < 0 ? "Không giới hạn" : item.SlconLai.Value.ToString(),
            LoaiDv = item.LoaiDv
        }).ToList();
    }

    private static bool EditService(string? code)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["SlconLai"] = "-1",
            ["DonGia"] = "0",
            ["LoaiDv"] = "Thức uống"
        };
        if (code is not null)
        {
            using var db = new HotelManagementContext();
            var item = db.DichVus.AsNoTracking().FirstOrDefault(service => service.MaDv == code && service.DaXoa != true);
            if (item is null)
            {
                MessageBox.Show("Không tìm thấy dịch vụ cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            values["MaDv"] = item.MaDv;
            values["TenDv"] = item.TenDv;
            values["LoaiDv"] = item.LoaiDv;
            values["DonGia"] = item.DonGia.ToString(CultureInfo.CurrentCulture);
            values["SlconLai"] = item.SlconLai?.ToString() ?? "-1";
        }

        var fields = EditFields.Select(field => field with
        {
            ReadOnly = code is not null && field.Name == "MaDv",
            Choices = field.Name == "LoaiDv" && values.TryGetValue("LoaiDv", out var currentType)
                ? new[] { "Thức uống", "Đồ ăn", "Dịch vụ", currentType }.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
                : field.Choices
        }).ToArray();
        using var dialog = new EntityEditDialog(
            code is null ? "FormThemDichVu" : "FormSuaDichVu",
            code is null ? "Thêm thông tin dịch vụ" : "Cập nhật thông tin dịch vụ",
            fields,
            values,
            input => SaveService(code, input),
            code is null ? new Size(602, 561) : new Size(540, 522),
            saveCaption: code is null ? "Thêm mới" : "Cập nhật");
        return dialog.ShowDialog() == DialogResult.OK;
    }

    private static bool SaveService(string? originalCode, IReadOnlyDictionary<string, string> input)
    {
        var code = input["MaDv"].Trim().ToUpperInvariant();
        var name = input["TenDv"].Trim();
        var type = input["LoaiDv"].Trim();
        if (!TryDecimal(input["DonGia"], out var price) || price <= 0)
            return Warn("Đơn giá phải là số lớn hơn 0.");
        var quantityText = input["SlconLai"].Trim();
        if (quantityText.Length == 0)
            quantityText = "-1";
        if (!int.TryParse(quantityText, out var quantity) || quantity < -1)
            return Warn("Số lượng còn lại không hợp lệ.");

        using var db = new HotelManagementContext();
        var item = originalCode is null
            ? db.DichVus.FirstOrDefault(service => service.MaDv == code)
            : db.DichVus.FirstOrDefault(service => service.MaDv == originalCode && service.DaXoa != true);
        if (originalCode is null && item is not null)
            return Warn("Mã dịch vụ đã tồn tại. Vui lòng nhập mã khác.");
        if (originalCode is not null && item is null)
            return Warn("Không tìm thấy dịch vụ cần cập nhật.");
        if (item is null)
        {
            item = new HotelManagement.Models.DichVu { MaDv = code, DaXoa = false };
            db.DichVus.Add(item);
        }
        item.TenDv = name;
        item.DonGia = price;
        item.SlconLai = quantity;
        item.LoaiDv = type;
        db.SaveChanges();
        return true;
    }

    private static void DeleteService(string code)
    {
        using var db = new HotelManagementContext();
        var item = db.DichVus.FirstOrDefault(service => service.MaDv == code && service.DaXoa != true);
        if (item is null)
            return;
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

    public sealed class ServiceRow
    {
        public string MaDv { get; init; } = "";
        public string TenDv { get; init; } = "";
        public decimal DonGia { get; init; }
        public string SoLuong { get; init; } = "";
        public string LoaiDv { get; init; } = "";
    }
}
