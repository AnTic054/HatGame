using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameUI : MonoBehaviour
{
    public static GameUI instance;

    public PlayerUIContainer[] playerContainers;
    public TextMeshProUGUI winText;


    private void Awake()
    {
        if (instance != null && instance == this)
        {
            gameObject.SetActive(false);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        InitializePlayerUI();
    }
    private void Update()
    {
        UpdatePlayerUI();
    }

    void InitializePlayerUI()
    {
        for (int i = 0; i < playerContainers.Length; i++)
        {
            PlayerUIContainer container = playerContainers[i];

            //only enable and modify the containers we need
            if (i < PhotonNetwork.PlayerList.Length)
            {
                container.obj.SetActive(true);
                container.nameText.text = PhotonNetwork.PlayerList[i].NickName;
                container.hatTimeSlider.maxValue = GameManager.instance.timeToWin;
            }

            else
                container.obj.SetActive(false);
        }
    }

    void UpdatePlayerUI()
    {
        //loops through all players
        for (int i = 0; i < GameManager.instance.players.Length; i++)
        {
            if (GameManager.instance.players[i] != null)
            {
                playerContainers[i].hatTimeSlider.value = GameManager.instance.players[i].curHatTime;
            }
        }
    }

    public void SetWinText(string winnerName)
    {
        winText.gameObject.SetActive(true);
        winText.text = winnerName + " Wins!";
    }
}

//holds info for players ui elements
[System.Serializable]
public class PlayerUIContainer
{
    public GameObject obj;
    public TextMeshProUGUI nameText;
    public Slider hatTimeSlider;
}
