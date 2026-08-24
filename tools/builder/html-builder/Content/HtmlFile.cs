namespace ereadian.builder.html.Content;

using System.Text;

public class HtmlFile
{
    public HtmlFile(string fullPath, string folder)
    {
        Dictionary<string, object> variables = [];
        Utility.SetFileInfo(variables, fullPath, folder);
        this.Variables = variables;
    }

    public Dictionary<string, object> Variables {get;}
}