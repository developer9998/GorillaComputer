using GorillaComputer.Model;
using System.Collections.Generic;

namespace GorillaComputer.Interface
{
    public interface IFunctionDatabase
    {
        /// <summary>
        /// the position of the current function in it's registry
        /// </summary>
        public int CurrentFunctionIndex { get; set; }

        /// <summary>
        /// the current function the computer is subject to viewing
        /// </summary>
        public ComputerFunction CurrentFunction { get; set; }

        /// <summary>
        /// any overrides that should be considered for altering the computer display
        /// </summary>
        public LazyFunctionOverride FunctionOverride { get; set; }

        /// <summary>
        /// the list of functions registered by the mod
        /// </summary>
        public List<ComputerFunction> FunctionRegistry { get; set; }
    }
}
