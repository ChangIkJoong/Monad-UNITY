using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private Core core;

    void Awake()
    {
        if (core == null)
        {
            core = FindAnyObjectByType<Core>();
        }

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (core != null)
            core.OnCoreDeathEvent += ShowGameOver;
    }

    void OnDisable()
    {
        if (core != null)
            core.OnCoreDeathEvent -= ShowGameOver;
    }

    private void ShowGameOver()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            Time.timeScale = 0f; // if it might interfere with any game over animations or effects, delete line.
        }
    }

    public void GoBackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
