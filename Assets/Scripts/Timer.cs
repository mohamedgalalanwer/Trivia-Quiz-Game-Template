using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour {

    public float TargetTime = 10;
    float CountDown = 0;
    [HideInInspector]
    public bool start = true, Inturupt = false, TimeUp = false, PlaySound = false;
    [HideInInspector]
    public int CurrentTime;

    // declare in start
    // set start = true in getquestionfromlist
    // set start  = false in btnclick
    // set interupt  = true in btnclick
    // if timeup in update      btn.avtove = true
    // play time up sounf un update
    //set currenttime in ui on update
    //call onbtnclick if timeup  inudate

    private void Start()
    {
        CountDown = TargetTime;
    }
    private void FixedUpdate()
    {
        if (start)
        {
            TimeUp = false;
            Inturupt = false;
            CountDown -= Time.deltaTime;

            CurrentTime = (int) CountDown + 1;
        }
        else
        {
            CountDown = TargetTime;
        }
        if(CountDown <= 0f)
        {
            CountDown = 2;
            TimeUp = true;
            start = false;
            PlaySound = true;
        }
        if (Inturupt)
        {
            TimeUp = true;
            start = false;
        }
    }
}
