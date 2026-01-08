using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public GameOverUI gameOverUI;
    public Transform playerCastle;
    public Transform enemyCastle;

    [Header("Settings")]
    public string mainMenuScene = "MainMenu";

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            Debug.Log("✅ GameManager: Initialized and Instance set.");
        }
        else
        {
            Debug.LogWarning(" GameManager: Duplicate instance found and destroyed.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (gameOverUI == null)
        {
            gameOverUI = FindFirstObjectByType<GameOverUI>();
            if (gameOverUI != null) Debug.Log(" GM: Auto-connected to GameOverUI");
            else Debug.LogError("❌ GM: Could not find GameOverUI in Scene!");
        }

        FindCastles();
    }

    void FindCastles()
    {
        Debug.Log("GameManager: Searching for Castles...");

        Castle[] castles = FindObjectsByType<Castle>(FindObjectsSortMode.None);
        foreach (var c in castles)
        {
            if (c.TryGetComponent(out BuildingBase b))
            {
                if (b.team == Unit.Team.Player) playerCastle = c.transform;
                else if (b.team == Unit.Team.Enemy) enemyCastle = c.transform;
            }
        }

        if (playerCastle == null)
        {
            GameObject pObj = GameObject.Find("PlayerCastel"); // Using exact name user might have
            if (pObj == null) pObj = GameObject.Find("PlayerCastle");
            if (pObj != null) playerCastle = pObj.transform;
        }

        if (enemyCastle == null)
        {
             GameObject eObj = GameObject.Find("EnemyCastel"); // Using exact name user might have
             if (eObj == null) eObj = GameObject.Find("EnemyCastle");
             if (eObj != null) enemyCastle = eObj.transform;
        }

        // 3. Log Results
        if (playerCastle != null) Debug.Log($" GM: Found Player Castle: {playerCastle.name}");
        else Debug.LogError("❌ GM: CRITICAL - Player Castle NOT FOUND!");

        if (enemyCastle != null) Debug.Log($" GM: Found Enemy Castle: {enemyCastle.name}");
        else Debug.LogError("❌ GM: CRITICAL - Enemy Castle NOT FOUND!");
    }

    void Update()
    {
        if (isGameOver) return;

        if (playerCastle == null)
        {
            EndGame(false);
        }
        else if (enemyCastle == null)
        {
            EndGame(true);
        }
    }

    public void EndGame(bool playerWon)
    {
        isGameOver = true;
        Time.timeScale = 1.0f; // Keep 1 or 0? 0 stops everything including UI animations if using unscaled time.
        Time.timeScale = 0f; 

        Debug.Log(playerWon ? "Victory!" : "Defeat!");

        if (gameOverUI != null)
        {
            gameOverUI.ShowPanel(playerWon);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(mainMenuScene);
    }
}
