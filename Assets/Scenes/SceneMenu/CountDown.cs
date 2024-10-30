using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : NetworkBehaviour
{
    public bool counting = false;
    public float startTime;

    public int countNumber = 3;

    public Text countText;

    [Rpc(SendTo.ClientsAndHost)]
    public void StartCountingRpc()
    {
        counting = true;
        startTime = Time.time;

        gameObject.SetActive(true);
    }    

    public void OnCountFinished()
    {
        counting = false;
        gameObject.SetActive(false);

        if (IsHost)
        {
            SceneManager.Get().StartRace();
        }
    }

    void Start()
    {

    }

    void Update()
    {
        if (counting)
        {
            float t = countNumber + startTime - Time.time;

            if (t < 0)
            {
                OnCountFinished();
            }

            else
            {
                countText.text = Mathf.CeilToInt(t).ToString();
            }
        }
    }
}
