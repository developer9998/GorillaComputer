using HarmonyLib;
using System.Reflection;

namespace GorillaComputer.Extension
{
    internal static class ReflectionEx
    {
        public static FieldInfo GetField(this object obj, string fieldName) 
        {
            return AccessTools.Field(obj.GetType(), fieldName);
        }

        public static PropertyInfo GetProperty(this object obj, string propertyName)
        {
            return AccessTools.Property(obj.GetType(), propertyName);
        }

        public static MethodInfo GetMethod(this object obj, string methodName)
        {
            return AccessTools.Method(obj.GetType(), methodName);
        }
    }
}
