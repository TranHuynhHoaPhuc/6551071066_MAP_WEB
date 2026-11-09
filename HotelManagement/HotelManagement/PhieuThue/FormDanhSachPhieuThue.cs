using HotelManagement.Models;
using HotelManagement.SoDoPhong;
using HotelManagement.UI;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.PhieuThue;

public sealed class FormDanhSachPhieuThue : EntityGridForm<FormDanhSachPhieuThue.TicketRow>
{
    public FormDanhSachPhieuThue() : base(
        "Danh sách phiếu thuê",
        "Danh sách phiếu thuê",
        [
            new(nameof(TicketRow.MaPt), "Mã phiếu thuê", 13),
            new(nameof(TicketRow.TenKhach), "Khách hàng", 23),
            new(nameof(TicketRow.NgayLapText), "Ngày lập", 18),
            new(nameof(TicketRow.NhanVien), "Nhân viên", 19),
            new(nameof(TicketRow.SoPhong), "Số phòng", 10),
            new(nameof(TicketRow.TrangThai), "Trạng thái", 17)
        ],
        LoadRows,
        row => row.MaPt,
        OpenTicket,
        CancelTicket,
        Properties.Resources.MenuDatPhong,
        Properties.Resources.details,
        Properties.Resources.delete,
        addCaption: "Đặt phòng",
        editCaption: "Chi tiết",
        deleteCaption: "Hủy phiếu")
    {
    }

    private static List<TicketRow> LoadRows(string keyword)
    {
        using var db = new HotelManagementContext();
        var query = db.PhieuThues.AsNoTracking()
            .Where(item => item.DaXoa != true && item.Ctdps.Any(detail => detail.DaXoa != true));
        if (keyword.Length > 0)
            query = query.Where(item => item.MaPt.Contains(keyword) || item.MaKhNavigation.TenKh.Contains(keyword) || item.MaNvNavigation.TenNv.Contains(keyword));

        var tickets = query.OrderByDescending(item => item.NgPt)
            .Select(item => new
            {
                item.MaPt,
                TenKhach = item.MaKhNavigation.TenKh,
                item.NgPt,
                NhanVien = item.MaNvNavigation.TenNv
            }).ToList();
        var codes = tickets.Select(item => item.MaPt).ToList();
        var details = db.Ctdps.AsNoTracking().Where(item => codes.Contains(item.MaPt) && item.DaXoa != true)
            .Select(item => new { item.MaPt, item.TrangThai }).ToList();
        return tickets.Select(ticket =>
        {
            var states = details.Where(item => item.MaPt == ticket.MaPt).Select(item => item.TrangThai).ToArray();
            var status = states.All(state => state == "Đã xong") ? "Đã hoàn tất"
                : states.Any(state => state == "Đang thuê") ? "Đang thuê"
                : states.Any(state => state == "Đã đặt") ? "Đã đặt"
                : "Đã hủy";
            return new TicketRow
            {
                MaPt = ticket.MaPt,
                TenKhach = ticket.TenKhach,
                NgayLapText = ticket.NgPt.ToString("dd/MM/yyyy HH:mm"),
                NhanVien = ticket.NhanVien,
                SoPhong = states.Length,
                TrangThai = status
            };
        }).ToList();
    }

    private static bool OpenTicket(string? code)
    {
        if (code is null)
        {
            using var booking = new FormDatPhong();
            return booking.ShowDialog() == DialogResult.OK;
        }
        using var details = new FormChiTietPhieuThue(code);
        details.ShowDialog();
        return true;
    }

    private static void CancelTicket(string code)
    {
        using var db = new HotelManagementContext();
        var ticket = db.PhieuThues.FirstOrDefault(item => item.MaPt == code && item.DaXoa != true);
        if (ticket is null) return;
        var details = db.Ctdps.Where(item => item.MaPt == code && item.DaXoa != true).ToList();
        if (details.Count == 0) return;
        if (details.Any(item => item.TrangThai != "Đã đặt" || item.CheckIn <= DateTime.Now))
            throw new InvalidOperationException("Chỉ có thể hủy phiếu gồm các phòng chưa đến giờ nhận phòng.");
        var detailCodes = details.Select(item => item.MaCtdp).ToList();
        if (db.HoaDons.Any(item => detailCodes.Contains(item.MaCtdp) && item.DaXoa != true))
            throw new InvalidOperationException("Phiếu đã có hóa đơn nên không thể hủy.");

        ticket.DaXoa = true;
        foreach (var detail in details)
        {
            detail.DaXoa = true;
            var room = db.Phongs.FirstOrDefault(item => item.MaPh == detail.MaPh);
            if (room is null || room.Ttph == "Đang sửa chữa") continue;
            var otherStatuses = db.Ctdps.AsNoTracking().Where(item => item.MaPh == detail.MaPh && item.MaCtdp != detail.MaCtdp && item.DaXoa != true)
                .Select(item => new { item.TrangThai, item.CheckIn, item.CheckOut }).ToList();
            if (otherStatuses.Any(item => item.TrangThai == "Đang thuê" && item.CheckIn <= DateTime.Now && item.CheckOut > DateTime.Now))
                room.Ttph = "Đang thuê";
            else if (otherStatuses.Any(item => item.TrangThai == "Đã đặt"))
                room.Ttph = "Đã đặt";
            else
                room.Ttph = "Bình thường";
        }
        db.SaveChanges();
    }

    public sealed class TicketRow
    {
        public string MaPt { get; init; } = "";
        public string TenKhach { get; init; } = "";
        public string NgayLapText { get; init; } = "";
        public string NhanVien { get; init; } = "";
        public int SoPhong { get; init; }
        public string TrangThai { get; init; } = "";
    }
}
