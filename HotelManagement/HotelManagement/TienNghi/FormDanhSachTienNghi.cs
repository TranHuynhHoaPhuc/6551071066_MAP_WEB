using HotelManagement.Models;
using HotelManagement.UI;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.TienNghi;

public sealed class FormDanhSachTienNghi : EntityGridForm<FormDanhSachTienNghi.AmenityRow>
{
    private static readonly EditField[] EditFields =
    [
        new("MaTn", "Mã tiện nghi", MaxLength: 5),
        new("TenTn", "Tên tiện nghi", MaxLength: 50)
    ];

    public FormDanhSachTienNghi() : base(
        "Quản lý thông tin tiện nghi",
        "Danh sách tiện nghi",
        [new(nameof(AmenityRow.MaTn), "Mã tiện nghi", 15), new(nameof(AmenityRow.TenTn), "Tên tiện nghi", 40)],
        LoadRows,
        row => row.MaTn,
        EditAmenity,
        DeleteAmenity,
        ResourceImages.FromBytes(Properties.Resources.TienNghi),
        Properties.Resources.edit,
        Properties.Resources.delete)
    {
    }

    private static List<AmenityRow> LoadRows(string keyword)
    {
        using var db = new HotelManagementContext();
        var query = db.TienNghis.AsNoTracking().Where(item => item.DaXoa != true);
        if (keyword.Length > 0)
            query = query.Where(item => item.MaTn.Contains(keyword) || item.TenTn.Contains(keyword));
        return query.OrderBy(item => item.MaTn)
            .Select(item => new AmenityRow { MaTn = item.MaTn, TenTn = item.TenTn })
            .ToList();
    }

    private static bool EditAmenity(string? code)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        if (code is not null)
        {
            using var db = new HotelManagementContext();
            var item = db.TienNghis.AsNoTracking().FirstOrDefault(amenity => amenity.MaTn == code && amenity.DaXoa != true);
            if (item is null)
            {
                MessageBox.Show("Không tìm thấy tiện nghi cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            values["MaTn"] = item.MaTn;
            values["TenTn"] = item.TenTn;
        }

        var fields = EditFields.Select(field => field with { ReadOnly = code is not null && field.Name == "MaTn" }).ToArray();
        using var dialog = new EntityEditDialog(
            code is null ? "FormThemTienNghi" : "FormSuaTienNghi",
            code is null ? "Thêm tiện nghi" : "Cập nhật tiện nghi",
            fields,
            values,
            input => SaveAmenity(code, input),
            new Size(520, 380),
            saveCaption: code is null ? "Thêm mới" : "Cập nhật");
        return dialog.ShowDialog() == DialogResult.OK;
    }

    private static bool SaveAmenity(string? originalCode, IReadOnlyDictionary<string, string> input)
    {
        var code = input["MaTn"].Trim().ToUpperInvariant();
        using var db = new HotelManagementContext();
        var item = originalCode is null
            ? db.TienNghis.FirstOrDefault(amenity => amenity.MaTn == code)
            : db.TienNghis.FirstOrDefault(amenity => amenity.MaTn == originalCode && amenity.DaXoa != true);
        if (originalCode is null && item is not null)
            return Warn("Mã tiện nghi đã tồn tại. Vui lòng nhập mã khác.");
        if (originalCode is not null && item is null)
            return Warn("Không tìm thấy tiện nghi cần cập nhật.");
        if (item is null)
        {
            item = new HotelManagement.Models.TienNghi { MaTn = code, DaXoa = false };
            db.TienNghis.Add(item);
        }
        item.TenTn = input["TenTn"].Trim();
        db.SaveChanges();
        return true;
    }

    private static void DeleteAmenity(string code)
    {
        using var db = new HotelManagementContext();
        var item = db.TienNghis.FirstOrDefault(amenity => amenity.MaTn == code && amenity.DaXoa != true);
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

    public sealed class AmenityRow
    {
        public string MaTn { get; init; } = "";
        public string TenTn { get; init; } = "";
    }
}
