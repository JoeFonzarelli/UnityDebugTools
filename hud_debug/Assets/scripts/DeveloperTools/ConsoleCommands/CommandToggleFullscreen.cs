namespace ConsoleCommands
{
    public class CommandToggleFullscreen : ConsoleCommand
    {
        public override void runCommand(string[] parameters)
        {
            #if UNITY_STANDALONE
            UnityEngine.Screen.fullScreen = !UnityEngine.Screen.fullScreen;
            #endif
        }

        CommandToggleFullscreen()
        {
            name = "toggle fullscreen";
            command = "fullscreen";
            description = "Use this command to toggle the fullscreen mode.";
            help = "\n- This command will swap between fullscreen and windowed mode.\n- This command won't work on SWITCH or consoles.\n- This command has no overloads and receives no parameters.";

            AddCommand();
        }

        public static CommandToggleFullscreen CreateCommand()
        {
            return new CommandToggleFullscreen();
        }
    }
}
