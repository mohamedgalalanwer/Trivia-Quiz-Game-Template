using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassScore : MonoBehaviour {
    public int Excellent, VeryGood;
    public GameObject Cup;
    public Image Grade;
    public Sprite SpriteExce, SpriteVGood, SpriteGood;
    public AudioClip Victory, GameOver;
    public int VictoryMinimum;
    public AudioSource Audiosource;
    public int FullMarkNumber = 100;
	// Use this for initialization
	void Start () {
        int score = PlayerPrefs.GetInt("Score", 0);
        this.gameObject.GetComponent<Text>().text = FullMarkNumber + "/" + score;
        if(score >= VictoryMinimum)
        {
            Audiosource.clip = Victory;
            Audiosource.Play();
        }
        else
        {
            Audiosource.clip = GameOver;
            Audiosource.Play();
        }
        if(score >= Excellent)
        {
            Cup.SetActive(true);
            Grade.sprite = SpriteExce;
        }
        else if(score >= VeryGood)
        {
            Cup.SetActive(false);
            Grade.sprite = SpriteVGood;
        }
        else
        {
            Cup.SetActive(false);
            Grade.sprite = SpriteGood;
        }
	}
}
