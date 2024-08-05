using BepInEx.Logging;

namespace GorillaComputer.Tool
{
    internal static class LogTool
    {
        public static ManualLogSource Logger;

        public static void Info(object data) => Log(data, LogLevel.Info);

        public static void Warning(object data) => Log(data, LogLevel.Warning);

        public static void Error(object data) => Log(data, LogLevel.Error);

        private static void Log(object data, LogLevel level = LogLevel.Info)
        {
#if DEBUG
            Logger.Log(level, data);
#endif
        }
    }
}
