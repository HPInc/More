using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using More;

class Options : CommandLineParser
{
    public static readonly Options Instance = new Options();

    public readonly CommandLineSwitch wince;
    public Options()
    {
        wince = new CommandLineSwitch("wince", "Generate build configuration for windows CE");
        Add(wince);
    }
    public override void PrintUsageHeader()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("metabuild generate [-options] <generator> <project-file>");
        Console.WriteLine();
        Console.WriteLine("<generator> : VisualStudio2008");
        Console.WriteLine();
    }
}

static class MetabuildMain
{
    static int Main(String[] args)
    {
        List<String> nonOptionArgs = Options.Instance.Parse(args);

        if (nonOptionArgs.Count == 0)
        {
            Options.Instance.PrintUsage();
            return 1;
        }

        string command = nonOptionArgs[0];
        nonOptionArgs.RemoveAt(0);
        if (command == "generate")
        {
            return Generate(nonOptionArgs);
        }
        else
        {
            Console.WriteLine("Error: unknown command '{0}'", command);
            return 1;
        }
    }
    static int Generate(List<String> args)
    {
        if (args.Count <= 1)
        {
            Console.WriteLine("Error: the 'generate' command requires a generator and one or more project files");
            return 1;
        }
        String generatorName = args[0];
        Generator generator = Generator.TryGet(generatorName);
        if (generator == null)
        {
            Console.WriteLine("Error: uknown generator '{0}'", generatorName);
            return 1;
        }
        args.RemoveAt(0);

        int failedProjects = 0;
        foreach (String metabuildFile in args)
        {
            MetabuildProject metabuild;
            try
            {
                metabuild = MetabuildProject.Load(metabuildFile);
            }
            catch (ProjectLoadException e)
            {
                Console.WriteLine(e.Message);
                failedProjects++;
                continue;
            }
            generator.Generate(metabuild);
        }

        if (failedProjects > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Error: failed to generated {0} project(s) out of {1}", failedProjects, args.Count);
        }
        return failedProjects;
    }
}

static class FileExt
{
    public static Boolean IsDirectory(this FileAttributes attributes)
    {
        return (attributes & FileAttributes.Directory) == FileAttributes.Directory;
    }
}

class ProjectLoadException : Exception
{
    public ProjectLoadException(string msg) : base(msg) { }
}

interface IUnresolvedReference
{
    Boolean Resolve(MetabuildProject metabuild);
}


class MetabuildProject
{
    public readonly String projectDir;
    public readonly String projectFile;
    public String generatePathPrefix;
    public readonly List<MetabuildUnit> units = new List<MetabuildUnit>();

    List<IUnresolvedReference> unresolvedReferences = null;

    private MetabuildProject(String projectDir, String projectFile)
    {
        this.projectDir = projectDir;
        this.projectFile = projectFile;
        this.unresolvedReferences = new List<IUnresolvedReference>();
        MetabuildUnit.LoadFile(this, projectFile);
        List<IUnresolvedReference> saveUnresolvedReferences = this.unresolvedReferences;
        this.unresolvedReferences = null;

        UInt32 resolveFailCount = 0;
        foreach (IUnresolvedReference reference in saveUnresolvedReferences)
        {
            if (!reference.Resolve(this))
                resolveFailCount++;
        }
        if (resolveFailCount > 0)
            throw new ProjectLoadException(String.Format("{0} unresolved reference(s)", resolveFailCount));
    }

    internal void AddUnresolvedReference(IUnresolvedReference unresolvedReference)
    {
        if (this.unresolvedReferences == null)
            throw new InvalidOperationException("Cannot add unresolved references because this project has already been processed");
        this.unresolvedReferences.Add(unresolvedReference);
    }

    public static MetabuildProject Load(string dirOrFile)
    {
        FileAttributes attributes = GetProjectFileAttributes(dirOrFile);
        if (!attributes.IsDirectory())
        {
            return new MetabuildProject(Path.GetDirectoryName(dirOrFile), dirOrFile);
        }

        String metabuildFile = null;
        foreach (string file in Directory.GetFiles(dirOrFile, "*.metaproj"))
        {
            if (metabuildFile != null)
                throw new ProjectLoadException(String.Format("directory '{0}' contains multiple .metaproj files", dirOrFile));
            metabuildFile = file;
        }
        if (metabuildFile == null)
            throw new ProjectLoadException(String.Format("directory '{0}' does not contain any .metaproj files", dirOrFile));
        return new MetabuildProject(dirOrFile, metabuildFile);
    }

    public static FileAttributes GetProjectFileAttributes(String dirOrFile)
    {
        try
        {
#if WindowsCE
            throw new NotImplementedException();
#else
            return File.GetAttributes(dirOrFile);
#endif
        }
        catch (FileNotFoundException)
        {
            throw new ProjectLoadException(String.Format("file or directory '{0}' does not exist", dirOrFile));
        }
    }

    public String MakeGeneratePath(String postfix)
    {
        return ((generatePathPrefix == null) ? projectFile : Path.Combine(projectDir, generatePathPrefix)) + postfix;
    }
}

interface IMetabuildVisitor
{
    void Visit(CSharpProject project);
}
abstract class MetabuildUnit
{
    private static void LoadDirOrFile(MetabuildProject metabuild, String dirOrFile)
    {
        FileAttributes attributes = MetabuildProject.GetProjectFileAttributes(dirOrFile);
        if (attributes.IsDirectory())
        {
            UInt32 metabuildFileCount = 0;
            foreach (string file in Directory.GetFiles(dirOrFile, "*.metabuild"))
            {
                metabuildFileCount++;
                LoadFile(metabuild, file);
            }
            if (metabuildFileCount == 0)
                throw new ProjectLoadException(String.Format("directory '{0}' does not contain any .metabuild files", dirOrFile));
        }
        else
        {
            LoadFile(metabuild, dirOrFile);
        }
    }

    // Assumption: filename already exists and is already verified to be a file
    public static void LoadFile(MetabuildProject metabuild, String filename)
    {
        Console.WriteLine("[DEBUG] Metabuild.LoadFile \"{0}\"", filename);
        String relativeDir = null;
        using (LfdTextReader reader = new LfdTextReader(new StreamReader(new FileStream(
            filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
        {
            LfdParser parser = new LfdParser(reader, filename);
            LfdLine line = parser.reader.ReadLineIgnoreComments();
            for (; ; )
            {
                if (line == null)
                    return;

                if (line.id == "Import")
                {
                    parser.errors.EnforceFieldCount(line, 1);
                    MetabuildUnit.LoadDirOrFile(metabuild, line.fields[0]);
                    line = parser.reader.ReadLineIgnoreComments();
                }
                else if (line.id == "CSharpProject")
                {
                    if (relativeDir == null)
                        relativeDir = Path.GetDirectoryName(filename);
                    line = CSharpProject.Parse(metabuild, parser, line, false, relativeDir);
                }
                else if (line.id == "CSharpTestProject")
                {
                    if (relativeDir == null)
                        relativeDir = Path.GetDirectoryName(filename);
                    line = CSharpProject.Parse(metabuild, parser, line, true, relativeDir);
                }
                else if (line.id == "GeneratePathPrefix")
                {
                    parser.errors.EnforceFieldCount(line, 1);
                    if (metabuild.generatePathPrefix != null)
                        throw parser.errors.Error(line, "GeneratePathPrefix was set more than once");
                    metabuild.generatePathPrefix = line.fields[0];
                    line = parser.reader.ReadLineIgnoreComments();
                }
                else
                {
                    throw parser.errors.Error(line, "unknown directive '{0}'", line.id);
                }
            }
        }
    }
    public abstract void Visit(IMetabuildVisitor visitor);
}


struct LfdErrorReporter
{
    public readonly String filenameForErrors;
    public LfdErrorReporter(String filenameForErrors)
    {
        this.filenameForErrors = filenameForErrors;
    }
    public ProjectLoadException ErrorNoLine(String fmt, params Object[] obj)
    {
        return new ProjectLoadException(String.Format("{0}: {1}",
            filenameForErrors, String.Format(fmt, obj)));
    }
    public ProjectLoadException Error(LfdLine line, String fmt, params Object[] obj)
    {
        return new ProjectLoadException(String.Format("{0}({1}) {2}",
            filenameForErrors, line.actualLineNumber, String.Format(fmt, obj)));
    }
    public void PrintError(LfdLine line, String fmt, params Object[] obj)
    {
        Console.WriteLine("{0}({1}) Error: {2}",
            filenameForErrors, line.actualLineNumber, String.Format(fmt, obj));
    }
    public void EnforceFieldCount(LfdLine line, UInt32 expected)
    {
        UInt32 fieldsLength = (line.fields == null) ? 0 : (UInt32)line.fields.Length;
        if (fieldsLength != expected)
        {
            throw Error(line, "property '{0}' requires {1} field(s) but got {2}",
                line.id, expected, fieldsLength);
        }
    }
}
struct LfdParser
{
    public readonly LfdTextReader reader;
    public readonly LfdErrorReporter errors;
    public LfdParser(LfdTextReader reader, String filenameForErrors)
    {
        this.reader = reader;
        this.errors = new LfdErrorReporter(filenameForErrors);
    }
}

enum OutputType
{
    Library, Exe, Module, Winexe
}
class CSharpProject : MetabuildUnit
{
    // relative directory name from root .metabuild project
    readonly String relativePath;
    readonly String name;
    readonly Boolean isTestProject;
    Guid projectGuid;
    OutputType outputType;
    String assemblyName;
    Boolean allowUnsafeBlocks;
    readonly List<String> referenceList = new List<String>();
    readonly List<CSharpProject> projectReferenceList = new List<CSharpProject>();
    readonly List<String> sourceList = new List<String>();

    CSharpProject(String relativePath, String name, Boolean isTestProject)
    {
        this.relativePath = relativePath;
        this.name = name;
        this.isTestProject = isTestProject;
    }

    public String RelativePath { get { return relativePath; } }
    public String Name { get { return name; } }
    public Boolean IsTestProject { get { return isTestProject; } }
    public Guid ProjectGuid
    {
        get { return projectGuid; }
        set
        {
            Debug.Assert(projectGuid == default(Guid));
            this.projectGuid = value;
        }
    }
    public OutputType OutputType { get { return outputType; } }
    public String AssemblyName
    {
        get { return assemblyName; }
        set
        {
            Debug.Assert(assemblyName == null);
            this.assemblyName = value;
        }
    }
    public Boolean AllowUnsafeBlocks { get { return allowUnsafeBlocks; } }
    public List<String> ReferenceList { get { return referenceList; } }
    public List<CSharpProject> ProjectReferenceList { get { return projectReferenceList; } }
    public List<String> SourceList { get { return sourceList; } }

    public override void Visit(IMetabuildVisitor visitor) { visitor.Visit(this); }
    public static LfdLine Parse(MetabuildProject metabuild, LfdParser parser, LfdLine projectLine, Boolean isTestProject, String relativePath)
    {
        parser.errors.EnforceFieldCount(projectLine, 1);
        CSharpProject project = new CSharpProject(relativePath, projectLine.fields[0], isTestProject);
        bool setOutputType = false;

        LfdLine line;
        for (; ; )
        {
            line = parser.reader.ReadLineIgnoreComments();
            if (line == null)
                break;
            if (line.id == "ProjectGuid")
            {
                parser.errors.EnforceFieldCount(line, 1);
                if (project.projectGuid != default(Guid))
                    throw parser.errors.Error(line, "ProjectGuid was set more than once");
                project.projectGuid = new Guid(line.fields[0]);
            }
            else if (line.id == "OutputType")
            {
                parser.errors.EnforceFieldCount(line, 1);
                if (setOutputType)
                    throw parser.errors.Error(line, "OutputType was set more than once");
                setOutputType = true;
                project.outputType = (OutputType)Enum.Parse(typeof(OutputType), line.fields[0], false);
            }
            else if (line.id == "AssemblyName")
            {
                parser.errors.EnforceFieldCount(line, 1);
                if (project.assemblyName != null)
                    throw parser.errors.Error(line, "AssemblyName was set more than once");
                project.assemblyName = line.fields[0];
            }
            else if (line.id == "AllowUnsafeBlocks")
            {
                parser.errors.EnforceFieldCount(line, 0);
                if (project.allowUnsafeBlocks)
                    throw parser.errors.Error(line, "AllowUnsafeBlocks was set more than once");
                project.allowUnsafeBlocks = true;
            }
            else if (line.id == "Reference")
            {
                parser.errors.EnforceFieldCount(line, 1);
                project.referenceList.Add(line.fields[0]);
            }
            else if (line.id == "ProjectReference")
            {
                metabuild.AddUnresolvedReference(new UnresolvedCSharpProjectReference(project, parser.errors, line));
            }
            else if (line.id == "Source")
            {
                parser.errors.EnforceFieldCount(line, 1);
                project.sourceList.Add(line.fields[0]);
            }
            else
            {
                break;
            }
        }

        //
        // Verify the project is valid
        //
        if (!setOutputType)
            throw parser.errors.ErrorNoLine("Missing the 'OutputType' property");
        if (project.assemblyName == null)
            project.assemblyName = project.name;
        if (project.projectGuid == default(Guid))
            project.projectGuid = Guid.NewGuid();
        metabuild.units.Add(project);
        return line;
    }

    class UnresolvedCSharpProjectReference : IUnresolvedReference
    {
        CSharpProject project;
        LfdErrorReporter errors;
        LfdLine line;
        String referenceName;
        public UnresolvedCSharpProjectReference(CSharpProject project, LfdErrorReporter errors, LfdLine line)
        {
            this.project = project;
            this.errors = errors;
            this.line = line;
            errors.EnforceFieldCount(line, 1);
            this.referenceName = line.fields[0];
        }
        public Boolean Resolve(MetabuildProject metabuild)
        {
            foreach (MetabuildUnit unit in metabuild.units)
            {
                CSharpProject unitAsCSharpProject = unit as CSharpProject;
                if (unitAsCSharpProject != null && unitAsCSharpProject.Name == referenceName)
                {
                    if (project.projectReferenceList.Contains(unitAsCSharpProject))
                    {
                        errors.PrintError(line, "project {0} was referenced multiple times", referenceName);
                        return false; // fail
                    }
                    project.projectReferenceList.Add(unitAsCSharpProject);
                    return true; // success
                }
            }
            errors.PrintError(line, "unknown project reference {0}", referenceName);
            return false; // fail
        }
    }
}

abstract class Generator
{
    public static Generator TryGet(string name)
    {
        if (name == "VisualStudio2008")
            return new VisualStudio2008Generator();
        return null;
    }
    public abstract void Generate(MetabuildProject metabuild);
}

struct ProjectTypeGuid
{
    public readonly String name;
    public readonly Guid guid;
    public ProjectTypeGuid(String name, Guid guid)
    {
        this.name = name;
        this.guid = guid;
    }

    public static readonly ProjectTypeGuid Test        = new ProjectTypeGuid("Test"             , new Guid("3AC096D0-A1C2-E12C-1390-A8335801FDAB"));
    public static readonly ProjectTypeGuid SmartDevice = new ProjectTypeGuid("Smart Device (C#)", new Guid("4D628B5B-2FBC-4AA6-8C16-197242AEB884"));
    public static readonly ProjectTypeGuid CSharp      = new ProjectTypeGuid("C#"               , new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"));
}
struct UniqueList<T>
{
    public readonly List<T> list;
    public UniqueList(List<T> list)
    {
        this.list = list;
    }
    public void Add(T item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
        }
    }
}


/**
NOTE: I originally wanted to have 1 project for both the big windows and WindowsCE builds, however,
      I was unable to make this work.  I was able to use MSBUILD to build both kinds with the same
      project file, however, it looks like VisualStudio2008 cannot load a project that contains builds
      for multiple platforms.
*/
class VisualStudio2008Generator : Generator
{
    String generatePath;
    public override void Generate(MetabuildProject metabuild)
    {
        generatePath = metabuild.MakeGeneratePath(".VisualStudio2008");
        Console.WriteLine("[DEBUG] generatePath '{0}'", generatePath);
        if (!Directory.Exists(generatePath))
        {
            Directory.CreateDirectory(generatePath);
        }

        Boolean generateForWince = false;
        for (;;)
        {
            ProjectGenerator generator = new ProjectGenerator(this, generateForWince);
            foreach (MetabuildUnit unit in metabuild.units)
            {
                unit.Visit(generator);
            }
            GenerateSolution(metabuild, generateForWince);

            if (generateForWince || !Options.Instance.wince.set)
            {
                break;
            }
            generateForWince = true;
        }
    }
    void GenerateSolution(MetabuildProject metabuild, Boolean wince)
    {
        String solutionFile = Path.Combine(generatePath, Path.GetFileNameWithoutExtension(metabuild.projectFile) +
            (wince ? ".CE.sln" : ".sln"));
        using (TextWriter writer = new StreamWriter(File.Open(solutionFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8))
        {
            writer.WriteLine();
            writer.WriteLine("Microsoft Visual Studio Solution File, Format Version 10.00");
            writer.WriteLine("# Visual Studio 2008");
            {
                SolutionProjectGenerator generator = new SolutionProjectGenerator(writer, wince);
                foreach (MetabuildUnit unit in metabuild.units)
                {
                    unit.Visit(generator);
                }
            }
            writer.WriteLine("Global");
            writer.WriteLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            writer.WriteLine("\t\tDebug|Any CPU = Debug|Any CPU");
            writer.WriteLine("\t\tRelease|Any CPU = Release|Any CPU");
            writer.WriteLine("\tEndGlobalSection");
            writer.WriteLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            {
                SolutionProjectConfigGenerator generator = new SolutionProjectConfigGenerator(writer, wince);
                foreach (MetabuildUnit unit in metabuild.units)
                {
                    unit.Visit(generator);
                }
            }
            writer.WriteLine("\tEndGlobalSection");
            writer.WriteLine("\tGlobalSection(SolutionProperties) = preSolution");
            writer.WriteLine("\t\tHideSolutionNode = FALSE");
            writer.WriteLine("\tEndGlobalSection");
            writer.WriteLine("EndGlobal");
        }
    }
    class SolutionProjectGenerator : IMetabuildVisitor
    {
        TextWriter writer;
        Boolean wince;
        Guid solutionGuid;
        public SolutionProjectGenerator(TextWriter writer, Boolean wince)
        {
            this.writer = writer;
            this.wince = wince;
            this.solutionGuid = Guid.NewGuid();
        }
        public void Visit(CSharpProject project)
        {
            if (project.IsTestProject && wince)
            {
                Console.WriteLine("[WARNING] generating wince test projects not implemented");
                return;
            }
            writer.WriteLine("Project(\"{{{0}}}\") = \"{1}\", \"{1}{2}.csproj\", \"{{{3}}}\"",
                solutionGuid.ToString().ToUpper(), project.Name, wince ? ".CE" : "", project.ProjectGuid);
            writer.WriteLine("EndProject");
        }
    }
    class SolutionProjectConfigGenerator : IMetabuildVisitor
    {
        TextWriter writer;
        Boolean wince;
        public SolutionProjectConfigGenerator(TextWriter writer, Boolean wince)
        {
            this.writer = writer;
            this.wince = wince;
        }
        public void Visit(CSharpProject project)
        {
            if (project.IsTestProject && wince)
            {
                Console.WriteLine("[WARNING] generating wince test projects not implemented");
                return;
            }
            writer.WriteLine("\t\t{{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU", project.ProjectGuid.ToString().ToUpper());
            writer.WriteLine("\t\t{{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU", project.ProjectGuid.ToString().ToUpper());
            writer.WriteLine("\t\t{{{0}}}.Release|Any CPU.ActiveCfg = Release|Any CPU", project.ProjectGuid.ToString().ToUpper());
            writer.WriteLine("\t\t{{{0}}}.Release|Any CPU.Build.0 = Release|Any CPU", project.ProjectGuid.ToString().ToUpper());
        }
    }
    String GetProjectFile(CSharpProject project, Boolean wince)
    {
        return Path.Combine(generatePath, project.Name) + (wince ? ".CE.csproj" : ".csproj");
    }
    class ProjectGenerator : IMetabuildVisitor
    {
        VisualStudio2008Generator generator;
        Boolean wince;
        public ProjectGenerator(VisualStudio2008Generator generator, Boolean wince)
        {
            this.generator = generator;
            this.wince = wince;
        }
        public void Visit(CSharpProject project)
        {
            if (project.IsTestProject && wince)
            {
                Console.WriteLine("[WARNING] generating wince test projects not implemented");
                return;
            }
            Console.WriteLine("[DEBUG] Visit {0}{1}", project.Name, wince ? " (wince)" : "");
            String projectFile = generator.GetProjectFile(project, wince);

            using (TextWriter writer = new StreamWriter(File.Open(projectFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                writer.WriteLine("<Project ToolsVersion=\"3.5\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
                writer.WriteLine("  <PropertyGroup>");
                writer.WriteLine("    <Configuration Condition=\" '$(Configuration)' == '' \">Debug</Configuration>");
                writer.WriteLine("    <Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform>");
                writer.WriteLine("    <SchemaVersion>2.0</SchemaVersion>");
                writer.WriteLine("    <ProjectGuid>{{{0}}}</ProjectGuid>", project.ProjectGuid.ToString().ToUpper());
                writer.WriteLine("    <OutputType>{0}</OutputType>", project.OutputType);
                writer.WriteLine("    <AssemblyName>{0}</AssemblyName>", project.AssemblyName);
                if (project.AllowUnsafeBlocks)
                    writer.WriteLine("    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>");
                {
                    UniqueList<ProjectTypeGuid> projectTypeGuids = new UniqueList<ProjectTypeGuid>(new List<ProjectTypeGuid>());
                    if (project.IsTestProject)
                    {
                        projectTypeGuids.Add(ProjectTypeGuid.Test);
                        projectTypeGuids.Add(ProjectTypeGuid.CSharp);
                    }
                    if (wince)
                    {
                        projectTypeGuids.Add(ProjectTypeGuid.SmartDevice);
                        projectTypeGuids.Add(ProjectTypeGuid.CSharp);
                    }
                    if (projectTypeGuids.list.Count > 0)
                    {
                        foreach (ProjectTypeGuid guid in projectTypeGuids.list)
                        {
                            writer.WriteLine("    <!-- {0} {1} -->", guid.guid.ToString().ToUpper(), guid.name);
                        }
                        writer.Write("    <ProjectTypeGuids>");
                        String prefix = "";
                        foreach (ProjectTypeGuid guid in projectTypeGuids.list)
                        {
                            writer.Write("{0}{{{1}}}", prefix, guid.guid.ToString().ToUpper());
                            prefix = ";";
                        }
                        writer.WriteLine("</ProjectTypeGuids>");
                    }
                }
                // NOTE: these properties CANNOT be added "conditionally" because visual studio cannot load a project this way.
                //       probably has something to do with Visual Studio "caching" the platform once for the project and not
                //       reloading it for every configuration
                //       So you cannot do something like this:
                //       <PropertyGroup Condition=" '$(Configuration)' == 'DebugWindowsCE' Or '$(Configuration)' == 'ReleaseWindowsCE' ">
                if (wince)
                {
                    writer.WriteLine("    <PlatformFamilyName>WindowsCE</PlatformFamilyName>");
                    writer.WriteLine("    <PlatformID>E2BECB1F-8C8C-41ba-B736-9BE7D946A398</PlatformID>");
                    writer.WriteLine("    <OSVersion>5.0</OSVersion>");
                    writer.WriteLine("    <NativePlatformName>Windows CE</NativePlatformName>");
                    writer.WriteLine("    <FormFactorID>");
                    writer.WriteLine("    </FormFactorID>");
                    writer.WriteLine("    <NoStdLib>true</NoStdLib>");
                    writer.WriteLine("    <NoConfig>true</NoConfig>");
                    writer.WriteLine("    <GenerateSerializationAssemblies>Off</GenerateSerializationAssemblies>");
                }
                writer.WriteLine("    <TargetFrameworkVersion>v3.5</TargetFrameworkVersion>");
                writer.WriteLine("    <FileAlignment>512</FileAlignment>");
                writer.WriteLine("    <WarningLevel>4</WarningLevel>");
                writer.WriteLine("    <ErrorReport>prompt</ErrorReport>");
                writer.WriteLine("    <OutputPath Condition=\" '$(Platform)' == 'AnyCPU' \">bin\\{0}$(Configuration)\\</OutputPath>", wince ? "CE\\" : "");
                writer.WriteLine("    <OutputPath Condition=\" '$(Platform)' != 'AnyCPU' \">bin\\{0}$(Platform)\\$(Configuration)\\</OutputPath>", wince ? "CE\\" : "");
                writer.WriteLine("  </PropertyGroup>");
                writer.WriteLine("  <PropertyGroup Condition=\" '$(Configuration)' == 'Debug'\">");
                writer.WriteLine("    <DebugSymbols>true</DebugSymbols>");
                writer.WriteLine("    <DebugType>full</DebugType>");
                writer.WriteLine("    <Optimize>false</Optimize>");
                writer.WriteLine("    <DefineConstants>DEBUG;TRACE{0}</DefineConstants>", wince ? ";$(PlatformFamilyName)" : "");
                writer.WriteLine("  </PropertyGroup>");
                writer.WriteLine("  <PropertyGroup Condition=\" '$(Configuration)' == 'Release'\">");
                writer.WriteLine("    <DebugType>pdbonly</DebugType>");
                writer.WriteLine("    <Optimize>true</Optimize>");
                writer.WriteLine("    <DefineConstants>TRACE{0}</DefineConstants>", wince ? ";$(PlatformFamilyName)" : "");
                writer.WriteLine("  </PropertyGroup>");

                /*
                //writer.WriteLine("  <!--");
                //writer.WriteLine("  <ProjectExtensions Condition=\" '$(PlatformFamilyName)' == 'WindowsCE' \">");
                writer.WriteLine("  <ProjectExtensions>");
                writer.WriteLine("    <VisualStudio>");
                writer.WriteLine("      <FlavorProperties GUID=\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\">");
                writer.WriteLine("        <HostingProcess disable=\"1\" />");
                writer.WriteLine("      </FlavorProperties>");
                writer.WriteLine("    </VisualStudio>");
                writer.WriteLine("  </ProjectExtensions>");
                //writer.WriteLine("-->");
                 */
                writer.WriteLine("  <ItemGroup>");
                if (wince)
                    writer.WriteLine("    <Reference Include=\"mscorlib\" />");
                foreach (String reference in project.ReferenceList)
                {
                    writer.WriteLine("    <Reference Include=\"{0}\" />", reference);
                }
                writer.WriteLine("  </ItemGroup>");
                writer.WriteLine("  <ItemGroup>");
                foreach (CSharpProject reference in project.ProjectReferenceList)
                {
                    writer.WriteLine("    <ProjectReference Include=\"{0}{1}.csproj\">", reference.Name, wince ? ".CE" : "");
                    writer.WriteLine("      <Name>{0}</Name>", reference.Name);
                    writer.WriteLine("      <Project>{{{0}}}</Project>", reference.ProjectGuid.ToString().ToUpper());
                    writer.WriteLine("    </ProjectReference>");
                }
                writer.WriteLine("  </ItemGroup>");
                writer.WriteLine("  <ItemGroup>");
                foreach (String source in project.SourceList)
                {
                    writer.WriteLine("    <Compile Include=\"..\\{0}\" />", Path.Combine(project.RelativePath, source));
                }
                writer.WriteLine("  </ItemGroup>");
                if (wince)
                    writer.WriteLine("  <Import Project=\"C:\\Windows\\Microsoft.NET\\Framework\\v3.5\\Microsoft.CompactFramework.CSharp.targets\" />");
                else
                    writer.WriteLine("  <Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />");
                writer.WriteLine("</Project>");
            }
        }
    }
}