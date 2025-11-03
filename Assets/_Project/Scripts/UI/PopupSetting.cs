using Audio;
using UnityEngine;
using UnityEngine.UI;

public class PopupSetting : Popup
{
    public Button toggleVolume;
    public Button exitBtn;
    public GameObject volumeOn;
    public GameObject volumeOff;
    public bool isMute = false;
    private void Start()
    {
        isMute = AudioManager.Ins.IsMute();
        SetState(isMute);
        toggleVolume.onClick.AddListener(() =>
        {
            AudioManager.Ins.ToggleVolume(!isMute);
            isMute = AudioManager.Ins.IsMute();
            SetState(isMute);
        });
        exitBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseCurrentPopup();
            TimeManager.Instance.ResumeTimer();
        });
    }
    void SetState(bool mute = false)
    {
        volumeOn.SetActive(!mute);
        volumeOff.SetActive(mute);
    }
}
