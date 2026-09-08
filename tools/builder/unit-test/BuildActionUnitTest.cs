namespace ereadian.builder.UnitTest;

using System.Globalization;
using System.Text;
using ereadian.builder.html;
using ereadian.builder.html.Nodes;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class BuildActionUnitTest
{
    [TestMethod]
    [DataRow("en-us", DisplayName = "English")]
    [DataRow("zh-hans", DisplayName = "Chinese")]
    public void BuildAction_RenderSiteBuildTime_ReturnExpected(string cultureName)
    {
        // Arrange
        ElementNode actionElement = new(
            BuildActions.BuildTypeAttributeName,
            [
                new(BuildActions.BuildTypeAttributeName, nameof(BuildActions.RenderSiteBuildTime))
            ],
            [],
            true);
        DateTime siteBuiltTime = DateTime.UtcNow;
        CultureInfo siteCulture = CultureInfo.GetCultureInfo(cultureName);
        Dictionary<string, object> variables = new()
        {
            { VariableNames.SiteCulture, siteCulture },
            { VariableNames.SiteBuildTime, siteBuiltTime },
        };

        // Act
        StringBuilder builder = new();
        BuildActions.RenderSiteBuildTime(actionElement, builder, variables);

        // Assert
        Assert.AreEqual(siteBuiltTime.ToString(siteCulture), builder.ToString());
    }

    [TestMethod]
    [DataRow(true, DisplayName = "No name specified")]
    [DataRow(false, DisplayName = "with name specified")]
    public void BuildAction_InjectHtmlElement_ReturnExpected(bool withNameSpecified)
    {
        // Arrange
        Random random = Random.Shared;
        int noNameElementCount = random.Next(5, 10);
        int namedElementCount = random.Next(5, 10);
        string prefix = TestUtility.CreateUniqueName("Prefix");
        StringBuilder expected = new(prefix);
        StringBuilder actual = new(prefix);

        int totalElementCount = noNameElementCount + namedElementCount;
        string tagName = TestUtility.CreateUniqueName("Content");
        string contentName = TestUtility.CreateUniqueName("Name");
        List<ElementNode> elements = new(totalElementCount);
        for (int i = 0; i < totalElementCount; i++)
        {
            string content = TestUtility.CreateUniqueName("Data");
            List<AttributeNode> attributes = [];

            if (i < noNameElementCount)
            {
                if (!withNameSpecified)
                {
                    expected.Append(content);
                }
            }
            else
            {
                expected.Append(content);
                if (withNameSpecified)
                {
                    attributes.Add(new AttributeNode("name", contentName));
                }
            }

            elements.Add(new(tagName, attributes, [new LiteratureNode(content)], false));
        }

        string otherElementName = TestUtility.CreateUniqueName("Other");
        Dictionary<string, List<ElementNode>> elementMapping = new()
        {
            {tagName, elements},
            {otherElementName, [ new ElementNode(otherElementName, [], [new LiteratureNode(TestUtility.CreateUniqueName("OtherData"))], false)]},
        };

        // Act
        List<AttributeNode> actionAttributes =
        [
            new(BuildActions.BuildTypeAttributeName, nameof(BuildActions.InjectHtmlElement)),
            new("tag", tagName),
        ];

        if (withNameSpecified)
        {
            actionAttributes.Add(new("name", contentName));
        }

        ElementNode actionElement = new ElementNode(BuildActions.BuildElementName, actionAttributes, [], true);
        Dictionary<string, object> variables = new()
        {
            {VariableNames.RootElementsToInject, elementMapping},
        };

        BuildActions.InjectHtmlElement(actionElement, actual, variables);
    }
}