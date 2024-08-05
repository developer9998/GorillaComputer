using GorillaComputer.Interface;
using GorillaNetworking;
using System;

namespace GorillaComputer.Model
{
    public abstract class ComputerFunction : IComputerFunction
    {
        public static event Action<ComputerFunction, string> RequestSetContent;

        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual bool IsParentalLocked { get; } = false;

        public abstract string GetFunctionContent();

        public virtual void OnFunctionOpened()
        {

        }

        public virtual void OnKeyPressed(GorillaKeyboardBindings key)
        {

        }

        public void SetFunctionContent()
        {
            SetFunctionContent(GetFunctionContent());
        }

        public void SetFunctionContent(string content)
        {
            RequestSetContent?.Invoke(this, content);
        }
    }
}
