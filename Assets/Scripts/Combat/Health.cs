using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Health : NetworkBehaviour
{
    [SerializeField] int maxHealth = 100;

    //only server can change this
    [SyncVar (hook = nameof(HandleHealthUpdated))]
    int currenthealth;


    ////// EVENTS
    public event Action ServerOnDie;
    //current health, max health
    public event Action<int , int> ClientOnHealthUpdate;



    #region Server

    public override void OnStartServer()
    {
        base.OnStartServer();
        currenthealth = maxHealth;
    }


    [Server]
    public void DealDamage(int damageAmount)
    {
        if(currenthealth <= 0)
        {
            return;
        }

        currenthealth = Mathf.Max(currenthealth - damageAmount, 0);

        if (currenthealth != 0) { return; }

        //dead, raise event to die
        ServerOnDie?.Invoke();
        Debug.Log("We died");
    }

    #endregion






    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdate?.Invoke(newHealth, maxHealth);
    }

    #endregion

}
