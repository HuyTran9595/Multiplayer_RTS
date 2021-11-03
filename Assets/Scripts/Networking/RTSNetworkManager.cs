using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitSpawnerPrefab = null;

    //conn is the connection to the client
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        //spawn a Unit Spawner for this player
        GameObject unitSpawnerInstance = Instantiate(unitSpawnerPrefab, 
            conn.identity.transform.position, 
            conn.identity.transform.rotation);

        //spawn it on the server, client owns the base
        NetworkServer.Spawn(unitSpawnerInstance, conn);
    }
}
