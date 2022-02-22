using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Assets.Scripts.Buildings;
public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] Building[] buildings = new Building[0];//edit in editor
    //this list exist both in client and server
    private List<Unit> myUnits = new List<Unit>();
    List<Building> myBuildings = new List<Building>();
    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    public List<Building> GetMyBuildings()
    {
        return myBuildings;
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
        //which building is it?
        foreach(Building building in buildings) {
            if(building.GetId() == buildingID) {
                buildingToSpawn = building;
            }
            break;
        }
        //invalid ID, do nothing
        if(buildingToSpawn is null) { return; }

        //valid ID, spawn building
        GameObject buildingInstance =  Instantiate(buildingToSpawn.gameObject, 
                                                    spawnPosition, 
                                                    buildingToSpawn.transform.rotation);

        //give ownership of this building to player
        //spawn on the network, show this building to all players
        NetworkServer.Spawn(buildingInstance, connectionToClient);

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
    #endregion
}
