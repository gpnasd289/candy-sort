using Audio;
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
    public PanelHome PanelHome;
    [Header("Popup")]
    public PopupWin PopupWin;
    public PopupLose PopupLose;
    public PopupSetting PopupSetting;
    public PopupHint PopupHint;
    public PopupRestart PopupRestart;
    [Header("UI Elements")]
    public TextMeshProUGUI moveCountText;
    public TextMeshProUGUI levelNameText;
    [Header("Button")]
    public Button restartButton;
    public Button homeButton;
    public Button hintButton;
    public Button settingButton;

    private int moveCount = 0;

    void Awake()
    {
        Instance = this;
        panelList.Add(PanelHome);
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
        Debug.Log("Close current popup");
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

        if (homeButton != null)
        {
            homeButton.onClick.AddListener(OnHomeClicked);
        }

        if (hintButton != null)
        {
            hintButton.onClick.AddListener(OnHintClicked);
        }

        if (settingButton != null)
        {
            settingButton.onClick.AddListener(OnSettingClicked);
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
        AudioManager.Ins.PlaySfx(SFX_TYPE.WIN);
        ShowPopup(PopupWin);
    }

    public void ShowLosePanel()
    {
        AudioManager.Ins.PlaySfx(SFX_TYPE.LOSE);
        ShowPopup(PopupLose);
    }

    private void OnRestartClicked()
    {
        if (TimeManager.Instance != null)
        {
            if (TimeManager.Instance.IsTimerActive())
            {
                TimeManager.Instance.PauseTimer();
                ShowPopup(PopupRestart);
            }
        }
    }

    private void OnHomeClicked()
    {
        ShowPanel(PanelHome);
        PanelHome.StartLoading(true);
    }

    private void OnHintClicked()
    {
        if (TimeManager.Instance != null)
        {
            if (TimeManager.Instance.IsTimerActive())
            {
                TimeManager.Instance.PauseTimer();
                ShowPopup(PopupHint);
            }
        }
    }

    private void OnSettingClicked()
    {
        if (TimeManager.Instance != null)
        {
            if (TimeManager.Instance.IsTimerActive())
            {
                TimeManager.Instance.PauseTimer();
                ShowPopup(PopupSetting);
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