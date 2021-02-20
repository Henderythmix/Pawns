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
    public Button shopTabButton;
    public Button upgradesTabButton;
    public Transform playerTabGrouper;
    public GameObject playerTabPrefab;
    [Header("Parameters for upgrades")]
    public float maxHealthObtainable = 500;
    [Header("Parameters for the shop")]
    public float maxObtainableCharacters = 6;

    List<GameObject> playerTabs;
    private void Start()
    {
        lmi = LevelManager.instance;
        SetupPlayerTabs();
    }

    public void BuyMaxHealth(float cost, int playerChar)
    {
        if (purchased(cost))
        {
            //lmi.playerCharactersAlive[playerChar].playerAIScript.health = lmi.playerCharactersAlive[playerChar].playerAIScript.maxHealth;
            lmi.playerCharactersSpawned[playerChar].playerAIScript.SetHealth(lmi.playerCharactersSpawned[playerChar].playerAIScript.maxHealth, true);
        }
    }

    public void BuyIncreasedMaxHealth(float cost, int playerchar, float amount)
    {
        if (purchased(cost))
        {
            lmi.playerCharactersSpawned[playerchar].playerAIScript.SetNewMaxHealth(amount, true, false, true);
        }
    }

    public void BuyBackPlayerCharacter(PlayerData thePlayer)
    {
        if (purchased(thePlayer.playerType.costToRevive))
        {
            SpawnCharacterShop(thePlayer);
        }
    }

    public void BuyNewPlayer(float cost, int idx)
    {
        if (purchased(cost))
        {
            Player chosenPlayer = lmi.playerCharacters[idx];
            PlayerData newPlayer = new PlayerData();

            newPlayer.damageOutput = chosenPlayer.playerCharacterType.damageOutput;
            newPlayer.health = chosenPlayer.playerCharacterType.health;
            newPlayer.maxHealth = chosenPlayer.playerCharacterType.health;
            newPlayer.playerType = chosenPlayer;

            lmi.playerCharactersGlobal.Add(newPlayer);

            SpawnCharacterShop(newPlayer);
        }
    }

    void SpawnCharacterShop(PlayerData thePlayer)
    {
        sPlayerController newSpawnedPlayer = Instantiate(lmi.playerPrefab).GetComponent<sPlayerController>();
        sAiController newSpawnedPlayerAi = newSpawnedPlayer.transform.GetChild(2).gameObject.GetComponent<sAiController>();

        newSpawnedPlayer.damageOutput = thePlayer.damageOutput;
        newSpawnedPlayerAi.health = thePlayer.health;
        newSpawnedPlayerAi.maxHealth = thePlayer.maxHealth;

        newSpawnedPlayer.transform.position = shopSpawnPos.position;
    }

    public void SetupPlayerTabs()
    {
        for (int i = 0; i < lmi.playerCharactersGlobal.Count; i++)
        {
            GameObject playerTab = Instantiate(playerTabPrefab);
            
            playerTabs.Add(playerTab);
        }
    }

    public void SwitchToUpgrades()
    {
        if (shopSection.Equals(ShopSections.SHOP))
        {
            shopUIHolder.SetActive(false);
            shopSection = ShopSections.UPGRADES;
        }
    }

    public void SwitchToShop()
    {
        if (shopSection.Equals(ShopSections.UPGRADES))
        {

            shopSection = ShopSections.SHOP;
        }
    }

    bool purchased(float cost)
    {
        if (lmi.playersMoney - cost >= 0)
        {
            lmi.playersMoney -= cost;
            return true;
        }
        return false;
    }

}
