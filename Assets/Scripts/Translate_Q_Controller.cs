using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.RTVoice;

public class Translate_Q_Controller : MonoBehaviour
{

    public Translate_Q_Class[] Questions;
    public ArabicText TextQuestion;
    public InputField TextAnswer;
    public GameObject ButtonAnswer;
    public GameObject TextTitle, TextCorrectAnswer;
    public GameObject HeaderPanel;
    public ArabicText TextHeaderPanel;

    public int Score = 0;
    
    Translate_Q_Class CurrentQuestion;
    Color OldHeaderPanelColor, CorrectColor = Color.green, WrongColor = Color.red;
    string OldTextHeaderPanel, CorrectText = "إجابة صحيحة", WrongText = "إجابة خاطئة",
        OldButtonText, NewButtonText = "إستمرار";
    List<int> DisplayedQuestions = new List<int>();
    bool AllQAnswered = false;
    bool ButtonFirstClick = true;

    public GameObject StarParent;
    public Sprite EmptyStar, FullStar;
    public Text TimerText;

    public GameObject CurrentCanvas, ScoreCanvas;
    public bool IsMainController = true;
    Timer T;
    public Image SpeakBTN;
    Color SpeakOldColor = Color.white, SpeaknewColor = Color.green;

    private void Start()
    {
        T = GetComponent<Timer>();
        //for full scene
        CurrentCanvas.SetActive(true);
        GetQuestionFromList();
        OldHeaderPanelColor = HeaderPanel.GetComponent<Image>().color;
        OldTextHeaderPanel = TextHeaderPanel.Text;
        OldButtonText = ButtonAnswer.transform.GetChild(0).GetComponent<ArabicText>().Text;
    }

    void GetQuestionFromList()
    {
        T.start = true;

        System.Random R = new System.Random();

        int i = -1;

        while (i == -1 || DisplayedQuestions.Contains(i) || i >= Questions.Length)
        {
            if (DisplayedQuestions.Count >= Questions.Length)
            {
                Debug.Log("All Translate Selected");
                AllQAnswered = true;
                return;
            }
            i = R.Next(Questions.Length);
        }
        DisplayedQuestions.Add(i);

        Translate_Q_Class Q = new Translate_Q_Class(Questions[i].Question, Questions[i].CorrectAnswer);
        TextQuestion.Text = Q.Question;
        TextCorrectAnswer.GetComponent<Text>().text = Q.CorrectAnswer;
        CurrentQuestion = Q;
        SoundController.PlaySoundQuestion();
    }

    void Update()
    {
        TimerText.text = T.CurrentTime.ToString();
        if (T.TimeUp)
        {
            ButtonAnswer.SetActive(true);
            if (T.PlaySound)
            {
                OnButtonClick();
                SoundController.PlaySoundTimeUP();
                T.PlaySound = false;
            }
        }
        else if (!TextAnswer.text.Trim().Equals(""))
            ButtonAnswer.SetActive(true);
        else
            ButtonAnswer.SetActive(false);
        if (AllQAnswered)
            DoNextQuestions();
    }

    void DoNextQuestions()
    {
        CurrentCanvas.SetActive(false);
        this.gameObject.SetActive(false);

        if (!IsMainController)
            GameControllerScript.ActivateCurrentController();
        else
        {
            ScoreCanvas.SetActive(true);
            PlayerPrefs.SetInt("Score", Score);
        }
    }

    public void OnButtonClick()
    {
        if (ButtonFirstClick)
        {
            T.start = false;
            T.Inturupt = true;
            ButtonFirstClick = false;
            string UserAnswer = TextAnswer.text;
            if (UserAnswer.Trim().ToLower().Equals(CurrentQuestion.CorrectAnswer.Trim().ToLower()))
            {
                AnswerDisplay(true);
            }
            else
            {
                AnswerDisplay(false);
            }
        }
        else
        {
            ButtonFirstClick = true;
            NextQuestion();
        }
    }

    void AnswerDisplay(bool Correct)
    {
        Speak(CurrentQuestion.CorrectAnswer);
        if (Correct)
        {
            Score += 10;
            SoundController.PlaySoundCorrect();
            //for Full Game Controller
            if (!IsMainController)
                GameControllerScript.Score += 10;
            HeaderPanel.GetComponent<Image>().color = CorrectColor;
            TextHeaderPanel.Text = CorrectText;
        }
        else
        {
            SoundController.PlaySoundWrong();
            HeaderPanel.GetComponent<Image>().color = WrongColor;
            TextHeaderPanel.Text = WrongText;
            TextTitle.SetActive(true);
            TextCorrectAnswer.SetActive(true);
        }
        ButtonAnswer.transform.GetChild(0).GetComponent<ArabicText>().Text = NewButtonText;
        TextAnswer.readOnly = true;

        ChangeStars();
    }

    void NextQuestion()
    {
        SpeakBTN.color = SpeakOldColor;
        HeaderPanel.GetComponent<Image>().color = OldHeaderPanelColor;
        TextHeaderPanel.Text = OldTextHeaderPanel;
        ButtonAnswer.transform.GetChild(0).GetComponent<ArabicText>().Text = OldButtonText;
        TextAnswer.text = "";
        TextAnswer.readOnly = false;
        ButtonAnswer.SetActive(false);
        TextTitle.SetActive(false);
        TextCorrectAnswer.SetActive(false);
        GetQuestionFromList();
    }


    void ChangeStars()
    {
        if (Score == Questions.Length * 10)
        {
            StarParent.transform.GetChild(0).GetComponent<Image>().sprite = FullStar;
            StarParent.transform.GetChild(1).GetComponent<Image>().sprite = FullStar;
            StarParent.transform.GetChild(2).GetComponent<Image>().sprite = FullStar;
        }
        else if (Score >= (Questions.Length * 10 * 0.6f))
        {
            print(Score + "  " + Questions.Length);
            StarParent.transform.GetChild(0).GetComponent<Image>().sprite = FullStar;
            StarParent.transform.GetChild(1).GetComponent<Image>().sprite = FullStar;
            StarParent.transform.GetChild(2).GetComponent<Image>().sprite = EmptyStar;
        }
        else if (Score >= (Questions.Length * 10 * 0.3f))
        {
            StarParent.transform.GetChild(0).GetComponent<Image>().sprite = FullStar;
            StarParent.transform.GetChild(1).GetComponent<Image>().sprite = EmptyStar;
            StarParent.transform.GetChild(2).GetComponent<Image>().sprite = EmptyStar;
        }
    }

    void Speak(string txt)
    {
        Speaker.Speak(txt);
        SpeakBTN.color = SpeaknewColor;
    }
}