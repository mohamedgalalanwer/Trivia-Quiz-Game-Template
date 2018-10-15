using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {

    public AudioClip SCorrect, SWrong, SGameOver, SVictory, SQestion, STimeUp;

    static AudioClip Correct, Wrong,
        GameOver, Victory,
        Question, TimeUp;
    public static AudioSource AS;
    // Use this for initialization
    void Start()
    {
        Correct = SCorrect; Wrong = SWrong; GameOver = SGameOver; Victory = SVictory;
        Question = SQestion; TimeUp = STimeUp;
        AS = GetComponent<AudioSource>();
    }

    public static void PlaySoundCorrect()
    {
        print(Correct.length);
        AS.clip = Correct;
        AS.Play();
    }
    public static void PlaySoundWrong()
    {
        AS.clip = Wrong;
        AS.Play();
    }
    public static void PlaySoundGameOver()
    {
        AS.clip = GameOver;
        AS.Play();
    }
    public static void PlaySoundVictory()
    {
        AS.clip = Victory;
        AS.Play();
    }
    public static void PlaySoundQuestion()
    {
        AS.clip = Question;
        AS.Play();
    }
    public static void PlaySoundTimeUP()
    {
        AS.clip = TimeUp;
        AS.Play();
    }

}