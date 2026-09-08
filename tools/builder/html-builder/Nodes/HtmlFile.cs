namespace ereadian.builder.html.Nodes;

using System.Text;

public class HtmlFile : HtmlFileBase
{
    public HtmlFile(string fullPath, string folder, bool allowComment) : base(fullPath, folder, allowComment)
    {
        string? templateName = null;
        if (this.Variables.TryGetValue(VariableNames.TemplateName, out object? value))
        {
            templateName = value as string;
        }

        this.TemplateName = templateName is null ? string.Empty : templateName.Trim();
    }

    public string TemplateName { get; }

    public void Render(in IDictionary<string, object> globalVariables)
    {
        StringBuilder builder = new(4196);
        Dictionary<string, object> variables = new(globalVariables);
        variables.Append(this.Variables);


        if (string.IsNullOrEmpty(this.TemplateName))
        {
            this.Render(builder, variables);
        }
        else
        {
            Dictionary<string, HtmlTemplate> templates;
            if (globalVariables.TryGetValue(VariableNames.TemplateCollection, out object? rawTemplates))
            {
                templates = (Dictionary<string, HtmlTemplate>)rawTemplates;
            }
            else
            {
                templates = new Dictionary<string, HtmlTemplate>();
                globalVariables.Add(VariableNames.TemplateCollection, templates);
            }

            if (templates.TryGetValue(this.TemplateName, out HtmlTemplate? template) || (template is null))
            {
                string templateFolder = (string)globalVariables[VariableNames.TemplateFolder];
                template = new HtmlTemplate(Path.Combine(templateFolder, $"{this.TemplateName}.html"), templateFolder, this.AllowComment);
                templates[this.TemplateName] = template;
            }

            Dictionary<string, List<ElementNode>> elementMapping = [];
            ElementNode? htmlElement = Utility.GetElementNodes(this.Nodes).FirstOrDefault(element => element.Name == "html");
            if (htmlElement != null)
            {
                IReadOnlyList<ElementNode> rootElements = Utility.GetElementNodes(htmlElement.Children);
                foreach(ElementNode element in rootElements)
                {
                    if (!elementMapping.TryGetValue(element.Name, out List<ElementNode>? contents))
                    {
                        contents = [];
                        elementMapping.Add(element.Name, contents);
                    }

                    contents.Add(element);
                }
            }

            variables[VariableNames.RootElementsToInject] = elementMapping;
            variables.Append(template.Variables);
            template.Render(builder, variables);
        }

        string outputRootFolder = (string)variables[VariableNames.OutputRootFolder];
        string relativeFolder = (string)variables[VariableNames.CurrentFolder];
        string fileName = (string)Variables[VariableNames.CurrentFileName];
        string outputFolder = Path.Combine(outputRootFolder, relativeFolder);
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string outputFullPath = Path.GetFullPath(Path.Combine(outputFolder, fileName));
        File.WriteAllText(outputFullPath, builder.ToString());
    }
}