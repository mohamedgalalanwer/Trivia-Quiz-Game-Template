using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Crosstales.RTVoice;


public class ChooseToTranslate_Q_Controller : MonoBehaviour
{

    public ChooseToTranslate_Q_Class[] Questions;

    public GameObject PrefabTextParent;
    public GameObject PanelQuestion, PanelWrongAnswer;
    public ArabicText TextQuestion;
    public GameObject ButtonAnswer;
    public GameObject HeaderPanel;
    public ArabicText TextHeaderPanel;

    public int Score = 0;

    ChooseToTranslate_Q_Class CurrentQuestion;
    Color OldHeaderPanelColor, OldBTNColor, CorrectColor = Color.green, WrongColor = Color.red, SemiColor = Color.yellow;
    string OldTextHeaderPanel, CorrectText = "إجابة صحيحة", WrongText = "إجابة خاطئة", SemiText = "إجابة شبه صحيحة",
        OldButtonText, NewButtonText = "إستمرار";
    List<int> DisplayedQuestions = new List<int>();
    bool AllQAnswered = false;
    bool ButtonFirstClick = true;

    int ChildCount1 = 0, ChildCount2 = 0;

    public GameObject StarParent;
    public Sprite EmptyStar, FullStar;
    public Text TimerText;
    public GameObject CurrentCanvas, ScoreCanvas;

    public bool IsMainController = true;
    Timer T;
    public Image SpeakBTN;
    Color SpeakOldColor = Color.white, SpeaknewColor = Color.green;

    void Start()
    {
        T = GetComponent<Timer>();
        //for full scene
        CurrentCanvas.SetActive(true);
        OldHeaderPanelColor = HeaderPanel.GetComponent<Image>().color;
        OldTextHeaderPanel = TextHeaderPanel.Text;
        OldButtonText = ButtonAnswer.transform.GetChild(0).GetComponent<ArabicText>().Text;
        OldBTNColor = PrefabTextParent.GetComponent<Image>().color;

        GetQuestionFromList();

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
                Debug.Log("All ChooseTOTranslate Selected");
                AllQAnswered = true;
                return;
            }
            i = R.Next(Questions.Length);
        }
        DisplayedQuestions.Add(i);

        ChooseToTranslate_Q_Class Q = new ChooseToTranslate_Q_Class(Questions[i].AnswerIsArabic, Questions[i].Question, Questions[i].AllPossibeWords,
            Questions[i].CorrectAnswer, Questions[i].SemiCorrectAnswers);
        //set All Values in UI:
        TextQuestion.Text = Q.Question;
        int temp = 0, child = 0;
        foreach (string item in Q.AllPossibeWords)
        {
            if (temp == 3 || temp == 6)
                child++;
            PrefabTextParent.transform.GetChild(0).GetComponent<ArabicText>().Text = item;
            PrefabTextParent.GetComponent<Image>().color = OldBTNColor;
            PrefabTextParent.GetComponent<Button>().interactable = true;
            GameObject B = Instantiate(PrefabTextParent, PanelQuestion.transform.GetChild(1).GetChild(child));
            B.GetComponent<Button>().onClick.AddListener(delegate () { OnTextParentClickFirst(B); });
            if (Q.AnswerIsArabic)
                B.transform.SetAsFirstSibling();
            temp++;
        }
        temp = child = 0;
        foreach (string item in Q.CorrectAnswer)
        {
            PanelWrongAnswer.transform.GetChild(0).GetChild(0).GetComponent<ArabicText>().Text += item;
        }
        CurrentQuestion = Q;
        ChildCount1 = ChildCount2 = 0;
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
        else if (PanelQuestion.transform.GetChild(0).GetChild(0).childCount > 0)
            ButtonAnswer.SetActive(true);
        else if (PanelQuestion.transform.GetChild(0).GetChild(1).childCount > 0)
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
        
        StarParent.transform.GetChild(0).GetComponent<Image>().sprite = EmptyStar;
        StarParent.transform.GetChild(1).GetComponent<Image>().sprite = EmptyStar;
        StarParent.transform.GetChild(2).GetComponent<Image>().sprite = EmptyStar;
    }

    void OnTextParentClickFirst(GameObject Button)
    {
        if (PanelQuestion.transform.GetChild(0).GetChild(0).childCount == 3)
            ChildCount1 = 1;
        else if (PanelQuestion.transform.GetChild(0).GetChild(0).childCount < 3)
            ChildCount1 = 0;
        Button.transform.SetParent(PanelQuestion.transform.GetChild(0).GetChild(ChildCount1));
        Button.GetComponent<Button>().onClick.AddListener(delegate () { OnTextParentClickSecond(Button); });
        if (CurrentQuestion.AnswerIsArabic)
            Button.transform.SetAsFirstSibling();

        ReChangeTextPosition(PanelQuestion.transform.GetChild(0).GetChild(0), PanelQuestion.transform.GetChild(0).GetChild(1));
        ReChangeTextPosition(PanelQuestion.transform.GetChild(1).GetChild(0), PanelQuestion.transform.GetChild(1).GetChild(1));
        ReChangeTextPosition(PanelQuestion.transform.GetChild(1).GetChild(1), PanelQuestion.transform.GetChild(1).GetChild(2));
    }

    void OnTextParentClickSecond(GameObject Button)
    {
        if (PanelQuestion.transform.GetChild(1).GetChild(1).childCount == 3)
            ChildCount2 = 2;
        else if (PanelQuestion.transform.GetChild(1).GetChild(0).childCount == 3)
            ChildCount2 = 1;
        else if (PanelQuestion.transform.GetChild(1).GetChild(0).childCount < 3)
            ChildCount2 = 0;
        Button.transform.SetParent(PanelQuestion.transform.GetChild(1).GetChild(ChildCount2));
        Button.GetComponent<Button>().onClick.AddListener(delegate () { OnTextParentClickFirst(Button); });
        if (CurrentQuestion.AnswerIsArabic)
            Button.transform.SetAsFirstSibling();

        ReChangeTextPosition(PanelQuestion.transform.GetChild(1).GetChild(1), PanelQuestion.transform.GetChild(1).GetChild(2));
        ReChangeTextPosition(PanelQuestion.transform.GetChild(1).GetChild(0), PanelQuestion.transform.GetChild(1).GetChild(1));
    }

    public void OnButtonClick()
    {
        if (ButtonFirstClick)
        {
            T.start = false;
            T.Inturupt = true;
            ButtonFirstClick = false;
            string UserAnswer = "", CorrectAnswer = "", temp = "";
            List<string> SemiCorrects = new List<string>();
            //User Answer
            foreach (Transform item in PanelQuestion.transform.GetChild(0))
            {
                if (item.childCount > 0)
                {
                    if (CurrentQuestion.AnswerIsArabic)
                    {
                        for (int d = item.childCount - 1; d >= 0; d--)
                        {
                            UserAnswer += item.GetChild(d).GetChild(0).gameObject.GetComponent<ArabicText>().Text + " ";
                        }
                    }
                    else
                    {
                        foreach (Transform W in item)
                        {
                            UserAnswer += W.GetChild(0).gameObject.GetComponent<ArabicText>().Text + " ";
                        }
                    }
                }
            }
            //CorrectAnswer
            foreach (string item in CurrentQuestion.CorrectAnswer)
            {
                CorrectAnswer += item + " ";
            }
            //All Semi Correct Answers
            foreach (SemiCorrectAnswer_Q_Class item in CurrentQuestion.SemiCorrectAnswers)
            {
                foreach (string w in item.SemiCorrectAnswer)
                {
                    temp += w + " ";
                }
                SemiCorrects.Add(temp.Trim().ToLower());
                temp = "";
            }
            if (UserAnswer.Trim().ToLower().Equals(CorrectAnswer.Trim().ToLower()))
            {
                AnswerDisplay(1, CorrectAnswer);
            }
            else if (SemiCorrects.Contains(UserAnswer.Trim().ToLower()))
            {
                AnswerDisplay(0, CorrectAnswer);
            }
            else
            {
                AnswerDisplay(-1, CorrectAnswer);
            }
        }
        else
        {
            ButtonFirstClick = true;
            NextQuestion();
        }
    }

    //Correct = 1, Semi = 0, Wrong = -1;
    void AnswerDisplay(int Correct, string CorrectAnswer)
    {
        Speak(CorrectAnswer);
        switch (Correct)
        {
            case 1:
                Score += 10;
                SoundController.PlaySoundCorrect();
                //for Full Game Controller
                if(!IsMainController)
                    GameControllerScript.Score += 10;
                HeaderPanel.GetComponent<Image>().color = CorrectColor;
                TextHeaderPanel.Text = CorrectText;
                foreach (Transform item in PanelQuestion.transform.GetChild(0))
                {
                    foreach (Transform Txt in item)
                    {
                        Txt.gameObject.GetComponent<Image>().color = CorrectColor;
                        Txt.gameObject.GetComponent<Button>().interactable = false;
                    }
                }
                PanelQuestion.transform.GetChild(1).gameObject.SetActive(false);
                break;
            case 0:
                Score += 5;
                //for Full Game Controller
                if (!IsMainController)
                    GameControllerScript.Score += 5;
                HeaderPanel.GetComponent<Image>().color = SemiColor;
                TextHeaderPanel.Text = SemiText;
                string[] Lst = CorrectAnswer.Trim().Split(' ');
                if (CurrentQuestion.AnswerIsArabic)
                    Array.Reverse(Lst);
                int i = 0;
                foreach (Transform item in PanelQuestion.transform.GetChild(0))
                {
                    foreach (Transform Txt in item)
                    {
                        if (Txt.transform.GetChild(0).gameObject.GetComponent<ArabicText>().Text == Lst[i])
                            Txt.gameObject.GetComponent<Image>().color = CorrectColor;
                        else
                        {
                            Txt.gameObject.GetComponent<Image>().color = SemiColor;
                            print(Txt.transform.GetChild(0).gameObject.GetComponent<ArabicText>().Text + "   " + Lst[i]);
                        }
                        Txt.gameObject.GetComponent<Button>().interactable = false;
                        i++;
                    }
                }
                i = 0;
                Instantiate(PanelQuestion.transform.GetChild(0).GetChild(0), PanelWrongAnswer.transform.GetChild(1));
                Instantiate(PanelQuestion.transform.GetChild(0).GetChild(1), PanelWrongAnswer.transform.GetChild(1));
                PanelQuestion.SetActive(false);
                PanelWrongAnswer.SetActive(true);
                PanelWrongAnswer.transform.GetChild(0).GetChild(0).GetComponent<ArabicText>().Text = CorrectAnswer;
                break;
            case -1:
                SoundController.PlaySoundWrong();
                HeaderPanel.GetComponent<Image>().color = WrongColor;
                TextHeaderPanel.Text = WrongText;
                foreach (Transform item in PanelQuestion.transform.GetChild(0))
                {
                    foreach (Transform Txt in item)
                    {
                        Txt.gameObject.GetComponent<Image>().color = WrongColor;
                        Txt.gameObject.GetComponent<Button>().interactable = false;
                    }
                }
                Instantiate(PanelQuestion.transform.GetChild(0).GetChild(0), PanelWrongAnswer.transform.GetChild(1));
                Instantiate(PanelQuestion.transform.GetChild(0).GetChild(1), PanelWrongAnswer.transform.GetChild(1));
                PanelQuestion.SetActive(false);
                PanelWrongAnswer.SetActive(true);
                PanelWrongAnswer.transform.GetChild(0).GetChild(0).GetComponent<ArabicText>().Text = CorrectAnswer;
                break;
        }
        ButtonAnswer.transform.GetChild(0).GetComponent<ArabicText>().Text = NewButtonText;
        ChangeStars();
    }

    void NextQuestion()
    {
        SpeakBTN.color = SpeakOldColor;
        HeaderPanel.GetComponent<Image>().color = OldHeaderPanelColor;
        TextHeaderPanel.Text = OldTextHeaderPanel;
        ButtonAnswer.transform.GetChild(0).GetComponent<ArabicText>().Text = OldButtonText;
        ButtonAnswer.SetActive(false);
        PanelQuestion.transform.GetChild(1).gameObject.SetActive(true);
        PanelQuestion.SetActive(true);
        PanelWrongAnswer.SetActive(false);
        foreach (Transform item in PanelQuestion.transform)
        {
            foreach (Transform item2 in item)
            {
                foreach (Transform item3 in item2)
                {
                    Destroy(item3.gameObject);
                }
            }
        }
        if (PanelWrongAnswer.transform.GetChild(1).childCount > 0)
            Destroy(PanelWrongAnswer.transform.GetChild(1).GetChild(1).gameObject);
        if (PanelWrongAnswer.transform.GetChild(1).childCount > 0)
            Destroy(PanelWrongAnswer.transform.GetChild(1).GetChild(0).gameObject);
        GetQuestionFromList();
    }

    void ReChangeTextPosition(Transform First, Transform Second)
    {
        if(First.childCount < 3 && Second.childCount > 0)
        {
            Transform T = Second.GetChild(0);
            T.SetParent(First);
            if (CurrentQuestion.AnswerIsArabic)
                T.SetAsFirstSibling();
        }
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
        if (!CurrentQuestion.AnswerIsArabic)
        {
            Speaker.Speak(txt);
            SpeakBTN.color = SpeaknewColor;
        }
    }
    
}
