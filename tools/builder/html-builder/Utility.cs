namespace ereadian.builder.html;

public static class Utility
{
    public static Dictionary<string, BuildTag> GetBuildTags()
    {
        string[] names = Enum.GetNames<BuildTag>();
        Dictionary<string, BuildTag> tags = new(names.Length, StringComparer.OrdinalIgnoreCase);
        foreach(string name in names)
        {
            tags.Add(name, Enum.Parse<BuildTag>(name));
        }

        return tags;
    }

    public static void SetFileInfo(Dictionary<string, object> variables, string fullPath, string folder)
    {
        variables[VariableNames.CurrentFileCreationTime] = File.GetCreationTimeUtc(fullPath);
        variables[VariableNames.CurrentFileLastModifiedTime] = File.GetLastWriteTimeUtc(fullPath);
        variables[VariableNames.CurrentFolder] = folder;
        variables[VariableNames.CurrentFileName] = Path.GetFileName(fullPath);
        variables[VariableNames.CurrentFileNameNoExtension] = Path.GetFileNameWithoutExtension(fullPath);
    }

}