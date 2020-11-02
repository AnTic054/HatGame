using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Status")]
    public bool gameEnded = false;
    public float timeToWin;
    public float invinDuration;
    private float hatPickupTime;

    [Header("Players")]
    public string playerPrefabLocation;
    public Transform[] spawnPoints;
    public PlayerController[] players;
    public int playerWithHat;
    private int playersInTheGame;


    //instance
    public static GameManager instance;

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
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void ImInGame()
    {
        playersInTheGame++;
        if (playersInTheGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position,Quaternion.identity);

        PlayerController playerScript = playerObj.GetComponent<PlayerController>();

        playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    // return the player with the requested id
    public PlayerController GetPlayer (int playerID)
    {
        return players.First(x => x.ID == playerID);
    }

    //returns the player with the requested game obj
    public PlayerController GetPlayer(GameObject playerOBJ)
    {
        return players.First(x => x.gameObject == playerOBJ);
    }

    //is called when player hits player with hat - giving them the hat
    [PunRPC]
    public void GiveHat(int playerID, bool initialGive)
    {
        //remove the hat from currently hatted player
        if (!initialGive)
        {
            GetPlayer(playerWithHat).SetHat(false);
        }

        // give the hat to new player
        playerWithHat = playerID;
        GetPlayer(playerID).SetHat(true);
        hatPickupTime = Time.time;
    }

    public bool CanGetHat()
    {
        if (Time.time > hatPickupTime + invinDuration)
        {
            return true;
        }
        else
            return false;
    }

    [PunRPC]
    void WinGame(int playerID)
    {
        gameEnded = true;
        PlayerController player = GetPlayer(playerID);
        //set ui to show whos won
        GameUI.instance.SetWinText(player.photonPlayer.NickName);

        Invoke("GoBackToMenu", 5.0f);
    }
    void GoBackToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.instance.ChangeScene("Menu");
    }
}
