using System.ComponentModel;
using System.Globalization;
using ClosedXML.Excel;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.BaoCao;

public sealed class FormBaoCaoKinhDoanh : Form
{
    private readonly DateTimePicker _from = new() { Format = DateTimePickerFormat.Short, Width = 132 };
    private readonly DateTimePicker _to = new() { Format = DateTimePickerFormat.Short, Width = 132 };
    private readonly Label _roomRevenue = new();
    private readonly Label _serviceRevenue = new();
    private readonly Label _bookings = new();
    private readonly Label _bestRoomType = new();
    private readonly Label _popularRoomType = new();
    private readonly Label _bestService = new();
    private readonly ChartPanel _roomChart = new("Doanh thu tiền phòng", pie: false);
    private readonly ChartPanel _bookingChart = new("Số phòng được đặt", pie: false);
    private readonly ChartPanel _serviceChart = new("Các dịch vụ được sử dụng", pie: true);
    private ReportSnapshot? _snapshot;

    public FormBaoCaoKinhDoanh()
    {
        Text = "Thống kê tình hình kinh doanh";
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        _from.Value = DateTime.Today.AddYears(-5);
        _to.Value = DateTime.Today;
        BuildLayout();
        Load += (_, _) => RefreshReport();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(10), BackColor = Color.FromArgb(242, 229, 239) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var filter = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(8, 10, 4, 4), BackColor = Color.FromArgb(224, 230, 247) };
        filter.Controls.Add(new Label { Text = "Từ ngày", AutoSize = true, Margin = new Padding(0, 7, 6, 0) });
        filter.Controls.Add(_from);
        filter.Controls.Add(new Label { Text = "Đến ngày", AutoSize = true, Margin = new Padding(18, 7, 6, 0) });
        filter.Controls.Add(_to);
        var refresh = new Button { Text = "Xem thống kê", Width = 130, Height = 31, BackColor = Color.LightSkyBlue, UseVisualStyleBackColor = false, Margin = new Padding(18, 0, 6, 0) };
        refresh.Click += (_, _) => RefreshReport();
        var export = new Button { Text = "Xuất Excel", Width = 110, Height = 31, BackColor = Color.DarkTurquoise, UseVisualStyleBackColor = false, Margin = new Padding(4, 0, 4, 0) };
        export.Click += (_, _) => ExportExcel();
        filter.Controls.Add(refresh); filter.Controls.Add(export);
        root.Controls.Add(filter, 0, 0);

        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(2, 5, 2, 3) };
        for (var i = 0; i < 3; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        cards.Controls.Add(CreateMetricCard("Doanh thu thuê phòng", _roomRevenue, Color.FromArgb(241, 126, 111)), 0, 0);
        cards.Controls.Add(CreateMetricCard("Doanh thu dịch vụ", _serviceRevenue, Color.FromArgb(183, 180, 92)), 1, 0);
        cards.Controls.Add(CreateMetricCard("Số phòng đã đặt", _bookings, Color.FromArgb(119, 173, 127)), 2, 0);
        root.Controls.Add(cards, 0, 1);

        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 3, 0, 0) };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(2, 2, 6, 2) };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.Controls.Add(CreateInsightCard("Loại phòng doanh thu cao nhất", _bestRoomType), 0, 0);
        left.Controls.Add(CreateInsightCard("Loại phòng được đặt nhiều nhất", _popularRoomType), 0, 1);
        left.Controls.Add(CreateInsightCard("Dịch vụ doanh thu cao nhất", _bestService), 0, 2);
        _serviceChart.Dock = DockStyle.Fill;
        left.Controls.Add(_serviceChart, 0, 3);
        content.Controls.Add(left, 0, 0);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(4, 2, 2, 2) };
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); right.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        _roomChart.Dock = DockStyle.Fill; _bookingChart.Dock = DockStyle.Fill;
        right.Controls.Add(_roomChart, 0, 0); right.Controls.Add(_bookingChart, 0, 1);
        content.Controls.Add(right, 1, 0);
        root.Controls.Add(content, 0, 2);
    }

    private static Control CreateMetricCard(string caption, Label value, Color color)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = color, Margin = new Padding(7, 3, 7, 3), Padding = new Padding(12) };
        var heading = new Label { Text = caption, Dock = DockStyle.Top, Height = 32, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold) };
        value.Dock = DockStyle.Fill; value.TextAlign = ContentAlignment.MiddleCenter; value.ForeColor = Color.White; value.Font = new Font("Microsoft Sans Serif", 15F, FontStyle.Bold);
        panel.Controls.Add(value); panel.Controls.Add(heading);
        return panel;
    }

    private static Control CreateInsightCard(string caption, Label value)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(227, 214, 231), Margin = new Padding(2, 3, 2, 3), Padding = new Padding(10, 4, 8, 4) };
        var heading = new Label { Text = caption, Dock = DockStyle.Top, Height = 22, ForeColor = Color.Teal, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) };
        value.Dock = DockStyle.Fill; value.TextAlign = ContentAlignment.MiddleLeft; value.ForeColor = Color.FromArgb(37, 75, 145); value.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
        panel.Controls.Add(value); panel.Controls.Add(heading);
        return panel;
    }

    private void RefreshReport()
    {
        var start = _from.Value.Date;
        var end = _to.Value.Date;
        if (end < start) { MessageBox.Show("Ngày kết thúc phải bằng hoặc sau ngày bắt đầu.", "Khoảng thời gian không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        try
        {
            using var db = new HotelManagementContext();
            var endExclusive = end.AddDays(1);
            var paid = db.HoaDons.AsNoTracking()
                .Where(invoice => invoice.DaXoa != true && invoice.TrangThai == "Đã thanh toán" && invoice.NgHd >= start && invoice.NgHd < endExclusive)
                .Select(invoice => new PaidInvoiceRow
                {
                    MaCtdp = invoice.MaCtdp,
                    RoomType = invoice.MaCtdpNavigation.MaPhNavigation.MaLphNavigation.TenLph,
                    RoomAmount = invoice.MaCtdpNavigation.ThanhTien ?? 0
                }).ToList();
            var ids = paid.Select(row => row.MaCtdp).Distinct().ToList();
            List<ServiceUsageRow> serviceRows = ids.Count == 0 ? [] : db.Ctdvs.AsNoTracking().Where(line => ids.Contains(line.MaCtdp) && line.DaXoa != true)
                .Select(line => new ServiceUsageRow { Name = line.MaDvNavigation.TenDv, Count = line.Sl, Amount = line.ThanhTien ?? ((line.DonGia ?? line.MaDvNavigation.DonGia) * line.Sl) })
                .ToList();
            var bookingRows = db.Ctdps.AsNoTracking().Where(detail => detail.DaXoa != true && detail.TrangThai != "Đã hủy" && detail.CheckIn >= start && detail.CheckIn < endExclusive)
                .Select(detail => detail.MaPhNavigation.MaLphNavigation.TenLph).ToList();

            var roomRevenue = paid.GroupBy(row => row.RoomType).Select(group => new ChartValue(group.Key, group.Sum(row => row.RoomAmount))).OrderByDescending(item => item.Value).ToList();
            var booked = bookingRows.GroupBy(name => name).Select(group => new ChartValue(group.Key, group.Count())).OrderByDescending(item => item.Value).ToList();
            var services = serviceRows.GroupBy(row => row.Name).Select(group => new ChartValue(group.Key, group.Sum(row => row.Count))).OrderByDescending(item => item.Value).ToList();
            var serviceRevenue = serviceRows.GroupBy(row => row.Name).Select(group => new ChartValue(group.Key, group.Sum(row => row.Amount))).OrderByDescending(item => item.Value).ToList();

            _snapshot = new ReportSnapshot(start, end, paid.Sum(row => row.RoomAmount), serviceRows.Sum(row => row.Amount), bookingRows.Count, roomRevenue, booked, services, serviceRevenue);
            _roomRevenue.Text = _snapshot.RoomRevenue.ToString("N0") + " VND";
            _serviceRevenue.Text = _snapshot.ServiceRevenue.ToString("N0") + " VND";
            _bookings.Text = _snapshot.BookingCount.ToString("N0");
            _bestRoomType.Text = roomRevenue.FirstOrDefault() is { } topRoom ? $"{topRoom.Name} ({topRoom.Value:N0} VND)" : "Chưa có dữ liệu trong kỳ";
            _popularRoomType.Text = booked.FirstOrDefault() is { } topBooked ? $"{topBooked.Name} ({topBooked.Value:N0} lượt)" : "Chưa có dữ liệu trong kỳ";
            _bestService.Text = serviceRevenue.FirstOrDefault() is { } topService ? $"{topService.Name} ({topService.Value:N0} VND)" : "Chưa có dữ liệu trong kỳ";
            _roomChart.Values = roomRevenue;
            _bookingChart.Values = booked;
            _serviceChart.Values = services;
        }
        catch (Exception ex) { MessageBox.Show($"Không tải được số liệu thống kê.\n{ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ExportExcel()
    {
        if (_snapshot is null) { MessageBox.Show("Hãy xem thống kê trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        using var dialog = new SaveFileDialog { Title = "Xuất báo cáo kinh doanh", Filter = "Excel Workbook (*.xlsx)|*.xlsx", FileName = $"BaoCao_{_snapshot.Start:yyyyMMdd}_{_snapshot.End:yyyyMMdd}.xlsx" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var workbook = new XLWorkbook();
            var summary = workbook.Worksheets.Add("Tổng quan");
            summary.Cell(1, 1).Value = "Báo cáo kinh doanh"; summary.Cell(1, 1).Style.Font.Bold = true;
            summary.Cell(2, 1).Value = "Từ ngày"; summary.Cell(2, 2).Value = _snapshot.Start;
            summary.Cell(3, 1).Value = "Đến ngày"; summary.Cell(3, 2).Value = _snapshot.End;
            summary.Cell(5, 1).Value = "Doanh thu tiền phòng"; summary.Cell(5, 2).Value = _snapshot.RoomRevenue;
            summary.Cell(6, 1).Value = "Doanh thu dịch vụ"; summary.Cell(6, 2).Value = _snapshot.ServiceRevenue;
            summary.Cell(7, 1).Value = "Số phòng đặt"; summary.Cell(7, 2).Value = _snapshot.BookingCount;
            WriteTable(workbook, "Doanh thu phòng", _snapshot.RoomByType);
            WriteTable(workbook, "Lượt đặt", _snapshot.BookingsByType);
            WriteTable(workbook, "Dịch vụ", _snapshot.ServicesByName);
            WriteTable(workbook, "Doanh thu dịch vụ", _snapshot.ServiceRevenueByName);
            foreach (var sheet in workbook.Worksheets) sheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);
            MessageBox.Show("Đã xuất báo cáo Excel.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Không xuất được báo cáo.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static void WriteTable(XLWorkbook workbook, string name, IReadOnlyList<ChartValue> data)
    {
        var sheet = workbook.Worksheets.Add(name.Length > 31 ? name[..31] : name);
        sheet.Cell(1, 1).Value = "Loại"; sheet.Cell(1, 2).Value = "Giá trị";
        sheet.Range(1, 1, 1, 2).Style.Font.Bold = true;
        for (var index = 0; index < data.Count; index++)
        {
            sheet.Cell(index + 2, 1).Value = data[index].Name;
            sheet.Cell(index + 2, 2).Value = data[index].Value;
        }
    }

    private sealed class PaidInvoiceRow
    {
        public string MaCtdp { get; init; } = "";
        public string RoomType { get; init; } = "";
        public decimal RoomAmount { get; init; }
    }

    private sealed class ServiceUsageRow
    {
        public string Name { get; init; } = "";
        public int Count { get; init; }
        public decimal Amount { get; init; }
    }

    private sealed record ReportSnapshot(DateTime Start, DateTime End, decimal RoomRevenue, decimal ServiceRevenue, int BookingCount,
        IReadOnlyList<ChartValue> RoomByType, IReadOnlyList<ChartValue> BookingsByType,
        IReadOnlyList<ChartValue> ServicesByName, IReadOnlyList<ChartValue> ServiceRevenueByName);
}

internal sealed record ChartValue(string Name, decimal Value);

internal sealed class ChartPanel : Control
{
    private IReadOnlyList<ChartValue> _values = [];
    private readonly bool _pie;
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<ChartValue> Values { get => _values; set { _values = value; Invalidate(); } }
    public string Title { get; }

    public ChartPanel(string title, bool pie)
    {
        Title = title;
        _pie = pie;
        DoubleBuffered = true;
        BackColor = Color.White;
        MinimumSize = new Size(220, 160);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var titleFont = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
        using var bodyFont = new Font("Microsoft Sans Serif", 8F);
        using var titleBrush = new SolidBrush(Color.Teal);
        using var textBrush = new SolidBrush(Color.DimGray);
        g.DrawString(Title, titleFont, titleBrush, new PointF(12, 10));
        var area = new Rectangle(12, 38, Math.Max(0, Width - 24), Math.Max(0, Height - 50));
        if (_values.Count == 0 || _values.All(value => value.Value <= 0))
        {
            g.DrawString("Chưa có dữ liệu trong kỳ", bodyFont, textBrush, area, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            return;
        }
        if (_pie) DrawPie(g, bodyFont, textBrush, area);
        else DrawBars(g, bodyFont, textBrush, area);
    }

    private void DrawBars(Graphics g, Font font, Brush text, Rectangle area)
    {
        var max = Math.Max(1, (double)_values.Max(item => item.Value));
        var axisScale = max >= 1_000_000 ? 1_000_000 : max >= 1_000 ? 1_000 : 1;
        var axisUnit = axisScale == 1_000_000 ? " tr" : axisScale == 1_000 ? " nghìn" : "";
        var plot = new Rectangle(area.X + 76, area.Y + 4, Math.Max(1, area.Width - 96), Math.Max(1, area.Height - 34));
        using var grid = new Pen(Color.LightGray);
        using var axis = new Pen(Color.Gray, 1.2F);
        using var axisLabelFormat = new StringFormat
        {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoWrap
        };
        for (var tick = 0; tick <= 4; tick++)
        {
            var y = plot.Bottom - tick * plot.Height / 4;
            g.DrawLine(grid, plot.Left, y, plot.Right, y);
            var value = (max * tick / 4 / axisScale).ToString("0.#", CultureInfo.InvariantCulture);
            var label = tick == 0 || axisUnit.Length == 0 ? value : value + axisUnit;
            g.DrawString(label, font, text, new RectangleF(area.X, y - 11, 68, 22), axisLabelFormat);
        }
        g.DrawLine(axis, plot.Left, plot.Top, plot.Left, plot.Bottom);
        g.DrawLine(axis, plot.Left, plot.Bottom, plot.Right, plot.Bottom);
        var slot = (float)plot.Width / _values.Count;
        var barWidth = Math.Max(8, slot * 0.52F);
        using var bar = new SolidBrush(Color.FromArgb(56, 132, 219));
        for (var index = 0; index < _values.Count; index++)
        {
            var height = (float)((double)_values[index].Value / max * (plot.Height - 2));
            var x = plot.Left + index * slot + (slot - barWidth) / 2;
            var y = plot.Bottom - height;
            g.FillRectangle(bar, x, y, barWidth, height);
            g.DrawString(_values[index].Name, font, text, new RectangleF(plot.Left + index * slot - 8, plot.Bottom + 3, slot + 16, 27), new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
        }
    }

    private void DrawPie(Graphics g, Font font, Brush text, Rectangle area)
    {
        var values = _values.Take(6).ToArray();
        var total = (double)values.Sum(item => item.Value);
        var diameter = Math.Min(area.Height - 4, area.Width / 2);
        var pie = new Rectangle(area.X + 8, area.Y + 5, Math.Max(1, diameter), Math.Max(1, diameter));
        Color[] colors = [Color.FromArgb(244, 148, 73), Color.FromArgb(57, 136, 208), Color.FromArgb(231, 80, 62), Color.FromArgb(81, 176, 121), Color.FromArgb(143, 93, 187), Color.FromArgb(228, 184, 55)];
        var angle = 0F;
        for (var index = 0; index < values.Length; index++)
        {
            var sweep = (float)((double)values[index].Value / total * 360);
            using var brush = new SolidBrush(colors[index % colors.Length]);
            g.FillPie(brush, pie, angle, sweep);
            angle += sweep;
        }
        var legendX = pie.Right + 12;
        var legendY = area.Y + 8;
        for (var index = 0; index < values.Length; index++)
        {
            using var brush = new SolidBrush(colors[index % colors.Length]);
            g.FillRectangle(brush, legendX, legendY + index * 22 + 3, 11, 11);
            var label = $"{values[index].Name} ({values[index].Value:N0})";
            g.DrawString(label, font, text, new RectangleF(legendX + 16, legendY + index * 22, Math.Max(10, area.Right - legendX - 18), 18), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
        }
    }
}
