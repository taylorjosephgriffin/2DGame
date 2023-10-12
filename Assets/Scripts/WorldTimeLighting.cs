using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorldTimeLighting : MonoBehaviour
{
  [System.Serializable]
  public class Phases
  {
    public Color32 phaseColor;
    public int phaseTimeStart;
    public int phaseTimeEnd;
    public float lightIntensity = 1f;
  }
  public Phases[] phases;
  public int previousPhaseIndex;
  public int currentPhaseIndex;
  public int nextPhaseIndex;
  public Light2D worldLight;

  public WorldTime worldTime;

  float timer = 0;

  public int timeAtStart;
  void Start()
  {
    timeAtStart = worldTime.currentMin;
    //if (timeAtStart >= 0 && timeAtStart < phases[0].phaseTimeEnd)
    //{
    //  previousPhaseIndex = phases.Length - 1;
    //  currentPhaseIndex = 0;
    //  nextPhaseIndex = 1;
    //}
    for (int i = 0; i < phases.Length; i++)
    {
      if (worldTime.currentMin >= phases[i].phaseTimeStart && worldTime.currentMin <= phases[i].phaseTimeEnd)
      {
        currentPhaseIndex = i;
        if (currentPhaseIndex == phases.Length - 1)
        {
          nextPhaseIndex = 0;
        }
        else
        {
          nextPhaseIndex = i + 1;
        }
      }
    }
    worldLight.color = phases[currentPhaseIndex].phaseColor;
    worldLight.intensity = phases[currentPhaseIndex].lightIntensity;
  }

  float calculateCurrentPhase()
  {
    // timer should be multipled by 2 for .5 time and 75 for 0 time idk why yet
    float value = Mathf.Lerp(0f, 1f, timer);
    timer += Time.deltaTime / ((phases[currentPhaseIndex].phaseTimeEnd - timeAtStart)) * 2;
    for (int i = 0; i < phases.Length; i++)
    {
      if (worldTime.currentMin >= ((phases[i].phaseTimeEnd - phases[i].phaseTimeStart) * .75) + phases[i].phaseTimeStart && worldTime.currentMin <= phases[i].phaseTimeEnd)
      {
        if (i != currentPhaseIndex)
        {
          currentPhaseIndex = i;
          if (currentPhaseIndex == 0) previousPhaseIndex = phases.Length - 1;
          else
          {
            previousPhaseIndex = currentPhaseIndex - 1;
          }
          if (i == phases.Length - 1) nextPhaseIndex = 0;
          else nextPhaseIndex = i + 1;
          timeAtStart = worldTime.currentMin;
          timer = 0;
        }
      }
    }
    return value;
  }

  void Update()
  {
    float value = calculateCurrentPhase();

    worldLight.color = Color32.Lerp(phases[currentPhaseIndex].phaseColor, phases[nextPhaseIndex].phaseColor, value);
    worldLight.intensity = Mathf.Lerp(phases[currentPhaseIndex].lightIntensity, phases[nextPhaseIndex].lightIntensity, value);
  }
}
