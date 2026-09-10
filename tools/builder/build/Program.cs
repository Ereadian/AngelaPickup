namespace ereadian.builder.site;

using System.Reflection;
using ereadian.builder.html;
using ereadian.builder.html.Nodes;

internal class Program
{
    private const string DefaultSourceFolder = "docs";
    private const string DefaultOutputFolder = "page-output";
    private const string DefaultTemplateFolder = "templates";
    private const string DefaultSharedContentFolder = "shared";
    private const string HtmlFileExtension = ".html";

    private static readonly string RepositoryRootFolder;

#if DEBUG
    private const bool EnableOverride = true;
#else
    private const bool EnableOverride = false;    
#endif

    static Program()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string assemblyFile = assembly.Location;
        string assemblyFolder = Path.GetDirectoryName(assemblyFile) ?? string.Empty;
        RepositoryRootFolder = Path.GetFullPath(Path.Combine(assemblyFolder, "../../../../../.."));
    }

    internal static int Main(string[] arguments)
    {
        string sourceFolder = arguments.Length < 1 ? GetRepositorySubFolder(DefaultSourceFolder) : Path.GetFullPath(arguments[0]);
        string targetFolder = arguments.Length < 2 ? GetRepositorySubFolder(DefaultOutputFolder) : Path.GetFullPath(arguments[1]);
        string templateFolder = arguments.Length < 3 ? GetRepositorySubFolder(DefaultTemplateFolder) : Path.GetFullPath(arguments[2]);
        string sharedFolder = arguments.Length < 4 ? GetRepositorySubFolder(DefaultSharedContentFolder) : Path.GetFullPath(arguments[3]);

        Transformer transformer = new(targetFolder, templateFolder, "zh-Hans", false);
        Console.WriteLine("Start building.");
        Console.WriteLine("\tSource: {0}", sourceFolder);
        Console.WriteLine("\tTarget: {0}", targetFolder);
        Console.WriteLine("\tTemplate: {0}", templateFolder);
        Console.WriteLine("\tCulture: {0}", transformer.SiteCultureInfo.DisplayName);
        Dictionary<string, List<IHtmlNode>> sharedContents = [];
        Process(transformer, sourceFolder, targetFolder, string.Empty ,sharedFolder, sharedContents);
        Console.WriteLine("Build completed.");
        return 0;
    }

    private static void Process(
        Transformer transformer,
        string sourceRootFolder,
        string targetRootFolder,
        string relativePath,
        string sharedFolder,
        Dictionary<string, List<IHtmlNode>> sharedContents)
    {
        string finalSourceFolder = GetPath(sourceRootFolder, relativePath);
        string finalTargetFolder = PrepareFolder(targetRootFolder, relativePath);
        Console.WriteLine("Process folder: '{0}'", relativePath);
        foreach (string fileFullPath in Directory.GetFiles(finalSourceFolder))
        {
            string fileName = Path.GetFileName(fileFullPath);
            string extension = Path.GetExtension(fileName);
            string finalTargetFileName = GetPath(finalTargetFolder, fileName);
            if (!HtmlFileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                Console.Write("\t copy {0}", fileName);
                File.Copy(fileFullPath, finalTargetFileName, EnableOverride);
            }
            else
            {
                Console.Write("\t process {0}", fileName);
                string finalHtml = transformer.ProcessFile(fileFullPath, relativePath, sharedFolder, sharedContents);
                File.WriteAllText(finalTargetFileName, finalHtml);
            }

            Console.WriteLine(" done");
        }

        foreach (string subFolderFullPath in Directory.GetDirectories(finalSourceFolder))
        {
            string folderName = Path.GetFileName(subFolderFullPath);
            Process(
                transformer,
                sourceRootFolder,
                targetRootFolder,
                Path.Combine(relativePath, folderName),
                sharedFolder,
                sharedContents);
        }
    }

    private static string GetRepositorySubFolder(string folder)
    {
        return GetPath(RepositoryRootFolder, folder);
    }

    private static string GetPath(string rootPath, string folder)
    {
        return string.IsNullOrEmpty(folder) ? rootPath : Path.GetFullPath(Path.Combine(rootPath, folder));
    }

    private static string PrepareFolder(string rootFolder, string subFolder)
    {
        string targetFolder = GetPath(rootFolder, subFolder);
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        return targetFolder;
    }
}
