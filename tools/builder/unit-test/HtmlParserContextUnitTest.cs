namespace ereadian.builder.UnitTest;

using System.Text;
using ereadian.builder.html;

[TestClass]
public sealed class HtmlParserContextUnitTest
{
    [TestMethod]
    public void HtmlParserContext_Constructor()
    {
        // Arrange
        Random random = Random.Shared;
        int folderDepth = random.Next(2, 5);
        StringBuilder pathBuilder = new();
        for(int depth=0; depth<folderDepth;depth++)
        {
            if (depth > 0)
            {
                _ = pathBuilder.Append(Path.PathSeparator);
            }

            _ = pathBuilder.Append(TestUtility.CreateUniqueName("Folder"));
        }

        string fileName = $"{TestUtility.CreateUniqueName("Test")}.html";
        using TemporaryFile temporaryFile = new (fileName);
        string fullPath = temporaryFile.FullPath;
        string folder = pathBuilder.ToString();
        string content = $"<html>{TestUtility.CreateUniqueName("Data")}</html>";
        File.WriteAllText(fullPath, content);

        // Act
        HtmlParserContext context = new(fullPath, folder);

        // Asset
        Assert.AreEqual(fullPath, context.FullPath);
        Assert.AreEqual(folder, context.Folder);
        Assert.AreEqual(content, context.Content);
        Assert.AreEqual(0, context.CurrentPosition);
    }
}
