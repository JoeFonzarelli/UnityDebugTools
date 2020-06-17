namespace ConsoleCommands
{
    public class CommandQuit : ConsoleCommand
    {
        public override void runCommand(string[] parameters)
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #elif UNITY_STANDALONE
                UnityEngine.Application.Quit();
            #endif
        }

        CommandQuit(){
            name = "Quit";
            command = "quit";
            description = "Use this command to quit the application.";
            help = "\n- This command will immediately execute Application.Quit().\n- This command won't work on SWITCH or consoles.\n- This command has no overloads and receives no parameters.";

            AddCommand();
        }

        public static CommandQuit CreateCommand(){
            return new CommandQuit();
        }
    }
}
