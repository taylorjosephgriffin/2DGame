using UnityEngine;

class PauseManager
{
    public static bool isPaused;
    public static void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public static void PlayGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }
}
