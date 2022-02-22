using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSPlayer : NetworkBehaviour
{
    //this list exist both in client and server
    [SerializeField] private List<Unit> myUnits = new List<Unit>();

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }


    #region Server
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
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
    #endregion
}
