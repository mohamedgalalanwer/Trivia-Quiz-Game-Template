using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.RTVoice;


public class ChooseWord_Q_ControllerV2 : MonoBehaviour {

    public ChooseWord_Q_Class[] Questions;

    public GameObject PrefabTextParent, PrefabTempText, PrefabWordPart;
    public GameObject PanelWrongAnswer,PanelWrite1,PanelWrite2,PanelAnswers,PanelHolder;
    public GameObject TextQuestion;
    public GameObject ButtonAnswer;
    public GameObject HeaderPanel;
    public ArabicText TextHeaderPanel;

    public int Score = 0;

    ChooseWord_Q_Class CurrentQuestion;
    Color OldHeaderPanelColor, OldBTNColor, CorrectColor = Color.green, WrongColor = Color.red;
    string OldTextHeaderPanel, CorrectText = "إجابة صحيحة", WrongText = "إجابة خاطئة",
        OldButtonText, NewButtonText = "إستمرار";
    List<int> DisplayedQuestions = new List<int>();
    bool AllQAnswered = false;
    bool ButtonFirstClick = true;

    int ChildCount = 0;

    GameObject TmpTxtObj, OldClickedBTN;
    // 1:Up 0    2: Up 1     3:Down 0   4: Down 1
    int WordPlace = 0;
    bool ShowAnswerButton = false;

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
        WordPlace = 0;
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
        ChooseWord_Q_Class Q = new ChooseWord_Q_Class(Questions[i].QuestIsArabic, Questions[i].Question,
            Questions[i].AnsFirstPart, Questions[i].AnsSecondPart, Questions[i].HiddenWord, Questions[i].AllPossibleWords);

        //set All Values in UI:
        if (Q.QuestIsArabic)
        {
            TextQuestion.GetComponent<ArabicText>().enabled = true;
            TextQuestion.GetComponent<ArabicText>().Text = Q.Question;
            PanelWrongAnswer.transform.GetChild(0).GetChild(0).GetComponent<ArabicText>().enabled = false;
            PanelWrongAnswer.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = Q.AnsFirstPart + " " + Q.HiddenWord + " " + Q.AnsSecondPart;
        }
        else
        {
            TextQuestion.GetComponent<ArabicText>().enabled = false;
            TextQuestion.GetComponent<Text>().text = Q.Question;
            PanelWrongAnswer.transform.GetChild(0).GetChild(0).GetComponent<ArabicText>().enabled = true;
            PanelWrongAnswer.transform.GetChild(0).GetChild(0).GetComponent<ArabicText>().Text = Q.AnsFirstPart + " " + Q.HiddenWord + " " + Q.AnsSecondPart;
        }
        GameObject P1 = Instantiate(PrefabWordPart, PanelWrite1.transform);
        P1.transform.GetChild(0).gameObject.GetComponent<ArabicText>().Text = Q.AnsFirstPart;
        GameObject P2 = Instantiate(PrefabWordPart, PanelWrite2.transform);
        P2.transform.GetChild(0).gameObject.GetComponent<ArabicText>().Text = Q.AnsSecondPart;

        if (Q.AnsFirstPart.Length > Q.AnsSecondPart.Length)
        {
            TmpTxtObj = Instantiate(PrefabTempText, PanelWrite2.transform);
            WordPlace = 4;
            if (Q.QuestIsArabic)
            {
                TmpTxtObj.transform.SetAsFirstSibling();
                WordPlace = 3;
            }
        }
        else
        {
            TmpTxtObj = Instantiate(PrefabTempText, PanelWrite1.transform);
            WordPlace = 2;
            if (!Q.QuestIsArabic)
            {
                TmpTxtObj.transform.SetAsFirstSibling();
                WordPlace = 1;
            }
        }
        int temp = 0, child = 0;
        foreach (string item in Q.AllPossibleWords)
        {
            if (temp == 3 || temp == 6)
                child++;
            PrefabTextParent.transform.GetChild(0).GetComponent<ArabicText>().Text = item;
            PrefabTextParent.GetComponent<Image>().color = OldBTNColor;
            PrefabTextParent.GetComponent<Button>().interactable = true;
            GameObject B = Instantiate(PrefabTextParent, PanelAnswers.transform.GetChild(child));
            B.GetComponent<Button>().onClick.AddListener(delegate () { OnTextParentClickFirst(B); });
            if (!Q.QuestIsArabic)
                B.transform.SetAsFirstSibling();
            temp++;
        }
        temp = child = 0;
        CurrentQuestion = Q;
        ChildCount = 0;
        OldClickedBTN = null;
        SoundController.PlaySoundQuestion();
    }

    void Update()
    {
        TimerText.text = T.CurrentTime.ToString();
        if (T.TimeUp)
        {
            //ButtonAnswer.SetActive(true);
            if (T.PlaySound)
            {
                ShowAnswerButton = true;
                OnButtonClick();
                SoundController.PlaySoundTimeUP();
                T.PlaySound = false;
            }
        }
        ButtonAnswer.SetActive(ShowAnswerButton);
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

    void OnTextParentClickFirst(GameObject Button)
    {
        if (OldClickedBTN != null)
        {
            OnTextParentClickSecond(OldClickedBTN);
        }
        TmpTxtObj.gameObject.SetActive(false);
        switch (WordPlace)
        {
            case 1:
                Button.transform.SetParent(PanelWrite1.transform);
                Button.transform.SetAsFirstSibling();
                break;
            case 2:
                Button.transform.SetParent(PanelWrite1.transform);
                break;
            case 3:
                Button.transform.SetParent(PanelWrite2.transform);
                Button.transform.SetAsFirstSibling();
                break;
            case 4:
                Button.transform.SetParent(PanelWrite2.transform);
                break;
        }
        Button.GetComponent<Button>().onClick.AddListener(delegate () { OnTextParentClickSecond(Button); });
        OldClickedBTN = Button;
        ShowAnswerButton = true;
        
        ReChangeTextPosition(PanelAnswers.transform.GetChild(0), PanelAnswers.transform.GetChild(1));
        ReChangeTextPosition(PanelAnswers.transform.GetChild(1), PanelAnswers.transform.GetChild(2));
    }

    void OnTextParentClickSecond(GameObject Button)
    {
        OldClickedBTN = null;//
        ShowAnswerButton = false;
        TmpTxtObj.gameObject.SetActive(true);
        if (PanelAnswers.transform.GetChild(1).childCount == 3)
            ChildCount = 2;
        else if (PanelAnswers.transform.GetChild(0).childCount == 3)
            ChildCount = 1;
        else if (PanelAnswers.transform.GetChild(0).childCount < 3)
            ChildCount = 0;
        Button.transform.SetParent(PanelAnswers.transform.GetChild(ChildCount));
        Button.GetComponent<Button>().onClick.AddListener(delegate () { OnTextParentClickFirst(Button); });
        if (!CurrentQuestion.QuestIsArabic)
            Button.transform.SetAsFirstSibling();

        ReChangeTextPosition(PanelAnswers.transform.GetChild(1), PanelAnswers.transform.GetChild(2));
        ReChangeTextPosition(PanelAnswers.transform.GetChild(0), PanelAnswers.transform.GetChild(1));
    }

    void ReChangeTextPosition(Transform First, Transform Second)
    {
        if (First.childCount < 3 && Second.childCount > 0)
        {
            Transform T = Second.GetChild(0);
            T.SetParent(First);
            if (!CurrentQuestion.QuestIsArabic)
                T.SetAsFirstSibling();
        }
    }

    public void OnButtonClick()
    {
        if (ButtonFirstClick)
        {
            T.start = false;
            T.Inturupt = true;
            ButtonFirstClick = false;
            string UserAnswer;
            if (OldClickedBTN != null)
                UserAnswer = OldClickedBTN.transform.GetChild(0).GetComponent<ArabicText>().Text;
            else
                UserAnswer = "";
            if (UserAnswer.Equals(CurrentQuestion.HiddenWord))
                AnswerDisplay(true);
            else
                AnswerDisplay(false);
        }
        else
        {
            ButtonFirstClick = true;
            NextQuestion();
        }
    }

    void AnswerDisplay(bool Correct)
    {
        Speak(CurrentQuestion.HiddenWord);
        if (Correct)
        {
            Score += 10;
            SoundController.PlaySoundCorrect();
            //for Full Game Controller
            if (!IsMainController)
                GameControllerScript.Score += 10;
            HeaderPanel.GetComponent<Image>().color = CorrectColor;
            TextHeaderPanel.Text = CorrectText;
            OldClickedBTN.GetComponent<Image>().color = CorrectColor;
            OldClickedBTN.GetComponent<Button>().interactable = false;
            PanelAnswers.SetActive(false);
        }
        else
        {
            SoundController.PlaySoundWrong();
            HeaderPanel.GetComponent<Image>().color = WrongColor;
            TextHeaderPanel.Text = WrongText;
            if (OldClickedBTN != null)
            {
                OldClickedBTN.GetComponent<Image>().color = WrongColor;
                OldClickedBTN.GetComponent<Button>().interactable = false;
            }

            Instantiate(PanelWrite1, PanelWrongAnswer.transform.GetChild(1));
            Instantiate(PanelWrite2, PanelWrongAnswer.transform.GetChild(1));
            PanelHolder.SetActive(false);
            PanelWrongAnswer.SetActive(true);
        }
        ButtonAnswer.transform.GetChild(0).GetComponent<ArabicText>().Text = NewButtonText;
        ChangeStars();
    }

    void NextQuestion()
    {

        ShowAnswerButton = false;
        SpeakBTN.color = SpeakOldColor;
        HeaderPanel.GetComponent<Image>().color = OldHeaderPanelColor;
        TextHeaderPanel.Text = OldTextHeaderPanel;
        ButtonAnswer.transform.GetChild(0).GetComponent<ArabicText>().Text = OldButtonText;
        ButtonAnswer.SetActive(false);
        PanelAnswers.SetActive(true);
        PanelHolder.SetActive(true);
        PanelWrongAnswer.SetActive(false);

        foreach (Transform item in PanelWrite1.transform)
        {
            Destroy(item.gameObject);
        }
        foreach (Transform item in PanelWrite2.transform)
        {
            Destroy(item.gameObject);
        }
        foreach (Transform item in PanelAnswers.transform)
        {
            foreach (Transform item2 in item)
            {
                Destroy(item2.gameObject);
            }
        }
        foreach (Transform item in PanelWrongAnswer.transform.GetChild(1))
        {
            Destroy(item.gameObject);
        }
        OldClickedBTN = null;
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
        if (CurrentQuestion.QuestIsArabic)
        {
            Speaker.Speak(txt);
            SpeakBTN.color = SpeaknewColor;
        }
    }
}
