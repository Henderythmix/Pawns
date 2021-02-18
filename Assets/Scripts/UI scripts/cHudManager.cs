using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class cHudManager : MonoBehaviour
{
    public static cHudManager instance;
    private void Awake()
    {
        instance = this;
    }


    public Text roundTimerText;
    public Text moneyText;
    public Text roundLevelText;

    public GameObject gameOverCanvas;
    private void Start()
    {
        moneyText.text = LevelManager.instance.playersMoney.ToString();
    }


    public void PlayAgain()
    {
        GameManager.instance.LoadScene(0);
    }
}
