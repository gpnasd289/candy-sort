using UnityEngine;
using UnityEngine.UI;

public class PopupHint : Popup
{
    public Button exitBtn;
    private void Start()
    {
        exitBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseCurrentPopup();
            TimeManager.Instance.ResumeTimer();
        });
    }
}
