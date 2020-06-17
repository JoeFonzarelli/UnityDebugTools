using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class HUD_DEBUG : MonoBehaviour
{

    public enum ConsoleMessageType
    {
        COMMAND, ERROR, WARNING, MISSING_COMMAND, LOG, HELP, INITIALIZATION
    }

    #region variables
    static ulong hash = 0; //prevent deleting the wrong volatile message
    struct Message
    {
        public string text;
        public Color color;

        ulong _hash;

        public Message(string text, Color color)
        {
            this.text = text;
            this.color = color;

            _hash = hash++;
            if (hash >= ulong.MaxValue-1) hash = ulong.MinValue; //avoid overflow
        }
    }

    struct RefMessage
    {
        public object obj;
        public FieldInfo fieldInfo;
        public PropertyInfo propertyInfo;
        public bool FieldReferenced; //if true, the user wants to print a field, else we are working with a property;
        public bool PropertyReferenced; //if true, the user wants to print a property, else we are working with a field. if both variables are null, print a default error message
        public Color color;
        ulong _hash;

        public string parameterName;
        public string prefix;

        public RefMessage(object obj, string parameterName, Color color, string prefix)
        {
            this.obj = obj;
            this.parameterName = parameterName;
            this.color = color;
            this.prefix = prefix;

            fieldInfo = this.obj.GetType().GetField(this.parameterName);
            propertyInfo = this.obj.GetType().GetProperty(this.parameterName);

            FieldReferenced = fieldInfo != null;
            PropertyReferenced = propertyInfo != null;
           
            _hash = hash++;
            if (hash >= ulong.MaxValue-1) hash = ulong.MinValue; //avoid overflow
        }
    }

    float deltaTime = 0.0f;

    bool flipflopColor = true; //alternate color between rows for better visibility
    Color flipflopColor_1 = Color.white, flipflopColor_2 = new Color(0.8f, 0.8f, 0.8f, 1);

    [Header("default print values")]
    public bool showFPS = true;
    public bool show_ms = true;
    public bool showSystemLanguage = true;
    public bool showGPU = true;

    public bool showCustomMessages = true;

    List<Message> tempMessages; //for messages printed temporarily 
    List<RefMessage> staticMessages; //for messages printed permanently
    List<Message> staticStringMessages; //for string messages printed permanently
    List<System.Tuple<RefMessage, Vector2>> staticMessagesAtPosition; //same as static messages, but placed in a custom position of the world
    List<System.Tuple<Message, Vector2>> staticStringMessagesAtPosition; //same as static string messages, but placed in a custom position of the world
    List<System.Tuple<Message, Vector2>> tempMessagesAtPosition; //same as temp messages, but placed in a custom position of the world

    internal int ypos;

    public Dictionary<string, ConsoleCommand> commands{get; private set;}


    static HUD_DEBUG _instance;
    public static HUD_DEBUG instance{
        get{
            if (_instance == null){
                
                GameObject Go = GameObject.Find("HUD_DEBUG");
                if (Go == null) Go = new GameObject("HUD_DEBUG");
                
                _instance = Go.GetComponent<HUD_DEBUG>();
                if (_instance == null) _instance = Go.AddComponent<HUD_DEBUG>();

                DontDestroyOnLoad(Go);
                _instance.init();
            }
            return _instance;
        }
        private set{
            _instance = value;
        }
    }

    bool toggleDeveloperConsole = false;
    Canvas developerConsole;
    TMPro.TextMeshProUGUI consoleText;
    Text consoleInputText;
    InputField consoleInput;


    internal bool toggleQuickButtons = false;
    int currentTab = 0;
    internal GameObject quickButtons; //the quickButtons container object
    GameObject[] tabs; //tab buttons on top, that enable/disable different panels
    GameObject[] panels; //panels that contain the options of every tab
    GameObject[] panelDefaultOption; //first button to select on every tab
 

    bool initialized = false;

    #endregion

    void init(){
        if (initialized) return;

        //1. init messages arrays
        tempMessages = new List<Message>();
        staticMessages = new List<RefMessage>();
        staticStringMessages = new List<Message>();
        staticMessagesAtPosition = new List<System.Tuple<RefMessage, Vector2>>();
        staticStringMessagesAtPosition = new List<System.Tuple<Message, Vector2>>();
        tempMessagesAtPosition = new List<System.Tuple<Message, Vector2>>();

        ypos = 0;

        //2. init developer console
        if (developerConsole == null)
            developerConsole = transform.GetComponentInChildren<Canvas>(true);

        if (developerConsole != null)
        {
            if (developerConsole.gameObject.activeSelf)
                developerConsole.gameObject.SetActive(false);

            consoleText = developerConsole.transform.Find("Panel/Scroll View/Viewport/Content/ConsoleText").GetComponent<TMPro.TextMeshProUGUI>();
            consoleInput = developerConsole.transform.Find("Panel/InputField").GetComponent<InputField>();
            consoleInputText = consoleInput.transform.Find("InputFieldText").GetComponent<Text>();
        }

        commands = new Dictionary<string, ConsoleCommand>();

        //3. init quick buttons panel
        if (quickButtons == null)
            quickButtons = transform.GetChild(1).gameObject;

        if (quickButtons != null)
        {
            if (quickButtons.activeSelf) quickButtons.SetActive(false);

            tabs = new GameObject[quickButtons.transform.GetChild(1).childCount];

            for (int i = 0; i < quickButtons.transform.GetChild(1).childCount; i++)
                tabs[i] = quickButtons.transform.GetChild(1).GetChild(i).gameObject;

            panels = new GameObject[quickButtons.transform.GetChild(2).childCount];

            for (int i = 0; i < quickButtons.transform.GetChild(2).childCount; i++)
                panels[i] = quickButtons.transform.GetChild(2).GetChild(i).gameObject;

            panelDefaultOption = new GameObject[panels.Length];

            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i].transform.GetChild(0).childCount > 0)
                    panelDefaultOption[i] = panels[i].transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
            }
        }

        initialized = true;

        CreateCommands();
    }

    void onSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode){
        UnityEngine.EventSystems.EventSystem eventSys = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSys == null){
            GameObject go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
        }
    }

    private void Start()
    {   
        if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += onSceneLoaded; //every time a scene is loaded, check if there is an event system Gameobject in the scene. If there is one, do nothing. If there is none, create one

    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= onSceneLoaded;
    }


    //make the commands that need an update function subscribe to this event
    public delegate void _OnUpdate();
    public static _OnUpdate OnUpdate;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f; //for fps counter

        //1. developer console
        //toggle developer console
        if (InputManager.instance.GetButtonDown("Toggle_Console", forceDetect: true) && !toggleQuickButtons){
            toggleDeveloperConsole = !toggleDeveloperConsole;
            developerConsole.gameObject.SetActive(toggleDeveloperConsole);

            if (toggleDeveloperConsole)
            {
                //avoid gameplay input
                //InputManager.instance.DisableMap("Default");
                //InputManager.instance.EnableMap("DEBUG");
                InputManager.instance.DisableInput(false);

                //automatically focus on the input field of the console
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(consoleInput.gameObject);
            }
            else
            {
                //restore gameplay input
                //InputManager.instance.EnableMap("Default");
                //InputManager.instance.DisableMap("DEBUG");
                InputManager.instance.EnableInput();
            }
        }
        if (toggleDeveloperConsole){ //if the console is open, check for input
            if (InputManager.instance.GetButtonDown("Console_Return", forceDetect:true)){
                if (consoleInputText.text != ""){ //if there is a message, send it to the console and execute the required command
                    AddmessageToConsole(consoleInputText.text, ConsoleMessageType.COMMAND);
                    bool success = ParseInput(consoleInputText.text);

                    if (success) consoleInputText.text = ""; //clear the input field if the command has been executed successfully

                    //reset the focus to the console input field
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(consoleInput.gameObject);
                }
            }
        }

        //2. quick buttons
        if (InputManager.instance.GetButtonDown("Toggle_QuickButtons", forceDetect: true) && !toggleDeveloperConsole){
            toggleQuickButtons = !toggleQuickButtons;
            quickButtons.SetActive(toggleQuickButtons);

            if (toggleQuickButtons)
            {
                //avoid gameplay input
                //InputManager.instance.DisableMap("Default");
                //InputManager.instance.EnableMap("DEBUG");
                InputManager.instance.DisableInput(false);

                //store the currently selected gameobject by the event system so we can restore it when this menu is closed
                prevSelectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                StartCoroutine(setSelectedTab());
            }
            else
            {
                ReestablishSystem_QuickButtons();
            }
        }
        if (toggleQuickButtons)
        {
            if (InputManager.instance.GetButtonDown("changeTabLeft", forceDetect: true))
            {
                --currentTab;
                if (currentTab < 0) currentTab = tabs.Length - 1;
                StartCoroutine(setSelectedTab());
            }
            else if (InputManager.instance.GetButtonDown("changeTabRight", forceDetect: true))
            {
                ++currentTab;
                if (currentTab > tabs.Length - 1) currentTab = 0;
                StartCoroutine(setSelectedTab());
            }
        }

        OnUpdate?.Invoke();
    }

    #region OnScreenMessages_Functions

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        ypos = 0;
        Rect rect;
        string text;
        GUIStyle style = new GUIStyle();

        #region defaultPrints
        if (showFPS)
        {
            ypos += 20;
            rect = new Rect(0, ypos, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 90;
            style.normal.textColor = flipflopColor ? flipflopColor_1 : flipflopColor_2;
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
            flipflopColor = !flipflopColor;
        }

        if (show_ms) {
            ypos += 20;
            rect = new Rect(0, ypos, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 90;
            style.normal.textColor = flipflopColor ? flipflopColor_1 : flipflopColor_2;

            FrameTiming[] timing = new FrameTiming[1];
            FrameTimingManager.CaptureFrameTimings();
            uint framesCaptured = FrameTimingManager.GetLatestTimings(1, timing);
            if (framesCaptured > 0)
                text = "cpu: " + timing[0].cpuFrameTime + "ms || gpu: " + timing[0].gpuFrameTime + "ms";
            else 
                text = "could not get frame timings";

            GUI.Label(rect, text, style);
            flipflopColor = !flipflopColor;
        }

        if (showSystemLanguage)
        {
            ypos += 20;
            rect = new Rect(0, ypos, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 90;
            style.normal.textColor = flipflopColor ? flipflopColor_1 : flipflopColor_2;
            text = "System language:  " + Application.systemLanguage.ToString();
            GUI.Label(rect, text, style);
            flipflopColor = !flipflopColor;
        }

        if (showGPU)
        {
            ypos += 20;
            rect = new Rect(0, ypos, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 90;
            style.normal.textColor = flipflopColor ? flipflopColor_1 : flipflopColor_2;
            text = "graphics Device:  " + SystemInfo.graphicsDeviceName + "  " + SystemInfo.graphicsDeviceVendor + "  " + SystemInfo.graphicsDeviceVersion + "  " + SystemInfo.graphicsMemorySize;
            GUI.Label(rect, text, style);
            flipflopColor = !flipflopColor;
        }

        #endregion

        #region userLists
        if (showCustomMessages){

            if (staticStringMessages.Count > 0){
                foreach (Message message in staticStringMessages)
                {
                    ypos += 20;
                    style.normal.textColor = message.color;
                    rect = new Rect(0, ypos, w, h * 2 / 100);
                    style.alignment = TextAnchor.UpperLeft;
                    style.fontSize = h * 2 / 90;
                    text = message.text;
                    GUI.Label(rect, text, style);
                }
            }

            if (staticMessages.Count > 0)
            {
                foreach (RefMessage message in staticMessages)
                {
                    ypos += 20;
                    style.normal.textColor = message.color;
                    rect = new Rect(0, ypos, w, h * 2 / 100);
                    style.alignment = TextAnchor.UpperLeft;
                    style.fontSize = h * 2 / 90;
                    text = message.prefix;
                    
                    if (message.FieldReferenced)
                        text += message.fieldInfo.GetValue(message.obj).ToString();
                    else if (message.PropertyReferenced)
                        text += message.propertyInfo.GetValue(message.obj).ToString();
                    else{
                        style.normal.textColor = Color.red;
                        text += "unaccessible variable! make sure it's a public variable and that the name given is written correctly (" + message.parameterName + ")";
                    }

                    GUI.Label(rect, text, style);
                }
            }

            if (tempMessages.Count > 0)
            {
                ypos += 20;
                foreach (Message message in tempMessages)
                {
                    ypos += 20;
                    style.normal.textColor = message.color;
                    rect = new Rect(0, ypos, w, h * 2 / 100);
                    style.alignment = TextAnchor.UpperLeft;
                    style.fontSize = h * 2 / 90;
                    text = message.text;
                    GUI.Label(rect, text, style);
                }
            }

            if (staticStringMessagesAtPosition.Count > 0)
            {
                foreach (System.Tuple<Message, Vector2> message in staticStringMessagesAtPosition)
                {
                    style.normal.textColor = message.Item1.color;
                    rect = new Rect(message.Item2.x, message.Item2.y, w, h * 2 / 100);
                    style.alignment = TextAnchor.UpperLeft;
                    style.fontSize = h * 2 / 90;
                    text = message.Item1.text;
                    GUI.Label(rect, text, style);
                }
            }

            if (staticMessagesAtPosition.Count > 0)
            {
                foreach(System.Tuple<RefMessage, Vector2> message in staticMessagesAtPosition)
                {
                    style.normal.textColor = message.Item1.color;
                    style.alignment = TextAnchor.UpperLeft;
                    rect = new Rect(message.Item2.x, message.Item2.y, w, h * 2 / 100);
                    style.fontSize = h * 2 / 90;
                    text = message.Item1.prefix;

                    if (message.Item1.FieldReferenced)
                        text += message.Item1.fieldInfo.GetValue(message.Item1.obj).ToString();
                    else if (message.Item1.PropertyReferenced)
                        text += message.Item1.propertyInfo.GetValue(message.Item1.obj).ToString();
                    else
                    {
                        style.normal.textColor = Color.red;
                        text += "unaccessible variable! make sure it's a public variable and that the name given is written correctly (" + message.Item1.parameterName + ")";
                    }
                }
            }

            if (tempMessagesAtPosition.Count > 0)
            {
                ypos += 20;
                foreach (System.Tuple<Message, Vector2> message in tempMessagesAtPosition)
                {
                    style.normal.textColor = message.Item1.color;
                    rect = new Rect(message.Item2.x, message.Item2.y, w, h * 2 / 100);
                    style.alignment = TextAnchor.UpperLeft;
                    style.fontSize = h * 2 / 90;
                    text = message.Item1.text;
                    GUI.Label(rect, text, style);
                }
            }
        }
        #endregion
    }

    public void printString_volatile(object value, Color? color = null, string prefix = "") //prints a string that will disappear after tempMessages_dt seconds
    {
        string _value = prefix + " ";
        _value += convertTypeToString(value);

        StartCoroutine(printString_coroutine(_value, color));
    }

    public void printString_volatile(object value, Vector2 position, Color? color = null, string prefix = "") //prints a string that will disappear after tempMessages_dt seconds
    {
        string _value = prefix + " ";
        _value += convertTypeToString(value);

        StartCoroutine(printString_coroutine(_value, color));
    }

    public void printString_static(object value, Color? color = null, string prefix = "") //prints a fixed message that will stay on screen forever
    {
        string _value = prefix + " ";
        _value += convertTypeToString(value);

        staticStringMessages.Add(new Message(_value, color ?? Color.blue));
    }

    public void printString_static(object value, Vector2 position, Color? color = null, string prefix = "") //prints a fixed message that will stay on screen forever in a position defined by the user
    {
        string _value = prefix + " ";
        _value += convertTypeToString(value);

        staticStringMessagesAtPosition.Add(new System.Tuple<Message, Vector2>(new Message(_value, color ?? Color.blue), position));
    }

    public void printString_static(in object value, string fieldValue, string prefix = "", Color? color = null) //prints a variable message that will stay on screen forever
    {
        string _value = prefix + " ";
        _value += convertTypeToString(value);

        staticMessages.Add(new RefMessage(_value, fieldValue, color ?? Color.cyan, prefix));   
    }

    public void printString_static(in object value, string fieldValue, Vector2 position, string prefix = "", Color? color = null) //prints a variable message that will stay on screen forever in a position defined by the user
    {
        string _value = prefix + " ";
        _value += convertTypeToString(value);

        staticMessagesAtPosition.Add(new System.Tuple<RefMessage, Vector2>(new RefMessage(_value, fieldValue, color ?? Color.cyan, prefix), position));
    }

    string convertTypeToString(object value){
        System.Type valueType = value.GetType();
        string _value = "";
        if (valueType == typeof(System.Int32) ||
            valueType == typeof(System.Int16) ||
            valueType == typeof(System.Int64) ||
            valueType == typeof(System.UInt16) ||
            valueType == typeof(System.UInt32) ||
            valueType == typeof(System.UInt64) ||
            valueType == typeof(System.SByte) ||
            valueType == typeof(System.Byte) ||
            valueType == typeof(System.Single) ||
            valueType == typeof(System.Decimal) ||
            valueType == typeof(System.Double))
            _value += value.ToString();
        else if (valueType == typeof(System.Boolean))
            _value += ((bool)value) ? "true" : "false";
        else if (valueType == typeof(System.String) ||
                 valueType == typeof(System.Char))
            _value += (string)value;
        else if (valueType == typeof(System.Text.StringBuilder))
            _value += value.ToString();
        else if (valueType == typeof(Transform))
        {
            Transform temp = (Transform)value;
            _value += "position: " + temp.position + " | rotation: " + temp.rotation + " | scale: " + temp.lossyScale;
        }
        else if (valueType == typeof(MeshFilter))
        {
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(TextMesh))
        {
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(MeshRenderer))
        {
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(SkinnedMeshRenderer))
        {
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(ParticleSystem))
        {
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(TrailRenderer))
        {
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(LineRenderer))
        {
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(LensFlare))
        {
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(Projector))
        {
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(Behaviour))
        {
            Behaviour temp = (Behaviour)value;
            Debug.Log("working on support  (behaviour " + temp.name + ")");
        }
        else if (valueType == typeof(MonoBehaviour))
        {
            MonoBehaviour temp = (MonoBehaviour)value;
            _value += "script : '" + temp.name + "' is" + ((temp.enabled) ? " " : " not ") + " enabled";
        }
        else
            _value += "Data type not supported!!! (" + valueType + ")";

        return _value;
    }

    [Space(30)]
    //create a message with a given color and add it to the list that will be used in the update function for being printed on screen.
    //wait for the given deltaTime for it to be erased from the list and not being printed anymore
    [SerializeField] float tempMessages_dt = 5;
    IEnumerator printString_coroutine(string value, Color? color = null) 
    {
        Color message_color = color ?? Color.yellow;
        float time = 0;

        Message message = new Message(value, message_color);

        if (!tempMessages.Contains(message))
            tempMessages.Add(message);

        while (time < tempMessages_dt)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (tempMessages.Contains(message))
            tempMessages.PopAt(tempMessages.IndexOf(message));
    }
    //same as above, but used for messages at a given position
    IEnumerator printString_coroutine(string value, Vector2 position, Color? color = null)
    {
        Color message_color = color ?? Color.yellow;
        float time = 0;

        System.Tuple<Message, Vector2> message = new System.Tuple<Message, Vector2>(new Message(value, message_color), position);

        if (!tempMessagesAtPosition.Contains(message))
            tempMessagesAtPosition.Add(message);

        while (time < tempMessages_dt)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (tempMessagesAtPosition.Contains(message))
            tempMessagesAtPosition.PopAt(tempMessagesAtPosition.IndexOf(message));
    }

    #endregion

    #region developerConsole_Functions
    private void CreateCommands(){ //CREATE COMMANDS TO BE ADDED TO THE CONSOLE HERE
        ConsoleCommands.CommandQuit.CreateCommand();
        ConsoleCommands.CommandToggleScreenMessages.CreateCommand();
        ConsoleCommands.CommandQualitySettings.CreateCommand();
        ConsoleCommands.CommandToggleFullscreen.CreateCommand();
        ConsoleCommands.CommandChangeResolution.CreateCommand();
    }

    public void AddCommandToConsole(string _name, ConsoleCommand _command){
        if (!commands.ContainsKey(_name)) commands.Add(_name, _command);
    }

    
    public void AddmessageToConsole(string msg, ConsoleMessageType type){

        if (!initialized) init();

        string color = "";
        switch (type){
            case ConsoleMessageType.COMMAND:
            color = "<color=#66ff66>"; 
            break;
            case ConsoleMessageType.ERROR:
            color = "<color=#cc0000>"; 
            break;
            case ConsoleMessageType.LOG:
            color = "<color=#e6e6e6>"; 
            break;
            case ConsoleMessageType.MISSING_COMMAND:
            color = "<color=#ff9966>"; 
            break;
            case ConsoleMessageType.WARNING:
            color = "<color=#ffcc00>"; 
            break;
            case ConsoleMessageType.INITIALIZATION:
            color = "<color=#cceeff>"; 
            break;
            case ConsoleMessageType.HELP:
            color = "<color=#b3d9ff>"; 
            break;
        }
        consoleText.text += color + msg + "</color>\n"; //add the text and create a new line
    }

    bool ParseInput(string input){ //returns if the command has been executed successfully
        string[] _input = input.Split(null); //split by spaces

        if (_input.Length == 0 || _input == null) //no input found. leave
            AddmessageToConsole("Command not recognized!", ConsoleMessageType.MISSING_COMMAND);
        else if (!commands.ContainsKey(_input[0])){ //the command doesn't exist. check if it's a console keyword and execute. otherwise launch the missing command message
            
            if (_input[0].ToLower() == "info" || _input[0].ToLower() == "help" || _input[0] == "?"){ //show a list of commands with their description
                string message = "SHOWING ALL COMMANDS AVAILABLE: ";
                for (int i = 0; i < commands.Count; i++) message += "\n"+(i+1)+". "+ commands.Values.ElementAt(i).command + " : " + commands.Values.ElementAt(i).description;
                AddmessageToConsole(message, ConsoleMessageType.HELP);
                return true;
            }
            else if (_input[0].ToLower() == "cls" || _input[0].ToLower() == "clear" || _input[0] == "delete"){ //clear the console
                consoleText.text = "";
                return true;
            }
            else
                AddmessageToConsole("Command not recognized!", ConsoleMessageType.MISSING_COMMAND);
        }
        else{ //the command exists, execute it
            
            if (_input.Length > 1){ //check if there are parameters or if the user requested help or description for the command. if not send the other elements as parameters
                if (_input[1].ToLower() == "help" || _input[1] == "?") commands[_input[0]].showHelp(); //show the help message
                else if (_input[1].ToLower() == "description" || _input[1].ToLower() == "info") commands[_input[0]].showDescription(); //show the command description
                else{
                    string[] parameters = new string[_input.Length-1];
                    System.Array.Copy(_input, 1, parameters, 0, _input.Length-1);
                    commands[_input[0]].runCommand(parameters);
                }
            }
            else{ //run a parameterless command
                commands[_input[0]].runCommand(null);
            }

            return true;
        }

        return false;
    }

    void HandleLog(string logMessage, string stackTrace, LogType type){ //get messages from unity console in the developer console
        if (!initialized) return;
        ConsoleMessageType messageType = ConsoleMessageType.LOG;
        switch(type){
            case LogType.Error:
            case LogType.Exception:
                messageType = ConsoleMessageType.ERROR;
                break;
            case LogType.Warning:                
                messageType = ConsoleMessageType.WARNING;
                break;
            case LogType.Log:
                messageType = ConsoleMessageType.LOG;
                break;
        }

        string _message = "[" + type.ToString() + "] " + logMessage + "\n(" +stackTrace +")";
        AddmessageToConsole(_message, messageType);
    }

    #endregion

    #region QuickButtons_Functions

    //once the quick buttons menu is closed, return the control to the player and reset all values to it's gameplay defaults
    public void ReestablishSystem_QuickButtons()
    {
        //reset the previously selected gameobject in the event system
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(prevSelectedObject);
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = prevSendNavigationEvents;

        //restore gameplay input
        //InputManager.instance.EnableMap("Default");
        //InputManager.instance.DisableMap("DEBUG");
        InputManager.instance.EnableInput();

        //reenable other canvases present in the scene
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            GameObject[] gos = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects();
            for (int j = 0; j < gos.Length; j++)
            {
                CanvasGroup canvas = gos[j].GetComponent<CanvasGroup>();
                if (canvas != null) canvas.interactable = true;
            }
        }
    }

    GameObject prevSelectedObject; //gameobject selected by the eventSystem before accessing the debug menu
    bool prevSendNavigationEvents = true; //prev value of this property of the eventsystem, so we make sure we reestablish the status quo when leaving the quick buttons menu
    IEnumerator setSelectedTab()
    {
        //1. highlight the tab title and enable the panel
        for (int i = 0; i < tabs.Length; i++){
            bool isCurrentTab = i == currentTab;
            
            tabs[i].GetComponent<TMPro.TextMeshProUGUI>().color = isCurrentTab ? Color.yellow : Color.white;
            panels[i].SetActive(isCurrentTab);
        }

        yield return null;

        //2. disable other canvases present in the scene
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            GameObject[] gos = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects();
            for (int j = 0; j < gos.Length; j++)
            {
                CanvasGroup canvas = gos[j].GetComponent<CanvasGroup>();
                if (canvas != null) canvas.interactable = false;
            }
        }

        yield return null;

        //3. automatically focus on the first item of the tab
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(panelDefaultOption[currentTab]);
        prevSendNavigationEvents = UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents;
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = true;
    }

    #endregion
}


static class ListExtension
{
    public static T PopAt<T>(this List<T> list, int index)
    {
        T r = list[index];
        list.RemoveAt(index);
        return r;
    }
}

public abstract class ConsoleCommand
{
    public string name{get; protected set;}
    public string command{get; protected set;}
    public string description{get; protected set;} //short description of the functionality
    public string help{get; protected set;} //extensive info about the functionality of the command

    public void AddCommand(){
        HUD_DEBUG.instance.AddCommandToConsole(command, this);
        HUD_DEBUG.instance.AddmessageToConsole(name + " command has been added to the console.", HUD_DEBUG.ConsoleMessageType.INITIALIZATION);
    }

    public abstract void runCommand(string[] paremeters);

    public void showHelp(){
        HUD_DEBUG.instance.AddmessageToConsole(name + " command help: " + help, HUD_DEBUG.ConsoleMessageType.HELP);
    }
    public void showDescription(){
        HUD_DEBUG.instance.AddmessageToConsole(name + " command description: " + description, HUD_DEBUG.ConsoleMessageType.HELP);
    }
}