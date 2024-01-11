using System;
using System.Runtime.InteropServices;

namespace EscapeTheMaze.Managers
{
    public static class ExitHandler
    {
        // Implementation taken from (modified a bit):
        // https://stackoverflow.com/a/22996350

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        private static EventHandler handler;

        public static void Initialize() => SetConsoleCtrlHandler(handler += new EventHandler(Handler), true);

        private static bool Handler(CtrlType sig)
        {
            // Export the users' data
            UserManager.ExportUsers();

            Environment.Exit(0);
            return true;
        }

        private enum CtrlType
        {
            CTRL_C_EVENT        = 0,
            CTRL_BREAK_EVENT    = 1,
            CTRL_CLOSE_EVENT    = 2,
            CTRL_LOGOFF_EVENT   = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
    }
}
