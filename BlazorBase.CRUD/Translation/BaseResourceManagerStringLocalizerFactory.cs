using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Translation
{
    public class BaseResourceManagerStringLocalizerFactory : ResourceManagerStringLocalizerFactory
    {
        readonly IResourceNamesCache ResourceNamesCache = new ResourceNamesCache();
        readonly ILoggerFactory LoggerFactory;

        public BaseResourceManagerStringLocalizerFactory(IOptions<LocalizationOptions> localizationOptions, ILoggerFactory loggerFactory)
            : base(localizationOptions, loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        protected override ResourceManagerStringLocalizer CreateResourceManagerStringLocalizer(
            Assembly assembly,
            string baseName)
        {
            return new BaseResourceManagerStringLocalizer(
                assembly,
                baseName,
                ResourceNamesCache,
                LoggerFactory.CreateLogger<ResourceManagerStringLocalizer>(),
                this
                );
        }
        /*
        protected virtual void RemoveGenericType(string baseName) {
            string assemblyName = type.GetTypeInfo().Assembly.GetName().Name;
            string typeName = type.Name;
            if (typeName.Contains('`'))
                typeName = typeName.Remove(typeName.IndexOf('`'));

            string baseName = (type.Namespace + "." + typeName).Substring(assemblyName.Length).Trim('.');
        }
        */
    }
}
