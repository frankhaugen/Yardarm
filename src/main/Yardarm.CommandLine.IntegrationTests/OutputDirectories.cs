namespace Yardarm.CommandLine.IntegrationTests;

public class OutputDirectories
{
    public OutputDirectories(string baseOutputDirectoryName = "output") => Base = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, baseOutputDirectoryName));
    public OutputDirectories(DirectoryInfo baseOutputDirectory) => Base = baseOutputDirectory;

    public DirectoryInfo Intermediate => Base.CreateSubdirectory("intermediate");
    public DirectoryInfo Base { get; }

    public FileInfo GetOutputFile(string fileName) => new(Path.Combine(Base.FullName, fileName));

    public void EnsureDirectoriesExist()
    {
        Base.Create();
        Intermediate.Create();
    }

    public void EnsureDirectoriesDoNotExist()
    {
        Base.Delete(true);
        Intermediate.Delete(true);
    }
}
