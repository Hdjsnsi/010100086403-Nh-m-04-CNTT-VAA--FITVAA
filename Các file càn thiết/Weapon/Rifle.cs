using Unity.Netcode.Components;
using UnityEngine;

public class Rifle : Weapon
{
    [SerializeField]private Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Shooting();
        Debug.DrawRay(shootPoint.position, shootPoint.forward * 100, Color.red);
    }
    protected override void Shooting()
    {
        if (nearestEnemy == Vector3.zero)
        {
            anim.SetInteger("Status_stg44", 0);
            return;
        } 

        Aim();
        if (distance.sqrMagnitude > fireRange * fireRange) return;
        anim.SetInteger("Status_stg44", 2);
        if (Time.time < nextTimeToFire) return;
        Fire();
    }
}
