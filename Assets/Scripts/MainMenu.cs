using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject optionsPanel;

    [Header("Cursor Settings")]
    public Texture2D cursorTexture;

    [Header("Background Settings")]
    public Image backgroundImage;
    public Sprite[] backgrounds;
    private int currentBackgroundIndex = 0;

    private void Start()
    {
        // Set custom hardware cursor (e.g. mini sword) and make sure it is visible
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }
        Cursor.visible = true;

        // Set default background if available
        if (backgrounds != null && backgrounds.Length > 0 && backgroundImage != null)
        {
            // Load saved background selection if it exists
            currentBackgroundIndex = PlayerPrefs.GetInt("SelectedBackgroundIndex", 0);
            if (currentBackgroundIndex >= backgrounds.Length)
            {
                currentBackgroundIndex = 0;
            }
            backgroundImage.sprite = backgrounds[currentBackgroundIndex];
        }

        // Show main panel and hide options panel initially
        if (mainPanel != null) mainPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        Debug.Log("[MainMenu] Loading game scene...");
        Cursor.visible = false; // Hide pointer when starting the game
        SceneManager.LoadScene("Escenario Prueba");
    }

    public void OpenOptions()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void NextBackground()
    {
        if (backgrounds == null || backgrounds.Length == 0 || backgroundImage == null) return;

        currentBackgroundIndex = (currentBackgroundIndex + 1) % backgrounds.Length;
        backgroundImage.sprite = backgrounds[currentBackgroundIndex];
        PlayerPrefs.SetInt("SelectedBackgroundIndex", currentBackgroundIndex);
        PlayerPrefs.Save();
        Debug.Log("[MainMenu] Background changed to: " + backgrounds[currentBackgroundIndex].name);
    }

    public void PreviousBackground()
    {
        if (backgrounds == null || backgrounds.Length == 0 || backgroundImage == null) return;

        currentBackgroundIndex--;
        if (currentBackgroundIndex < 0)
        {
            currentBackgroundIndex = backgrounds.Length - 1;
        }
        backgroundImage.sprite = backgrounds[currentBackgroundIndex];
        PlayerPrefs.SetInt("SelectedBackgroundIndex", currentBackgroundIndex);
        PlayerPrefs.Save();
        Debug.Log("[MainMenu] Background changed to: " + backgrounds[currentBackgroundIndex].name);
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Quitting game...");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
