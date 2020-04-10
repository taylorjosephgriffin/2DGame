using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionRendererSorter : MonoBehaviour
{
    
    public Renderer myRenderer;
    
    public GameObject sortAnchor;

    private void LateUpdate()
    {
        myRenderer.sortingOrder = (int)(sortAnchor.transform.position.y * -100);
    }
}
