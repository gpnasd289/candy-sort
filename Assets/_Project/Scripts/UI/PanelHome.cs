using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PanelHome : Panel
{
    public Image bgloading;
    public Image imgLoading;
    public Button btnPlay;
    public void Start()
    {
        btnPlay.onClick.AddListener(() =>
        {
            GameManager.Instance.InitializeLevel();
            StartLoading(false, () =>
            {
                LevelGenerator.Instance.StartTimer();
            });
        });
    }
    public void StartLoading(bool isHome, Action onComplete = null)
    {
        bgloading.gameObject.SetActive(true);
        imgLoading.gameObject.SetActive(true);
        btnPlay.interactable = false;
        btnPlay.transform.localScale = Vector3.zero;
        imgLoading.fillAmount = 0;
        imgLoading.DOFillAmount(1f, 3f).OnComplete(() =>
        {
            bgloading.gameObject.SetActive(false);
            imgLoading.gameObject.SetActive(false);
            if (isHome) btnPlay.transform.DOScale(1f, 1f).SetEase(Ease.OutBounce).OnComplete(() => btnPlay.interactable = true);
            else
            {
                Close();
                onComplete?.Invoke();
            }
        });
    }
}
