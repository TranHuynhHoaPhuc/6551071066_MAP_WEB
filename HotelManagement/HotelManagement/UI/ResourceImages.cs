using System.Drawing;

namespace HotelManagement.UI;

internal static class ResourceImages
{
    public static Image FromBytes(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }
}
