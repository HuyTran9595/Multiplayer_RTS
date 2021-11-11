using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


//move the projectile forwards every frame
//move forward because UnitFiring sets up the forward vector correctly.
public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] Rigidbody rigidbody = null;
    [SerializeField] float lauchForce = 10f;
    [SerializeField] float destroyAfterSeconds = 5f;

    private void Start()
    {
        rigidbody.velocity = transform.forward * lauchForce;
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
    #endregion
}
