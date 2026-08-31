using System.Text;
using ereadian.builder.html;
using ereadian.builder.html.Nodes;

namespace ereadian.builder.UnitTest;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class UtilityUnitTest
{
    [TestMethod]
    public void Utility_RenderNodes_RenderNodes_ReturnExpected()
    {
        // Arrange
        Random random = Random.Shared;
        int nodeCount = random.Next(5, 10);
        StringBuilder expected = new();
        Dictionary<string, object> variables = [];
        List<IHtmlNode> nodes = [];
        for (int i = 0; i < nodeCount; i++)
        {
            string name = TestUtility.CreateUniqueName($"var{i}");
            string value = TestUtility.CreateUniqueName($"value{i}");
            string content = TestUtility.CreateUniqueName($"data{i}");
            _ = expected.Append(content).Append(value);
            variables.Add(name, value);
            nodes.Add(new NodeForTest(name, content));
        }

        // Act
        StringBuilder actual = new();
        Utility.RenderNodes(nodes, actual, variables);

        // Assert
        Assert.AreEqual(expected.ToString(), actual.ToString());
    }

    [TestMethod]
    public void Utility_GetAttributeValue_ReturnExpected()
    {
        // Arrange
        Random random = Random.Shared;
        int attributeCount = random.Next(5, 10);
        string[] names = new string[attributeCount];
        string[] values = new string[attributeCount];
        List<AttributeNode> attributes = [];
        for (int i = 0; i < attributeCount; i++)
        {
            string name = names[i] = TestUtility.CreateUniqueName("name");
            string value = values[i] = TestUtility.CreateUniqueName("value");
            attributes.Add(new AttributeNode(name, $"'{value}'"));
        }

        // Act
        int position = random.Next(attributeCount);
        string actual = Utility.GetAttributeValue(attributes, names[position]);

        // Assert
        Assert.AreEqual(values[position], actual);
    }

    [TestMethod]
    public void Utility_GetAttributeValue_NotFoundReturnEmpty()
    {
        // Arrange
        List<AttributeNode> attributes = [ new AttributeNode("name", "'Value'") ];

        // Act
        string actual = Utility.GetAttributeValue(attributes, "unknown");

        // Assert
        Assert.AreEqual(string.Empty, actual);
    }

    private class NodeForTest(string name, string content) : IHtmlNode
    {
        public NodeType NodeType => NodeType.Dynamic;

        public void Render(in StringBuilder builder, in IDictionary<string, object> variables)
        {
            _ = builder.Append(content).Append(variables[name]);
        }
    }
}
