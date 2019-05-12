using UnityEngine;
using UnityEditor;

public class Radar_Window : EditorWindow
{
    public enum MinimapMode{
        CAMERA_RENDER,
        CUSTOM_TEXTURE,
        CUSTOM_TEXTURE_OPTIMIZED
    }

    public enum MinimapMask{
        SQUARE,
        CIRCLE,
        CUSTOM
    }

    public struct CustomWaypoint{
        public string tag;
        public Sprite waypointImage;
        public int depth;
    };

    public enum CameraProjectionType{
        Perspective, Orthographic
    }

    Vector2 scrollviewPos = Vector2.zero;

    bool createCanvas = false;
    bool canvasFoldOut = true;
    UnityEngine.UI.CanvasScaler.ScaleMode CanvasScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
    Vector2Int CanvasReferenceResolution = new Vector2Int(1920, 1080);
    RenderMode CanvasRenderMode = RenderMode.ScreenSpaceOverlay;


    bool createCamera = false;
    CameraProjectionType cameraProjection = CameraProjectionType.Perspective;
    RenderTexture renderTex;
    

    MinimapMode minimapMode = MinimapMode.CAMERA_RENDER;
    MinimapMask minimapMask = MinimapMask.SQUARE;

    Sprite CustomMask = null;
    Sprite CustomTexture = null;
    
    GUIStyle style;

    [MenuItem("Window/Custom Radar")]
	public static void ShowWindow ()
	{
		Radar_Window window = GetWindow<Radar_Window>("Custom Radar");
        window.minSize = new Vector2(300, 100);
	}

    private void OnGUI()
    {   
        scrollviewPos = EditorGUILayout.BeginScrollView(scrollviewPos);

        style = new GUIStyle(EditorStyles.largeLabel);
        style.fontStyle = FontStyle.Bold;
        style.fixedWidth = 120;

        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Canvas options", style);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        createCanvas = EditorGUILayout.Toggle("Create new canvas", createCanvas);
        if (createCanvas)
        {
            GUILayout.Space(5);
            style = new GUIStyle(EditorStyles.foldoutHeader);
            style.fixedWidth = 185;
            style.fontStyle = FontStyle.Bold;
            canvasFoldOut = EditorGUILayout.Foldout(canvasFoldOut, "Canvas advanced options", true, style);
            if (canvasFoldOut)
            {
                CanvasOptions();
                GUILayout.Space(5);
            }
        }
    
        style = new GUIStyle(EditorStyles.largeLabel);
        style.fontStyle = FontStyle.Bold;
        style.fixedWidth = 120;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Minimap options", style);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    
        GUILayout.Space(15);

        
        minimapMode = (MinimapMode)EditorGUILayout.EnumPopup("minimap version", minimapMode);

        switch(minimapMode){
            case MinimapMode.CAMERA_RENDER:
            createCamera = EditorGUILayout.Toggle("Create camera?", createCamera);
            if (createCamera){
                cameraProjection = (CameraProjectionType)EditorGUILayout.EnumPopup("Projection", cameraProjection);
                
                //define culling mask 
            
                if (cameraProjection == CameraProjectionType.Orthographic){
                    //define size
                }
                else{
                    //define fov
                }   
            }
            break;
            case MinimapMode.CUSTOM_TEXTURE:
                CustomTexture = (Sprite)EditorGUILayout.ObjectField("Map texture",CustomTexture, typeof(Sprite), false);
            break;
        }

        //list of custom waypoints to show on map. 

        //image for player

        //show icons on border?

        //compass

        //minimap position
        //minimap size

        //border?
        //custom border?

        //Create radar button
        CreateButton();

        EditorGUILayout.EndScrollView();
    }

    private void CreateButton()
    {
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        style = new GUIStyle(GUI.skin.button);
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 12;

        if (GUILayout.Button("Create radar!", style, GUILayout.Width(Screen.width * 2 / 3), GUILayout.Height(30)))
        {
            createRadar();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }


    private void CanvasOptions()
    {   
        CanvasScaleMode = (UnityEngine.UI.CanvasScaler.ScaleMode)EditorGUILayout.EnumPopup("Canvas Scale Mode", CanvasScaleMode);

        switch (CanvasScaleMode)
        {
            case UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize:
                CanvasReferenceResolution = EditorGUILayout.Vector2IntField("Reference resolution", CanvasReferenceResolution);
                CanvasRenderMode = (RenderMode)EditorGUILayout.EnumPopup("Canvas Render Mode", CanvasRenderMode);
                break;
            case UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize:
                break;
            case UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPhysicalSize:
                break;
        }
    }
    
    void createRadar(){

        if (minimapMode == MinimapMode.CAMERA_RENDER){
            //autocreate rendertexture and apply camera to it
            renderTex = new RenderTexture(512 , 512, 16, RenderTextureFormat.ARGB32);
            renderTex.Create();
            AssetDatabase.CreateAsset(renderTex, "Assets/InstaMap/Minimap_RenderTex.renderTexture");
        }

        Canvas[] canvasList = GameObject.FindObjectsOfType<Canvas>();
        GameObject canvas;
        if (canvasList.Length == 0){
            canvas = new GameObject("Minimap_Canvas");
            canvas.AddComponent(typeof(RectTransform));
            canvas.AddComponent(typeof(Canvas));
            canvas.AddComponent(typeof(UnityEngine.UI.CanvasScaler));
            canvas.AddComponent(typeof(UnityEngine.UI.GraphicRaycaster));

            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = CanvasScaleMode;
            canvas.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution = CanvasReferenceResolution;
        }
        else
            canvas = canvasList[0].gameObject;
            
        GameObject go = new GameObject("radar");
        go.transform.parent = canvas.transform;
    }
}
