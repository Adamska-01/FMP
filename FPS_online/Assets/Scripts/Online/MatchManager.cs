using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private NETInputManager inputManger;
    public GameObject deathCamera;
    public IEnumerator CO_kill;

    public static MatchManager instance;
    private void Awake()
    {
        instance = this;
    }

    //Events
    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayer,
        UpdateStat,
        NextMatch,
        TimerSync
    }


    public List<PlayerInfo> players = new List<PlayerInfo>();
    private List<Leaderboard> lboardPlayers = new List<Leaderboard>();
    public int index { get; private set; } 

    //Game ending 
    public enum GameStates
    {
        Waiting,
        Playing,
        Ending
    }

    public int killsToWin = 3; 
    public GameStates state = GameStates.Waiting;
    public float waitAfterEnding = 7.0f;

    //Next match
    public bool perpetual;

    //Timer
    public float matchLength = 180f;
    private float currentMatchTime;
    private float sendTimer;

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
            SceneManager.LoadScene(0);
        else
        { 
            NewPlayerSend(PhotonNetwork.NickName); 
            state = GameStates.Playing;

            SetupTimer();
        }

        inputManger = FindObjectOfType<NETInputManager>();
    }


    void Update()
    {
        if (inputManger.ShowLeaderboard && !NETUIController.instance.leaderboard.gameObject.activeInHierarchy && state != GameStates.Ending)
            ShowLeaderboard();
        else if (!inputManger.ShowLeaderboard && NETUIController.instance.leaderboard.gameObject.activeInHierarchy && state != GameStates.Ending)
        {
            NETUIController.instance.leaderboard.SetActive(false); 
            state = GameStates.Playing;
        }

        //Timer
        if (PhotonNetwork.IsMasterClient)
        { 
            if (currentMatchTime >= 0.0f && state == GameStates.Playing)
            {
                currentMatchTime -= Time.deltaTime; 
                if (currentMatchTime <= 0.0f)
                {
                    currentMatchTime = 0.0f;

                    state = GameStates.Ending;

                    ListPlayersSend(); //Keeps state up to date and checks state 
                }

                UpdateTimerDisplay();

                //Send current time every second 
                sendTimer -= Time.deltaTime;
                if (sendTimer <= 0.0f)
                {
                    sendTimer += 1.0f;

                    TimerSend();
                }
            }
        }
    }


    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        //Avoid Errors
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        /*
         *Range of .Code is 0-256. anything above 200 is are
         *Reserved by the Photon system for handling things
        */
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;


            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayer:
                    ListPlayerReceive(data);
                    break;
                case EventCodes.UpdateStat:
                    UpdateStatsReceive(data);
                    break;
                case EventCodes.NextMatch:
                    NextMatchReceive();
                    break;
                case EventCodes.TimerSync:
                    TimerReceive(data);
                    break;
            }
        }
    }


    //-------------------------Events <Send/Receive>-------------------------
    public void NewPlayerSend(string username)
    {
        object[] package = new object[4]; //4: name, actor, kills, deaths;
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        //Send package
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, //Send only to master client
            new SendOptions { Reliability = true } //TCP is always reliable by design
            );
    }

    public void NewPlayerReceive(object[] dataReceived)
    {
        PlayerInfo player = new PlayerInfo((string)dataReceived[0], (int)dataReceived[1], (int)dataReceived[2], (int)dataReceived[3]);
        
        //Sometimes a bug occours and I have two instances of the same player in the leaderboard
        int index = players.FindIndex(x => x.actor == player.actor);
        if (index == -1) 
            players.Add(player);

        //Update list 
        ListPlayersSend();
    }

    public void ListPlayersSend()
    {
        //Construct the list
        object[] package = new object[players.Count + 1]; //+1 for sending game state
        package[0] = state;

        for (int i = 0; i < players.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = players[i].name;
            piece[1] = players[i].actor;
            piece[2] = players[i].kills;
            piece[3] = players[i].deaths;

            package[i + 1] = piece; //+1 cause state is at index 0
        }
        
        //Send package (list)
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, //Send to all clients
            new SendOptions { Reliability = true } //TCP is always reliable by design
            );
    }

    public void ListPlayerReceive(object[] dataReceived)
    {
        players.Clear(); //all players cleared

        state = (GameStates)dataReceived[0]; //Update state

        //recreating the list
        for (int i = 1; i < dataReceived.Length; i++) //i=1 cause state was at 0
        {
            object[] pieceOfPlayer = (object[])dataReceived[i]; //player piece is being stored
            PlayerInfo player = new PlayerInfo(
                (string)pieceOfPlayer[0],
                (int)pieceOfPlayer[1],
                (int)pieceOfPlayer[2],
                (int)pieceOfPlayer[3]);

            players.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)   //assigning our index
                index = i - 1; //-1 cause of the state...
        }

        StateCheck();
    }

    public void UpdateStatsSend(int actorSending, int statToUpdate, int amountToChange, string killerOrKilled)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange, killerOrKilled };

        //Send package (list)
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, //Send to all clients
            new SendOptions { Reliability = true } //TCP is always reliable by design
            );
    }

    public void UpdateStatsReceive(object[] dataReceived)
    {
        int actor = (int)dataReceived[0];
        int statType = (int)dataReceived[1];
        int amount = (int)dataReceived[2];

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].actor == actor)
            {
                switch (statType)
                {
                    case 0: //kills
                        players[i].kills += amount;

                        if (PhotonNetwork.LocalPlayer.ActorNumber == players[i].actor)
                        {
                            NETUIController.instance.currentKillText.text = "You Have <color=red>Killed</color> " + "<color=blue>" + (string)dataReceived[3] + "</color>";
                            if (CO_kill != null)
                                StopCoroutine(CO_kill); 
                            CO_kill = KillTextActivation();
                            StartCoroutine(CO_kill); 
                        } 
                        break;
                    case 1: //deaths
                        players[i].deaths += amount;

                        TMP_Text killFeed = Instantiate(NETUIController.instance.killsFeedPrefab, NETUIController.instance.KillsFeed.transform).GetComponent<TMP_Text>();
                        killFeed.text = (string)dataReceived[3] + " <color=red>Killed</color> " + players[i].name; 
                        break;
                }
                //if that player is us, update stats
                if (i == index)
                {
                    UpdateStatsDisplay();
                }

                //Update leaderboard while active
                if (NETUIController.instance.leaderboard.activeInHierarchy)
                    ShowLeaderboard();

                break;
            }
        }

        ScoreCheck();
    }

    private void UpdateStatsDisplay()
    {
        if (players.Count > index)
        {
            NETUIController.instance.killsIndicator.text = $"Kills: <b><color=green>{players[index].kills}</color></b>";
            NETUIController.instance.deathsIndicator.text = $"Deaths: <b><color=red>{players[index].deaths}</color></b>";
        }
        else
        {
            NETUIController.instance.killsIndicator.text = "Kills: <b><color=green>0</color></b>";
            NETUIController.instance.deathsIndicator.text = "Deaths: <b><color=red>0</color></b>";
        }
    }

    public void NextMatchSend()
    {
        //Send package
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NextMatch,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, //Send to all
            new SendOptions { Reliability = true } //TCP is always reliable by design
            );
    }

    public void NextMatchReceive()
    {
        state = GameStates.Playing;

        NETUIController.instance.endScreen.SetActive(false);
        NETUIController.instance.leaderboard.SetActive(false);
        deathCamera.SetActive(false);

        foreach (PlayerInfo player in players)
        {
            player.kills = 0;
            player.deaths = 0;
        }
        
        UpdateStatsDisplay();

        var playerMngr = FindObjectsOfType<PlayerManager>();
        foreach (var item in playerMngr)
        {
            if (item.PV.IsMine)
                item.CreateController();
        }
        //Reset timer
        SetupTimer();
    }

    public void TimerSend()
    {
        object[] package = new object[] { (int)currentMatchTime, state };

        //Send package
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.TimerSync,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, //Send to all
            new SendOptions { Reliability = true } //TCP is always reliable by design
            );
    }

    public void TimerReceive(object[] dataReceived)
    {
        currentMatchTime = (int)dataReceived[0];
        state = (GameStates)dataReceived[1];

        UpdateTimerDisplay(); 
    }


    //-------------------------Helper Functions-------------------------
    public void SetupTimer()
    {
        if (matchLength > 0)
        {
            currentMatchTime = matchLength;
            UpdateTimerDisplay();
        }
    }

    public void UpdateTimerDisplay()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);

        NETUIController.instance.timerText.text = timeToDisplay.Minutes.ToString("0") + ":" + timeToDisplay.Seconds.ToString("00");

        if (currentMatchTime <= 30)
            NETUIController.instance.timerText.text = $"<color=red>{NETUIController.instance.timerText.text}</color>";
    }

    private void ShowLeaderboard()
    {
        //Show leaderboard
        NETUIController.instance.leaderboard.SetActive(true);

        //Clear previous leaderboard list 
        foreach (Leaderboard lp in lboardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lboardPlayers.Clear(); 

        //Sort list by the highest kills
        List<PlayerInfo> sortedPlayerList = players.OrderByDescending(x => x.kills).ToList();

        //Create new leaderboard
        foreach (PlayerInfo player in sortedPlayerList)
        {
            Leaderboard newPlayerDisplay = Instantiate(NETUIController.instance.leaderboardPlayerDisplay, NETUIController.instance.leaderboard.transform);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths, (PhotonNetwork.LocalPlayer.NickName == player.name && PhotonNetwork.LocalPlayer.ActorNumber == player.actor));

            newPlayerDisplay.gameObject.SetActive(true);

            lboardPlayers.Add(newPlayerDisplay);
        }
    }

    private void ScoreCheck()
    {
        bool winnerFound = false;

        foreach (PlayerInfo player in players)
        {
            if (player.kills >= killsToWin && killsToWin > 0) //0 == no kill limit
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if (PhotonNetwork.IsMasterClient && state != GameStates.Ending)
            {
                state = GameStates.Ending;
                ListPlayersSend();
            }
        }
    }

    private void StateCheck()
    {
        if (state == GameStates.Ending)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        state = GameStates.Ending; //makes sure

        if (PhotonNetwork.IsMasterClient)
        {
            var players = FindObjectsOfType<PlayerController>();
            foreach (var item in players)
            {
                PhotonNetwork.Destroy(item.gameObject.GetComponent<PhotonView>());
            }
        }

        //Death camera
        deathCamera.SetActive(true);

        //Show UI
        NETUIController.instance.OpenPanel(PanelType.END);
        ShowLeaderboard();

        //Activate cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
          
        StartCoroutine(EndCO());
    } 

    private IEnumerator EndCO()
    {
        float timer = waitAfterEnding;
        while (timer > 0.0f)
        {
            --timer;
            NETUIController.instance.nextMatchtimeText.text = "Next Round In: <color=red>" + timer.ToString("0") + "</color>";
            yield return new WaitForSeconds(1);
        } 

        if (!perpetual) //Back to the main menu
        {
            var rm = FindObjectOfType<RoomManager>()?.gameObject;
            if(rm != null) Destroy(rm);
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom(); //leave room and return to menu
        }
        else //Start new match
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NextMatchSend();
                //PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex);
                //if (!Launcher.instance.changeMapBetweenRounds)
                //    NextMatchSend();
                //else
                //{
                //    int newLevel = Random.Range(0, Launcher.instance.Maps.Length);

                //    if (Launcher.instance.Maps[newLevel] == SceneManager.GetActiveScene().name)
                //        NextMatchSend();
                //    else
                //        PhotonNetwork.LoadLevel(Launcher.instance.Maps[newLevel]);
                //}
            }
        }
    }

    private IEnumerator KillTextActivation()
    {
        NETUIController.instance.currentKillText.CrossFadeAlpha(1.0f, 0.0f, true);
        NETUIController.instance.currentKillText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3.0f);

        NETUIController.instance.currentKillText.CrossFadeAlpha(0.0f, 2.0f, true);
        while (NETUIController.instance.currentKillText.color.a > 0.0f)
        { 
            yield return null;
        }

        NETUIController.instance.currentKillText.gameObject.SetActive(false);
    }


    //-----------------------------Photon Callbacks-----------------------------
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int index = players.FindIndex(x => x.name == otherPlayer.NickName);
        if (index != -1)
            players.RemoveAt(index);
        else //no need for all clients to execute the rest
            return;

        ListPlayersSend();
        //PhotonNetwork.SendAllOutgoingCommands(); //Send this immediately
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom(); 
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene(0);
        }
    }  
}

[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor;
    public int kills;
    public int deaths;

    public PlayerInfo()
    {
        name = string.Empty;
        kills = deaths = 0;
    }
    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;
    }
}