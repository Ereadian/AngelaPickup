using ereadian.builder.html.Content;

namespace ereadian.builder.html;

public static class Utility
{
    public static Dictionary<string, BuildTag> BuildTags {get;} = GetBuildTags();

    public static void SetFileInfo(Dictionary<string, object> variables, string fullPath, string folder)
    {
        variables[VariableNames.CurrentFileCreationTime] = File.GetCreationTimeUtc(fullPath);
        variables[VariableNames.CurrentFileLastModifiedTime] = File.GetLastWriteTimeUtc(fullPath);
        variables[VariableNames.CurrentFolder] = folder;
        variables[VariableNames.CurrentFileName] = Path.GetFileName(fullPath);
        variables[VariableNames.CurrentFileNameNoExtension] = Path.GetFileNameWithoutExtension(fullPath);
    }

    public static IReadOnlyList<IHtmlNode> LoadNodes(HtmlParserContext context, Dictionary<string, object> variables)
    {
        List<IHtmlNode> nodes = [];
        while (!context.IsEnd())
        {
            int tagStart = context.Content.IndexOf('<', context.CurrentPosition);
            if (tagStart < 0)
            {
                tagStart = context.Content.Length;
            }

            int length = tagStart - context.CurrentPosition;
            if (length > 0)
            {
                PlaintNode node = new PlaintNode(context.Content.Substring(context.CurrentPosition, length));
                nodes.Add(node);
            }

            context.CurrentPosition = tagStart;
            if (context.IsEnd())
            {
                break;
            }

            if (context.StartsWith("<!"))
            {
                const string CDataOpenTag = "<![CDATA[";
                const string CDataCloseTag = "]]>";
                const string DocTypeOpenTag = "<!DOCTYPE";
                const string CommentOpenTag = "<!--";
                const string CommentCloseTag = "-->";
                if (context.StartsWith(CDataOpenTag))
                {
                    nodes.Add(CreateMarkNode(context, CDataOpenTag, CDataCloseTag));
                }
                else if (context.StartsWith(DocTypeOpenTag))
                {
                    nodes.Add(CreateMarkNode(context, DocTypeOpenTag, ">"));
                }
                else if (context.StartsWith(CommentOpenTag))
                {
                    nodes.Add(CreateMarkNode(context, CommentOpenTag, CommentCloseTag));
                }
                else
                {
                    throw new InvalidDataException(
                        $"Unknown segment. File: '{context.FullPath}'.Content:\n{context.Content.Substring(context.CurrentPosition)}");
                }

                continue;
            }
        }

        return nodes;
    }

    private static PlaintNode CreateMarkNode(HtmlParserContext context, string openTag, string closeTag)
    {
        int endTagPosition = context.Content.IndexOf(closeTag, context.CurrentPosition + openTag.Length);
        if (endTagPosition < 0)
        {
            throw new InvalidDataException(
                $"Incomplete segment for '{openTag}'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(context.CurrentPosition)}");
        }

        int endPosition = endTagPosition + closeTag.Length;
        string data = context.Content.Substring(context.CurrentPosition, endPosition - context.CurrentPosition);
        context.CurrentPosition = endPosition;
        return new PlaintNode(data);
    }

    private static Dictionary<string, BuildTag> GetBuildTags()
    {
        string[] names = Enum.GetNames<BuildTag>();
        Dictionary<string, BuildTag> tags = new(names.Length, StringComparer.OrdinalIgnoreCase);
        foreach (string name in names)
        {
            tags.Add(name, Enum.Parse<BuildTag>(name));
        }

        return tags;
    }
}