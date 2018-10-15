using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChooseToTranslate_Q_Class {

    public bool AnswerIsArabic;
    public string Question;
    public List<string> AllPossibeWords;
    public List<string> CorrectAnswer;
    public List<SemiCorrectAnswer_Q_Class> SemiCorrectAnswers;

    public ChooseToTranslate_Q_Class(bool answerisarabic, string question, List<string> allPossibeWords, List<string> correctAnswer
        , List<SemiCorrectAnswer_Q_Class> semiCorrectAnswers)
    {
        AnswerIsArabic = answerisarabic;
        Question = question;
        AllPossibeWords = allPossibeWords;
        CorrectAnswer = correctAnswer;
        SemiCorrectAnswers = semiCorrectAnswers;
    }
}
[System.Serializable]
public class SemiCorrectAnswer_Q_Class
{
    public List<string> SemiCorrectAnswer;
}
