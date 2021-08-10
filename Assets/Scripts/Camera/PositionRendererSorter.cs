using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionRendererSorter : MonoBehaviour
{
  public Renderer myRenderer;
  public float offset;
  public GameObject sortAnchor;

  private void LateUpdate()
  {
    myRenderer.sortingOrder = (int)((sortAnchor.transform.position.y + offset) * -100);
  }
}
