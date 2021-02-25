using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class cShopFunctionality : MonoBehaviour
{
    public enum ShopSections
    {
        UPGRADES,
        SHOP,
    }

    public ShopSections shopSection = ShopSections.SHOP;
    LevelManager lmi;
    public Transform shopSpawnPos;
    public GameObject shopUIHolder;
    public GameObject upgradeUIHolder;
    public Image shopTabButton;
    public Image upgradesTabButton;
    public RectTransform playerTabGrouper;
    public GameObject playerTabPrefab;
    public AudioSource purchaseSound;
    [Header("Parameters for upgrades")]
    public float maxHealthObtainable = 500;
    public float maxDamageOutput = 100;
    [Header("Parameters for the shop")]
    public float maxObtainableCharacters = 6;
    private Color selectedButtonColor;
    private Color normalColor;
    List<cPlayerTabShop> playerTabs;
    private void Start()
    {
        lmi = LevelManager.instance;
        SetupPlayerTabs();
        selectedButtonColor = shopTabButton.color;
        normalColor = upgradesTabButton.color;
    }

    public void BuyMaxHealth(float cost, PlayerData thePlayer)
    {
        if (purchased(cost))
        {
            //lmi.playerCharactersAlive[playerChar].playerAIScript.health = lmi.playerCharactersAlive[playerChar].playerAIScript.maxHealth;
            //lmi.playerCharactersSpawned[playerChar].playerAIScript.SetHealth(lmi.playerCharactersSpawned[playerChar].playerAIScript.maxHealth, true);
            bool found = false;
            for (int i = 0; i < lmi.playerCharactersSpawned.Count; i++)
            {
                if (lmi.playerCharactersSpawned[i].thisPlayerData == thePlayer)
                {
                    lmi.playerCharactersSpawned[i].playerAIScript.SetHealth(thePlayer.health, true);
                    found = true;
                }
            }
            if (!found)
            {
                thePlayer.health = thePlayer.maxHealth;
            }
        }
    }

    public void BuyIncreasedMaxHealth(float cost, PlayerData thePlayer, float amount)
    {
        sPlayerController foundPlayer = lmi.playerCharactersSpawned[0];
        float curMaxHealth = 0;
        for (int i = 0; i < lmi.playerCharactersSpawned.Count; i++)
        {
            if (lmi.playerCharactersSpawned[i].thisPlayerData == thePlayer)
            {
                foundPlayer = lmi.playerCharactersSpawned[i];
                curMaxHealth = foundPlayer.playerAIScript.maxHealth;
                break;
            }
        }
        if (curMaxHealth == 0)
        {
            curMaxHealth = thePlayer.maxHealth;
            foundPlayer = null;
        }
        if (curMaxHealth < maxHealthObtainable)
        {
            if (purchased(cost))
            {
                //lmi.playerCharactersSpawned[playerchar].playerAIScript.SetNewMaxHealth(amount, true, false, true);
                if (foundPlayer)
                {
                    foundPlayer.playerAIScript.SetNewMaxHealth(amount, true, false, true);
                    foundPlayer.playerAIScript.SetHealth(amount, false);
                }
                else
                {
                    thePlayer.maxHealth += amount;
                    thePlayer.health += amount;
                }
                // This means they aren't spawned at the moment (dead or merged)

            }
        }
    }

    public void BuyIncreasedDamage(float cost, PlayerData thePlayer, float amount)
    {
        if (purchased(cost))
        {
            bool found = false;
            for (int i = 0; i < lmi.playerCharactersSpawned.Count; i++)
            {
                if (lmi.playerCharactersSpawned[i].thisPlayerData == thePlayer)
                {
                    found = true;
                    lmi.playerCharactersSpawned[i].damageOutput += amount;
                }
            }

            if (!found)
                thePlayer.damageOutput += amount;
        }
    }

    public void BuyBackPlayerCharacter(PlayerData thePlayer)
    {
        if (purchased(thePlayer.playerType.costToRevive))
        {
            SpawnCharacterShop(thePlayer);
        }
    }

    public void BuyNewPlayer(float cost)
    {
        if (lmi.playerCharactersGlobal.Count < maxObtainableCharacters)
        {
            if (purchased(cost))
            {
                Player chosenPlayer = lmi.playerCharacters[0];
                PlayerData newPlayer = new PlayerData();

                newPlayer.damageOutput = chosenPlayer.playerCharacterType.damageOutput;
                newPlayer.health = chosenPlayer.playerCharacterType.health;
                newPlayer.maxHealth = chosenPlayer.playerCharacterType.health;
                newPlayer.playerType = chosenPlayer;

                lmi.playerCharactersGlobal.Add(newPlayer);

                //Debug.Log(newPlayer.playerType);

                SpawnCharacterShop(newPlayer);
                CreatePlayerTab(newPlayer, playerTabs.Count + 1);
            }
        }
    }

    public void HealAllCache(float cost)
    {
        if (purchased(cost))
        {
            for (int i = 0; i < lmi.currentCache.Count; i++)
            {
                lmi.currentCache[i].health = lmi.currentCache[i].maxHealth;
            }
        }
    }

    public void SpawnCharacterShop(PlayerData thePlayer)
    {
        sPlayerController newSpawnedPlayer = Instantiate(lmi.playerPrefab).GetComponent<sPlayerController>();
        sAiController newSpawnedPlayerAi = newSpawnedPlayer.transform.GetChild(2).gameObject.GetComponent<sAiController>();
        Debug.Log(thePlayer.playerType);
        newSpawnedPlayer.damageOutput = thePlayer.damageOutput;
        newSpawnedPlayer.thisPlayerData = thePlayer;
        newSpawnedPlayer.playerAIScript = newSpawnedPlayerAi;
        newSpawnedPlayerAi.health = thePlayer.health;
        newSpawnedPlayerAi.maxHealth = thePlayer.maxHealth;

        newSpawnedPlayer.InitPlayer();

        newSpawnedPlayer.transform.position = shopSpawnPos.position;
    }

    public void SetupPlayerTabs()
    {

        playerTabs = new List<cPlayerTabShop>();
        for (int i = 0; i < lmi.playerCharactersGlobal.Count; i++)
        {
            CreatePlayerTab(lmi.playerCharactersGlobal[i], i+1);
        }
        playerTabGrouper.sizeDelta = new Vector2(playerTabGrouper.sizeDelta.x, 187.5f*lmi.playerCharactersGlobal.Count);
    }

    void CreatePlayerTab(PlayerData thePlayer, int count)
    {
        cPlayerTabShop playerTab = Instantiate(playerTabPrefab, playerTabGrouper).GetComponent<cPlayerTabShop>();
        playerTab.playerCountText.text = count.ToString();
        playerTab.currentDamageOutputText.text = "Current: " + thePlayer.damageOutput.ToString();
        playerTab.currentMaxHealthText.text = "Current: " + thePlayer.health.ToString();
        playerTab.currentHealthText.text = "Current: " + thePlayer.maxHealth.ToString();
        playerTab.thisPlayer = thePlayer;
        playerTab.deadPlayerShowOver.SetActive(thePlayer.dead);
        playerTab.theShopScript = this;
        playerTabs.Add(playerTab);
    }

    public void SwitchToUpgrades()
    {
        if (shopSection.Equals(ShopSections.SHOP))
        {
            for (int i = 0; i < lmi.playerCharactersGlobal.Count; i++)
            {
                if (lmi.playerCharactersGlobal[i].dead)
                {
                    playerTabs[i].deadPlayerShowOver.SetActive(true);
                }
            }
            shopUIHolder.SetActive(false);
            upgradeUIHolder.SetActive(true);
            upgradesTabButton.color = selectedButtonColor;
            shopTabButton.color = normalColor;
            shopSection = ShopSections.UPGRADES;
        }
    }

    public void SwitchToShop()
    {
        if (shopSection.Equals(ShopSections.UPGRADES))
        {
            upgradeUIHolder.SetActive(false);
            shopUIHolder.SetActive(true);
            shopTabButton.color = selectedButtonColor;
            upgradesTabButton.color = normalColor;
            shopSection = ShopSections.SHOP;
        }
    }

    public bool purchased(float cost)
    {
        if (lmi.playersMoney - cost >= 0)
        {
            lmi.playersMoney -= cost;
            cHudManager.instance.moneyText.text = lmi.playersMoney.ToString();
            return true;
        }
        return false;
    }

}
