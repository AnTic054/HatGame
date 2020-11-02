using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject lobbyScreen;
    public GameObject allLobbiesScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button joinRoomButton;

    [Header("Lobby Screen")]
    public TextMeshProUGUI playerListText;
    public Button startGameButton;

    [Header("All Lobbies Screen")]
    public TextMeshProUGUI lobbiesListText;
    public Button joinButton;

    private void Start()
    {
        //disabling buttons at the start since we arent connected yet
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;

    }

    public override void OnConnectedToMaster()
    {
        //re-enabling the buttons once we connect to master server
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }
    void SetScreen(GameObject screen)
    {
        //deativate all screens
        mainScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        allLobbiesScreen.SetActive(false);
        //activate the requested screen;
        screen.SetActive(true);

    }

    //called when we create a room
    public void OnCreateRoomButton(TMP_InputField roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
    }

    //called when we join a room
    public void OnJoinRoomButton(TMP_InputField roomNameInput)
    {
        NetworkManager.instance.JoinRoom(roomNameInput.text);
    }

    //called when player name input field is updated
    public void OnPlayerNameUpdate(TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    //called when we join a room
    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen);

        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // we don't RPC it like when we join the lobby
        // that's because OnJoinRoom is only called for the client who just joined
        // OnPlayerLeftRoom gets called for all clients in the room, so we don't need to RPC
        UpdateLobbyUI();
    }

    [PunRPC]
    public void UpdateLobbyUI()
    {
        playerListText.text = "";

        //display all players currently in the lobby
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            playerListText.text += player.NickName + "\n";
        }

        // only the host can start the game
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.interactable = true;
        }
        else
        {
            startGameButton.interactable = false;
        }
    }

    public void OnLeaveLobbyButton()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }

    //called when start button is clicked 
    //only host can click this button
    public void OnStartGameButton()
    {
        //tell all players in the room to change scene
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }

    public void OnQuitBUtton()
    {
        Application.Quit();
    }

    public void OnBackButton()
    {
        SetScreen(mainScreen);
    }
}
