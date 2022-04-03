using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamColorSetter : NetworkBehaviour
{
    [SerializeField] List<Renderer> colorRenderers = new List<Renderer>();

    [SyncVar(hook = nameof(ClientHandleTeamColorUpdated))] 
    Color teamColor = new Color();

    #region SERVER
    public override void OnStartServer() {
        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
        teamColor = player.GetTeamColor();
    }


    #endregion


    #region CLIENT

    private void ClientHandleTeamColorUpdated(Color oldColor, Color newColor) {
        for(int i = 0; i < colorRenderers.Count; i++) {
            Renderer renderer = colorRenderers[i];
            renderer.material.SetColor("_BaseColor", newColor);
        }
    }
    #endregion
}
