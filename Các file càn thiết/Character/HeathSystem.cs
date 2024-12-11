using Unity.Netcode;
using UnityEngine;

public class HeathSystem : NetworkBehaviour
{
    private float currentHeath;
    [SerializeField]private float maxHeath;
    public Transform targetTag;
    void Awake()
    {
        currentHeath = maxHeath;
        targetTag = transform.Find("Target");
    }

    // Update is called once per frame
    void Update()
    {
        Die();
    }
    void TakeDamage(float damage)
    {
        currentHeath -= damage;
    }
    void Die()
    {
        if(currentHeath <= 0 || transform.position.y < 0)
        {
            DieServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)] // Đánh dấu đây là Server RPC
    void DieServerRpc()
    {
        NetworkObject.Despawn(); 
        Destroy(gameObject);
    }
    
    private void OnCollisionEnter(Collision other)
    {
       if(other.gameObject.tag == "Damaged")
       {
            Ammo ammo = other.gameObject.GetComponent<Ammo>();
            TakeDamage(ammo.Hit());
       } 
    }
}
