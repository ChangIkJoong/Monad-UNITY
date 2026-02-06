using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "BaseScene"; //gameplay scene

    void Start()
    {
        // If your gameplay locks the cursor, unlock it in the menu:
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Play()
    {
        // Safety check: make sure timescale isn't stuck at 0 from pause menus, etc.
        Time.timeScale = 1f;

        SceneManager.LoadScene(gameSceneName);
    }

    public void Options()
    {
        Application.Quit();
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}
