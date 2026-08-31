namespace ereadian.builder.UnitTest;

using System.Text;
using ereadian.builder.html;
using ereadian.builder.html.Nodes;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class DynamicNodeUnitTest
{
    [TestMethod]
    public void DynamicNode_Constructor_AllPropertiesAreSet()
    {
        // Arrange
        ElementNode element = new(TestUtility.CreateUniqueName("element"), [], [], true);

        // Act
        DynamicNode node = new(element, BuildActions.RenderSiteBuildTime);

        // Assert
        Assert.AreEqual(NodeType.Dynamic, node.NodeType);
    }

    [TestMethod]
    public void DynamicNode_Render_ReturnExpected()
    {
        // Arrange
        string name = TestUtility.CreateUniqueName("name");
        ElementNode element = new(name, [], [], true);
        ElementNode? actualNode = null;
        StringBuilder? actualBuilder = null;
        IDictionary<string, object>? actualVariables = null;
        DynamicNode node = new(element, BuildActionForTest);

        // Act
        StringBuilder builder = new();
        Dictionary<string, object> variables = [];
        node.Render(builder, variables);

        // Assert
        Assert.IsNotNull(actualNode);
        Assert.AreSame(element, actualNode);
        Assert.IsNotNull(actualBuilder);
        Assert.AreSame(builder, actualBuilder);
        Assert.IsNotNull(actualVariables);
        Assert.AreSame(variables, actualVariables);

        void BuildActionForTest(ElementNode node, StringBuilder builder, IDictionary<string, object> variables)
        {
            actualNode = node;
            actualBuilder = builder;
            actualVariables = variables;
        }
    }
}