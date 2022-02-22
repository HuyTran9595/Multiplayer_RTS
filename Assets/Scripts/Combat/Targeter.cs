using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Assets.Scripts.Buildings;

//this class represent anything that can shoot at other things
//stick to the unit so it can target things
public class Targeter : NetworkBehaviour
{
    private Targetable target;



    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }


    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }


    public Targetable GetTarget()
    {
        return target;
    }

    #region Server

    //clients call this function to set a unit's target on server
    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        //if it doesn't have Targetable component, return
        if(!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget))
        {
            return;
        }
        target = newTarget;
    }

    //when the server try to move a unit, it clears the unit's target   
    [Server]
    public void ClearTarget()
    {

        target = null;

    }


    [Server]
    void ServerHandleGameOver()
    {
        ClearTarget();
    }

    #endregion
}
