using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Word_Q_Class {
    public string Question, Answer;
    public Sprite Image;
    public bool IsArabicLitters;

    public Word_Q_Class(string question, string answer, Sprite image, bool isarabiclitters)
    {
        Question = question;
        Answer = answer;
        Image = image;
        IsArabicLitters = isarabiclitters;
    }
}
