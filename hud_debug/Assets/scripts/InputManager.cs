using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InputManager : MonoBehaviour
{    
    public bool isInputEnabled { get; private set; } = true;


    static InputManager _instance;
    public static InputManager instance{
        get{
            if (_instance == null){
                GameObject newGo = new GameObject("InputManager");
                DontDestroyOnLoad(newGo);
                _instance = newGo.AddComponent<InputManager>();
                _instance.init();
            }

            return _instance;
        }
    }

    bool initialized = false;
    void init()
    {
        if (initialized) return;

        initialized = true;
    }

    public bool GetButtonDown(string actionName, bool forceDetect = false /*if true, skip the inputEnabled condition*/){
        if (!forceDetect)
            if (!isInputEnabled) return false; 
        
        return Input.GetButtonDown(actionName);
    }
    public bool GetButtonUp(string actionName, bool forceDetect = false /*if true, skip the inputEnabled condition*/){
        if (!forceDetect)
            if (!isInputEnabled) return false;
        
        return Input.GetButtonUp(actionName);

    }
    public bool GetButton(string actionName, bool forceDetect = false /*if true, skip the inputEnabled condition*/){
        if (!forceDetect)
            if (!isInputEnabled) return false;

        return Input.GetButton(actionName);

    }

    public float GetAxis(string actionName, bool forceDetect = false /*if true, skip the inputEnabled condition*/){
        if (!forceDetect)
            if (!isInputEnabled) return 0;

        return Input.GetAxis(actionName);
    }

    //public void EnableMap(string categoryName, bool isolate = false){        
    //    if (isolate) player.controllers.maps.SetAllMapsEnabled(false);      
    //}

    //public void DisableMap(string categoryName){                
    //    player.controllers.maps.SetMapsEnabled(false, categoryName);
    //}

    //public void DisableAllMaps(){
    //    player.controllers.maps.SetAllMapsEnabled(false);
    //}

    public void DisableInput(bool disableEventSystems = false) //disable all input coming from the player
    {
        isInputEnabled = false;
        if (disableEventSystems) UnityEngine.EventSystems.EventSystem.current.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>().enabled = false;
    }

    public void EnableInput() //enable all input coming from the player
    {
        isInputEnabled = true;
        UnityEngine.EventSystems.EventSystem.current.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>().enabled = true;
    }
}