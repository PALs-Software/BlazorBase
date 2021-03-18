using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Services
{
    class GenericClassStringLocalizer
    {
        private readonly IStringLocalizerFactory StringLocalizerFactory;

        public GenericClassStringLocalizer(IStringLocalizerFactory factory)
        {
            StringLocalizerFactory = factory;
        }

        public IStringLocalizer GetLocalizer(Type type)
        {
            string assemblyName = type.GetTypeInfo().Assembly.GetName().Name;
            string typeName = type.Name.Remove(type.Name.IndexOf('`'));
            string baseName = (type.Namespace + "." + typeName).Substring(assemblyName.Length).Trim('.');

            var localizer = StringLocalizerFactory.Create(baseName, assemblyName);

            return localizer;
        }
    }
}
