using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace WebJobSdkHost
{
    class DynamicAssemblyTypeLocator : ITypeLocator
    {
        private Assembly _assembly;

        public DynamicAssemblyTypeLocator(Assembly assembly)
        {
            this._assembly = assembly;
        }
        public IReadOnlyList<Type> GetTypes()
        {
            return this._assembly.GetTypes();
        }
    }
}