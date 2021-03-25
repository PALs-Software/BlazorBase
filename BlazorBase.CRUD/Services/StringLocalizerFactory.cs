using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Services
{
    class StringLocalizerFactory
    {
        private readonly IStringLocalizerFactory IStringLocalizerFactory;

        public StringLocalizerFactory(IStringLocalizerFactory factory)
        {
            IStringLocalizerFactory = factory;
        }

        public IStringLocalizer GetLocalizer(Type type)
        {
            string assemblyName = type.GetTypeInfo().Assembly.GetName().Name;
            string typeName = type.Name;
            if (typeName.Contains('`'))
                typeName = typeName.Remove(typeName.IndexOf('`'));

            string baseName = (type.Namespace + "." + typeName).Substring(assemblyName.Length).Trim('.');

            var localizer = IStringLocalizerFactory.Create(baseName, assemblyName);

            return localizer;
        }
    }
}
