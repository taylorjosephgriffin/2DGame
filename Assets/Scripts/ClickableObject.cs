using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour, IPointerClickHandler
{
  public InventoryUI inventoryUI;
  public InventorySlot inventorySlot;
  public void OnPointerClick(PointerEventData eventData)
  {
    //if (eventData.button == PointerEventData.InputButton.Left)
    //    Debug.Log("Left click");
    //else if (eventData.button == PointerEventData.InputButton.Middle)
    //    Debug.Log("Middle click");
    if (eventData.button == PointerEventData.InputButton.Right)
      inventoryUI.selectedSlot = inventorySlot;
  }
}