namespace ereadian.builder.UnitTest;

using System.Text;
using System.Web;
using ereadian.builder.html.Nodes;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class AttributeNodeUnitTest
{
    [TestMethod]
    public void AttributeNode_Constructor_AllPropertiesAreSet()
    {
        // Arrange
        string name = TestUtility.CreateUniqueName("name");
        string data = $"'{TestUtility.CreateUniqueName("data")}'";

        // Act
        AttributeNode attribute = new(name, data);

        // Assert
        Assert.AreEqual(name, attribute.Name);
        Assert.AreEqual(data, attribute.Data);
        Assert.AreEqual(NodeType.Attribute, attribute.NodeType);
    }

    [TestMethod]
    [DataRow('"', DisplayName = "Double Quote")]
    [DataRow('\'', DisplayName = "Single Quote")]
    public void AttributeNode_Value_ReturnExpected(char quotationMark)
    {
        // Arrange
        string name = TestUtility.CreateUniqueName("name");
        string value = $"{TestUtility.CreateUniqueName("prefix")}\"{TestUtility.CreateUniqueName("suffix")}";
        string data = $"{quotationMark}{HttpUtility.HtmlEncode(value)}{quotationMark}";

        // Act
        AttributeNode attribute = new(name, data);

        // Assert
        Assert.AreEqual(value, attribute.Value);
    }

    [TestMethod]
    public void AttributeNode_Render_ReturnExpected()
    {
        // Arrange
        string name = TestUtility.CreateUniqueName("name");
        string data = $"'{TestUtility.CreateUniqueName("data")}'";

        // Act
        AttributeNode attribute = new(name, data);
        StringBuilder builder = new();
        Dictionary<string, object> variables = new();
        attribute.Render(builder, variables);

        // Assert
        Assert.AreEqual($"{name}={data}", builder.ToString());
    }
}