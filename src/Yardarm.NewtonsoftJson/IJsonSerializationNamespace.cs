﻿using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.NewtonsoftJson
{
    public interface IJsonSerializationNamespace
    {
        NameSyntax Name { get; }
        NameSyntax DiscriminatorConverter { get; }
        NameSyntax DynamicAdditionalPropertiesDictionary { get; }
        NameSyntax JsonTypeSerializer { get; }
        NameSyntax NullableDynamicAdditionalPropertiesDictionary { get; }

        TypeSyntax AdditionalPropertiesDictionary(TypeSyntax valueType);
    }
}
