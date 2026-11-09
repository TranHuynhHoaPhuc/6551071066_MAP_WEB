using System.Globalization;
using HotelManagement.DangNhap;
using HotelManagement.Models;
using HotelManagement.PhieuThue;
using HotelManagement.UI;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.SoDoPhong;

public sealed class FormDanhSachDatPhong : EntityGridForm<FormDanhSachDatPhong.BookingRow>
{
    public FormDanhSachDatPhong() : base(
        "Danh sách đặt phòng",
        "Danh sách đặt phòng",
        [
            new(nameof(BookingRow.MaCtdp), "Mã chi tiết", 10),
            new(nameof(BookingRow.TenKhach), "Khách hàng", 17),
            new(nameof(BookingRow.MaPh), "Mã phòng", 9),
            new(nameof(BookingRow.CheckInText), "Nhận phòng", 15),
            new(nameof(BookingRow.CheckOutText), "Trả phòng", 15),
            new(nameof(BookingRow.TrangThai), "Trạng thái", 12),
            new(nameof(BookingRow.ThanhTien), "Thành tiền", 13, "N0")
        ],
        LoadRows,
        row => row.MaCtdp,
        OpenBooking,
        CancelBooking,
        Properties.Resources.MenuDatPhong,
        Properties.Resources.details,
        Properties.Resources.delete,
        addCaption: "Đặt phòng",
        editCaption: "Chi tiết",
        deleteCaption: "Hủy đặt")
    {
    }

    private static List<BookingRow> LoadRows(string keyword)
    {
        using var db = new HotelManagementContext();
        var query = db.Ctdps.AsNoTracking()
            .Where(item => item.DaXoa != true && item.MaPtNavigation.DaXoa != true);
        if (keyword.Length > 0)
            query = query.Where(item => item.MaCtdp.Contains(keyword) || item.MaPh.Contains(keyword) ||
                item.MaPtNavigation.MaKhNavigation.TenKh.Contains(keyword));

        return query.OrderByDescending(item => item.CheckIn)
            .Select(item => new BookingRow
            {
                MaCtdp = item.MaCtdp,
                TenKhach = item.MaPtNavigation.MaKhNavigation.TenKh,
                MaPh = item.MaPh,
                CheckIn = item.CheckIn,
                CheckOut = item.CheckOut,
                TrangThai = item.TrangThai,
                ThanhTien = item.ThanhTien ?? 0
            })
            .AsEnumerable()
            .Select(item => item with
            {
                CheckInText = item.CheckIn.ToString("dd/MM/yyyy HH:mm"),
                CheckOutText = item.CheckOut.ToString("dd/MM/yyyy HH:mm")
            })
            .ToList();
    }

    private static bool OpenBooking(string? code)
    {
        if (code is null)
        {
            using var dialog = new FormDatPhong();
            return dialog.ShowDialog() == DialogResult.OK;
        }

        using var db = new HotelManagementContext();
        var maPhieuThue = db.Ctdps.AsNoTracking()
            .Where(detail => detail.MaCtdp == code && detail.DaXoa != true)
            .Select(detail => detail.MaPt)
            .FirstOrDefault();
        if (maPhieuThue is null)
            return false;

        using var form = new FormChiTietPhieuThue(maPhieuThue);
        form.ShowDialog();
        return true;
    }

    private static void CancelBooking(string code)
    {
        using var db = new HotelManagementContext();
        var item = db.Ctdps.FirstOrDefault(detail => detail.MaCtdp == code && detail.DaXoa != true);
        if (item is null)
            return;
        if (!string.Equals(item.TrangThai, "Đã đặt", StringComparison.OrdinalIgnoreCase) || item.CheckIn <= DateTime.Now)
            throw new InvalidOperationException("Chỉ có thể hủy phiếu đặt chưa đến giờ nhận phòng.");
        if (db.HoaDons.Any(invoice => invoice.MaCtdp == code && invoice.DaXoa != true))
            throw new InvalidOperationException("Đặt phòng đã có hóa đơn nên không thể hủy.");

        item.DaXoa = true;
        if (!db.Ctdps.Any(detail => detail.MaPt == item.MaPt && detail.MaCtdp != code && detail.DaXoa != true))
        {
            var ticket = db.PhieuThues.FirstOrDefault(booking => booking.MaPt == item.MaPt);
            if (ticket is not null)
                ticket.DaXoa = true;
        }
        db.SaveChanges();
    }

    public sealed record BookingRow
    {
        public string MaCtdp { get; init; } = "";
        public string TenKhach { get; init; } = "";
        public string MaPh { get; init; } = "";
        public DateTime CheckIn { get; init; }
        public DateTime CheckOut { get; init; }
        public string CheckInText { get; init; } = "";
        public string CheckOutText { get; init; } = "";
        public string TrangThai { get; init; } = "";
        public decimal ThanhTien { get; init; }
    }
}
