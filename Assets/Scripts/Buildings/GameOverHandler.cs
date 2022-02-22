using System.Collections;
using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;


namespace Assets.Scripts.Buildings
{
    public class GameOverHandler : NetworkBehaviour
    {
        List<UnitBase> bases = new List<UnitBase>();

        public static event Action<string> ClientOnGameOver;
        public static event Action ServerOnGameOver;


        #region Server
        public override void OnStartServer()
        {
            UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
            UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
        }


        public override void OnStopServer()
        {
            UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
            UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
        }



        [Server]
        void ServerHandleBaseSpawned(UnitBase unitBase)
        {
            bases.Add(unitBase);
        }

        [Server]
        void ServerHandleBaseDespawned(UnitBase unitBase)
        {
            bases.Remove(unitBase);
            if(bases.Count != 1)
            {
                return;
            }
            else
            {
                //game is over, only 1 player left.
                Debug.Log("Game Over");
                int winnerID = bases[0].connectionToClient.connectionId;

                RpcGameOver($"Player {winnerID}");
                ServerOnGameOver?.Invoke();
            }
        }

        #endregion



        #region Client

        [ClientRpc]
        void RpcGameOver(string winner)
        {
            ClientOnGameOver?.Invoke(winner);
        }

        #endregion
    }
}