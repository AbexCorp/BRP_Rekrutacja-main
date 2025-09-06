using UnityEngine;
using UnityEngine.UI;

public class InventoryView : UiView
{
    [Header("Inventory Elements")] [SerializeField]
    private SoulInformation SoulItemPlaceHolder;

    [SerializeField] private Text Description;
    [SerializeField] private Text Name;
    [SerializeField] private Image Avatar;
    [SerializeField] private Button UseButton;
    [SerializeField] private Button DestroyButton;

    private RectTransform _contentParent;
    private GameObject _currentSelectedGameObject;
    private SoulInformation _currentSoulInformation;

    public override void Awake()
    {
        base.Awake();
        _contentParent = (RectTransform)SoulItemPlaceHolder.transform.parent;
        InitializeInventoryItems();
    }

    private void InitializeInventoryItems()
    {
        for (int i = 0, j = SoulController.Instance.Souls.Count; i < j; i++)
        {
            SoulInformation newSoul = Instantiate(SoulItemPlaceHolder.gameObject, _contentParent).GetComponent<SoulInformation>();
            newSoul.gameObject.TryGetComponent<RectTransform>(out var rec);
            newSoul.SetSoulItem(SoulController.Instance.Souls[i], () => SoulItem_OnClick(newSoul), () => UpdateScrollBar(rec));
        }

        SoulItemPlaceHolder.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ClearSoulInformation();
    }

    private void ClearSoulInformation()
    {
        Description.text = "";
        Name.text = "";
        Avatar.sprite = null;
        SetupUseButton(false);
        SetupDestroyButton(false);
        _currentSelectedGameObject = null;
        _currentSoulInformation = null;
    }

    public void SoulItem_OnClick(SoulInformation soulInformation)
    {
        _currentSoulInformation = soulInformation;
        _currentSelectedGameObject = soulInformation.gameObject;
        SetupSoulInformation(soulInformation.soulItem);
    }


    #region Scroll

    [SerializeField] private ScrollRect Scroll;
    [SerializeField] private RectTransform ScrollContent;
    [SerializeField] private GridLayoutGroup ScrollGridLayoutGroup;
    public void UpdateScrollBar(RectTransform selectedElement)
    {
        if (GUIController.Instance.PointerNavigation)
            return;
        float cellHeight = ScrollGridLayoutGroup.cellSize.y;
        float verticalSpacing = ScrollGridLayoutGroup.spacing.y;

        Vector3 selectedElementPos = selectedElement.localPosition;

        int rowIndex = Mathf.FloorToInt(-selectedElementPos.y / (cellHeight + verticalSpacing));

        float contentHeight = ScrollContent.rect.height;
        float viewportHeight = Scroll.viewport.rect.height;
        float scrollableHeight = contentHeight - viewportHeight;

        float targetYPosition = rowIndex * (cellHeight + verticalSpacing);
        float targetNormalizedPosition = Mathf.InverseLerp(0, scrollableHeight, targetYPosition);
        Scroll.verticalNormalizedPosition = Mathf.Clamp01(1 - targetNormalizedPosition);
    }

    #endregion


    private void SetupSoulInformation(SoulItem soulItem)
    {
        Description.text = soulItem.Description;
        Name.text = soulItem.Name;
        Avatar.sprite = soulItem.Avatar;
        SetupUseButton(soulItem.CanBeUsed);
        SetupDestroyButton(soulItem.CanBeDestroyed);
    }

    private void SelectElement(int index)
    {

    }

    private void CantUseCurrentSoul()
    {
        PopUpInformation popUpInfo = new PopUpInformation { DisableOnConfirm = true, UseOneButton = true, Header = "CAN'T USE", Message = "THIS SOUL CANNOT BE USED IN THIS LOCALIZATION" };
        GUIController.Instance.ShowPopUpMessage(popUpInfo);
    }

    private void UseCurrentSoul(bool canUse)
    {
        if (!canUse)
        {
            CantUseCurrentSoul();
        }
        else
        {
            //USE SOUL
            GameEvents.SoulUsed?.Invoke(_currentSoulInformation);
            Destroy(_currentSelectedGameObject);
            ClearSoulInformation();
        }
    }

    private void DestroyCurrentSoul()
    {
        Destroy(_currentSelectedGameObject);
        ClearSoulInformation();
    }

    private void SetupUseButton(bool active)
    {
        UseButton.onClick.RemoveAllListeners();
        if (active)
        {
            bool isInCorrectLocalization = GameControlller.Instance.IsCurrentLocalization(_currentSoulInformation.soulItem.UsableInLocalization);
            PopUpInformation popUpInfo = new PopUpInformation
            {
                DisableOnConfirm = isInCorrectLocalization,
                UseOneButton = false,
                Header = "USE ITEM",
                Message = "Are you sure you want to USE: " + _currentSoulInformation.soulItem.Name + " ?",
                Confirm_OnClick = () => UseCurrentSoul(isInCorrectLocalization)
            };
            UseButton.onClick.AddListener(() => GUIController.Instance.ShowPopUpMessage(popUpInfo));

            if(UseButton.TryGetComponent<Selectable>(out var b))
            {
                if(isInCorrectLocalization == false)
                    b.interactable = false;
                else
                    b.interactable = true;
            }
        }
        UseButton.gameObject.SetActive(active);
    }

    private void SetupDestroyButton(bool active)
    {
        DestroyButton.onClick.RemoveAllListeners();
        if (active)
        {
            PopUpInformation popUpInfo = new PopUpInformation
            {
                DisableOnConfirm = true,
                UseOneButton = false,
                Header = "DESTROY ITEM",
                Message = "Are you sure you want to DESTROY: " + Name.text + " ?",
                Confirm_OnClick = () => DestroyCurrentSoul()
            };
            DestroyButton.onClick.AddListener(() => GUIController.Instance.ShowPopUpMessage(popUpInfo));
        }

        DestroyButton.gameObject.SetActive(active);
    }
}