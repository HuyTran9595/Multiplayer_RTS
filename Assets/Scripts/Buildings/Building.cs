using System.Collections;
using UnityEngine;
using Mirror;
using System;

namespace Assets.Scripts.Buildings
{
    public class Building : NetworkBehaviour
    {
        [SerializeField] GameObject buildingPreview = null;
        [SerializeField] Sprite icon = null;
        [SerializeField] int price = 100;
        [SerializeField] int id = -1;

        public static event Action<Building> ServerOnBuildingSpawned;
        public static event Action<Building> ServerOnBuildingDespawned;

        public static event Action<Building> AuthorityOnBuildingSpawned;
        public static event Action<Building> AuthorityOnBuildingDespawned;


        public GameObject GetBuildingPreview()
        {
            return buildingPreview;
        }

        public Sprite GetIcon()
        {
            return icon;
        }

        public int GetId()
        {
            return id;
        }

        public int GetPrice()
        {
            return price;
        }


        #region Server

        public override void OnStartServer()
        {
            base.OnStartServer();
            ServerOnBuildingSpawned?.Invoke(this);
        }


        public override void OnStopServer()
        {
            base.OnStopServer();
            ServerOnBuildingDespawned?.Invoke(this);
        }

        #endregion



        #region Client


        /// <summary>
        /// this function is called by Client and Host -> need to restrict the host
        /// </summary>
        public override void OnStartAuthority()
        {

            AuthorityOnBuildingSpawned?.Invoke(this);
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

            AuthorityOnBuildingDespawned?.Invoke(this);
        }

        #endregion
    }
}