namespace ereadian.builder.html;

public class HtmlParserContext
{
    public Dictionary<string, object> Variables {get;}
    public string FullPath {get;}
    public string Folder {get;}
    public string Content {get;}
    public int CurrentPosition {get; set;} = 0;

    public HtmlParserContext(string fullPath, string folder)
    {
        this.FullPath = fullPath;
        this.Folder = folder;
        this.Content = File.ReadAllText(fullPath);
        this.Variables = [];
        Utility.SetFileInfo(this.Variables, fullPath, folder);
    }

    public bool IsEnd()
    {
        return this.CurrentPosition >= this.Content.Length; 
    }

    public char GetCurrentChar()
    {
        return this.Content[this.CurrentPosition];
    }

    /// <summary>
    /// Process whitespace.
    /// </summary>
    /// <returns>True if meets none-whitespace character. False if reaches end.</returns>
    public bool SkipWhiteSpace()
    {
        while(!this.IsEnd())
        {
            char c = this.GetCurrentChar();
            if (!char.IsWhiteSpace(c))
            {
                return true;
            }

            this.CurrentPosition++;
        }

        return false;
    }
}