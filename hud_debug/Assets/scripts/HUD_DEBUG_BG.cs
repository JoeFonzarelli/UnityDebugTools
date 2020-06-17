using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD_DEBUG_BG : MonoBehaviour {

	static HUD_DEBUG_BG _instance;

	Texture2D box_bg;
	GUIStyle box_style;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;

            if (box_bg == null)
                box_bg = new Texture2D(1, 1);
            if (box_style == null)
                box_style = new GUIStyle();
            return;
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }
    }
	
	// Update is called once per frame
	void OnGUI() {
		
		GUI.depth = 1000;

        box_bg.SetPixel(0,0, new Color(0f, 0f, 0f, 0.4f));
		box_bg.Apply();

		box_style.normal.background = box_bg;
		GUI.Box(new Rect(0, 0, 1000, HUD_DEBUG.instance.ypos), GUIContent.none, box_style);
	} 
}
