namespace ereadian.builder.UnitTest;

using System.Text;
using ereadian.builder.html;
using ereadian.builder.html.Nodes;

[TestClass]
[ExcludeFromCodeCoverage]
public sealed class HtmlFileUnitTest
{
    [TestMethod]
    [DataRow(true, DisplayName = "Allow comment")]
    [DataRow(false, DisplayName = "Not allow comment")]
    public void HtmlFile_Constructor_AllPropertiesAreSet(bool allowComment)
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
        HtmlFile file = new(fullPath, folderName, allowComment);

        // Assert
        Assert.AreEqual(allowComment, file.AllowComment);
        Assert.IsTrue(TestUtility.AreNodeListsEqual(expected, file.Nodes));
    }

    [TestMethod]
    [DataRow(true, DisplayName = "Has template")]
    [DataRow(false, DisplayName = "No template")]
    public void HtmlFile_Template_ReturnExpected(bool requestTemplate)
    {
        // Arrange
        Random random = Random.Shared;
        StringBuilder builder = new(TestUtility.CreateUniqueName("prefix"));
        string templateName = TestUtility.CreateUniqueName("Template");

        if (requestTemplate)
        {
            _ = builder.Append("<template name='").Append(templateName).Append("' />");
        }

        _ = builder.Append(TestUtility.CreateUniqueName("suffix")); ;


        string folderName = TestUtility.CreateUniqueName("TestFolder");
        using TemporaryFolder temporaryFolder = new(folderName);
        string fileName = $"{TestUtility.CreateUniqueName("file")}.html";
        string fullPath = Path.Combine(temporaryFolder.FullPath, fileName);
        File.WriteAllText(fullPath, builder.ToString());

        // Act
        HtmlFile file = new(fullPath, folderName, true);

        // Assert
        Assert.AreEqual(requestTemplate ? templateName : string.Empty, file.TemplateName);
    }

    [TestMethod]
    public void HtmlFile_Render_WithoutTemplate_ReturnExpected()
    {
        // Arrange
        Random random = Random.Shared;
        int elementCount = random.Next(5, 10);
        StringBuilder builder = new();

        for (int i = 0; i < elementCount; i++)
        {

            string name = TestUtility.CreateUniqueName($"element{i}");
            _ = builder.Append('<').Append(name).Append("/>");
        }

        string testFolderName = TestUtility.CreateUniqueName("TestFolder");
        using TemporaryFolder temporaryFolder = new(testFolderName);
        string sourceRootFolder = Path.Combine(temporaryFolder.FullPath, TestUtility.CreateUniqueName("source"));
        Directory.CreateDirectory(sourceRootFolder);
        string targetRootFolder = Path.Combine(temporaryFolder.FullPath, TestUtility.CreateUniqueName("target"));
        Directory.CreateDirectory(targetRootFolder);
        string currentFolder = TestUtility.CreateUniqueName("folder");
        Directory.CreateDirectory(Path.Combine(sourceRootFolder, currentFolder));

        string fileName = $"{TestUtility.CreateUniqueName("file")}.html";
        string sourceFullPath = Path.Combine(sourceRootFolder, currentFolder, fileName);
        File.WriteAllText(sourceFullPath, builder.ToString());

        HtmlFile file = new(sourceFullPath, currentFolder, true);

        // Act
        Dictionary<string, object> globalVariables = new()
        {
            {VariableNames.OutputRootFolder, targetRootFolder },
        };

        file.Render(globalVariables);

        // Assert
        string targetFullPath = Path.Combine(targetRootFolder, currentFolder, fileName);
        Assert.IsTrue(File.Exists(targetFullPath));
        string actual = File.ReadAllText(targetFullPath);
        Assert.AreEqual(builder.ToString(), actual);
    }

    [TestMethod]
    public void HtmlFile_Render_WithTemplate_ReturnExpected()
    {
        // Arrange
        Random random = Random.Shared;
        int elementCount = random.Next(5, 10);
        int childCount = random.Next(5, 10);

        const string HtmlStartTag = "<html>";
        string prefix = TestUtility.CreateUniqueName("prefix") + HtmlStartTag;
        StringBuilder templateHtmlBuilder = new(prefix);
        StringBuilder expectedHtmlBuilder = new(prefix);
        string templateName = TestUtility.CreateUniqueName("template");
        StringBuilder fileHtmlBuilder = new(HtmlStartTag);
        _ = fileHtmlBuilder.Append("<template name='").Append(templateName).Append("'/>");

        string suffix;
        for (int rootElementIndex = 0; rootElementIndex < elementCount; rootElementIndex++)
        {
            prefix = TestUtility.CreateUniqueName("segment_start");
            string tagName = TestUtility.CreateUniqueName($"element{rootElementIndex}");
            _ = fileHtmlBuilder.Append('<').Append(tagName).Append('>');
            _ = expectedHtmlBuilder.Append('<').Append(tagName).Append('>').Append(prefix);
            for (int childIndex = 0; childIndex < childCount; childIndex++)
            {
                string content = $"<div>{TestUtility.CreateUniqueName($"content_{rootElementIndex}_{childIndex}")}</div>";
                _ = expectedHtmlBuilder.Append(content);
                _ = fileHtmlBuilder.Append(content);
            }

            _ = fileHtmlBuilder.Append("</").Append(tagName).Append('>');

            suffix = TestUtility.CreateUniqueName("segment_end");
            _ = expectedHtmlBuilder.Append(suffix).Append("</").Append(tagName).Append('>');

            _ = templateHtmlBuilder.Append('<').Append(tagName).Append('>')
                .Append(prefix)
                .Append('<').Append(BuildActions.BuildElementName).Append(' ').Append(BuildActions.BuildTypeAttributeName).Append("='").Append(nameof(BuildActions.InjectHtmlElement)).Append("' tag='").Append(tagName).Append("'/>")
                .Append(suffix)
                .Append("</").Append(tagName).Append('>');
        }

        const string HtmlEndTag = "</html>";
        _ = fileHtmlBuilder.Append(HtmlEndTag);
        suffix = TestUtility.CreateUniqueName("suffix") + HtmlEndTag;
        _ = templateHtmlBuilder.Append(suffix);
        _ = expectedHtmlBuilder.Append(suffix);

        string testFolderName = TestUtility.CreateUniqueName("TestFolder");
        using TemporaryFolder temporaryFolder = new(testFolderName);

        string sourceRootFolder = Path.Combine(temporaryFolder.FullPath, TestUtility.CreateUniqueName("source"));
        Directory.CreateDirectory(sourceRootFolder);
        string currentFolder = TestUtility.CreateUniqueName("folder");
        Directory.CreateDirectory(Path.Combine(sourceRootFolder, currentFolder));
        string fileName = $"{TestUtility.CreateUniqueName("file")}.html";
        string sourceFullPath = Path.Combine(sourceRootFolder, currentFolder, fileName);
        File.WriteAllText(sourceFullPath, fileHtmlBuilder.ToString());

        string templateFolder = Path.Combine(temporaryFolder.FullPath, "template");
        Directory.CreateDirectory(templateFolder);
        File.WriteAllText(Path.Combine(templateFolder, $"{templateName}.html"), templateHtmlBuilder.ToString());

        string targetRootFolder = Path.Combine(temporaryFolder.FullPath, TestUtility.CreateUniqueName("target"));
        Directory.CreateDirectory(targetRootFolder);

        HtmlFile file = new(sourceFullPath, currentFolder, true);

        // Act
        Dictionary<string, object> globalVariables = new()
        {
            {VariableNames.OutputRootFolder, targetRootFolder },
            {VariableNames.TemplateFolder, templateFolder},
        };

        file.Render(globalVariables);

        // Assert
        Assert.AreEqual(templateName, file.TemplateName);

        string targetFullPath = Path.Combine(targetRootFolder, currentFolder, fileName);
        Assert.IsTrue(File.Exists(targetFullPath));
        string actual = File.ReadAllText(targetFullPath);
        Assert.AreEqual(expectedHtmlBuilder.ToString(), actual);
    }
}