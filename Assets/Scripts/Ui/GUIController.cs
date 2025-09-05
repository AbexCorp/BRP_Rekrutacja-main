using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{

    #region singleton

    public static GUIController Instance;

    private void Awake()
    {
        DisableOnStartObject.SetActive(false);
        Instance = this;
    }

    #endregion

    [SerializeField] private GameObject DisableOnStartObject;
    [SerializeField] private RectTransform ViewsParent;
    [SerializeField] private GameObject InGameGuiObject;
    [SerializeField] private PopUpView PopUp;
    [SerializeField] private PopUpScreenBlocker ScreenBlocker;

    private void Start()
    {
        if (ScreenBlocker) ScreenBlocker.InitBlocker();
    }

    private void ActiveInGameGUI(bool active)
    {
        InGameGuiObject.SetActive(active);
    }

    public void ShowPopUpMessage(PopUpInformation popUpInfo)
    {
        PopUpView newPopUp = Instantiate(PopUp, ViewsParent) as PopUpView;
        newPopUp.ActivePopUpView(popUpInfo);
    }

    public void ActiveScreenBlocker(bool active, PopUpView popUpView)
    {
        if (active) ScreenBlocker.AddPopUpView(popUpView);
        else ScreenBlocker.RemovePopUpView(popUpView);
    }
    ////////////////////////////////////
    public void SetDefaultUiSelection()
    {
        ChangeUISelection(EventSystem.current.firstSelectedGameObject);
    }
    public void ChangeUISelection(GameObject uiObject)
    {
        EventSystem.current.SetSelectedGameObject(uiObject);
    }
    private void FindNavigation()
    {
        Debug.Log(EventSystem.current.currentInputModule == null);
        var inputModule = EventSystem.current.currentInputModule as UnityEngine.InputSystem.UI.InputSystemUIInputModule;
        InputAction navigation = inputModule.actionsAsset.FindAction("Navigate");
        navigation.performed += AttemptNavigation;
    }
    private void AttemptNavigation(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject == null)
            SetDefaultUiSelection();
    }
    ////////////////////////////////////


    #region IN GAME GUI Clicks

    public void InGameGUIButton_OnClick(UiView viewToActive)
    {
        viewToActive.ActiveView(() => ActiveInGameGUI(true));

        ActiveInGameGUI(false);
        GameControlller.Instance.IsPaused = true;
    }

    public void ButtonQuit()
    {
        Application.Quit();
    }
    
    #endregion
}