using System;

namespace GorillaComputer.Model
{
    [Flags]
    public enum ComputerAppearanceFlags
    {
        /// <summary>
        /// no modifications are made to the computer
        /// </summary>
        None =          0,

        /// <summary>
        /// the main text of the computer is modified
        /// </summary>
        Primary =       1 << 0,

        /// <summary>
        /// the navigation box of the computer is modified
        /// </summary>
        Navigation =    1 << 1,

        All = Primary | Navigation
    }
}
