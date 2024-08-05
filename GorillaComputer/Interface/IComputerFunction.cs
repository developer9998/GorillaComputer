using GorillaNetworking;

namespace GorillaComputer.Interface
{
    public interface IComputerFunction
    {
        public string Name { get; }
        public string Description { get; }
        public bool IsParentalLocked { get; }
        public string GetFunctionContent();
        public void OnFunctionOpened();
        public void OnKeyPressed(GorillaKeyboardBindings key);
    }
}
