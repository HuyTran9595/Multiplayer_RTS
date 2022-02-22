using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Buildings;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitSpawnerPrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

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


    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map")){
            //spawn this on the server
            GameOverHandler instanceGameOverHandler = Instantiate(gameOverHandlerPrefab);

            //spawn this on every client
            NetworkServer.Spawn(instanceGameOverHandler.gameObject);
        }
    }
}
