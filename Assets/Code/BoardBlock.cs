using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class BoardBlock : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    public Image blockImage;

    // Input events
    public delegate void OnBlockPointerDownDelegate(BoardBlock boardBlock, PointerEventData.InputButton inputButton);
    public event OnBlockPointerDownDelegate OnBlockPointerDown;
    public delegate void OnBlockPointerUpDelegate(GameObject gameObject, PointerEventData.InputButton inputButton);
    public event OnBlockPointerUpDelegate OnBlockPointerUp;
    public delegate void OnBlockPointerEnterDelegate(BoardBlock boardBlock, PointerEventData.InputButton inputButton);
    public event OnBlockPointerEnterDelegate OnBlockPointerEnter;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnBlockPointerDown(this, eventData.button);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnBlockPointerUp(this.gameObject, eventData.button);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnBlockPointerEnter.Invoke(this, eventData.button);
        }
    }

    void Awake()
    {
        blockImage = GetComponent<Image>();
    }

    public void Fade(bool doFade)
    {
        blockImage.color = (doFade) ? new Color(1f, 1f, 1f, 0.3f) : Color.white;
    }
}
