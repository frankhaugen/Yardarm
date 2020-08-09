﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Generation.Internal
{
    internal class TypeGeneratorRegistry : ITypeGeneratorRegistry
    {
        private readonly IServiceProvider _serviceProvider;

        public TypeGeneratorRegistry(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ITypeGenerator Get<T>(LocatedOpenApiElement<T> element)
            where T : IOpenApiSerializable
        {

            return _serviceProvider.GetRequiredService<ITypeGeneratorRegistry<T>>().Get(element);
        }
    }
}