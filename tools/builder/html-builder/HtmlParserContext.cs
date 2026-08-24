namespace ereadian.builder.html;

using System.Globalization;
using System.Text;
using ereadian.builder.html.Content;

public class HtmlParserContext(string fullPath, string folder)
{
    public string FullPath => fullPath;
    public string Folder => folder;
    public string Content {get;} = File.ReadAllText(fullPath);
    public int CurrentPosition {get; set;} = 0;

    public void SetFileInfo(Dictionary<string, object> variables)
    {
        Utility.SetFileInfo(variables, fullPath, folder);
    }

    /// <summary>
    /// Process whitespace.
    /// </summary>
    /// <param name="builder">the build to save the whitespace(s). No action if it is null.</param>
    /// <returns>True if meets none-whitespace character. False if reaches end.</returns>
    public bool ProcessWhiteSpace(StringBuilder? builder = null)
    {
        while(this.CurrentPosition < this.Content.Length)
        {
            char c = this.Content[this.CurrentPosition];
            if (!char.IsWhiteSpace(c))
            {
                return true;
            }

            if (builder is not null)
            {
                _ = builder.Append(c);
            }

            this.CurrentPosition++;
        }

        return false;
    }
}