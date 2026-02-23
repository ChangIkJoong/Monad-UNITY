using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlayQuitButton : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
        Debug.Log("Open MainMenu - Scene");
    }
    
}
