namespace ereadian.builder.UnitTest;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class TemporaryFolderUnitTest
{
    [TestMethod]
    public void TemporaryFolder_Test()
    {
        // Arrange
        string folderName = TestUtility.CreateUniqueName("TestFolder");
        string fullPath;
        string fullFilePath;

        // Act and Assert
        using (TemporaryFolder temporaryFolder = new (folderName))
        {
            fullPath = temporaryFolder.FullPath;
            fullFilePath = Path.Combine(fullPath, TestUtility.CreateUniqueName("TestFile"));
            File.WriteAllText(fullFilePath, "Test");
            Assert.IsTrue(Directory.Exists(temporaryFolder.FullPath));
            Assert.IsTrue(File.Exists(fullFilePath));
        }

        Assert.IsFalse(Directory.Exists(fullPath));
        Assert.IsFalse(File.Exists(fullFilePath));
    }
}
