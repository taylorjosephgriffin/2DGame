using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public bool[] isFull;
    public GameObject[] slots;
    private int previousSlotLength;
    private int currentSlotLength;

    private void Start()
    {
        CheckStatusOfSlots();
    }

    public void CheckStatusOfSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].transform.childCount == 0)
            {
                SetColorOfSlot(i, "REMOVE");
            }
        }
    }

    public void SetColorOfSlot(int slotIndex, string status)
    {
        if (status == "REMOVE") slots[slotIndex].GetComponent<Image>().color = new Color32(212, 212, 212, 255);
        if (status == "ADD") slots[slotIndex].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
    }
}
