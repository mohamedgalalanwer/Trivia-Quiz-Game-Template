using UnityEngine.UI;
using UnityEngine;

public class Buttons : MonoBehaviour {
    public AudioSource ASource1, ASource2;
    public Sprite SoundOn, SoundOff;
    public GameObject SoundButton;
    
    public void SoundBtnClick()
    {
        if (ASource1.mute)
        {
            ASource1.mute = false;
            ASource2.mute = false;
            SoundButton.GetComponent<Image>().sprite = SoundOn;
        }
        else
        {
            ASource1.mute = true;
            ASource2.mute = true;
            SoundButton.GetComponent<Image>().sprite = SoundOff;
        }
    }
}
