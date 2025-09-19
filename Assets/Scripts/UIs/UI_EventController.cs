using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class UI_EventController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnBegineDragHandler = null;
    public Action<PointerEventData> OnDragHandler = null;
    public Action<PointerEventData> OnDragEndHanlder = null;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (OnBegineDragHandler != null)
        {
            OnBegineDragHandler.Invoke(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (OnDragHandler != null)
        {
            OnDragHandler.Invoke(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (OnDragEndHanlder != null)
        {
            OnDragEndHanlder.Invoke(eventData);
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClickHandler != null)
        {
            OnClickHandler.Invoke(eventData);
        }
    }
}
