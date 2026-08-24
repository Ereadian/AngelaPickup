namespace ereadian.builder.UnitTest;

[TestClass]
public sealed class TemporaryFileUnitTest
{
    [TestMethod]
    public void TemporaryFile_Test()
    {
        // Arrange
        string fileName = $"{TestUtility.CreateUniqueName("Test")}.html";
        string fullPath;
        using (TemporaryFile temporaryFile = new (fileName))
        {
            fullPath = temporaryFile.FullPath;
            File.WriteAllText(fullPath, "Test");
            Assert.IsTrue(File.Exists(temporaryFile.FullPath));
        }

        Assert.IsFalse(File.Exists(fullPath));
    }
}
