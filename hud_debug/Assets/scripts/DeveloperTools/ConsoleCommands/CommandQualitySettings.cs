namespace ConsoleCommands
{
    public class CommandQualitySettings : ConsoleCommand
    {
        public override void runCommand(string[] parameters)
        {
            if (parameters.Length > 0)
            {
                if (parameters[0].ToLower() == "get")
                    HUD_DEBUG.instance.AddmessageToConsole("quality setting currently in use: " + UnityEngine.QualitySettings.names[UnityEngine.QualitySettings.GetQualityLevel()], HUD_DEBUG.ConsoleMessageType.LOG);
                
                else if (parameters[0].ToLower() == "info")
                {
                    HUD_DEBUG.instance.AddmessageToConsole("available quality settings: ", HUD_DEBUG.ConsoleMessageType.LOG);
                    for (int i = 0; i < UnityEngine.QualitySettings.names.Length; i++)
                    {
                        HUD_DEBUG.instance.AddmessageToConsole("- " + UnityEngine.QualitySettings.names[i], HUD_DEBUG.ConsoleMessageType.LOG);
                    }
                }

                else if (parameters[0].ToLower() == "set")
                {
                    bool found = false;
                    for (int i = 0; i < UnityEngine.QualitySettings.names.Length; i++)
                    {
                        if (parameters[1].ToLower() == UnityEngine.QualitySettings.names[i].ToLower())
                        {
                            UnityEngine.QualitySettings.SetQualityLevel(i, true);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        HUD_DEBUG.instance.AddmessageToConsole("QUALITY SETTING NOT FOUND!!!", HUD_DEBUG.ConsoleMessageType.ERROR);
                        HUD_DEBUG.instance.AddmessageToConsole("available quality settings: ", HUD_DEBUG.ConsoleMessageType.LOG);
                        for (int i = 0; i < UnityEngine.QualitySettings.names.Length; i++)
                        {
                            HUD_DEBUG.instance.AddmessageToConsole("- " + UnityEngine.QualitySettings.names[i], HUD_DEBUG.ConsoleMessageType.LOG);
                        }
                    }
                }
            }
        }

        CommandQualitySettings()
        {
            name = "Quality";
            command = "quality";
            description = "Use this command to check and change the quality settings.";
            help = "\n- This command will change the quality settings.\n- Use get to display the name of the quality setting in use. \n- Use info to get the names of all the available settings. \n- Use set + settingname to change the quality setting. \nWARNING: This command may cause the game to freeze momentarily!!!";

            AddCommand();
        }

        public static CommandQualitySettings CreateCommand()
        {
            return new CommandQualitySettings();
        }
    }
}
