using System;

namespace GorillaComputer.Models
{
    /// <summary>
    /// This attribute is targed towards a class that represents a custom ComputerScreen and inherits such
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ComputerCustomScreenAttribute : Attribute;
}
