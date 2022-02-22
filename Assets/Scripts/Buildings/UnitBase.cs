using System.Collections;
using UnityEngine;
using Mirror;
using System;
namespace Assets.Scripts.Buildings
{
    public class UnitBase : NetworkBehaviour
    {
        [SerializeField] Health health = null;


        public static event Action<UnitBase> ServerOnBaseSpawned;
        public static event Action<UnitBase> ServerOnBaseDespawned;

        //int = id of player that die
        public static event Action<int> ServerOnPlayerDie;

        #region Server

        public override void OnStartServer()
        {
            health.ServerOnDie += ServerHandleDie;
            ServerOnBaseSpawned?.Invoke(this);
        }


        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleDie;
            ServerOnBaseDespawned?.Invoke(this);
        }


        [Server]
        void ServerHandleDie()
        {

            ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);
            NetworkServer.Destroy(gameObject);
        }

        #endregion


        #region Client


        #endregion
    }



}