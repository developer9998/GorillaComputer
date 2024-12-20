using GorillaComputer.Models;

namespace GorillaComputer.Extension
{
    public static class KeyEx
    {
        public static bool IsNumericKey(this KeyBinding key) => key <= KeyBinding.nine;

        public static bool IsFunctionKey(this KeyBinding key) => key >= KeyBinding.up && key <= KeyBinding.option3;

        public static bool TryParseNumber(this KeyBinding key, out int number)
        {
            number = IsNumericKey(key) ? (int)key : -1;
            return number != -1;
        }

        public static string GetKeyString(this KeyBinding key) => key.TryParseNumber(out int number) ? number.ToString() : key.ToString();
    }
}
