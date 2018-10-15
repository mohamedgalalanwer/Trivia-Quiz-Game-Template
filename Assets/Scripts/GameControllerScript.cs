using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameControllerScript : MonoBehaviour {

    public List<GameObject> AllControllers;
    static List<GameObject> QuizControllers;
    public GameObject ScoreCanvas;

    //static List<int> DoneControllers = new List<int>();
    static bool AllQAnswered = false;
    static public GameObject CurrentController;
    public static int Score = 0;

    private void Start()
    {
        QuizControllers = AllControllers;
        ActivateCurrentController();
    }

    private void Update()
    {
        if (AllQAnswered)
        {
            ScoreCanvas.SetActive(true);
            PlayerPrefs.SetInt("Score", Score);
        }
    }

    public static void GetControllersFromList()
    {
        System.Random R = new System.Random();

        int i = -1;

        while (i == -1 || i >= QuizControllers.Count)
        {
            if (QuizControllers.Count <= 0)
            {
                Debug.Log("All Controllers Selected");
                AllQAnswered = true;
                return;
            }
            i = R.Next(QuizControllers.Count);
        }

        CurrentController = QuizControllers[i];
        QuizControllers.RemoveAt(i);
    }

    public static void ActivateCurrentController()
    {
        if (!AllQAnswered)
        {
            GetControllersFromList();
            CurrentController.SetActive(true);
        }
    }
}
