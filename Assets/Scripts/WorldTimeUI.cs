using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldTimeUI : MonoBehaviour
{
  public WorldTime worldTime;
  public Text timeUI;
  public Text dayUI;
  public GameObject worldTimeVisualization;
  float currentRotation;

  // Start is called before the first frame update
  void Start()
  {
    currentRotation = (float)worldTime.currentMin / 4;
  }

  // Update is called once per frame
  void Update()
  {
    currentRotation = (float)worldTime.currentMin / 4;
    UpdateUITimer();
  }

  private void UpdateUITimer()
  {
    int hourDisplay;
    int minuteDisplay;
    hourDisplay = worldTime.currentHour;
    minuteDisplay = worldTime.currentTenMin;
    if (worldTime.currentHour == 0)
    {
      hourDisplay = 12;
    }
    worldTimeVisualization.transform.localRotation = Quaternion.Euler(0, 0, currentRotation);
    string meridian = "";
    string extraZero = "";
    if (worldTime.currentMin < 720) meridian = "A.M.";
    else meridian = "P.M.";
    if (worldTime.currentTenMin == 0) extraZero = "0";
    else extraZero = "";


    timeUI.text = hourDisplay + ":" + minuteDisplay + extraZero + " " + meridian;
    dayUI.text = "DAY " + worldTime.dayCount;
  }
}
