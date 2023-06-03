using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class OnlineGameManager : MonoBehaviourPunCallbacks
{
    public const string NETWORK_PLAYER_PREFAB_NAME = "NetworkPlayerObject";
 
    private const string GAME_STARTED_RPC = nameof(GameStarted);
    private const string COUNTDOWN_STARTED_RPC = nameof(CountdownStarted);

    private int someVariable;
    public bool hasGameStarted = false;

    [SerializeField] private TextMeshProUGUI gameModeText;
    [SerializeField] private TextMeshProUGUI playersScoreText;
    [SerializeField] private TextMeshProUGUI currentSpawnPointsInfoText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Button startGameButtonUI;
    public SpawnPoint[] spawnPoints;
    
    private PlayerController localPlayerController;

    private bool isCountingForStartGame;
    private float timeLeftForStartGame = 0;
    
    public void StartGameCountdown()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int countdownRandomTime = Random.Range(3, 8);
            photonView.RPC(COUNTDOWN_STARTED_RPC,
                RpcTarget.AllViaServer, countdownRandomTime);
            startGameButtonUI.interactable = false;
        }
    }
    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        Debug.Log("Masterclient has been switched!" + Environment.NewLine
        + "Masterclient is now actor number " + newMasterClient.ActorNumber);
    }
    
    #region RPCS

    [PunRPC]
    void CountdownStarted(int countdownTime)
    {
        isCountingForStartGame = true;
        timeLeftForStartGame = countdownTime;
        countdownText.gameObject.SetActive(true);
    }
    
    [PunRPC]
    void GameStarted()
    {
        hasGameStarted = true;
        localPlayerController.canControl = true;
        isCountingForStartGame = false;
        Debug.Log("Game Started!!! WHOW");
    }

    #endregion

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {

            if (PhotonNetwork.IsMasterClient)
            {
                startGameButtonUI.interactable = true;
            }

            gameModeText.text = PhotonNetwork.CurrentRoom.CustomProperties[Constants.GAME_MODE].ToString();
            foreach (KeyValuePair<int, Player>
                         player in PhotonNetwork.CurrentRoom.Players)
            {
                if (player.Value.CustomProperties
                    .ContainsKey(Constants.PLAYER_STRENGTH_SCORE_PROPERTY_KEY))
                {
                    playersScoreText.text +=
                        player.Value.CustomProperties[Constants.PLAYER_STRENGTH_SCORE_PROPERTY_KEY]
                            += Environment.NewLine;
                }
            }
        }
    }

    private void Update()
    {
        if (isCountingForStartGame)
        {
            timeLeftForStartGame -= Time.deltaTime;
            countdownText.text = Mathf.Ceil(timeLeftForStartGame).ToString();
            if (timeLeftForStartGame <= 0)
            {
                isCountingForStartGame = false;
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC(GAME_STARTED_RPC, RpcTarget.AllViaServer);
                }
            }
        }

        string spawnPointsText = string.Empty;

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            spawnPointsText += spawnPoint.ID + " " + spawnPoint.taken + Environment.NewLine;
        }

        currentSpawnPointsInfoText.text = spawnPointsText;
    }

    private void OnValidate()
    {
        int currentID = 0;
        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            spawnPoint.ID = currentID++;
        }
    }

   


}
