using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText = null;

    private RTSPlayer player;

    private void Update() {
        if (player == null) {
            //identity = our player game object OF the connection
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            if(player != null) {
                ClientHandleResourcesUpdated(player.GetResources());//display first time
                player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated; //subscribe
            }
        }
    }

    private void OnDestroy() {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;//unsub
    }

    private void ClientHandleResourcesUpdated(int newResources) {
        resourcesText.text = $"Resources: {newResources}"; 
    }
}
