using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSelectedHotbarItem : MonoBehaviour
{
  PlayerControls controls;
  public List<Transform> hotBarItems = new List<Transform>();
  Vector2 scrollValue;
  public int currentIndex = 0;
  SetSelectedButton script;
  // Start is called before the first frame update
  void Start()
  {
    foreach (Transform child in transform)
    {
      hotBarItems.Add(child);
    }
    script = GetComponent<SetSelectedButton>();
    controls = new PlayerControls();
    controls.Gameplay.Enable();
    controls.Gameplay.CycleHotbar.performed += ctx => SetSelectedItem(ctx.ReadValue<Vector2>());
    //Debug.Log(hotBarItems.Count);
  }

  void SetSelectedItem(Vector2 ctx)
  {
    if (ctx.y > 0)
    {
      if (currentIndex >= hotBarItems.Count - 1)
      {
        currentIndex = 0;
      }
      else
      {
        currentIndex += 1;
      }
    }

    if (ctx.y < 0)
    {
      if (currentIndex == 0)
      {
        currentIndex = hotBarItems.Count - 1;
      }
      else
      {
        currentIndex -= 1;
      }
    }
  }

  private void Update()
  {
    script.SetSelectedUI(hotBarItems[currentIndex].gameObject);
  }
}
