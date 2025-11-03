using UnityEngine;
using UnityEngine.UI;

public class PopupLose : Popup
{
    public Button tryAgainBtn;
    private void Start()
    {
        tryAgainBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseCurrentPopup();
            GameManager.Instance.InitializeLevel();
            UIManager.Instance.ShowPanel(UIManager.Instance.PanelHome);
            UIManager.Instance.PanelHome.StartLoading(false, () =>
            {
                LevelGenerator.Instance.StartTimer();
            });
        });
    }
}
