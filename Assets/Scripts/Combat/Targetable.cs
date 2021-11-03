using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//this class represents anything that can be shooted at
public class Targetable : NetworkBehaviour
{
    [SerializeField] Transform aimAtPoint = null;

    public Transform GetAimAtPoint()
    {
        return aimAtPoint;
    }
}
