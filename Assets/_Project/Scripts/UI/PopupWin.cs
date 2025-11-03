using UnityEngine;
using UnityEngine.UI;

public class PopupWin : Popup
{
    public Button nextBtn;
    private void Start()
    {
        nextBtn.onClick.AddListener(() =>
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
