using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming TextMeshPro, fall back to Text if needed

public class GameOverUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public Text titleTextLegacy; // Fallback if using legacy Text
    public Button restartButton;
    public Button menuButton;

    [Header("Colors")]
    public Color winColor = Color.green;
    public Color loseColor = Color.red;

    private GameManager manager;

    void Start()
    {
        manager = GameManager.Instance;
        if (manager == null) manager = FindFirstObjectByType<GameManager>();

        if (manager != null)
        {
            // 🔄 Two-way Binding for safety
            if (manager.gameOverUI == null) manager.gameOverUI = this;
        }

        panel.SetActive(false);

        // Bind Buttons
        if (restartButton != null) restartButton.onClick.AddListener(() => manager.RestartGame());
        if (menuButton != null) menuButton.onClick.AddListener(() => manager.GoToMainMenu());
    }

    public void ShowPanel(bool playerWon)
    {
        panel.SetActive(true);

        string msg = playerWon ? "VICTORY!" : "DEFEAT!";
        Color col = playerWon ? winColor : loseColor;

        if (titleText != null)
        {
            titleText.text = msg;
            titleText.color = col;
        }
        else if (titleTextLegacy != null)
        {
            titleTextLegacy.text = msg;
            titleTextLegacy.color = col;
        }

        // 🔊 SFX Game Over
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(playerWon ? SoundType.Victory : SoundType.Defeat);
        }
    }
}
