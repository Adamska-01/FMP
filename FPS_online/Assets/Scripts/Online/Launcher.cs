using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Launcher : MonoBehaviourPunCallbacks //Access to callbacks for room creation, errrors, joining lobbies etc.
{
    public static Launcher Instance;
    void Awake()
    {
        Instance = this;        
    }

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    //Rooms
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    //Players 
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;

    [SerializeField] GameObject startGameButton;
    List<RoomInfo> currentRoomList;

    void Start()
    {
        MenuManager.Instance.OpenMenu(MenuManager.MenuType.TITLE);
    }


    public void ConnectToServer()
    {
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings(); //Connect to the fixed region
        MenuManager.Instance.OpenMenu(MenuManager.MenuType.LOADING);
    }

    public void CreateRoom(TMP_Text _text)
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
            return; 

        //Set room's properties
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 8;
        ro.CustomRoomPropertiesForLobby = new string[1] { "matchType" }; 
        ro.CustomRoomProperties = new Hashtable() { { "matchType", _text.text } }; 

        PhotonNetwork.CreateRoom(roomNameInputField.text, ro);

        MenuManager.Instance.OpenMenu(MenuManager.MenuType.LOADING);
    }

    public void FindRooms(TMP_Text _matchType)
    { 
        //Clear the list every time you get an update
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        //Instantiate room button and set it up
        for (int i = 0; i < currentRoomList.Count; i++)
        {
            //Photon doesn't remove rooms that have been removed from the list
            //instead it set a bool that flags it as "removed", thus skip the iteration
            if (currentRoomList[i].RemovedFromList ||
                currentRoomList[i].PlayerCount >= currentRoomList[i].MaxPlayers ||
                (currentRoomList[i].CustomProperties["matchType"].ToString() != _matchType.text &&
                !_matchType.text.Contains("All")))
                continue;

            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(currentRoomList[i]);
        }
    }

    public void LeaveRoom()
    { 
        PhotonNetwork.LeaveRoom();

        MenuManager.Instance.OpenMenu(MenuManager.MenuType.LOADING);
    }

    public void JoinRoom(RoomInfo _info)
    {
        PhotonNetwork.JoinRoom(_info.Name);
        MenuManager.Instance.OpenMenu(MenuManager.MenuType.LOADING); 
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(2);
    }

    public void StartSinglePlayerGame()
    {
        RoomManager.Instance.gameObject.SetActive(false);
        PhotonNetwork.LoadLevel(1);
    }

    public void DisconnetFromServer()
    {
        if (PhotonNetwork.IsConnected)
        { 
            PhotonNetwork.Disconnect();
            MenuManager.Instance.OpenMenu(MenuManager.MenuType.LOADING);
        }
        else
        { 
            ConnectToServer();
        }
    }
    

    public void QuitGame()
    { 
        Application.Quit(); 
    }


    //----------------------Photon Callbacks----------------------
    public override void OnConnectedToMaster() //called when connecting to master server
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby(); //Need to be in a lobby to find/create rooms

        //Automatically load the scene for all clients (when host switches scene)
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        MenuManager.Instance.OpenMenu(MenuManager.MenuType.TITLE);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        MenuManager.Instance.OpenMenu(MenuManager.MenuType.MULTIPLAYER);
        PhotonNetwork.NickName = "Player" + Random.Range(0, 1000).ToString("0000");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu(MenuManager.MenuType.ROOM);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        //Clear the list before instantiating a new one
        foreach (Transform trans in playerListContent)
        {
            Destroy(trans.gameObject);
        }
        //Create list of players 
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu(MenuManager.MenuType.ERROR); 
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //In case the host leaves, if we are the new host set the start game button to true, else false
        startGameButton.SetActive(PhotonNetwork.IsMasterClient); 
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu(MenuManager.MenuType.MULTIPLAYER);

        //Clear the list of players when leaving the room
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        currentRoomList = new List<RoomInfo>(roomList); 
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) //When a player enters he room (NOT US)
    { 
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer); 
    }
}
