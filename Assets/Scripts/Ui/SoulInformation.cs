using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SoulInformation : MonoBehaviour
{
    [SerializeField] private Image MainImage;
    [SerializeField] private Button SoulButton;
    [HideInInspector] public SoulItem soulItem;

    public void SetSoulItem(SoulItem _soulItem, Action OnSoulClick = null, Action OnSoulSelect = null)
    {
        soulItem = _soulItem;
        MainImage.sprite = soulItem.Avatar;
        if (OnSoulClick != null) SoulButton.onClick.AddListener(() => OnSoulClick());
        if(gameObject.TryGetComponent<OnSelectHandler>(out var h))
        {
            if (OnSoulSelect == null)
                return;
            h.OnSelectEvent += OnSoulSelect;
        }
    }
}