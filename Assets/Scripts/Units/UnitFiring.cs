using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class UnitFiring : NetworkBehaviour
{

    [SerializeField] Targeter targeter = null;
    [SerializeField] GameObject projectilePrefab = null;
    [SerializeField] Transform projectileSpawnPoint = null;
    [SerializeField] float fireRange = 12f;
    [SerializeField] float fireRate = 1f; //1 fire per second 
    [SerializeField] float rotationSpeed = 20f; //rotate 20 degree per second

    float lastFiredTime;

    #region Server
    //Prevents client from running Update logic
    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();
        //don't have a target = do nothing
        if (target is null)
        {
            return;
        }
        //if we can't fire, do nothing
        if (!CanFireAtTarget())
        {
            return;
        }

        //if we can fire, Rotate towards target, then fire.
        RotateTowardsTarget();

        HandleFiring();
    }

    private void HandleFiring()
    {
        Targetable target = targeter.GetTarget();
        //if we can fire
        if (Time.time > (1 / fireRate) + lastFiredTime)
        {
            //fire:
            //get forward vector for the projectile: from projectileSpawnPoint to target Aim at point
            Quaternion projectileRotation = Quaternion.LookRotation(target.GetAimAtPoint().position -
                                                                    projectileSpawnPoint.position);
            //spawn projectile on Server
            GameObject projectileInstance = Instantiate(projectilePrefab,
                                                        projectileSpawnPoint.position,
                                                        projectileRotation);
            //put projectile on the network, make it owned by the client (who owns this unit firing scripts)
            NetworkServer.Spawn(projectileInstance, connectionToClient);

            //update time
            lastFiredTime = Time.time;

        }
    }

    private void RotateTowardsTarget()
    {
        //get the rotation vector to look at our target
        Quaternion targetRotation = Quaternion.LookRotation(targeter.GetTarget().transform.position - transform.position);

        //rotate towards target
        transform.rotation = Quaternion.RotateTowards(from: transform.rotation,
                                                      to: targetRotation,
                                                      maxDegreesDelta: rotationSpeed * Time.deltaTime);
    }



    //if in fireRange, can fire.
    [Server]
    private bool CanFireAtTarget()
    {
        Targetable target = targeter.GetTarget();
        return (Vector3.Distance(target.transform.position, transform.position) <= fireRange);
    }
    #endregion
}
