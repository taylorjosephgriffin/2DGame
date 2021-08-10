using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippedItem : MonoBehaviour
{
    public Plantable equippedItem;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (equippedItem != null) {
            GetComponent<SpriteRenderer>().sprite = equippedItem.itemDropIcon;
        }
    }
}
