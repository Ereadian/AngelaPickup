namespace ereadian.builder.site;

using System.Globalization;
using System.Reflection;
using ereadian.builder.html;

internal class Program
{
    private const string DefaultSourceFolder = "docs";
    private const string DefaultTargetFolder = "page-output";
    private const string TemplateFolder = "templates";
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
        string targetFolder = arguments.Length < 2 ? GetRepositorySubFolder(DefaultTargetFolder) : Path.GetFullPath(arguments[1]);

        Transformer transformer = new(GetRepositorySubFolder(TemplateFolder), new CultureInfo("zh-Hans"));
        Console.WriteLine("Start building.");
        Console.WriteLine("\tSource: {0}", sourceFolder);
        Console.WriteLine("\tTarget: {0}", targetFolder);
        Console.WriteLine("\tCulture: {0}", transformer.SiteCultureInfo.DisplayName);
        Process(transformer, sourceFolder, targetFolder, string.Empty, string.Empty);
        Console.WriteLine("Build completed.");
        return 0;
    }

    private static void Process(
        Transformer transformer,
        string sourceRootFolder,
        string targetRootFolder,
        string sourceFolder,
        string targetFolder)
    {
        string finalSourceFolder = GetPath(sourceRootFolder, sourceFolder);
        string finalTargetFolder = PrepareFolder(targetRootFolder, targetFolder);
        Console.WriteLine("Process folder: '{0}' => '{1}'", sourceFolder, targetFolder);
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
                string finalHtml = transformer.ProcessFile(fileFullPath, sourceFolder);
                File.WriteAllText(finalTargetFileName, finalHtml);
            }

            Console.WriteLine(" done");
        }
    }

    private static string GetPath(string rootPath, string folder)
    {
        return Path.GetFullPath(Path.Combine(rootPath, folder));
    }

    private static string GetRepositorySubFolder(string folder)
    {
        return GetPath(RepositoryRootFolder, folder);
    }

    private static string PrepareFolder(string rootFolder, string subFolder)
    {
        string targetFolder = string.IsNullOrEmpty(subFolder) ? rootFolder : GetPath(rootFolder, subFolder);
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        return targetFolder;
    }
}
