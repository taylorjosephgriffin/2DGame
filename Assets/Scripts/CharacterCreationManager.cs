using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCreationManager : MonoBehaviour
{
    public static CharacterCreationManager instance;
    public GameObject characterCreationUI;

    PlayerControls controls;

    bool characterCreationOpen = false;

    private void Awake()
    {
        if (instance == null) instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        controls = new PlayerControls();
        controls.Gameplay.Enable();
        controls.Gameplay.CharacterCreation.performed += ctx => triggerCharacterCreationStateChange();
    }

    // Update is called once per frame
    void triggerCharacterCreationStateChange()
    {
        if (!characterCreationOpen && UIManager.currentUIState == UIManager.UIStates.IN_GAME)
        {
            characterCreationOpen = true;
            characterCreationUI.gameObject.SetActive(true);
            PauseManager.PauseGame();
            UIManager.currentUIState = UIManager.UIStates.CHARACTER_CREATION;
        }
        else if (characterCreationOpen  && UIManager.currentUIState == UIManager.UIStates.CHARACTER_CREATION)
        {
            characterCreationOpen = false;
            characterCreationUI.gameObject.SetActive(false);
            PauseManager.PlayGame();
            UIManager.currentUIState = UIManager.UIStates.IN_GAME;
        }
    }
}
