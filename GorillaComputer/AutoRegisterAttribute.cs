using System;

namespace GorillaComputer
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class AutoRegisterAttribute : Attribute;
}
