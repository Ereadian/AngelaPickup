namespace ereadian.builder.UnitTest;

[ExcludeFromCodeCoverage]
public class TemporaryFolder : IDisposable
{
    public TemporaryFolder(string folderName)
    {
        string fullPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), folderName));
        this.FullPath = fullPath;

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    public string FullPath {get;}

    public void Dispose()
    {
        if (Directory.Exists(this.FullPath))
        {
            Directory.Delete(this.FullPath, true);
        }
    }
}