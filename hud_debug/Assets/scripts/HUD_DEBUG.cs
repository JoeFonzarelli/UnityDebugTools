using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HUD_DEBUG : MonoBehaviour
{   

    static ulong hash = 0; //prevent deleting the wrong volatile message
    struct Message{
        public string text;
        public Color color;

        ulong _hash;

        public Message(string text, Color color)
        {
            this.text = text;
            this.color = color;
            _hash = hash++;
        }

    }

    float deltaTime = 0.0f;

	bool flipflopColor = true; //alternate color between rows for better visibility
	Color flipflopColor_1 = Color.white, flipflopColor_2 = new Color(0.8f, 0.8f, 0.8f, 1);

	[Header("default print values")]
	[SerializeField] bool showFPS = true;
    [SerializeField] bool show_ms = true;
	[SerializeField] bool showSystemLanguage = true;


    List<Message> tempMessages; //for messages printed temporarily 
    List<Message> staticMessages; //for messages printed permanently
    

    private int _ypos;
    public int ypos{
        get {return _ypos;}

        set {_ypos = value;} 
    }

    public static HUD_DEBUG _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);

            tempMessages = new List<Message>();
            staticMessages = new List<Message>();
            return;
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }
        ypos = 0;
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
		ypos = 0;
		Rect rect;
		string text;
        GUIStyle style = new GUIStyle();

		if (showFPS) {
            ypos += 20;
            rect = new Rect (0, ypos, w, h * 2 / 100);
			style.alignment = TextAnchor.UpperLeft;
			style.fontSize = h * 2 / 90;
			style.normal.textColor = flipflopColor?flipflopColor_1:flipflopColor_2;
			float msec = deltaTime * 1000.0f;
			float fps = 1.0f / deltaTime;
			text = string.Format ("{0:0.0} ms ({1:0.} fps)", msec, fps);
			GUI.Label (rect, text, style);
			flipflopColor = !flipflopColor;
		}

        // if (show_ms) {
        //     ypos += 20;
        //     rect = new Rect(0, 0, w, h * 2 / 100);
        //     style.alignment = TextAnchor.UpperLeft;
        //     style.fontSize = h * 2 / 90;
        //     style.normal.textColor = flipflopColor ? flipflopColor_1 : flipflopColor_2;

        //     FrameTiming timing = new FrameTiming();
        //     text = "cpu: " + timing.cpuFrameTime + "ms || gpu: " + timing.gpuFrameTime + "ms";
        //     GUI.Label(rect, text, style);
        //     flipflopColor = !flipflopColor;
        // }

        if (showSystemLanguage){
			ypos+=20;
			rect = new Rect(0, ypos, w, h * 2 / 100);
	        style.alignment = TextAnchor.UpperLeft;
			style.fontSize = h * 2 / 90;
			style.normal.textColor = flipflopColor?flipflopColor_1:flipflopColor_2;
			text = "System language:  "+ Application.systemLanguage.ToString();
			GUI.Label(rect, text, style);
			flipflopColor = !flipflopColor;
		}

        if(staticMessages.Count > 0){
            foreach(Message message in staticMessages)
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

        
        if (tempMessages.Count > 0) { 
            ypos+=20;
            foreach(Message message in tempMessages)
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
    }


    public void printString_volatile(object value, Color? color = null, string prefix = "") //prints a string that will disappear after tempMessages_dt seconds
    {   
        string _value = prefix + " ";
        System.Type valueType = value.GetType();
        if (valueType == typeof(System.Int32) ||
            valueType == typeof(System.Int16) ||
            valueType == typeof(System.Int64) ||
            valueType == typeof(System.UInt16)||
            valueType == typeof(System.UInt32)||
            valueType == typeof(System.UInt64)||
            valueType == typeof(System.SByte) ||
            valueType == typeof(System.Byte)  ||
            valueType == typeof(System.Single)||
            valueType == typeof(System.Decimal)||
            valueType == typeof(System.Double))
            _value += value.ToString();
        else if (valueType == typeof(System.Boolean))
            _value += ((bool)value)?"true":"false";
        else if (valueType == typeof(System.String) ||
                 valueType == typeof(System.Char))
            _value += (string)value;
        else if (valueType == typeof(System.Text.StringBuilder))
            _value += value.ToString();
        else if (valueType == typeof(Transform)){
            Transform temp = (Transform)value;
            _value += "position: " + temp.position + " | rotation: " + temp.rotation + " | scale: " + temp.lossyScale;
        }
        else if (valueType == typeof(MeshFilter)){
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(TextMesh)){
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(MeshRenderer)){
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(SkinnedMeshRenderer)){
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(ParticleSystem)){
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(TrailRenderer)){
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(LineRenderer)){
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(LensFlare)){
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(Projector)){
            Debug.Log("working on support " + valueType);
        }
        else if (valueType == typeof(Behaviour)){
            Behaviour temp = (Behaviour)value;
            Debug.Log("working on support  (behaviour " + temp.name +")");
        }
        else if (valueType == typeof(MonoBehaviour)){
            MonoBehaviour temp = (MonoBehaviour)value;
            _value += "script : '" + temp.name + "' is" + ((temp.enabled)?" ":" not ") + " enabled";
        }
        else
            _value += "Data type not supported!!! (" +valueType+")";

        StartCoroutine(printString_coroutine(_value, color));
    }

    public void printString_static(string value, Color? color = null){

    }

    public void printString_static(ref object value, Color? color = null){

    }

    [Space(30)]
    [SerializeField] float tempMessages_dt = 5;
    IEnumerator printString_coroutine(string value, Color? color = null)
    {
        Color message_color = color ?? Color.yellow;
		float time = 0;

        Message message = new Message(value, message_color);

        if (!tempMessages.Contains(message))
            tempMessages.Add(message);

		while (time < tempMessages_dt) {
			time += Time.deltaTime;
			yield return null;
		}

        if (tempMessages.Contains(message))
            tempMessages.PopAt(tempMessages.IndexOf(message));
	}
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