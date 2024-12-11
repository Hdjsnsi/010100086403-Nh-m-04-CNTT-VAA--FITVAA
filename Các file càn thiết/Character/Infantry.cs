using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class Infantry : Character
{
    [SerializeField] private Animator anim;
    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    protected override void Movement()
    {
        if (rb.linearVelocity.sqrMagnitude >= 1)
        {
            anim.SetInteger("Status_walk", 1);
        }
        else
        {
            anim.SetInteger("Status_walk", 0);

        }

        StillOnGround();
        if (nearestEnemy == Vector3.zero) return;

        Vector3 lookAt = new Vector3(targetDistance.x, 0, targetDistance.z);
        float directed = Vector3.Angle(transform.forward, lookAt);
        Rotate();

        if (directed > 10) return;
        // Gán trạng thái animation dựa trên điều kiện
        Move();
    }
}
