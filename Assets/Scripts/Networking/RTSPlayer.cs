using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Assets.Scripts.Buildings;
using System;
public class RTSPlayer : NetworkBehaviour
{
    [SyncVar (hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;

    public event Action<int> ClientOnResourcesUpdated;

    //[SerializeField] Building[] buildings = new Building[2];//edit in editor
    public List<Building> buildings = new List<Building>();
    //this list exist both in client and server
    private List<Unit> myUnits = new List<Unit>();
    List<Building> myBuildings = new List<Building>();

    [SerializeField] LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] float rangeLimit = 5f;
    private Color teamColor = new Color();


    public Color GetTeamColor() {
        return teamColor;
    }

    [Server]
    public void SetTeamColor(Color color) {
        teamColor = color;
    }

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }

    public int GetResources() {
        return resources;
    }

    [Server]
    public void SetResources(int resources) {
        this.resources = resources;
    }


    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 spawnPosition) {
        if (Physics.CheckBox(spawnPosition + buildingCollider.center,
                            buildingCollider.size / 2, Quaternion.identity, buildingBlockLayer)) {
            return false; //overlapping something, we do not spawn
        }


        //close enough to 1 of our buildings?
        for (int i = 0; i < myBuildings.Count; i++) {
            Building building = myBuildings[i];
            if (Vector3.Distance(spawnPosition, building.transform.position) <= rangeLimit) {
                return true;
            }

        }
        return false;
    }



    #region Server
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingID, Vector3 spawnPosition) {

        Building buildingToSpawn = null;
        int length = buildings.Count;
        //which building is it?

        for(int i = 0; i < length; i++) {
            Building building = buildings[i];
            Debug.Log(building.GetId() + "  " + buildingID);
            if (building.GetId() == buildingID) {
                buildingToSpawn = building;
            }
        }

        //foreach(Building building in buildings) {
        //    Debug.Log(building.GetId() + "  " + buildingID);
        //    if(building.GetId() == buildingID) {
        //        buildingToSpawn = building;
        //    }
        //    break;
        //}
        //invalid ID, do nothing
        if(buildingToSpawn is null) {
            Debug.LogWarning("null building, id = " + buildingID);
            return; }

        //valid ID!


        //valid resource?
        if(resources < buildingToSpawn.GetPrice()) { return; }

        //valid position?
        BoxCollider buildingCollider = buildingToSpawn.GetComponent<BoxCollider>();
        if(!CanPlaceBuilding(buildingCollider, spawnPosition)) {
            return;
        }



        //pass every check point! spawn building!

        GameObject buildingInstance =  Instantiate(buildingToSpawn.gameObject, 
                                                    spawnPosition, 
                                                    buildingToSpawn.transform.rotation);

        //give ownership of this building to player
        //spawn on the network, show this building to all players
        NetworkServer.Spawn(buildingInstance, connectionToClient);


        SetResources(resources - buildingToSpawn.GetPrice());
    }




    /// <summary>
    /// When a unit is respawned, it will trigger this function
    /// this function will add the unit to the myUnit list of the right client
    /// </summary>
    private void ServerHandleUnitSpawned(Unit unit)
    {
        //server check. if the unit doesn't belong to the connecting client, do nothing
        //we need server check because it's static delegate -> ALL client will call the same function
        if(unit.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        myUnits.Add(unit);
    }

    /// <summary>
    /// When unit dies, the server will remove the unit from client's unit list
    /// </summary>
    private void ServerHandleUnitDespawned(Unit unit)
    {
        //server check. if the unit doesn't belong to the connecting client, do nothing
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        myUnits.Remove(unit);
    }


    private void ServerHandleBuildingSpawned(Building building)
    {
        //server check. if the unit doesn't belong to the connecting client, do nothing
        //we need server check because it's static delegate -> ALL client will call the same function
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        myBuildings.Add(building);
    }


    private void ServerHandleBuildingDespawned(Building building)
    {
        //server check. if the unit doesn't belong to the connecting client, do nothing
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        myBuildings.Remove(building);
    }







    #endregion

    #region Client

    /// <summary>
    /// this function is called by Client and Host
    /// </summary>
    public override void OnStartAuthority()
    {
        if(NetworkServer.active)
        {
            return;
        }
        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    /// <summary>
    /// this function is called by Client and Host
    /// </summary>
    public override void OnStopClient()
    {
        base.OnStopClient();
        if (!isClientOnly || !hasAuthority)
        {
            return;
        }

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    /// <summary>
    /// When 1 unit created, ALL client will try to add it to their list
    /// so we need to check if the client actually owns the unit
    /// </summary>
    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        
        //we still need to add unit in myUnit on the Client side
        //basically the client has 1 myUnit, the server has 1 too. 
        myUnits.Add(unit);
        
    }
    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }


    ////Building

    private void AuthorityHandleBuildingSpawned(Building building)
    {

        //we still need to add unit in myUnit on the Client side
        //basically the client has 1 myUnit, the server has 1 too. 
        myBuildings.Add(building);

    }
    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    private void ClientHandleResourcesUpdated(int oldResource, int newResource) {
        ClientOnResourcesUpdated?.Invoke(newResource);
    }
    #endregion
}
