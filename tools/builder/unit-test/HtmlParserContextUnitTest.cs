namespace ereadian.builder.UnitTest;

using System.Text;
using ereadian.builder.html;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class HtmlParserContextUnitTest
{
    public enum Location
    {
        Start,
        Middle,
        End
    }

    [TestMethod]
    public void HtmlParserContext_Constructor_AllPropertiesAreSet()
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
    [DataRow(Location.Start, DisplayName = "At beginning")]
    [DataRow(Location.Middle, DisplayName = "In the middle")]
    [DataRow(Location.End, DisplayName = "At the end")]
    public void HtmlParserContext_IsEnd(Location positionCode)
    {
        // Arrange
        Random random = Random.Shared;
        string content = TestUtility.CreateUniqueName("content");
        HtmlParserContext context = CreateContext(random, content);
        context.CurrentPosition = positionCode switch
        {
            Location.Start => 0,
            Location.Middle => content.Length / 2,
            _ => content.Length,
        };

        // Act
        bool isEnd = context.IsEnd();

        // Assert
        Assert.AreEqual(positionCode > Location.Middle, isEnd);
    }

    [TestMethod]
    [DataRow(Location.Start, DisplayName = "At beginning")]
    [DataRow(Location.Middle, DisplayName = "In the middle")]
    [DataRow(Location.End, DisplayName = "At the end")]
    public void HtmlParserContext_GetCurrentChar_ReturnExpected(Location positionCode)
    {
        // Arrange
        Random random = Random.Shared;
        int characterCount = random.Next(10, 20);
        StringBuilder builder = new StringBuilder(characterCount);
        HashSet<char> used = new();
        for (int i = 0; i < characterCount; i++)
        {
            char c = (char)('a' + random.Next(characterCount));
            while (used.Contains(c))
            {
                c++;
                if (c > 'z')
                {
                    c = 'a';
                }
            }
            _ = builder.Append(c);
        }

        int position = positionCode switch
        {
            Location.Start => 0,
            Location.Middle => characterCount / 2,
            _ => characterCount - 1
        };

        char expected = builder[position];
        HtmlParserContext context = CreateContext(random, builder.ToString());
        context.CurrentPosition = position;

        // Act
        char actual = context.GetCurrentChar();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(Location.Start, DisplayName = "At beginning")]
    [DataRow(Location.Middle, DisplayName = "In the middle")]
    [DataRow(Location.End, DisplayName = "At the end")]
    public void HtmlParserContext_StartWith_FindExist_ReturnTrue(Location positionCode)
    {
        // Arrange
        Random random = Random.Shared;
        int prefixCharCount = positionCode == Location.Start ? 0 : random.Next(5, 10);
        StringBuilder builder = new();
        AppendRandomString(builder, random, prefixCharCount);
        string searchValue = TestUtility.CreateUniqueName("value");
        _ = builder.Append(searchValue);
        int suffixCharCount = positionCode == Location.End ? 0 : random.Next(5, 10);
        AppendRandomString(builder, random, suffixCharCount);

        HtmlParserContext context = CreateContext(random, builder.ToString());

        // Arrange
        bool expected = context.StartsWith(searchValue, prefixCharCount);

        // Assert
        Assert.IsTrue(expected);
    }

    [TestMethod]
    public void HtmlParserContext_StartWith_NotExist_ReturnFalse()
    {
        // Arrange
        Random random = Random.Shared;
        string searchValue = TestUtility.CreateUniqueName("value");
        int count =random.Next(searchValue.Length * 2);
        StringBuilder builder = new (count);
        _ = builder.Append(searchValue);
        string content = string.Empty;
        do
        {
            AppendRandomString(builder, random, count);
            content = builder.ToString();
        } while(content.IndexOf(searchValue) < 0);

        HtmlParserContext context = CreateContext(random, content);

        // Arrange
        bool expected = context.StartsWith(searchValue, searchValue.Length);

        // Assert
        Assert.IsFalse(expected);
    }


    [TestMethod]
    public void HtmlParserContext_StartWith_NoEnoughSize_ReturnFalse()
    {
        // Arrange
        Random random = Random.Shared;
        string searchValue = TestUtility.CreateUniqueName("value");
        int count =random.Next(5, 10);
        StringBuilder builder = new (count);
        AppendRandomString(builder, random, count);
        _ = builder.Append(searchValue.Substring(0, searchValue.Length / 2));
        HtmlParserContext context = CreateContext(random, builder.ToString());

        // Arrange
        bool expected = context.StartsWith(searchValue, count);

        // Assert
        Assert.IsFalse(expected);
    }

    public static void AppendRandomString(StringBuilder builder, Random random, int count)
    {
        for (int i = 0; i < count; i++)
        {
            _ = builder.Append((char)random.Next(' ', '~' - ' '));
        }
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
