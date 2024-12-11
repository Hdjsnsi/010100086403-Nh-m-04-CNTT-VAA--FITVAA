    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using Unity.Netcode;
public class Weapon : NetworkBehaviour
{
    protected Transform shootPoint;
    protected Transform weapon;
    protected Transform[] allObject;
    protected Vector3 nearestEnemy;
    private Character character;
    protected Vector3 distance;
    protected float fireRange;
    protected float nextTimeToFire;
    

    [Header("Weapon Status")]
    [SerializeField] private GameObject ammoType;
    [SerializeField] private float damage;
    [SerializeField] private float fireRate; // Tốc độ bắn

    // Thêm các biến cho độ giật
    [Header("Recoil Settings")]
    private float recoilVerticalAmount; // Độ giật mỗi lần bắn
    [SerializeField] private float recoilRecoverySpeed; // Tốc độ hồi phục về vị trí ban đầu
    protected float currentVerticalRecoil = 0f;

    // Start is called before the first frame update
    protected void Awake()
    {
        nextTimeToFire = Time.time + fireRate;
        character = GetComponent<Character>();
        fireRange = character.fireRange;
        GetAllObject();
        InvokeRepeating("GetEnemy", 0, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        Shooting();
        Debug.DrawRay(shootPoint.position, shootPoint.forward * 100, Color.red);
        //Debug.DrawRay(weapon.position, weapon.forward * 100, Color.red);

    }
    
    void GetAllObject()
    {
        allObject = GetComponentsInChildren<Transform>();
        shootPoint = allObject.Where(a => a.gameObject.name == "Shoot Point").FirstOrDefault();
        weapon = allObject.Where(a => a.gameObject.name == "Bone").FirstOrDefault();
        allObject = new Transform[0];
    }

    protected virtual void Shooting()
    {
        if (nearestEnemy == Vector3.zero) return;
        Aim();
        if (distance.sqrMagnitude > fireRange * fireRange) return;
        if (Time.time < nextTimeToFire) return;
        Fire();
        //Recoil();
        
    }

    protected void Aim()
    {
        Vector3 enemyShootPoint = nearestEnemy - shootPoint.position;
        Vector3 localEnemyShootPointDirection = shootPoint.parent.InverseTransformDirection(enemyShootPoint);
        Quaternion shootPointRotation = Quaternion.LookRotation(localEnemyShootPointDirection);
        shootPoint.localRotation = Quaternion.Lerp(shootPoint.localRotation,shootPointRotation,Time.deltaTime);
    }
    
    protected void Fire()
    {
        
        nextTimeToFire = Time.time + fireRate;
        // GameObject ammo = Instantiate(ammoType, shootPoint.position, shootPoint.rotation);
        // ammo.GetComponent<Rigidbody>().AddForce(shootPoint.forward * 100, ForceMode.Impulse);
        SpawnServerRpc();
            
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnServerRpc()
    {
        GameObject ammo = Instantiate(ammoType, shootPoint.position, shootPoint.rotation);
        NetworkObject networkAmmo = ammo.GetComponent<NetworkObject>();
        networkAmmo.Spawn();
        Ammo damageSet = ammo.GetComponent<Ammo>();
        damageSet.AmmoDamage(damage);
        ammo.GetComponent<Rigidbody>().AddForce(shootPoint.forward * 100, ForceMode.Impulse);
    }

    void GetEnemy() 
    {
        try
        {
            nearestEnemy = character.NearestEnemy();
            distance = nearestEnemy - transform.position; 
        }catch{}
    }
    void Recoil()
    {
        // Hồi phục độ giật
        currentVerticalRecoil = Mathf.Lerp(currentVerticalRecoil, 0f, Time.deltaTime * recoilRecoverySpeed);
        // Áp dụng độ giật vào weapon
        weapon.localRotation = Quaternion.Euler(currentVerticalRecoil, weapon.localRotation.eulerAngles.y, weapon.localRotation.eulerAngles.z); // Cập nhật độ giật
    }
}
