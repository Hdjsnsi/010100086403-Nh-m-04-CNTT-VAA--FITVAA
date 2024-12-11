using UnityEngine;

public class RocketLauncher : Weapon
{
    [SerializeField]private Animator anim;
    public int index;
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
            anim.SetInteger("Status_panzerschreck", 2);
            return;
        } 
        Aim();
        if (distance.sqrMagnitude > fireRange * fireRange) return;
        if (Time.time < nextTimeToFire) return;
        Fire();
        anim.SetInteger("Status_panzerschreck",3);
        
    }
}
