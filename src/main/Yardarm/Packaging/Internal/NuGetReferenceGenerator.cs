﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using NuGet.Versioning;
using Yardarm.Generation;

namespace Yardarm.Packaging.Internal
{
    internal class NuGetReferenceGenerator : IReferenceGenerator
    {
        private const string NetStandardLibrary = "NETStandard.Library";

        private readonly GenerationContext _context;

        public NuGetReferenceGenerator(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IAsyncEnumerable<MetadataReference> Generate(CancellationToken cancellationToken = default)
        {
            var lockFile = _context.NuGetRestoreInfo?.LockFile;
            Debug.Assert(lockFile is not null);

            var dependencies = ExtractDependencies(lockFile);

            return dependencies.Select(dependency => MetadataReference.CreateFromFile(dependency)).ToAsyncEnumerable();
        }

        private IEnumerable<string> ExtractDependencies(LockFile lockFile)
        {
            // Get the libraries to import for our current target
            LockFileTarget lockFileTarget = lockFile.Targets
                .First(p => p.TargetFramework == _context.CurrentTargetFramework);

            // Collect all DLL files from CompileTimeAssemblies from that target
            // Note that we apply File.Exists since there may be multiple paths we're searching for each file listed
            List<string> dependencies = lockFileTarget.Libraries
                .SelectMany(p => p.CompileTimeAssemblies.Select(q => new
                {
                    Library = p,
                    Path = q.Path.Replace('/', Path.DirectorySeparatorChar)
                }))
                .Where(p => Path.GetExtension(p.Path) == ".dll")
                .SelectMany(
                    _ => lockFile.PackageFolders.Select(p => p.Path),
                    (dependency, folder) =>
                        Path.Combine(folder, dependency.Library.Name.ToLowerInvariant(), dependency.Library.Version.ToString(), dependency.Path)
                )
                .Where(File.Exists)
                .ToList();

            var frameworkInformation = lockFile.PackageSpec.TargetFrameworks
                .First(p => p.FrameworkName == _context.CurrentTargetFramework);

            // Collect platform reference assemblies, i.e. .NET 6 assemblies
            dependencies.AddRange(frameworkInformation.FrameworkReferences
                .Select(frameworkReference =>
                {
                    string refAssemblyName = $"{frameworkReference.Name}.Ref";

                    var version = frameworkInformation.FrameworkName.Version;
                    var versionRange = new VersionRange(
                        new NuGetVersion(version),
                        maxVersion: new NuGetVersion(version.Major, version.Minor + 1, 0),
                        includeMaxVersion: false);

                    return _context.NuGetRestoreInfo!.Providers.GlobalPackages.FindPackagesById(refAssemblyName)
                        .Where(package => versionRange.Satisfies(package.Version))
                        .MaxBy(package => package.Version)?.ExpandedPath;
                })
                .Where(directory => directory is not null)
                .SelectMany(directory =>
                    Directory.GetFiles(directory!, "*.dll", new EnumerationOptions
                    {
                        MatchType = MatchType.Win32,
                        RecurseSubdirectories = true
                    })
                    .Select(file => (file, relativePath: Path.GetRelativePath(directory!, file).Replace(Path.DirectorySeparatorChar, '/')))
                    .Where(file => file.relativePath.StartsWith($"ref/{lockFileTarget.TargetFramework.GetShortFolderName()}/"))
                    .Select(file => file.file)));

            LockFileTargetLibrary? netstandardLibrary = lockFileTarget.Libraries.FirstOrDefault(p => p.Name == NetStandardLibrary);
            if (netstandardLibrary is not null)
            {
                // NETStandard.Library is a bit different, it has reference assemblies in the build/netstandard2.0/ref directory
                // which are imported via a MSBuild target file in the package. So we need to emulate that behavior here.

                string refDirectory = lockFile.PackageFolders.Select(p => p.Path)
                    .Select(p => Path.Combine(p, netstandardLibrary.Name.ToLowerInvariant(),
                        netstandardLibrary.Version.ToString()))
                    .First(Directory.Exists);
                refDirectory = Path.Combine(refDirectory, "build", "netstandard2.0", "ref");

                dependencies.AddRange(Directory.EnumerateFiles(refDirectory, "*.dll"));
            }

            return Deduplicate(dependencies);
        }

        // If more than one version of the same assembly is referenced, take the highest version number.
        // This can happen in cases like .NET Standard 2.1 including System.Threading.Tasks.Extensions
        // but some other transitively depending on the System.Threading.Tasks.Extensions NuGet package.
        private IEnumerable<string> Deduplicate(IEnumerable<string> dependencyPaths) =>
            dependencyPaths
                .Select(path => (path, name: AssemblyName.GetAssemblyName(path)))
                .GroupBy(p => (p.name.Name, p.name.CultureName, GetPublicKeyTokenString(p.name)))
                .Select(p => p
                    .OrderByDescending(q => q.name.Version)
                    .Select(q => q.path)
                    .First());
        private static string? GetPublicKeyTokenString(AssemblyName name)
        {
            byte[]? token = name.GetPublicKeyToken();
            if (token == null)
            {
                return null;
            }

            var sb = new StringBuilder(16);
            for (int i = 0; i < token.Length; i++)
            {
                sb.AppendFormat("{0:x2}", token[i]);
            }

            return sb.ToString();
        }
    }
}
