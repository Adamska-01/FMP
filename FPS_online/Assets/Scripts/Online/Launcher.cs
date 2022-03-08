using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public class Launcher : MonoBehaviourPunCallbacks //Access to callbacks for room creation, errrors, joining lobbies etc.
{
    public static Launcher Instance;
    void Awake()
    {
        Instance = this;        
    }

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField nicknameInputField; private static bool hasSetNick = false;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    //Rooms
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    //Players 
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;

    [SerializeField] GameObject startGameButton;

    [SerializeField] TMP_Dropdown findDropDown;

    List<RoomInfo> currentRoomList = new List<RoomInfo>();

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

    public void CreateRoom(TMP_Dropdown _dropD)
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
            return; 

        //Set room's properties
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 8;
        ro.IsVisible = true;
        ro.CustomRoomPropertiesForLobby = new string[1] { "matchType" }; //makes sure that other clients on the lobby can see it
        ro.CustomRoomProperties = new Hashtable() { { "matchType", _dropD.options[_dropD.value].text } }; 

        PhotonNetwork.CreateRoom(roomNameInputField.text, ro);

        MenuManager.Instance.OpenMenu(MenuManager.MenuType.LOADING);
    } 

    public void SetNickname()
    {
        if(!string.IsNullOrEmpty(nicknameInputField.text))
        {
            PhotonNetwork.NickName = nicknameInputField.text;

            //Store name 
            PlayerPrefs.SetString("playerName", nicknameInputField.text);

            hasSetNick = true;

            MenuManager.Instance.OpenMenu(MenuManager.MenuType.MULTIPLAYER);
        }
    }

    public void FindRooms(TMP_Dropdown _dropD)
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
                (currentRoomList[i].CustomProperties["matchType"].ToString() != _dropD.options[_dropD.value].text &&
                !_dropD.options[_dropD.value].text.Contains("All")))
                continue;
             
            Sprite sprite = default;
            string matchType = currentRoomList[i].CustomProperties["matchType"].ToString();
            if (matchType.Contains("Deathmatch"))
                sprite = _dropD.options[0].image;
            else if(matchType.Contains("Conquest"))
                sprite = _dropD.options[1].image;

            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(currentRoomList[i], sprite);
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

        //Set Nickname
        if(!hasSetNick)
        {
            MenuManager.Instance.OpenMenu(MenuManager.MenuType.NICKNAME);

            if(PlayerPrefs.HasKey("playerName"))
            {
                nicknameInputField.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
            MenuManager.Instance.OpenMenu(MenuManager.MenuType.MULTIPLAYER);
        } 
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
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed to join the room: " + message;
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
        if (roomListContent)
        {
            foreach (Transform trans in roomListContent)
            {
                if(trans) Destroy(trans.gameObject);
            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var item in roomList)
        {
            int index = currentRoomList.FindIndex(x => x.Name == item.Name);
            if(index == -1) 
                currentRoomList.Add(item);
            else
            {
                currentRoomList.RemoveAt(index);
                currentRoomList.Add(item);
            }    
        }
        currentRoomList.RemoveAll(x => x.RemovedFromList);

        FindRooms(findDropDown);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) //When a player enters he room (NOT US)
    { 
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer); 
    }
}
