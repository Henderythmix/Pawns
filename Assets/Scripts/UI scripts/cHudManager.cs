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


    public Text timerText;
    public Text endOfWaveTimerText;
    public Text moneyText;
    public Text roundLevelText;
    public GameObject mergedObjectHolder;
    public Text mergedNumberText;
    public GameObject playerTabPrefab;
    public Transform areaToMergeTabs;

    public GameObject gameOverCanvas;
    private void Start()
    {
        moneyText.text = LevelManager.instance.playersMoney.ToString();
    }

    //IEnumerator mergePlayerTabs(List<Transform> tabsBeingMerged)
    //{
    //    while (tabsBeingMerged.Count > 0)
    //    {

    //    }
    //    yield return new WaitForEndOfFrame();
    //}
    public void LoadScene(int idx)
    {
        GameManager.instance.LoadScene(idx);
    }
}
