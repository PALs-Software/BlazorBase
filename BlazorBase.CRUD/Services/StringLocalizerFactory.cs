using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Services
{
    public class StringLocalizerFactory
    {
        protected IStringLocalizerFactory IStringLocalizerFactory { get; }
        protected Dictionary<Type, IStringLocalizer> CachedLocalizers { get; set; } = new Dictionary<Type, IStringLocalizer>();

        public StringLocalizerFactory(IStringLocalizerFactory factory)
        {
            IStringLocalizerFactory = factory;
        }

        public IStringLocalizer GetLocalizer(Type type)
        {
            if (CachedLocalizers.ContainsKey(type))
                return CachedLocalizers[type];

            string assemblyName = type.GetTypeInfo().Assembly.GetName().Name;
            string typeName = type.Name;
            if (typeName.Contains('`'))
                typeName = typeName.Remove(typeName.IndexOf('`'));

            string baseName = (type.Namespace + "." + typeName).Substring(assemblyName.Length).Trim('.');
            var localizer = IStringLocalizerFactory.Create(baseName, assemblyName);
            
            CachedLocalizers.Add(type, localizer);
            return localizer;
        }
    }
}
