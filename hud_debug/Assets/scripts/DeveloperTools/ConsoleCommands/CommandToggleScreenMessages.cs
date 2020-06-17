namespace ConsoleCommands
{
    public class CommandToggleScreenMessages : ConsoleCommand
    {
        public override void runCommand(string[] parameters)
        {
            if (parameters == null || parameters.Length == 0){
                HUD_DEBUG.instance.AddmessageToConsole("no parameters given for this command!", HUD_DEBUG.ConsoleMessageType.MISSING_COMMAND);
            }
            else if (parameters[0].ToLower() == "fps")
                HUD_DEBUG.instance.showFPS = !HUD_DEBUG.instance.showFPS;
            else if (parameters[0].ToLower() == "lang" || parameters[0].ToLower() == "language")
                HUD_DEBUG.instance.showSystemLanguage = !HUD_DEBUG.instance.showSystemLanguage;
            else if (parameters[0].ToLower() == "ms")
                HUD_DEBUG.instance.show_ms = !HUD_DEBUG.instance.show_ms;
            else if (parameters[0].ToLower() == "messages")
                HUD_DEBUG.instance.showCustomMessages = !HUD_DEBUG.instance.showCustomMessages;
            else
                HUD_DEBUG.instance.AddmessageToConsole("the parameter given isn't defined! try using \"fps\",\"lang\",\"ms\" or \"messages\"", HUD_DEBUG.ConsoleMessageType.MISSING_COMMAND);
        }

        CommandToggleScreenMessages(){
            name = "On screen messages toggle";
            command = "toggle";
            description = "Toggles the messages from the screen log.";
            help = "\n- Toggles the messages from the screen log.\n- Use \"fps\" to show/hide the fps counter.\n" +
            "- Use \"lang\" to show/hide the language counter.\n- Use \"ms\" to show/hide the CPU/GPU ms counter.\n- Use \"messages\" to show/hide all of the custom on screen logs.";

            AddCommand();
        }

        public static CommandToggleScreenMessages CreateCommand(){
            return new CommandToggleScreenMessages();
        }
    }
}
