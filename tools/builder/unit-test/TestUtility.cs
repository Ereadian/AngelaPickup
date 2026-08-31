namespace ereadian.builder.UnitTest;

using System.Text;
using ereadian.builder.html;

[ExcludeFromCodeCoverage]
public static class TestUtility
{
    public static string CreateUniqueName(string? prefix = "Data")
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }

    public static string CreateRandomFolders(int folderDepth)
    {
        StringBuilder builder = new();
        for (int depth = 0; depth < folderDepth; depth++)
        {
            if (depth > 0)
            {
                _ = builder.Append(Path.PathSeparator);
            }

            _ = builder.Append(TestUtility.CreateUniqueName("Folder"));
        }

        return builder.ToString();
    }

    public static void AppendRandomWhiteSpaces(StringBuilder builder, Random random, int count)
    {
        const string WhiteSpaces = " \n\r\t";
        for (int i=0; i<count;i++)
        {
            _ = builder.Append(WhiteSpaces[random.Next(WhiteSpaces.Length)]);
        }
    }

    public static string CreateRandomWhiteSpaces(Random random, int count)
    {
        StringBuilder builder = new(count);
        AppendRandomWhiteSpaces(builder, random, count);
        return builder.ToString();
    }


    public static HtmlParserContext CreateContext(Random random, string content, bool allowComment = true)
    {
        string fileNameNoExtension = TestUtility.CreateUniqueName("Test");
        string fileName = $"{fileNameNoExtension}.html";
        using TemporaryFile temporaryFile = new(fileName);
        string fullPath = temporaryFile.FullPath;
        string folder = TestUtility.CreateRandomFolders(random.Next(2, 5));
        File.WriteAllText(fullPath, content);

        return new HtmlParserContext(fullPath, folder, allowComment);
    }
}