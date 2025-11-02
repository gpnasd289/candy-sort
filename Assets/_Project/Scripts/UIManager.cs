using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private List<Panel> panelList = new();
    private Stack<Popup> popupStack = new();
    [Header("Panel")]
    public Panel PanelHome;
    public Panel PanelLoading;
    [Header("Popup")]
    public Popup PopupWin;
    public Popup PopupLose;
    public Popup PopupSetting;
    public Popup PopupHint;
    public Popup PopupRestart;
    [Header("UI Elements")]
    public TextMeshProUGUI moveCountText;
    public TextMeshProUGUI levelNameText;
    [Header("Button")]
    public Button restartButton;
    public Button undoButton;
    public Button hintButton;
    public Button pauseButton;

    private int moveCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        panelList.Add(PanelHome);
        panelList.Add(PanelLoading);
    }

    void Start()
    {
        SetupButtons();
    }
    public void ShowPopup(Popup popup, bool overlay = false)
    {
        if (!overlay)
        {
            if (popupStack.Count > 0)
            {
                popupStack.Peek().Close();
            }
        }

        popupStack.Push(popup);
        popup.Open();
    }

    public void CloseCurrentPopup()
    {
        Debug.Log("Closet current popup");
        if (popupStack.Count == 0) return;

        Popup top = popupStack.Pop();
        top.Close();

        if (popupStack.Count > 0)
        {
            popupStack.Peek().Open();
        }
    }

    public void CloseAllPopups()
    {
        while (popupStack.Count > 0)
        {
            popupStack.Pop().Close();
        }
    }
    public void ShowPanel(Panel panel)
    {
        foreach (var panel_item in panelList)
        {
            if (panel_item != panel)
            {
                panel_item.Close();
            }
            else
            {
                panel_item.Open();
            }
        }
    }
    public void StartGame()
    {
        CloseAllPopups();
    }

    private void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (undoButton != null)
        {
            undoButton.onClick.AddListener(OnUndoClicked);
        }

        if (hintButton != null)
        {
            hintButton.onClick.AddListener(OnHintClicked);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseClicked);
        }
    }

    public void IncrementMoveCount()
    {
        moveCount++;
        UpdateMoveCount();
    }

    public void ResetMoveCount()
    {
        moveCount = 0;
        UpdateMoveCount();
    }

    private void UpdateMoveCount()
    {
        if (moveCountText != null)
        {
            moveCountText.text = "Moves: " + moveCount;
        }
    }

    public void ShowWinPanel()
    {
        ShowPopup(PopupWin);
    }

    public void ShowLosePanel()
    {
        ShowPopup(PopupLose);
    }

    private void OnRestartClicked()
    {
        // Reload current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    private void OnUndoClicked()
    {
        // Implement undo functionality
        Debug.Log("Undo clicked");
        // This would need to be connected to a move history system
    }

    private void OnHintClicked()
    {
        // Implement hint functionality
        Debug.Log("Hint clicked");
    }

    private void OnPauseClicked()
    {
        // Toggle pause state
        if (TimeManager.Instance != null)
        {
            if (TimeManager.Instance.IsTimerActive())
            {
                TimeManager.Instance.PauseTimer();
                if (pauseButton != null)
                {
                    TextMeshProUGUI buttonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = "Resume";
                }
                ShowMessage("Game Paused", 1f);
            }
            else
            {
                TimeManager.Instance.ResumeTimer();
                if (pauseButton != null)
                {
                    TextMeshProUGUI buttonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = "Pause";
                }
            }
        }
    }

    public void SetLevelName(string name)
    {
        if (levelNameText != null)
        {
            levelNameText.text = name;
        }
    }

    public void ShowMessage(string message, float duration = 2f)
    {
        StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    private IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        // Create temporary message text
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(transform);

        TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = message;
        messageText.fontSize = 36;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.color = Color.white;

        RectTransform rectTransform = messageObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(400, 100);

        yield return new WaitForSeconds(duration);

        Destroy(messageObj);
    }
}