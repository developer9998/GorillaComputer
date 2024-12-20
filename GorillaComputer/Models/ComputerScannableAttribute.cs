using System;

namespace GorillaComputer.Models
{
    /// <summary>
    /// This attribute is targed towards an assembly that will be read by GorillaComputer
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class ComputerScannableAttribute : Attribute;
}
