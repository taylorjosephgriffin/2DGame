using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class UIManager
{
    public enum UIStates {
        INVENTORY,
        IN_GAME,
        AUDIO_SETTINGS,
        CHARACTER_CREATION

    }
    static public UIStates currentUIState = UIStates.IN_GAME;

    private void Start()
    {
        currentUIState = UIStates.IN_GAME;
    }
}
