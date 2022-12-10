using CommandLine;

namespace DotNetCompression
{
  public class CommandLineOptions
  {
    [Option('s', "source", Required = true, HelpText = "Source directory.")]
    public string Source { get; set; }

    [Option('d', "destination", Required = true, HelpText = "Destination file as DateTime format (see https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).")]
    public string Destination { get; set; }

    [Option('r', "remove-except", Required = false, HelpText = "Only keep the latest archives up to the given count.")]
    public int RemoveExcept { get; set; }

    [Option('i', "ignore-file", Required = false, HelpText = "File to get information on which files to ignore. This must be in the .gitignore format.")]
    public string IgnoreFile { get; set; }

    [Option('g', "use-gitignore-files", Required = false, HelpText = "Specifies that all .gitignore files should be added as ignore files.")]
    public bool UseGitignoreFiles { get; set; }

    [Option('p', "password", Required = false, HelpText = "Password to encrypt the resulting archive with.")]
    public string Password { get; set; }
  }
}
