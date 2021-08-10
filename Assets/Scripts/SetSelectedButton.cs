using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSelectedButton : MonoBehaviour
{
    public GameObject selectedUIOutline;
    public GameObject defaultSelection;

    void Start()
    {
        SetSelectedUI(defaultSelection); 
    }

    public void SetSelectedUI(GameObject selectedItem) {
        selectedUIOutline.transform.SetParent(selectedItem.transform);
        selectedUIOutline.SetActive(true);
        selectedUIOutline.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
    }
}
