namespace HotelManagement;

public sealed class FormTrangChu : UserControl
{
    public FormTrangChu()
    {
        var pictureBox1 = new PictureBox
        {
            Dock = DockStyle.Fill,
            Image = Properties.Resources.pictureIcon,
            Location = Point.Empty,
            Margin = Padding.Empty,
            Name = "pictureBox1",
            SizeMode = PictureBoxSizeMode.StretchImage,
            TabStop = false
        };
        Controls.Add(pictureBox1);
    }
}
