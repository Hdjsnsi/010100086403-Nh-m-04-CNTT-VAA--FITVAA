
using SoldierType;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using WebSocketSharp;

public class Character : NetworkBehaviour
{
    protected Rigidbody rb;

    [Header("Character status")]
    [SerializeField]protected float speed;
    public float fireRange;
    protected GameObject[] allEnemy;
    protected Vector3 nearestEnemy;
    protected Vector3 targetDistance;
    [SerializeField] protected LayerMask ground;
    public string enemyTag;
    
    
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        InvokeRepeating("Detect",0,1f);
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        
    }

    

    // Update is called once per frame
    //Xác định mục tiêu 
    void Detect()
    {
        if(enemyTag.IsNullOrEmpty()) return;
        nearestEnemy = Detecting.DetectEnemy(transform.position,enemyTag);
        if(nearestEnemy == Vector3.zero) return;
        Vector3 enemyDistance = nearestEnemy - transform.position;
        targetDistance = enemyDistance;
    }

    //
    //.
    //

    //Nhận vật chuyển động và đi đến kẻ địch
    protected virtual void Movement()
    {   
        StillOnGround();
        if(nearestEnemy == Vector3.zero) return;
        Vector3 lookAt = new Vector3(targetDistance.x,0,targetDistance.z);
        float directed = Vector3.Angle(transform.forward,lookAt);
        Debug.Log(directed);
        Rotate();
        if(directed > 10) return;
        Move();
    }
    
    protected void StillOnGround()
    {
        if(Physics.Raycast(transform.position,Vector3.down * 2f,ground))
        {
            rb.linearDamping = 5f;   
        }else
        {
            rb.linearDamping = 0f;
        }
    }
    protected void Move()
    {
        if(targetDistance.sqrMagnitude < fireRange * fireRange) return; 
        if(Physics.Raycast(transform.position,Vector3.down * 2,out RaycastHit hit))
        {
            LimitVelocity();
            
            Vector3 direction = targetDistance.normalized;
            rb.AddForce(transform.forward * speed * 10,ForceMode.Force);
        }
    }
    protected void Rotate()
    {
        if(targetDistance == Vector3.zero) return;
        Quaternion thisRotate = Quaternion.LookRotation(targetDistance);
        thisRotate = Quaternion.Euler(0,thisRotate.eulerAngles.y,0);
        transform.rotation = Quaternion.Lerp(transform.rotation,thisRotate,Time.deltaTime);
    }
    private void LimitVelocity()
    {
        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float currentSpeed = currentVelocity.sqrMagnitude;
        if(currentSpeed >= speed * speed)
        {
            Vector3 limitedSpeed = currentVelocity.normalized * speed;
            rb.linearVelocity = new Vector3(limitedSpeed.x,rb.linearVelocity.y,limitedSpeed.z);
        }
    }
    
    //
    //.
    //

    public Vector3 NearestEnemy()
    {
        return nearestEnemy;
    }
}
    