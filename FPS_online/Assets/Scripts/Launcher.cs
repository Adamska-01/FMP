using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

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


    void Start()
    {
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings(); //Connect to the fixed region
        MenuManager.Instance.OpenMenu("loading");
    }


    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
            return;

        PhotonNetwork.CreateRoom(roomNameInputField.text);

        MenuManager.Instance.OpenMenu("loading");
    }

    public void LeaveRoom()
    { 
        PhotonNetwork.LeaveRoom();

        MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo _info)
    {
        PhotonNetwork.JoinRoom(_info.Name);
        MenuManager.Instance.OpenMenu("loading"); 
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
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

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        MenuManager.Instance.OpenMenu("title");
        PhotonNetwork.NickName = "Player" + Random.Range(0, 1000).ToString("0000");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu("room");
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
        MenuManager.Instance.OpenMenu("error"); 
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //In case the host leaves, if we are the new host set the start game button to true, else false
        startGameButton.SetActive(PhotonNetwork.IsMasterClient); 
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");

        //Clear the list of players when leaving the room
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //Clear the list every time you get an update
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        //Instantiate room button and set it up
        for (int i = 0; i < roomList.Count; i++)
        {
            //Photon doesn't remove rooms that have been removed from the list
            //instead it set a bool that flags it as "removed", thus skip the iteration
            if (roomList[i].RemovedFromList) 
                continue;

            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) //When a player enters he room (NOT US)
    { 
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer); 
    }
}
