using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PanelHome : Panel
{
    public Image bgloading;
    public Image imgLoading;
    public Button btnPlay;
    private void Start()
    {
        StartLoading();
    }
    public void StartLoading()
    {
        btnPlay.interactable = false;
        btnPlay.transform.localScale = Vector3.zero;
        imgLoading.fillAmount = 0;
        imgLoading.DOFillAmount(1f, 3f).OnComplete(() =>
        {
            bgloading.gameObject.SetActive(false);
            imgLoading.gameObject.SetActive(false);
            btnPlay.transform.DOScale(1f, 1f).SetEase(Ease.OutBounce).OnComplete(() => btnPlay.interactable = true);
        });
    }
}
