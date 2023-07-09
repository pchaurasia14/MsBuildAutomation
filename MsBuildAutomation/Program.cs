using Microsoft.Build.Locator;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        // var x = MSBuildLocator.QueryVisualStudioInstances().ToArray();
        // MSBuildLocator.RegisterInstance(x.First());
        MSBuildLocator.RegisterDefaults();

        Thread.GetDomain().AssemblyLoad += RegisterMSBuildAssemblyPath;
        Thread.GetDomain().AssemblyResolve += RedirectMSBuildAssemblies;
        
        SomeBuildFunction();

        Console.WriteLine("Completed!!!!");

        Console.ReadLine();

        static void RegisterMSBuildAssemblyPath(object? sender, AssemblyLoadEventArgs args)
        {
            var assemblyPath = args.LoadedAssembly.Location;

            if (Path.GetFileName(assemblyPath) == "Microsoft.Build.dll")
                Settings.MSBuildAssemblyDir = Path.GetDirectoryName(assemblyPath);
        }

        static Assembly? RedirectMSBuildAssemblies(object? sender, ResolveEventArgs args)
        {
            if (Settings.MSBuildAssemblyDir == null)
                return null;

            try
            {
                var assemblyFilename = $"{args.Name.Split(',')[0]}.dll";
                var potentialAssemblyPath = Path.Combine(Settings.MSBuildAssemblyDir, assemblyFilename);

                return Assembly.LoadFrom(potentialAssemblyPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        static void SomeBuildFunction()
        {

            BuildParameters buildParameters = new BuildParameters()
            {
                Loggers = new[]
                {
                    new Microsoft.Build.Logging.ConsoleLogger(),
                    //new Microsoft.Build.Logging.FileLogger()
                    //{
                    //    Verbosity = LoggerVerbosity.Diagnostic
                    //}
                },
                DisableInProcNode = true,


            };

            string projectPath = """D:\StudioWorks\NETInternals\DeleteThis\DeleteThis\DeleteThis.csproj""";
            BuildRequestData buildRequestData = new(projectPath, new Dictionary<string, string>(), null, new[] { "Restore", "Build" }, null);
            BuildManager buildManager = BuildManager.DefaultBuildManager;
            BuildResult buildResult = buildManager.Build(buildParameters, buildRequestData);


            if (buildResult.OverallResult == BuildResultCode.Success)
            {
                // Build succeeded
            }
            else
            {
                // Build failed
            }

            foreach (var buildMessage in buildResult.ResultsByTarget.Values.SelectMany(result => result.Items))
            {
                // Access build output messages
                if (buildMessage is BuildErrorEventArgs buildError)
                {
                    // Access build error messages
                    var errorMessage = buildError.Message;
                    var errorCode = buildError.Code;
                    var errorFile = buildError.File;
                    var errorLine = buildError.LineNumber;
                    var errorColumn = buildError.ColumnNumber;
                    // Handle the error as needed

                    Console.WriteLine($"\n{errorCode}\t {errorMessage} \n\t{errorFile}\n\t {errorLine}\n\t {errorColumn}");
                }
                else if (buildMessage is BuildWarningEventArgs buildWarning)
                {
                    // Access build warning messages
                    var warningMessage = buildWarning.Message;
                    var warningCode = buildWarning.Code;
                    var warningFile = buildWarning.File;
                    var warningLine = buildWarning.LineNumber;
                    var warningColumn = buildWarning.ColumnNumber;
                    // Handle the warning as needed
                    Console.WriteLine($"\n{warningCode}\t {warningMessage} \n\t{warningFile}\n\t {warningLine}\n\t {warningColumn}");

                }
                else if (buildMessage is BuildMessageEventArgs buildMessageEvent)
                {
                    // Access other build messages
                    var message = buildMessageEvent.Message;
                    Console.WriteLine($"{message}");
                    // Handle the message as needed
                }
            }
        }


        //await SyntaxTreeEval();


        //static async Task SyntaxTreeEval()
        //{
        //    var workspace = MSBuildWorkspace.Create();
        //    workspace.LoadMetadataForReferencedProjects = true;

        //    workspace.WorkspaceFailed += (sender, e) =>
        //    {
        //        Console.WriteLine($"{sender?.GetType()} => {e.Diagnostic}");
        //    };

        //    var path = """D:\StudioWorks\NETInternals\DeleteThis\DeleteThis.sln""";

        //    var solution = await workspace.OpenSolutionAsync(path);

        //    foreach (var document in solution.Projects.SelectMany(t => t.Documents))
        //    {
        //        var root = await document.GetSyntaxRootAsync();

        //        if (root == null) continue;

        //        foreach (var typeDeclarationSyntax in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        //        {
        //            var nsDeclarationSyntax = typeDeclarationSyntax.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();

        //            var typeName = typeDeclarationSyntax.Identifier.Text;
        //            var nameSpace = nsDeclarationSyntax?.Name.ToFullString();

        //            Console.WriteLine($" -> {nameSpace}.{typeName}");
        //        }
        //    }
        //}
    }
}

static class Settings
{
    public static string? MSBuildAssemblyDir { get; set; }
}
