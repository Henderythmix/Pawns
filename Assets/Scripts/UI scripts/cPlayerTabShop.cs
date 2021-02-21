using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cPlayerTabShop : MonoBehaviour
{
    [HideInInspector]public cShopFunctionality theShopScript;
    public Text playerCountText;
    [Header("Current stat texts")]
    public Text currentMaxHealthText;
    public Text currentDamageOutputText;
    public Text currentHealthText;
    [Header("Price texts")]
    public Text healthPriceText;
    public Text damageOutputPriceText;
    public Text maxHealthPriceText;
    public GameObject deadPlayerShowOver;
    [HideInInspector]public PlayerData thisPlayer;

    public void PurchaseHealth(float cost)
    {
        theShopScript.BuyMaxHealth(cost, thisPlayer);
        currentHealthText.text = "Current: " + thisPlayer.maxHealth;
    }

    public void PurchaseMaxHealth(float cost)
    {
        float tmp = thisPlayer.maxHealth;
        theShopScript.BuyIncreasedMaxHealth(cost, thisPlayer, 100);
        if (tmp == thisPlayer.maxHealth)
            tmp += 100;

        Debug.Log(tmp);
        currentMaxHealthText.text = "Current: " + tmp;
    }

    public void PurchaseIncreasedDamage(float cost)
    {
        float tmp = thisPlayer.damageOutput;
        theShopScript.BuyIncreasedDamage(cost, thisPlayer, 25);
        if (tmp == thisPlayer.damageOutput)
            tmp += 25;
        currentDamageOutputText.text = "Current: " + tmp;
    }

    public void BuyDeadPlayerBack()
    {
        deadPlayerShowOver.SetActive(false);
        if (theShopScript.purchased(thisPlayer.playerType.costToRevive))
        {
            thisPlayer.dead = false;
            theShopScript.SpawnCharacterShop(thisPlayer);
        }
    }
}
