using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharchterChoosing : MonoBehaviourPun
{
    private PlayerController localPlayerController;

    private Dictionary<Button, Button> PlayerMasterConfirmMapping = new Dictionary<Button, Button>();

    private Dictionary<Button, Button> CharchterPlayerConfirmMapping = new Dictionary<Button, Button>();

    [SerializeField] private OnlineGameManager onlineGameManager;

    [SerializeField] private SpawnPoint[] spawnPoint;

    [Header("UI")]

    [SerializeField] private Button[] CharchterChoiceButtons;
    [SerializeField] private Button[] MasteClientConfirmChioceButtons;
    [SerializeField] private Button[] PlayerConfirmChoiceButtons;
    [SerializeField] private Canvas CharchterChoiceUI;

    [Header("Const Strings")]

    private const string NETWORK_PLAYER_PREFAB_NAME = "NetworkPlayerObject";
    private const string SPAWN_PLAYER_CLIENT_RPC = nameof(SpawnPlayer);
    private const string PLACE_CHARCHTER_RPC = nameof(PlaceCharchter);
    private const string CHOOSE_CHARCHTER_RPC = nameof(ChooseCharchterRPC);
    private const string ASK_FOR_RANDOM_SPAWN_POINT_RPC = nameof(AskForRandomSpawnPoint);
    private const string BUTTON_DISAPPER_RPC = nameof(ButtonDisapper);


    private void Start()
    {
        spawnPoint = onlineGameManager.spawnPoints;

        foreach (Button button in MasteClientConfirmChioceButtons)
        {
            button.gameObject.SetActive(false);
        }

        foreach (Button button in PlayerConfirmChoiceButtons)
        {
            button.gameObject.SetActive(false);
        }

        SetButtonMapDictioneryPlayer();
        SetButtonMapDictioneryMaster();
    }

    public void SetButtonMapDictioneryPlayer()
    {
        for (int i = 0; i < CharchterChoiceButtons.Length; i++)
        {
            Button charchterChoiceButton = CharchterChoiceButtons[i];
            Button confirmButton = PlayerConfirmChoiceButtons[i];

            PlayerMasterConfirmMapping.Add(charchterChoiceButton, confirmButton);
            confirmButton.gameObject.SetActive(false);

        }
    }

    public void SetButtonMapDictioneryMaster()
    {
        for (int i = 0; i < MasteClientConfirmChioceButtons.Length; i++)
        {
            Button charchterChoiceButton = CharchterChoiceButtons[i];
            Button confirmButton = MasteClientConfirmChioceButtons[i];

            CharchterPlayerConfirmMapping.Add(charchterChoiceButton, confirmButton);
            confirmButton.gameObject.SetActive(false);
        }
    }

    public void ChooseChacrchter(int characterButtonIndex)
    {
        photonView.RPC(CHOOSE_CHARCHTER_RPC, RpcTarget.AllViaServer, characterButtonIndex);
    }
    public void ConfirmChacrchter(int characterButtonIndex)
    {
        photonView.RPC(PLACE_CHARCHTER_RPC, RpcTarget.AllViaServer, characterButtonIndex);
    }

    public void ConfirmLocation(int characterButtonIndex)
    {
        CharchterChoiceUI.gameObject.SetActive(false);

        if (!CharchterChoiceUI.gameObject.activeSelf)
        {
            photonView.RPC(ASK_FOR_RANDOM_SPAWN_POINT_RPC, RpcTarget.MasterClient);
            photonView.RPC(SPAWN_PLAYER_CLIENT_RPC, RpcTarget.AllViaServer, characterButtonIndex);
        }

    }
    private SpawnPoint GetSpawnPointByID(int targetID)
    {
        foreach (SpawnPoint spawnPoint in spawnPoint)
        {
            if (spawnPoint.ID == targetID)
                return spawnPoint;
        }

        return null;
    }


    #region RPC

    [PunRPC]
    void AskForRandomSpawnPoint(PhotonMessageInfo messageInfo)
    {
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();
        foreach (SpawnPoint spawnPoint in spawnPoint)
        {
            if (!spawnPoint.taken)
                availableSpawnPoints.Add(spawnPoint);
        }

        SpawnPoint chosenSpawnPoint =
            availableSpawnPoints[UnityEngine.Random.Range(0, availableSpawnPoints.Count)];
        chosenSpawnPoint.taken = true;

        bool[] takenSpawnPoints = new bool[spawnPoint.Length];
        for (int i = 0; i < spawnPoint.Length; i++)
        {
            takenSpawnPoints[i] = spawnPoint[i].taken;
        }
        photonView.RPC(SPAWN_PLAYER_CLIENT_RPC,
            messageInfo.Sender, chosenSpawnPoint.ID,
            takenSpawnPoints);
    }

    [PunRPC]
    void SpawnPlayer(int spawnPointID, bool[] takenSpawnPoints)
    {
        SpawnPoint spawnPoint = GetSpawnPointByID(spawnPointID);
        localPlayerController =
            PhotonNetwork.Instantiate(NETWORK_PLAYER_PREFAB_NAME,
                    spawnPoint.transform.position,
                    spawnPoint.transform.rotation)
                .GetComponent<PlayerController>();

        for (int i = 0; i < takenSpawnPoints.Length; i++)
        {
            this.spawnPoint[i].taken = takenSpawnPoints[i];
        }

    }


    [PunRPC]
    public void ChooseCharchterRPC(int characterButtonIndex)
    {
        List<int> confirmButtonIndices = new List<int>();

        foreach (KeyValuePair<Button, Button> pair in CharchterPlayerConfirmMapping)
        {
            Button characterButton = pair.Key;
            Button confirmButton = pair.Value;


            confirmButton.gameObject.SetActive(true);
            confirmButtonIndices.Add(Array.IndexOf(CharchterChoiceButtons, characterButton));

            photonView.RPC(BUTTON_DISAPPER_RPC, RpcTarget.AllViaServer, confirmButtonIndices.ToArray());
        }


        foreach (Button confirmButton in PlayerConfirmChoiceButtons)
        {
            confirmButton.gameObject.SetActive(true);
        }

    }

    [PunRPC]
    public void PlaceCharchter(int characterButtonIndex)
    {
        foreach (KeyValuePair<Button, Button> pair in PlayerMasterConfirmMapping)
        {
            Button characterButton = pair.Key;
            Button MasterConfirmButtons = pair.Value;


            MasterConfirmButtons.gameObject.SetActive(true);

            photonView.RPC(BUTTON_DISAPPER_RPC, RpcTarget.AllViaServer, characterButtonIndex);
        }

        foreach (Button confirmButton in PlayerConfirmChoiceButtons)
        {
            confirmButton.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    public void ButtonDisapper(int[] characterButtonIndices)
    {
        foreach (int index in characterButtonIndices)
        {
            Button pressedButton = MasteClientConfirmChioceButtons[index];
            pressedButton.gameObject.SetActive(false);
        }
    }

    #endregion

}


