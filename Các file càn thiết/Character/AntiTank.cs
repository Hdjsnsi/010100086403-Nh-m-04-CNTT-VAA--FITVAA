using UnityEngine;

public class AntiTank : Character
{
    private Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
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
