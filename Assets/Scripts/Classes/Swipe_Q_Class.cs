using UnityEngine;

[System.Serializable]
public class Swipe_Q_Class {
    public string QuestionWord, Answer1, Answer2;
    public CorrectAnswerDirection CorrectAnswer;
    public Sprite Answer1Img, Answer2Img;
    public bool isArabicAnswer;

    public Swipe_Q_Class(string questionWord, string answer1, string answer2, CorrectAnswerDirection correctAnswer, Sprite answer1Img, Sprite answer2Img)
    {
        QuestionWord = questionWord;
        Answer1 = answer1;
        Answer2 = answer2;
        CorrectAnswer = correctAnswer;
        Answer1Img = answer1Img;
        Answer2Img = answer2Img;
    }
}
public enum CorrectAnswerDirection
{
    FirstAnswer,
    SecondAnswer
}
public enum SwpipeDirection
{
    None, UP, Down
}
