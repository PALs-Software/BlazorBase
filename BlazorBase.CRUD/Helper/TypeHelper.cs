using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Helper
{
    public static class TypeHelper
    {
        public static List<Type> NumericTypes { get; } = new List<Type>(){
            typeof(decimal),
            typeof(decimal?),
            typeof(double),
            typeof(double?),
            typeof(float),
            typeof(float?),
            typeof(int),
            typeof(int?),
            typeof(long),
            typeof(long?)
        };

        public static List<Type> DecimalTypes { get; } = new List<Type>(){
            typeof(decimal),
            typeof(decimal?),
            typeof(double),
            typeof(double?),
            typeof(float),
            typeof(float?)
        };
    }
}
