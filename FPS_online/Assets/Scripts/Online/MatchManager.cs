using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour, IOnEventCallback
{
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


    public List<PlayerInfo> Players = new List<PlayerInfo>();
    public int index { get; private set; }

    //private List<LeaderboardPlayer> lboardPlayers = new List<LeaderboardPlayer>();

    //Game ending 
    public enum GameStates
    {
        Waiting,
        Playing,
        Ending
    }

    public int killsToWin = 3;
    public Transform mapCamPoint;
    public GameStates state = GameStates.Waiting;
    public float waitAfterEnding = 5f;

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

        }
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
                //case EventCodes.ListPlayer:
                //    ListPlayerReceive(data);
                //    break;
                //case EventCodes.UpdateStat:
                //    UpdateStatsReceive(data);
                //    break;
                //case EventCodes.NextMatch:
                //    NextMatchReceive();
                //    break;
                //case EventCodes.TimerSync:
                //    TimerReceive(data);
                //    break;
            }
        }
    }

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
        Players.Add(player);

        //Update list 
        //ListPlayersSend();
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