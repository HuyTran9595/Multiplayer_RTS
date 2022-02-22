using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Buildings;
using TMPro;
using Mirror;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text winnerNameText = null;
    [SerializeField] GameObject gameOverDisplayParent = null;

    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }


    void ClientHandleGameOver(string winner)
    {
        winnerNameText.text = $"{winner} Has Won!";
        gameOverDisplayParent.SetActive(true);
    }


    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            //stop hosting

            NetworkManager.singleton.StopHost();
        }
        else
        {
            //stop client
            NetworkManager.singleton.StopClient();
        }
    }
}
