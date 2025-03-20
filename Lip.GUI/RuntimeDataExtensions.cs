namespace Lip.GUI;

internal static class RuntimeDataExtensions
{
    public static ImageSource? LoadServerIcon(this Config _, Guid iconId)
    {
        string path = Path.Combine(MauiProgram.IconsDirectory, $"{iconId}.png");
        if (File.Exists(path))
            return ImageSource.FromFile(path);
        else
            return null;
    }
}
