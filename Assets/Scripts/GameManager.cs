using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("遊戲物件")]
    public ArmController armController;
    public Transform cookieTarget;       // 餅乾目標
    public Transform[] obstacles;        // 障礙物陣列

    [Header("UI")]
    public Text levelText;
    public Text statusText;
    public Button restartButton;
    public Button nextLevelButton;

    [Header("關卡設定")]
    public int currentLevel = 1;
    public int maxLevel = 5;

    private bool gameOver = false;
    private bool levelComplete = false;

    void Start()
    {
        SetupLevel(currentLevel);

        // 設定按鈕事件
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(NextLevel);
            nextLevelButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!gameOver && !levelComplete)
        {
            CheckCollisions();
            CheckWinCondition();
        }

        // ESC 鍵重新開始
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            RestartLevel();
        }
    }

    void CheckCollisions()
    {
        Vector3 armTip = armController.GetTipPosition();

        // 檢查是否碰到障礙物
        foreach (Transform obstacle in obstacles)
        {
            if (obstacle != null && obstacle.gameObject.activeInHierarchy)
            {
                float distance = Vector2.Distance(
                    new Vector2(armTip.x, armTip.y),
                    new Vector2(obstacle.position.x, obstacle.position.y)
                );

                // 假設障礙物碰撞半徑為0.3f
                if (distance < 0.3f)
                {
                    GameOver("碰到障礙物了！");
                    return;
                }
            }
        }
    }

    void CheckWinCondition()
    {
        if (cookieTarget == null) return;

        Vector3 armTip = armController.GetTipPosition();
        float distance = Vector2.Distance(
            new Vector2(armTip.x, armTip.y),
            new Vector2(cookieTarget.position.x, cookieTarget.position.y)
        );

        // 檢查是否到達餅乾
        if (distance < 0.4f) // 餅乾碰撞半徑
        {
            LevelComplete();
        }
    }

    void GameOver(string reason)
    {
        gameOver = true;

        if (statusText != null)
            statusText.text = $"遊戲失敗！\n{reason}\n按ESC重新開始";

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);

        Debug.Log($"Game Over: {reason}");
    }

    void LevelComplete()
    {
        levelComplete = true;

        if (statusText != null)
        {
            if (currentLevel >= maxLevel)
            {
                statusText.text = "恭喜！\n你完成了所有關卡！";
            }
            else
            {
                statusText.text = $"關卡 {currentLevel} 完成！\n準備下一關？";
                if (nextLevelButton != null)
                    nextLevelButton.gameObject.SetActive(true);
            }
        }

        Debug.Log($"Level {currentLevel} Complete!");
    }

    void RestartLevel()
    {
        gameOver = false;
        levelComplete = false;

        // 重置手臂
        armController.ResetArm();

        // 重置UI
        if (statusText != null)
            statusText.text = "";

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(false);

        // 重新設定當前關卡
        SetupLevel(currentLevel);

        Debug.Log($"Restarting Level {currentLevel}");
    }

    void NextLevel()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            gameOver = false;
            levelComplete = false;

            // 重置手臂
            armController.ResetArm();

            // 重置UI
            if (statusText != null)
                statusText.text = "";

            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(false);

            // 設定新關卡
            SetupLevel(currentLevel);

            Debug.Log($"Starting Level {currentLevel}");
        }
    }

    void SetupLevel(int level)
    {
        if (levelText != null)
            levelText.text = $"第 {level} 關";

        // 根據關卡設定餅乾位置
        SetupCookiePosition(level);

        // 根據關卡設定障礙物
        SetupObstacles(level);

        Debug.Log($"Level {level} Setup Complete");
    }

    void SetupCookiePosition(int level)
    {
        if (cookieTarget == null) return;

        // 簡單的關卡設計 - 餅乾位置會根據關卡改變
        Vector3 basePosition = new Vector3(0, -3f, 0);

        switch (level)
        {
            case 1:
                cookieTarget.position = basePosition;
                break;
            case 2:
                cookieTarget.position = basePosition + Vector3.right * 1f;
                break;
            case 3:
                cookieTarget.position = basePosition + Vector3.left * 1f;
                break;
            case 4:
                cookieTarget.position = basePosition + Vector3.right * 2f;
                break;
            case 5:
                cookieTarget.position = basePosition + Vector3.left * 2f;
                break;
        }
    }

    void SetupObstacles(int level)
    {
        // 先隱藏所有障礙物
        foreach (Transform obstacle in obstacles)
        {
            if (obstacle != null)
                obstacle.gameObject.SetActive(false);
        }

        // 根據關卡啟用對應的障礙物
        int activeObstacles = Mathf.Min(level, obstacles.Length);

        for (int i = 0; i < activeObstacles; i++)
        {
            if (obstacles[i] != null)
            {
                obstacles[i].gameObject.SetActive(true);

                // 簡單的障礙物位置設定
                Vector3 obstaclePos = new Vector3(
                    Random.Range(-3f, 3f),
                    Random.Range(-1f, 1f),
                    0f
                );
                obstacles[i].position = obstaclePos;
            }
        }
    }

    // 外部調用 - 手動觸發遊戲結束（供障礙物腳本使用）
    public void TriggerGameOver(string reason)
    {
        if (!gameOver && !levelComplete)
        {
            GameOver(reason);
        }
    }
}