using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.RTVoice;

public class Word_Q_Controller : MonoBehaviour {

    public Word_Q_Class[] Questions;

    public GameObject PrefabWordParent;
    public ArabicText TextQuestion;
    public Image TextPicture;
    public GameObject PanelQ, PanelA;
    public GameObject ButtonAnswer;
    public GameObject HeaderPanel;
    public ArabicText TextHeaderPanel;
    public GameObject CorrectAnswer;

    public int Score = 0;

    Word_Q_Class CurrentQuestion;
    Color OldHeaderPanelColor, OldBTNColor, CorrectColor = Color.green, WrongColor = Color.red;
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

    void Start()
    {
        T = GetComponent<Timer>();
        //for full scene
        CurrentCanvas.SetActive(true);
        OldHeaderPanelColor = HeaderPanel.GetComponent<Image>().color;
        OldTextHeaderPanel = TextHeaderPanel.Text;
        OldButtonText = ButtonAnswer.transform.GetChild(0).GetComponent<ArabicText>().Text;
        OldBTNColor = PrefabWordParent.GetComponent<Image>().color;

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

        Word_Q_Class Q = new Word_Q_Class(Questions[i].Question, Questions[i].Answer, Questions[i].Image, Questions[i].IsArabicLitters);

        //set All Values in UI:
        TextQuestion.Text = Q.Question;
        TextPicture.sprite = Q.Image;
        char[] chars = Shuffle(Q.Answer).ToUpper().ToCharArray();
        foreach (char item in chars)
        {
            PrefabWordParent.transform.GetChild(0).GetComponent<ArabicText>().Text = item.ToString();
            PrefabWordParent.GetComponent<Image>().color = OldBTNColor;
            PrefabWordParent.GetComponent<Button>().interactable = true;
            GameObject B = Instantiate(PrefabWordParent, PanelA.transform);
            B.GetComponent<Button>().onClick.AddListener(delegate () { OnTextParentClickFirst(B); });
            if (Q.IsArabicLitters)
                B.transform.SetAsFirstSibling();
        }
        CorrectAnswer.GetComponent<ArabicText>().Text = Q.Answer.ToUpper();
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
        else if (PanelA.transform.childCount < 2)
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
        Button.transform.SetParent(PanelQ.transform);
        Button.GetComponent<Button>().onClick.AddListener(delegate () { OnTextParentClickSecond(Button); });
        if (CurrentQuestion.IsArabicLitters)
            Button.transform.SetAsFirstSibling();
    }

    void OnTextParentClickSecond(GameObject Button)
    {
        Button.transform.SetParent(PanelA.transform);
        Button.GetComponent<Button>().onClick.AddListener(delegate () { OnTextParentClickFirst(Button); });
        if (CurrentQuestion.IsArabicLitters)
            Button.transform.SetAsFirstSibling();
    }

    public void OnButtonClick()
    {
        if (ButtonFirstClick)
        {
            T.start = false;
            T.Inturupt = true;
            ButtonFirstClick = false;

            string UserAnswer = "";
            if (CurrentQuestion.IsArabicLitters)
                for (int d = PanelQ.transform.childCount - 1; d >= 0; d--)
                    UserAnswer += PanelQ.transform.GetChild(d).GetChild(0).gameObject.GetComponent<ArabicText>().Text;
            else
                foreach (Transform item in PanelQ.transform)
                    UserAnswer += item.transform.GetChild(0).gameObject.GetComponent<ArabicText>().Text;

            if (UserAnswer.Trim().ToLower().Equals(CurrentQuestion.Answer.Trim().ToLower()))
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
        Speak(CurrentQuestion.Answer);
        if (Correct)
        {
            Score += 10;
            SoundController.PlaySoundCorrect();
            //for Full Game Controller
            if (!IsMainController)
                GameControllerScript.Score += 10;
            HeaderPanel.GetComponent<Image>().color = CorrectColor;
            TextHeaderPanel.Text = CorrectText;
            foreach (Transform item in PanelQ.transform)
            {
                item.gameObject.GetComponent<Image>().color = CorrectColor;
                item.gameObject.GetComponent<Button>().interactable = false;
            }
        }
        else
        {
            SoundController.PlaySoundWrong();
            HeaderPanel.GetComponent<Image>().color = WrongColor;
            TextHeaderPanel.Text = WrongText;
            foreach (Transform item in PanelQ.transform)
            {
                item.gameObject.GetComponent<Image>().color = WrongColor;
                item.gameObject.GetComponent<Button>().interactable = false;
            }
            if(!CurrentQuestion.IsArabicLitters)
                for (int i = PanelA.transform.childCount - 1; i > 0; i--)
                    Destroy(PanelA.transform.GetChild(i).gameObject);
            else
                for (int i = 0; i < PanelA.transform.childCount - 1; i++)
                    Destroy(PanelA.transform.GetChild(i).gameObject);
            CorrectAnswer.SetActive(true);
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
        CorrectAnswer.SetActive(false);

        foreach (Transform item in PanelQ.transform)
        {
            Destroy(item.gameObject);
        }
        for (int i = PanelA.transform.childCount - 1; i > 0 ; i--)
        {
            Destroy(PanelA.transform.GetChild(i).gameObject);
        }
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

    string Shuffle(string str)
    {
        char[] array = str.ToCharArray();
        System.Random rng = new System.Random();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
        return new string(array);
    }

    void Speak(string txt)
    {
        if (!CurrentQuestion.IsArabicLitters)
        {
            Speaker.Speak(txt);
            SpeakBTN.color = SpeaknewColor;
        }
    }
}
