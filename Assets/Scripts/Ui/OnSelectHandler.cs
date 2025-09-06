using UnityEngine;
using UnityEngine.EventSystems;

public class OnSelectHandler : MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        OnSelectEvent?.Invoke();
    }
    public event System.Action OnSelectEvent;
}
