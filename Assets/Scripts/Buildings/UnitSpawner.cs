using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//This class handle the functions of the Unit Spawner
//Note that we still need to spawn a Unit SPawner when players log in
//-> RTSNetworkManager
public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;

    [SerializeField] Health health;

    //Server handles all the logics
    #region Server
    [Command]
    private void CmdSpawnUnit()
    {
        GameObject unitInstance = Instantiate(unitPrefab, 
            unitSpawnPoint.position, 
            unitSpawnPoint.rotation);

        //spawn in the server
        NetworkServer.Spawn(unitInstance, connectionToClient);
    }



    public override void OnStartServer()
    {
        base.OnStartServer();
        health.ServerOnDie += ServerHandleDie;
    }


    public override void OnStopServer()
    {
        base.OnStopServer();
        health.ServerOnDie -= ServerHandleDie;
    }


    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }


    #endregion


    //Clients calls the Server functions
    #region Client

    //whenever i click on this game obj
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        //if this obj doesn't belong to this player, return
        //otherwise, a player can click on the enemy's spawner and spawn his own troops
        if (!hasAuthority)
        {
            return;
        }
        CmdSpawnUnit();
    }

    #endregion
}
