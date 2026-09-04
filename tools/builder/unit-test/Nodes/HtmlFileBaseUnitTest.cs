namespace ereadian.builder.UnitTest;

using System.Text;
using ereadian.builder.html;
using ereadian.builder.html.Nodes;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class HtmlFileBaseUnitTest
{
    [TestMethod]
    [DataRow(true, DisplayName = "Allow comment")]
    [DataRow(false, DisplayName = "Not allow comment")]
    public void HtmlFileBase_Constructor_AllPropertiesAreSet(bool allowComment)
    {
        // Arrange
        Random random = Random.Shared;
        int elementCount = random.Next(5, 10);
        StringBuilder builder = new();

        List<IHtmlNode> expected = new(elementCount);
        for (int i = 0; i < elementCount; i++)
        {

            string name = TestUtility.CreateUniqueName($"element{i}");
            _ = builder.Append('<').Append(name).Append(" />");
            expected.Add(new ElementNode(name, [], [], true));
        }

        string folderName = TestUtility.CreateUniqueName("TestFolder");
        using TemporaryFolder temporaryFolder = new(folderName);
        string fileName = $"{TestUtility.CreateUniqueName("file")}.html";
        string fullPath = Path.Combine(temporaryFolder.FullPath, fileName);
        File.WriteAllText(fullPath, builder.ToString());

        // Act
        HtmlFileBase fileBase = new(fullPath, folderName, allowComment);

        // Assert
        Assert.AreEqual(allowComment, fileBase.AllowComment);
        Assert.IsTrue(TestUtility.AreNodeListsEqual(expected, fileBase.Nodes));
    }
}