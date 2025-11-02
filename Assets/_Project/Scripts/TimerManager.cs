using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject timeDisplayPanel;
    public TextMeshProUGUI timeText;
    public Image timeFillImage;
    public Color normalTimeColor = Color.white;
    public Color warningTimeColor = Color.yellow;
    public Color criticalTimeColor = Color.red;

    [Header("Time Warning Settings")]
    public float warningThreshold = 30f; // Show warning at 30 seconds
    public float criticalThreshold = 10f; // Show critical at 10 seconds
    public bool enablePulseAnimation = true;
    public bool enableTickSound = true;

    private float timeLimit = 0f;
    private float timeRemaining = 0f;
    private bool isTimerActive = false;
    private bool hasTimeLimit = false;
    private bool timeExpired = false;

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
    }

    void Update()
    {
        if (isTimerActive && hasTimeLimit)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                OnTimeExpired();
            }

            UpdateTimeDisplay();
        }
    }

    public void StartTimer(float timeLimitSeconds)
    {
        if (timeLimitSeconds <= 0)
        {
            // No time limit
            hasTimeLimit = false;
            isTimerActive = false;
            HideTimeDisplay();
            return;
        }

        timeLimit = timeLimitSeconds;
        timeRemaining = timeLimitSeconds;
        hasTimeLimit = true;
        isTimerActive = true;
        timeExpired = false;

        ShowTimeDisplay();
        UpdateTimeDisplay();
    }

    public void PauseTimer()
    {
        isTimerActive = false;
    }

    public void ResumeTimer()
    {
        if (hasTimeLimit && !timeExpired)
        {
            isTimerActive = true;
        }
    }

    public void StopTimer()
    {
        isTimerActive = false;
        hasTimeLimit = false;
        HideTimeDisplay();
    }

    public void AddTime(float seconds)
    {
        if (hasTimeLimit)
        {
            timeRemaining += seconds;
            if (timeRemaining > timeLimit)
            {
                timeRemaining = timeLimit;
            }

            // Show bonus time effect
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage($"+{seconds:F0}s Bonus Time!", 1.5f);
            }
        }
    }

    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Update color based on time remaining
            if (timeRemaining <= criticalThreshold)
            {
                timeText.color = criticalTimeColor;
                if (enablePulseAnimation)
                {
                    StartCoroutine(PulseText());
                }
                if (enableTickSound && Mathf.FloorToInt(timeRemaining) != Mathf.FloorToInt(timeRemaining + Time.deltaTime))
                {
                    PlayTickSound();
                }
            }
            else if (timeRemaining <= warningThreshold)
            {
                timeText.color = warningTimeColor;
            }
            else
            {
                timeText.color = normalTimeColor;
            }
        }

        if (timeFillImage != null)
        {
            float fillAmount = timeRemaining / timeLimit;
            timeFillImage.fillAmount = fillAmount;

            // Update fill color
            if (timeRemaining <= criticalThreshold)
            {
                timeFillImage.color = criticalTimeColor;
            }
            else if (timeRemaining <= warningThreshold)
            {
                timeFillImage.color = warningTimeColor;
            }
            else
            {
                timeFillImage.color = normalTimeColor;
            }
        }
    }

    private IEnumerator PulseText()
    {
        if (timeText == null)
            yield break;

        Vector3 originalScale = timeText.transform.localScale;
        float pulseDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < pulseDuration && timeRemaining <= criticalThreshold && isTimerActive)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed * Mathf.PI * 4) * 0.1f;
            timeText.transform.localScale = originalScale * scale;
            yield return null;
        }

        timeText.transform.localScale = originalScale;
    }

    private void OnTimeExpired()
    {
        if (timeExpired)
            return;

        timeExpired = true;
        isTimerActive = false;

        Debug.Log("Time's Up!");

        // Show time out message
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("Time's Up!", 2f);
        }

        GameManager.Instance.ClearDragState();

        // Trigger level failed
        StartCoroutine(DelayedLevelFailed());
    }

    private IEnumerator DelayedLevelFailed()
    {
        yield return new WaitForSeconds(2f);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLosePanel();
        }

        GameManager.Instance.ClearDragState();
    }

    private void PlayTickSound()
    {
        // Play tick sound effect
    }

    private void ShowTimeDisplay()
    {
        if (timeDisplayPanel != null)
        {
            timeDisplayPanel.SetActive(true);
        }
    }

    private void HideTimeDisplay()
    {
        if (timeDisplayPanel != null)
        {
            timeDisplayPanel.SetActive(false);
        }
    }

    // Public getters
    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    public float GetTimeLimit()
    {
        return timeLimit;
    }

    public bool HasTimeLimit()
    {
        return hasTimeLimit;
    }

    public bool IsTimeExpired()
    {
        return timeExpired;
    }

    public bool IsTimerActive()
    {
        return isTimerActive;
    }

    public float GetTimePercentage()
    {
        if (timeLimit <= 0)
            return 1f;
        return timeRemaining / timeLimit;
    }
}