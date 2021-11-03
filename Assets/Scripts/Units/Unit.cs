using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private Targeter targeter = null;

    #region Server_Var
    //these works the same as delegate
    //these are the same as delegate in Moimachi Quest system
    //only being called on the server
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;

    #endregion

    #region Client_Var
    //Authority = Client which has authority over this Unit
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    #endregion
    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }

    public Targeter GetTargeter()
    {
        return targeter;
    }


    #region Server

    //when a unit is created, it will check and invoke ServerOnUnitSpawned
    // -> will add this unit to the right client unit list in RTSPlayer.cs
    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
    }


    public override void OnStopServer()
    {
        ServerOnUnitDespawned?.Invoke(this);
    }
    #endregion

    #region Client
    [Client]//make sure that the server cannot run this method
    public void Select()
    {
        if (!hasAuthority) //if not our unit, do nothing
        {
            return;
        }
        onSelected?.Invoke(); //if it's null, don't invoke it
        //invoke = enable the green cicle below it
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority)
        {
            return;
        }
        onDeselected?.Invoke();
    }



    /// <summary>
    /// this function is called by Client and Host -> need to restrict the host
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        //in client side, we need to check for authority, otherwise all clients can own this unit
        if (!hasAuthority)
        {
            return;
        }
        //if not client, return
        if (!isClientOnly)
        {
            return;
        }

        AuthorityOnUnitSpawned?.Invoke(this);
    }

    /// <summary>
    /// this function is called by Client and Host -> need to restrict the host
    /// </summary>
    public override void OnStopClient()
    {
        base.OnStopClient();
        //in client side, we need to check for authority, otherwise all clients can own this unit
        if (!hasAuthority)
        {
            return;
        }
        //if not client, return
        if (!isClientOnly)
        {
            return;
        }
        AuthorityOnUnitDespawned?.Invoke(this);
    }
    #endregion
}
