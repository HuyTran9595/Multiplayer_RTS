using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//This class handle the functions of the Unit Spawner
//Note that we still need to spawn a Unit SPawner when players log in
//-> RTSNetworkManager
public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Unit unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;

    [SerializeField] Health health;
    [SerializeField] private TMP_Text remainingUnitText = null;
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7;
    [SerializeField] private float unitSpawnDuration = 5f;


    [SyncVar(hook = nameof(ClientHandleQueuedUnitUpdated))]
    int queuedUnits;


    [SyncVar]
    float unitTimer;

    RTSPlayer player = null;
    float progressImageVelocity;



    private void Update() {
        if(player == null) {
            player = connectionToClient.identity.GetComponent<RTSPlayer>();
        }
        //Host: do both
        if (isServer) {
            ProduceUnits();
        }

        if (isClient) {
            UpdateTimerDisplay();
        }
    }



    //Server handles all the logics
    #region Server
    [Command]
    private void CmdSpawnUnit()
    {
        if(queuedUnits == maxUnitQueue) { return; }
        
        int resources = player.GetResources();
        if(resources < unitPrefab.GetResourceCost()) {
            return;
        }

        queuedUnits++;
        player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());


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

    [Server]
    private void ProduceUnits() {
        if (queuedUnits <= 0) {
            return;
        }

        unitTimer += Time.deltaTime;
        if (unitTimer < unitSpawnDuration) { return; }


        GameObject unitInstance = Instantiate(unitPrefab.gameObject,
    unitSpawnPoint.position,
    unitSpawnPoint.rotation);

        //spawn in the server
        NetworkServer.Spawn(unitInstance, connectionToClient);

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnPoint.position.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        queuedUnits--;
        unitTimer = 0;


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

    private void ClientHandleQueuedUnitUpdated(int oldUnitQueued, int newUnitQueued) {
        remainingUnitText.text = newUnitQueued.ToString();
    }


    [Client]
    private void UpdateTimerDisplay() {
        float newProgress = unitTimer / unitSpawnDuration;
        if (newProgress < unitProgressImage.fillAmount) {
            unitProgressImage.fillAmount = newProgress;
        } else {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount,
                                                            newProgress, ref progressImageVelocity, 0.1f);
        }
    }
    #endregion
}
