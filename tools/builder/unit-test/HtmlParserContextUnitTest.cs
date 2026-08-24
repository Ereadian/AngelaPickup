namespace ereadian.builder.UnitTest;

using System.Text;
using ereadian.builder.html;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class HtmlParserContextUnitTest
{
    [TestMethod]
    public void HtmlParserContext_Constructor()
    {
        // Arrange
        Random random = Random.Shared;

        string fileNameNoExtension = TestUtility.CreateUniqueName("Test");
        string fileName = $"{fileNameNoExtension}.html";
        using TemporaryFile temporaryFile = new(fileName);
        string fullPath = temporaryFile.FullPath;
        string folder = TestUtility.CreateRandomFolders(random.Next(2, 5));
        string content = $"<html>{TestUtility.CreateUniqueName("Data")}</html>";
        File.WriteAllText(fullPath, content);

        // Act
        HtmlParserContext context = new(fullPath, folder);

        // Asset
        Assert.AreEqual(fullPath, context.FullPath);
        Assert.AreEqual(folder, context.Folder);
        Assert.AreEqual(content, context.Content);
        Assert.AreEqual(0, context.CurrentPosition);

        Dictionary<string, object> variables = context.Variables;
        Assert.IsNotNull(variables[VariableNames.CurrentFileCreationTime]);
        Assert.IsNotNull(variables[VariableNames.CurrentFileLastModifiedTime]);
        Assert.AreEqual(folder, variables[VariableNames.CurrentFolder]);
        Assert.AreEqual(fileName, variables[VariableNames.CurrentFileName]);
        Assert.AreEqual(fileNameNoExtension, variables[VariableNames.CurrentFileNameNoExtension]);
    }

    [TestMethod]
    [DataRow("<", DisplayName = "Symbol")]
    [DataRow("ABC", DisplayName = "Letter")]
    [DataRow(null, DisplayName = "At the end")]
    public void HtmlParserContext_SkipWhiteSpace_HasSpaces_Skipped(string suffix)
    {
        // Arrange
        StringBuilder builder = new();
        string prefix = TestUtility.CreateUniqueName("prefix");
        _ = builder.Append(prefix);

        Random random = Random.Shared;
        int whitespaceCount = random.Next(5, 10);
        TestUtility.AppendRandomWhiteSpaces(builder, random, whitespaceCount);

        if (suffix != null)
        {
            _ = builder.Append(suffix);
        }

        HtmlParserContext context = CreateContext(random, builder.ToString());
        context.CurrentPosition = prefix.Length;

        // Act
        bool isEnd = context.SkipWhiteSpace();

        // Assert
        Assert.AreEqual(prefix.Length + whitespaceCount, context.CurrentPosition);
        Assert.AreEqual(suffix != null, isEnd);
    }

    [TestMethod]
    [DataRow("<", DisplayName = "Symbol")]
    [DataRow("ABC", DisplayName = "Letter")]
    [DataRow(null, DisplayName = "At the end")]
    public void HtmlParserContext_SkipWhiteSpace_NoSpaces_Skipped(string suffix)
    {
        // Arrange
        StringBuilder builder = new();
        string prefix = TestUtility.CreateUniqueName("prefix");
        _ = builder.Append(prefix);

        Random random = Random.Shared;

        if (suffix != null)
        {
            _ = builder.Append(suffix);
        }

        HtmlParserContext context = CreateContext(random, builder.ToString());
        context.CurrentPosition = prefix.Length;

        // Act
        bool isEnd = context.SkipWhiteSpace();

        // Assert
        Assert.AreEqual(prefix.Length, context.CurrentPosition);
        Assert.AreEqual(suffix != null, isEnd);
    }

    [TestMethod]
    [DataRow(0, DisplayName = "At beginning")]
    [DataRow(1, DisplayName = "In the middle")]
    [DataRow(2, DisplayName = "At the end")]
    public void HtmlParserContext_IsEnd(int positionCode)
    {
        // Arrange
        Random random = Random.Shared;
        string content = TestUtility.CreateUniqueName("content");
        HtmlParserContext context = CreateContext(random, content);
        switch(positionCode)
        {
            case 0:
                context.CurrentPosition = 0;
                break;
            case 1:
                context.CurrentPosition = content.Length / 2;
                break;
            default:
                context.CurrentPosition = content.Length;
                break;
        }

        // Act
        bool isEnd = context.IsEnd();

        // Assert
        Assert.AreEqual(positionCode > 1, isEnd);
    }

    private static HtmlParserContext CreateContext(Random random, string content)
    {
        string fileNameNoExtension = TestUtility.CreateUniqueName("Test");
        string fileName = $"{fileNameNoExtension}.html";
        using TemporaryFile temporaryFile = new(fileName);
        string fullPath = temporaryFile.FullPath;
        string folder = TestUtility.CreateRandomFolders(random.Next(2, 5));
        File.WriteAllText(fullPath, content);

        return new HtmlParserContext(fullPath, folder);
    }
}
