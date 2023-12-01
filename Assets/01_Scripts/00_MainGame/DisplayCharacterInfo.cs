using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisplayCharacterInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject panel;
    [SerializeField] private string[] charaDist;
    [SerializeField] private Text display;

    public void SetData(int current)
    {
        display.text = charaDist[current];
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        panel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        panel.SetActive(false);
    }
}
