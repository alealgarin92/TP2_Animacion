using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public PlayerController player;
    public Button retryButton;

    private void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClick);
        }
    }

    private void OnRetryClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
