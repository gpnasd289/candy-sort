using UnityEngine;
using UnityEngine.UI;

public class PopupRestart : Popup
{
    public Button exitBtn;
    public Button restartBtn;
    private void Start()
    {
        restartBtn.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene
            (
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        });
        exitBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseCurrentPopup();
            TimeManager.Instance.ResumeTimer();
        });
    }
}
