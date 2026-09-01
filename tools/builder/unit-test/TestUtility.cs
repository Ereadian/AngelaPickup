namespace ereadian.builder.UnitTest;

using System.Text;
using ereadian.builder.html;
using ereadian.builder.html.Nodes;

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
        for (int i = 0; i < count; i++)
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

    public static bool AreAttributeListsEqual(IReadOnlyList<AttributeNode> expected, IReadOnlyList<AttributeNode> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (int i = 0; i < expected.Count; i++)
        {
            AttributeNode expectedAttribute = expected[i];
            AttributeNode actualAttribute = actual[i];
            if ((expectedAttribute.Name != actualAttribute.Name) || (expectedAttribute.Value != actualAttribute.Value))
            {
                return false;
            }
        }

        return true;
    }

    public static bool AreNodesEqual(IHtmlNode expected, IHtmlNode actual)
    {
        if (expected.NodeType != actual.NodeType)
        {
            return false;
        }

        switch (expected.NodeType)
        {
            case NodeType.Dynamic:
                DynamicNode expectedDynamicNode = expected as DynamicNode ?? throw new InvalidCastException();
                DynamicNode actualDynamicNode = actual as DynamicNode ?? throw new InvalidCastException();
                const string DynamicNameAttribute = "name";
                return Utility.GetAttributeValue(expectedDynamicNode.Element.Attributes, DynamicNameAttribute)
                    == Utility.GetAttributeValue(actualDynamicNode.Element.Attributes, DynamicNameAttribute);
            case NodeType.Literature:
                LiteratureNode expectedLiteratureNode = expected as LiteratureNode ?? throw new InvalidCastException();
                LiteratureNode actualLiteratureNode = actual as LiteratureNode ?? throw new InvalidCastException();
                return expectedLiteratureNode.Content == actualLiteratureNode.Content;
            case NodeType.Element:
                ElementNode expectedElementNode = expected as ElementNode ?? throw new InvalidCastException();
                ElementNode actualElementNode = actual as ElementNode ?? throw new InvalidCastException();
                if (expectedElementNode.Name != actualElementNode.Name)
                {
                    return false;
                }

                if (!AreAttributeListsEqual(expectedElementNode.Attributes, actualElementNode.Attributes))
                {
                    return false;
                }


                return AreNodeListsEqual(expectedElementNode.Children, actualElementNode.Children);
        }

        return false;
    }

    public static bool AreNodeListsEqual(IReadOnlyList<IHtmlNode> expected, IReadOnlyList<IHtmlNode> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (int i = 0; i < expected.Count; i++)
        {
            if (!AreNodesEqual(expected[i], actual[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static IReadOnlyList<AttributeNode> CreateAttributes(int count, StringBuilder builder, string namePrefix = "name", string valuePrefix = "value")
    {
        AttributeNode[] attributes = new AttributeNode[count];
        for (int i = 0; i < count; i++)
        {
            string name = TestUtility.CreateUniqueName(namePrefix);
            string value = TestUtility.CreateUniqueName(valuePrefix);
            string data = $"'{value}'";
            _ = builder.Append($" {name}={data}");
            attributes[i] = new AttributeNode(name, data);
        }

        return attributes;
    }
}