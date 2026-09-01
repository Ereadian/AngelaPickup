using System.Reflection;
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
        List<AttributeNode> attributes = [new AttributeNode("name", "'Value'")];

        // Act
        string actual = Utility.GetAttributeValue(attributes, "unknown");

        // Assert
        Assert.AreEqual(string.Empty, actual);
    }

    [TestMethod]
    public void Utility_LoadNodes_EmptyContentReturnEmpty()
    {
        // Arrange
        HtmlParserContext context = TestUtility.CreateContext(Random.Shared, string.Empty);

        // Act
        IReadOnlyList<IHtmlNode> nodes = Utility.LoadNodes(context);

        // Assert
        Assert.IsEmpty(nodes);
    }

    [TestMethod]
    [DataRow("   ", "\n", true, true, DisplayName = "Literature")]
    [DataRow("<![CDATA[", "]]>", true, true, DisplayName = "CData")]
    [DataRow("<!DOCTYPE", ">", true, true, DisplayName = "Doc Type")]
    [DataRow("<!--", "-->", true, true, DisplayName = "Comment (allowed)")]
    [DataRow("<!--", "-->", false, false, DisplayName = "Comment (not allowed)")]
    public void Utility_LoadNodes_SingleLiteratureNode_ReturnExpected(string startTag, string endTag, bool allowComment, bool generated)
    {
        // Arrange
        string content = $"{startTag}{TestUtility.CreateUniqueName("content")}{endTag}";
        HtmlParserContext context = TestUtility.CreateContext(Random.Shared, content, allowComment);

        // Act
        IReadOnlyList<IHtmlNode> nodes = Utility.LoadNodes(context);

        // Assert
        if (generated)
        {
            StringBuilder builder = new();
            Dictionary<string, object> variables = [];
            Utility.RenderNodes(nodes, builder, variables);
            Assert.AreEqual(content, builder.ToString());
        }
        else
        {
            Assert.IsEmpty(nodes);
        }
    }

    [TestMethod]
    public void Utility_LoadNodes_SingleVariableNode_ReturnExpected()
    {
        // Arrange
        string name = TestUtility.CreateUniqueName("name");
        string value = TestUtility.CreateUniqueName("value");
        string content = $"<variable name='{name}' value='{value}' />";
        HtmlParserContext context = TestUtility.CreateContext(Random.Shared, content, true);

        // Act
        IReadOnlyList<IHtmlNode> nodes = Utility.LoadNodes(context);

        // Asset
        Assert.IsEmpty(nodes);
        Assert.AreEqual(value, context.Variables[name]);
    }

    [TestMethod]
    public void Utility_LoadNodes_SingleTemplateNode_ReturnExpected()
    {
        // Arrange
        string name = TestUtility.CreateUniqueName("name");
        string content = $"<template name='{name}'/>";
        HtmlParserContext context = TestUtility.CreateContext(Random.Shared, content, true);

        // Act
        IReadOnlyList<IHtmlNode> nodes = Utility.LoadNodes(context);

        // Asset
        Assert.IsEmpty(nodes);
        Assert.AreEqual(name, context.Variables[VariableNames.TemplateName]);
    }

    [TestMethod]
    public void Utility_LoadNodes_SingleBuildNode_ReturnExpected()
    {
        // Arrange
        List<string> buildNames = BuildActions.Actions.Keys.ToList();
        IReadOnlyList<IHtmlNode>[] allNodes = new IReadOnlyList<IHtmlNode>[buildNames.Count];

        // Act
        for (int i = 0; i < buildNames.Count; i++)
        {
            string content = $"<build name='{buildNames[i]}' />";
            HtmlParserContext context = TestUtility.CreateContext(Random.Shared, content, true);
            allNodes[i] = Utility.LoadNodes(context);
        }

        // Assert
        for (int i = 0; i < buildNames.Count; i++)
        {
            IReadOnlyList<IHtmlNode> nodes = allNodes[i];
            Assert.HasCount(1, nodes);
            Assert.IsInstanceOfType<DynamicNode>(nodes[0]);
        }
    }

    [TestMethod]
    public void Utility_LoadNodes_NestedElementNodes_ReturnExpected()
    {
        // Arrange
        Random random = Random.Shared;
        int depth = random.Next(2, 5);
        int width = random.Next(2, 5);
        int attributeCount = random.Next(5, 10);
        StringBuilder builder = new();
        List<IHtmlNode> expected = CreateElements(builder, depth, width, attributeCount);

        // Act
        HtmlParserContext context = TestUtility.CreateContext(Random.Shared, builder.ToString(), true);
        IReadOnlyList<IHtmlNode> actual = Utility.LoadNodes(context);

        // Assert
        Assert.IsTrue(TestUtility.AreNodeListsEqual(expected, actual));
    }

    private static List<IHtmlNode> CreateElements(StringBuilder builder, int layerId, int childCount, int attributeCount)
    {
        List<IHtmlNode> nodes = [];
        for (int i = 0; i < childCount; i++)
        {
            nodes.Add(CreateElement(builder, layerId, i, childCount, attributeCount));
        }

        return nodes;
    }

    private static ElementNode CreateElement(StringBuilder builder, int layerId, int siblingId, int childCount, int attributeCount)
    {
        string elementName = TestUtility.CreateUniqueName($"Element_{layerId}_{siblingId}");
        _ = builder.Append('<').Append(elementName);
        List<AttributeNode> attributeNodes = [];
        for (int i = 0; i < attributeCount; i++)
        {
            string suffix = $"{layerId}_{siblingId}_{i}";
            string attributeName = TestUtility.CreateUniqueName($"attribute_name_{suffix}");
            string attributeValue = TestUtility.CreateUniqueName($"attribute_value_{suffix}");
            string attributeData = $"'{attributeValue}'";
            _ = builder.Append($" {attributeName}={attributeData}");
            attributeNodes.Add(new AttributeNode(attributeName, attributeData));
        }

        if (--layerId < 0)
        {
            _ = builder.Append(" />");
            return new ElementNode(elementName, attributeNodes, [], true);
        }

        _ = builder.Append('>');
        List<IHtmlNode> children = CreateElements(builder, layerId, childCount, attributeCount);
        _ = builder.Append("</").Append(elementName).Append('>');
        return new ElementNode(elementName, attributeNodes, children, false);
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
