using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowPlayerNickName : MonoBehaviourPunCallbacks
{
    [SerializeField] string testPlayerName;
    [SerializeField] TextMeshProUGUI nickNameText;
    OnlineGameManager onlineGameManager = new OnlineGameManager();

    // Start is called before the first frame update
    void Start()
    {
        nickNameText.text = photonView.Owner.NickName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
