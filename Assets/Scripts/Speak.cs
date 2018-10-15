using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.RTVoice;

public class Speak : MonoBehaviour {
    public string txt;
    public string VoiceName;
    public void Talk()
    {
        Speaker.Speak(txt);
    }
}
