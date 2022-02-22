using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


//move the projectile forwards every frame
//move forward because UnitFiring sets up the forward vector correctly.
public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] Rigidbody rb = null;
    [SerializeField] float lauchForce = 10f;
    [SerializeField] float destroyAfterSeconds = 5f;

    [SerializeField] int damage = 20;


    private void Start()
    {
        rb.velocity = transform.forward * lauchForce;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }





    #region Server

    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }


    //deal damage if collide with smt has health
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity nwIdentity))
        {
            //collide with ourself? do nothing
            if(nwIdentity.connectionToClient == connectionToClient) { return; }
        }

        if(other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damage);
            DestroySelf();
        }
    }
    #endregion
}
