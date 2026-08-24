namespace ereadian.builder.UnitTest;

[ExcludeFromCodeCoverage]
public class TemporaryFile(string fileName) : IDisposable
{
    public string FullPath {get;} = Path.GetFullPath(Path.Combine(Path.GetTempPath(), fileName));

    public void Dispose()
    {
        if (File.Exists(this.FullPath))
        {
            File.Delete(this.FullPath);
        }
    }
}