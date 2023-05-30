using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowPlayerNickName : MonoBehaviour
{
    [SerializeField] string testPlayerName;
    [SerializeField] TextMeshProUGUI nickNameText;

    // Start is called before the first frame update
    void Start()
    {
        nickNameText.text = testPlayerName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
