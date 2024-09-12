using GorillaComputer.Interface;
using GorillaNetworking;
using System;

namespace GorillaComputer.Model
{
    public abstract class ComputerFunction : IComputerFunction
    {
        public static event Action<ComputerFunction, string> RequestUpdateMonitor;

        public abstract string Name { get; }

        public abstract string Description { get; }

        public virtual bool IsParentalLocked { get; } = false;

        public abstract string GetFunctionText();

        public virtual void OnFunctionOpened()
        {

        }

        public virtual void OnKeyPressed(GorillaKeyboardBindings key)
        {

        }

        public void UpdateMonitor()
        {
            UpdateMonitor(GetFunctionText());
        }

        public void UpdateMonitor(string content)
        {
            RequestUpdateMonitor?.Invoke(this, content);
        }
    }
}
