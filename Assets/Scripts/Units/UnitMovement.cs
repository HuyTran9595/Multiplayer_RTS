using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Assets.Scripts.Buildings;
public class UnitMovement : NetworkBehaviour
{

    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;//chase when distance greater than this
    #region Server


    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }


    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }


    [Server]
    void ServerHandleGameOver()
    {
        agent.ResetPath();
    }


    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();
        //if we have a target, we just chase him
        if(target != null)
        {
            if (Vector3.Distance(target.transform.position, transform.position) > chaseRange)
            {
                //chase him
                agent.SetDestination(target.transform.position);
            }
            else if(agent.hasPath)
            {
                //stop
                agent.ResetPath();
            }
            return;
        }

        //if we don't have a path -> we are not moving -> we don't do anything -> keep don't do anything
        if (!agent.hasPath) { return; }

        //if we have a path, and moving towards destination, keep doing that
        if(agent.remainingDistance > agent.stoppingDistance)
        {
            return;
        }
        else //we are close enough to destination, we stop
        {
            agent.ResetPath();
        }
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }


    [Server] 
    public void ServerMove(Vector3 position) {
        targeter.ClearTarget();
        //if not valid position, do nothing
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) {
            return;
        }

        agent.SetDestination(hit.position);
    }
    #endregion

    #region Client


    #endregion
}
