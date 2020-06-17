namespace ConsoleCommands
{
    public class CommandChangeResolution : ConsoleCommand
    {
        public override void runCommand(string[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                if (parameters[0].ToLower() == "get") HUD_DEBUG.instance.AddmessageToConsole(UnityEngine.Screen.currentResolution.ToString(), HUD_DEBUG.ConsoleMessageType.LOG);
                else if (parameters[0].ToLower() == "available")
                {
                    HUD_DEBUG.instance.AddmessageToConsole("Available resolutions:", HUD_DEBUG.ConsoleMessageType.LOG);
                    for (int i = 0; i < UnityEngine.Screen.resolutions.Length; i++)
                        HUD_DEBUG.instance.AddmessageToConsole(i + ") " + UnityEngine.Screen.resolutions[i].ToString(), HUD_DEBUG.ConsoleMessageType.LOG);
                }
                else if (int.TryParse(parameters[0], out int desiredHeight)){
                    //1. check if the value given is a height value
                    for (int i = 0; i < UnityEngine.Screen.resolutions.Length; i++)
                    {
                        if (UnityEngine.Screen.resolutions[i].height == desiredHeight)
                        {
                            UnityEngine.Screen.SetResolution(UnityEngine.Screen.resolutions[i].width, UnityEngine.Screen.resolutions[i].height, UnityEngine.Screen.fullScreen);
                            HUD_DEBUG.instance.AddmessageToConsole("resolution changed to: " + UnityEngine.Screen.resolutions[i].ToString(), HUD_DEBUG.ConsoleMessageType.LOG);
                            return;
                        }
                    }

                    //2. there is no resolution available with the given integer. check if it corresponds to an index of the array and assign that resolution instead 
                    if (desiredHeight < UnityEngine.Screen.resolutions.Length)
                    {
                        UnityEngine.Screen.SetResolution(UnityEngine.Screen.resolutions[desiredHeight].width, UnityEngine.Screen.resolutions[desiredHeight].height, UnityEngine.Screen.fullScreen);
                        HUD_DEBUG.instance.AddmessageToConsole("resolution changed to: " + UnityEngine.Screen.resolutions[desiredHeight].ToString(), HUD_DEBUG.ConsoleMessageType.LOG);
                        return;
                    }

                    //3. value given is not valid. leave
                    HUD_DEBUG.instance.AddmessageToConsole(desiredHeight + " is not a valid value! make sure it it a valid value", HUD_DEBUG.ConsoleMessageType.ERROR);
                    HUD_DEBUG.instance.AddmessageToConsole("Available resolutions:", HUD_DEBUG.ConsoleMessageType.LOG);
                    for (int i = 0; i < UnityEngine.Screen.resolutions.Length; i++)
                        HUD_DEBUG.instance.AddmessageToConsole(i + ") " + UnityEngine.Screen.resolutions[i].ToString(), HUD_DEBUG.ConsoleMessageType.LOG);
                }
                else
                    HUD_DEBUG.instance.AddmessageToConsole("parameter not recognized!", HUD_DEBUG.ConsoleMessageType.ERROR);

            }
            else //set the resolution to the next value in the array
            {

                int currentResolution = 0;
                UnityEngine.Debug.Log(UnityEngine.Screen.width + "x" + UnityEngine.Screen.height);
                for (int i = 0; i < UnityEngine.Screen.resolutions.Length; i++)
                {
                    if (UnityEngine.Screen.resolutions[i].refreshRate != 60) continue;
                    UnityEngine.Debug.Log(i + " - " + UnityEngine.Screen.resolutions[i].ToString());
                    if (UnityEngine.Screen.resolutions[i].width == UnityEngine.Screen.width &&
                        UnityEngine.Screen.resolutions[i].height == UnityEngine.Screen.height)
                    {
                        UnityEngine.Debug.Log("found! " + i);
                        currentResolution = i;
                        break;
                    }
                }

                ++currentResolution;
                if (currentResolution >= UnityEngine.Screen.resolutions.Length) currentResolution = 0;

                UnityEngine.Screen.SetResolution(UnityEngine.Screen.resolutions[currentResolution].width, UnityEngine.Screen.resolutions[currentResolution].height, UnityEngine.Screen.fullScreen);
                HUD_DEBUG.instance.AddmessageToConsole("resolution changed to: " + UnityEngine.Screen.resolutions[currentResolution].ToString() + " (" + currentResolution + ")", HUD_DEBUG.ConsoleMessageType.LOG);
            }
        }

        CommandChangeResolution()
        {
            name = "Resolution";
            command = "resolution";
            description = "Use this command to change the resolution.";
            help = "\n- This command will change the screen resolution().\n- This command won't work on SWITCH or consoles.\n- pass as parameter any height value to change to a 16:9 resolution with that window height (if available). " +
                "The index of the resolution can also be given (retrieve the indexes with the command \"resolution available\". if no parameter is given, the resolutions will be set the order shown automatically" +
                "\n- use the parameter \"get\" to get the current display resolution";

            AddCommand();
        }

        public static CommandChangeResolution CreateCommand()
        {
            return new CommandChangeResolution();
        }
    }
}
