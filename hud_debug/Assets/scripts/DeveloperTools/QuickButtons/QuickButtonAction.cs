using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class QuickButtonAction : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    Button button;
    internal TMPro.TextMeshProUGUI displayValue;

    ConsoleCommand command;

    [Space(10)]
    public string commandName;

    [Space(5)]
    public string[] commandParameters; //if there are different parameters to select from, add them all here
    internal int currentParameter;

    [Space(10)]
    public bool turnOffPanelWhenButtonPressed = false; //if true, disables the panel as soon as this option is used

    [Space(10)]
    public bool allowInMainTitle = true; //if true, allow this command to be used in the main title scene
    public bool allowInGameplay = true; //if true, allow this command to be used in the gameplay scene

    bool isSelected = false; //is this button selected?

    // Start is called before the first frame update
    public virtual void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        command = HUD_DEBUG.instance.commands[commandName];

        if (transform.parent.childCount == 2) displayValue = transform.parent.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>(); //there is a tmpro object alongside the button. take it and use it to display the value
        if (displayValue != null) SetDisplayValue();

        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetSceneAt(0), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        //bool mainTitleLoaded = false;
        //bool gameplayLoaded = false;

        //for(int i = 0; i < SceneManager.sceneCount; i++){
        //    if (SceneManager.GetSceneAt(i).name == "MainTitle") mainTitleLoaded = true;
        //    else if (SceneManager.GetSceneAt(i).name == "Gameplay") gameplayLoaded = true;
        //}

        //if (mainTitleLoaded && allowInMainTitle) button.interactable = true;
        //else if (gameplayLoaded && allowInGameplay) button.interactable = true;
        //else button.interactable = false;
    }

    public virtual void OnEnable()
    {
        if (displayValue != null) SetDisplayValue();
    }

    private void OnDisable()
    {
        isSelected = false;
    }

    bool allowInput = true;
    float consecutiveInputDelay = 0.3f;

    private void Update()
    {
        if (!allowInput) return; //don't allow consecutive inputs. only one per axis movement
        if (commandParameters.Length == 0) return; //skip if there are no commands
        if (!isSelected) return; //avoid inputs from another button

        float horizontalAxis = InputManager.instance.GetAxis("UIHorizontal");
        if (horizontalAxis >= 0.1f) //change parameter to the next one to the right
        {
            allowInput = false;
            Invoke("AllowInput", consecutiveInputDelay);

            ++currentParameter;
            currentParameter %= commandParameters.Length;
            SetDisplayValue();
        }
        else if (horizontalAxis <= -0.1f) //change parameter to the next one to the left
        {
            allowInput = false;
            Invoke("AllowInput", consecutiveInputDelay);

            --currentParameter;
            if (currentParameter < 0) currentParameter = commandParameters.Length - 1;
            SetDisplayValue();
        }
    }

    void AllowInput()
    {
        allowInput = true;
    }

    void OnClick()
    {
        //execute the action 
        if (commandParameters.Length > 0)
        {
            command.runCommand(commandParameters[currentParameter].Split(new char[] { ' ' }));
        }
        else
            command.runCommand(null);

        //change the display value
        if (displayValue != null) SetDisplayValue();

        //if requested, turn off the menu
        if (turnOffPanelWhenButtonPressed)
        {
            HUD_DEBUG.instance.toggleQuickButtons = false;
            HUD_DEBUG.instance.quickButtons.SetActive(false);
            HUD_DEBUG.instance.ReestablishSystem_QuickButtons();
        }
    }

    public void OnSelect(BaseEventData data)
    {
        isSelected = true;
    }

    public void OnDeselect(BaseEventData data)
    {
        isSelected = false;
    }

    public virtual void SetDisplayValue() { }
}
