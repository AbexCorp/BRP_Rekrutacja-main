using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

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
        StartCoroutine(FindInputActions());
        ChangeUISelection(null);
        SubscribeScoreEvents();
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


    #region Gamepad Navigation

    public bool PointerNavigation { get; private set; }
    private IEnumerator FindInputActions()
    {
        yield return null;

        var inputModule = EventSystem.current.currentInputModule as UnityEngine.InputSystem.UI.InputSystemUIInputModule;

        InputAction navigation = inputModule.actionsAsset.FindAction("Navigate");
        navigation.performed += SetGamepadNavigation;

        InputAction point = inputModule.actionsAsset.FindAction("Point");
        point.performed += SetPointerNavigation;

        InputAction cancel = inputModule.actionsAsset.FindAction("Cancel");
        cancel.performed += TriggerCancel;
    }
    private void SetPointerNavigation(InputAction.CallbackContext context)
    {
        PointerNavigation = true;
        if (EventSystem.current.currentSelectedGameObject != null)
            ChangeUISelection(null);
    }
    private void SetGamepadNavigation(InputAction.CallbackContext context)
    {
        PointerNavigation = false;
        if (EventSystem.current.currentSelectedGameObject == null)
            SetDefaultUiSelection();
    }
    private void TriggerCancel(InputAction.CallbackContext context)
    {
        if (_activeViews.LastOrDefault() == null)
            return;
        _activeViews.LastOrDefault().GetBackButton().onClick?.Invoke();
    }


    private List<UiView> _activeViews = new();
    public void SetActiveView(UiView view)
    {
        if(_activeViews.Contains(view) == false)
            _activeViews.Add(view);
        else if(_activeViews.Contains(view))
            _activeViews.Remove(view);
        SetDefaultUiSelection();

        if (_activeViews.Count == 0)
            SetActiveWorldSpaceUI(true);
        else
            SetActiveWorldSpaceUI(false);
    }
    public void SetDefaultUiSelection()
    {
        if (_activeViews.Count > 0)
        {
            if(_activeViews.LastOrDefault() != null)
                ChangeUISelection(_activeViews.LastOrDefault().GetDefaultSelection().gameObject);
        }
        else
            ChangeUISelection(EventSystem.current.firstSelectedGameObject);
    }
    public void ChangeUISelection(GameObject uiObject)
    {
        if (PointerNavigation)
        {
            if(EventSystem.current.currentSelectedGameObject != null)
                StartCoroutine(DelayUISelectionChange(null));
            return;
        }
        StartCoroutine(DelayUISelectionChange(uiObject));
    }
    private IEnumerator DelayUISelectionChange(GameObject uiObject)
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(uiObject);
    }

    [SerializeField] private GameObject _worldSpaceUI = null;
    private void SetActiveWorldSpaceUI(bool value)
    {
        foreach(var go in _worldSpaceUI.GetComponentsInChildren<Selectable>())
        {
            go.interactable = value;
        }
    }

    #endregion


    #region ScoreBoard

    [SerializeField] private TMP_Text _scoreCounter;
    private int _score = 0;
    public void AddScore(IEnemy enemy)
    {
        int score = 0;
        if (enemy.GetEnemyObject().TryGetComponent<SoulEnemy>(out var e))
        {
            if (e.DiedToVulnerability)
                score += 1;
        }
        score += 2;
        AddScore(score);
    }
    public void AddScore(SoulInformation soulInfo)
    {
        int score = 0;
        //check soul value
            score += 3;
        //
        AddScore(score);
    }
    private void AddScore(int amount)
    {
        _score += amount;
        if (_score < 0)
            _score = int.MaxValue;
        _scoreCounter.text = $"Score: {_score}";
    }
    private void SubscribeScoreEvents()
    {
        GameEvents.EnemyKilled += AddScore;
        GameEvents.SoulUsed += AddScore;
    }

    #endregion


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