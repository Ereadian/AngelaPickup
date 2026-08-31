namespace ereadian.builder.UnitTest;

using System.Text;
using ereadian.builder.html.Nodes;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class LiteratureNodeUnitTest
{
    [TestMethod]
    public void LiteratureNode_Constructor_AllPropertiesAreSet()
    {
        // Arrange
        string data = TestUtility.CreateUniqueName("data");

        // Act
        LiteratureNode node = new(data);

        // Assert
        Assert.AreEqual(NodeType.Literature, node.NodeType);
    }

    [TestMethod]
    public void AttributeNode_Render_ReturnExpected()
    {
        // Arrange
        string data = TestUtility.CreateUniqueName("data");

        // Act
        LiteratureNode node = new(data);
        StringBuilder builder = new();
        Dictionary<string, object> variables = [];
        node.Render(builder, variables);

        // Assert
        Assert.AreEqual(data, builder.ToString());
    }
}