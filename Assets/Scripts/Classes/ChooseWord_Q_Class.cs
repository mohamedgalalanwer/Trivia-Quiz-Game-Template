using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChooseWord_Q_Class {

    public bool QuestIsArabic;
    public string Question;
    public string AnsFirstPart,AnsSecondPart;
    public string HiddenWord;
    public List<string> AllPossibleWords;

    public ChooseWord_Q_Class(bool questionIsArabic, string question,
        string answerFirstPart, string answerSecondPart, string hiddenWord, List<string> allPossibleWords)
    {
        QuestIsArabic = questionIsArabic;
        Question = question;
        AnsFirstPart = answerFirstPart;
        AnsSecondPart = answerSecondPart;
        HiddenWord = hiddenWord;
        AllPossibleWords = allPossibleWords;
    }
}
