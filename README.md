# DotNetCompression
Cross-platform CLI to compress folders using C# and .NET Core.

This is mainly opinionated towards my everyday usage but includes some nice features like `.gitignore` parsing, automatic archive removal, archive integrity checks, automatic date insertion, and password-protected archives.

A basic command could look like the following:

```sh
dotnet run -- --source . --destination "..\/\T\e\s\t\/yyyy-MM-dd-HH-mm-ss \B\a\c\k\u\p.\zip" --remove-except 2 --use-gitignore-files --password pw
```

## Capabilities
```sh
dotnet run -- --help
```

```
Copyright (C) 2023 Marius Freitag

  -s, --source                 Required. Source directory.

  -d, --destination            Required. Destination file as DateTime format (see https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).

  -r, --remove-except          Only keep the latest archives up to the given count.

  -i, --ignore-file            File to get information on which files to ignore. This must be in the .gitignore format.

  -g, --use-gitignore-files    Specifies that all .gitignore files should be added as ignore files.

  -p, --password               Password to encrypt the resulting archive with.

  --help                       Display this help screen.

  --version                    Display version information.
```

## Development
- Install [.NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- Run the program with `dotnet run`
- Format the source code with `dotnet format`
- Build and lint the project with `dotnet build /WarnAsError`
