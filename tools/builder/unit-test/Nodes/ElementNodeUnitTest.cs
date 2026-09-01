namespace ereadian.builder.UnitTest;

using System.Text;
using ereadian.builder.html;
using ereadian.builder.html.Nodes;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class ElementNodeUnitTest
{
    [TestMethod]
    public void ElementNode_Constructor_Short_AllPropertiesAreSet()
    {
        // Arrange
        Random random = Random.Shared;
        StringBuilder builder = new();

        string name = TestUtility.CreateUniqueName("element");
        _ = builder.Append('<').Append(name);

        int attributeCount = random.Next(5, 10);
        IReadOnlyList<AttributeNode> attributes = TestUtility.CreateAttributes(attributeCount, builder);
        _ = builder.Append('>');

        // Act
        ElementNode element = new(name, attributes, [], false);

        // Assert
        Assert.AreEqual(name, element.Name);
        Assert.AreEqual(attributes, element.Attributes);
        Assert.IsEmpty(element.Children);
    }

    [TestMethod]
    public void ElementNode_Constructor_HasChildren_AllPropertiesAreSet()
    {
        // Arrange
        Random random = Random.Shared;
        StringBuilder builder = new();

        string name = TestUtility.CreateUniqueName("element");
        _ = builder.Append('<').Append(name);

        int attributeCount = random.Next(5, 10);
        IReadOnlyList<AttributeNode> attributes = TestUtility.CreateAttributes(attributeCount, builder);
        _ = builder.Append('>');

        int childCount = random.Next(5, 10);
        IReadOnlyList<IHtmlNode> children = CreateLiteratureNodes(childCount, builder);

        // Act
        ElementNode element = new(name, attributes, children, false);

        // Assert
        Assert.AreEqual(name, element.Name);
        Assert.AreEqual(attributes, element.Attributes);
        Assert.AreSame(children, element.Children);
    }

    [TestMethod]
    public void ElementNode_Render_ReturnExpected()
    {
        // Arrange
        Random random = Random.Shared;
        StringBuilder builder = new();

        string name = TestUtility.CreateUniqueName("element");
        _ = builder.Append('<').Append(name);

        int attributeCount = random.Next(5, 10);
        IReadOnlyList<AttributeNode> attributes = TestUtility.CreateAttributes(attributeCount, builder);
        _ = builder.Append('>');

        int childCount = random.Next(5, 10);
        IReadOnlyList<IHtmlNode> children = CreateLiteratureNodes(childCount, builder);

        _ = builder.Append("</").Append(name).Append('>');

        // Act
        ElementNode element = new(name, attributes, children, false);
        StringBuilder result = new();
        Dictionary<string, object> variables = [];
        element.Render(result, variables);

        // Assert
        Assert.AreEqual(builder.ToString(), result.ToString());
    }

    [TestMethod]
    public void ElementNode_Parse_Empty_ReturnExpected()
    {
        // Arrange
        string name = TestUtility.CreateUniqueName("element");
        string content = $"<{name}/>";
        HtmlParserContext context = TestUtility.CreateContext(Random.Shared, content);

        // Act
        ElementNode element = ElementNode.Parse(context);

        // Assert
        Assert.AreEqual(name, element.Name);
        Assert.IsEmpty(element.Attributes);
        Assert.IsEmpty(element.Children);
    }

    [TestMethod]
    public void ElementNode_Parse_Short_ReturnExpected()
    {
        // Arrange
        Random random = Random.Shared;
        StringBuilder builder = new();

        string name = TestUtility.CreateUniqueName("element");
        _ = builder.Append('<').Append(name);

        IReadOnlyList<AttributeNode> attributes = TestUtility.CreateAttributes(random.Next(5, 10), builder);
        _ = builder.Append("/>");
        HtmlParserContext context = TestUtility.CreateContext(Random.Shared, builder.ToString());

        // Act
        ElementNode element = ElementNode.Parse(context);

        // Assert
        Assert.AreEqual(name, element.Name);
        Assert.IsEmpty(element.Children);
        TestUtility.AreAttributeListsEqual(attributes, element.Attributes);
    }

    [TestMethod]
    public void ElementNode_Parse_ContainsChildren_ReturnExpected()
    {
        // Arrange
        Random random = Random.Shared;
        StringBuilder builder = new();

        string name = TestUtility.CreateUniqueName("element");
        _ = builder.Append('<').Append(name);

        IReadOnlyList<AttributeNode> attributes = TestUtility.CreateAttributes(random.Next(5, 10), builder);
        _ = builder.Append('>');

        int childCount = random.Next(5, 10);
        IReadOnlyList<IHtmlNode> children = CreateLiteratureNodes(childCount, builder);

        _ = builder.Append("</").Append(name).Append('>');

        HtmlParserContext context = TestUtility.CreateContext(Random.Shared, builder.ToString());

        // Act
        ElementNode element = ElementNode.Parse(context);

        // Assert
        Assert.AreEqual(name, element.Name);
        TestUtility.AreAttributeListsEqual(attributes, element.Attributes);
        TestUtility.AreNodeListsEqual(children, element.Children);
    }

    private static LiteratureNode CreateLiteratureNode(string prefix = "data")
    {
        string content = $"<![CDATA[{TestUtility.CreateUniqueName(prefix)}]]>";
        return new LiteratureNode(content);
    }

    private static IReadOnlyList<IHtmlNode> CreateLiteratureNodes(int count, StringBuilder builder)
    {
        IHtmlNode[] nodes = new IHtmlNode[count];
        for (int i=0; i<count;i++)
        {
            LiteratureNode node = CreateLiteratureNode($"Literature_{i}");
            nodes[i] = node;
            _ = builder.Append(node.Content);
        }

        return nodes;
    }
}