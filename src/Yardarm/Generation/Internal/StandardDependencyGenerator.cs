﻿using System.Collections.Generic;
using NuGet.LibraryModel;
using NuGet.Versioning;

namespace Yardarm.Generation.Internal
{
    internal class StandardDependencyGenerator : IDependencyGenerator
    {
        public IEnumerable<LibraryDependency> GetDependencies()
        {
            yield return new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
                    Name = "NETStandard.Library",
                    TypeConstraint = LibraryDependencyTarget.Package,
                    VersionRange = VersionRange.Parse("2.0.3")
                }
            };

            yield return new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
                    Name = "System.ComponentModel.Annotations",
                    TypeConstraint = LibraryDependencyTarget.Package,
                    VersionRange = VersionRange.Parse("4.7.0")
                }
            };
        }
    }
}