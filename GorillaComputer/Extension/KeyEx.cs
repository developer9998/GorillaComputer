using GorillaNetworking;

namespace GorillaComputer.Extension
{
    public static class KeyEx
    {
        public static bool IsNumberKey(this GorillaKeyboardBindings key)
        {
            return key >= GorillaKeyboardBindings.zero && key <= GorillaKeyboardBindings.nine;
        }

        public static bool IsFunctionKey(this GorillaKeyboardBindings key)
        {
            return key >= GorillaKeyboardBindings.up && key <= GorillaKeyboardBindings.option3;
        }

        public static bool TryParseNumber(this GorillaKeyboardBindings key, out int number)
        {
            number = 0;

            if (!IsNumberKey(key)) return false;

            number = (int)key;

            return true;
        }

        public static string GetKeyString(this GorillaKeyboardBindings key)
        {
            if (key.TryParseNumber(out int number))
            {
                return number.ToString();
            }

            return key.ToString();
        }
    }
}
