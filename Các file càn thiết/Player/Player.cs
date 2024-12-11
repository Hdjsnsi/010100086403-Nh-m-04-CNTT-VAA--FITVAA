using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;


public class Player : NetworkBehaviour
{
    public float speed;
    public float sensitive;
    float xRotation,yRotation;
    private Rigidbody rb;
    private Camera playerCamera;
    private AudioListener playerListener;


    public event Action OnEditStateChanged;
    public bool isEdit;
    public bool isLockedEdit;
    public bool isSettingUI;

    // Start is called before the first frame update
    void Awake()
    {
        
    }
    void Start()
    {
        playerCamera = GetComponent<Camera>();
        playerListener = GetComponent<AudioListener>();
        rb = GetComponent<Rigidbody>();
        isEdit = false;
        isSettingUI = false;
        LockCursor();
        OnEditStateChanged += LockCursor;
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) return;
        if(Input.GetKeyDown(KeyCode.E)) SetIsEdit();
        PlayerMove();
        PlayerLook();
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkCamera();   
    }

    

    void NetworkCamera()
    {
        playerCamera = GetComponent<Camera>();
        playerListener = GetComponent<AudioListener>();
        // Kiểm tra xem player này có phải là của client hiện tại không
        if (IsLocalPlayer)
        {
            // Bật camera cho player của client này
            playerCamera.enabled = true;
            playerListener.enabled = true;
        }
        else
        {
            // Tắt camera cho player không phải của client này
            playerCamera.enabled = false;
            playerListener.enabled = false;

        }
    }
    void PlayerMove()
    {
        LimitVelocity();
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        if(horizontal == 0 && vertical == 0) return;   
        Vector3 direction = transform.forward * vertical + transform.right * horizontal;
        rb.AddForce(direction.normalized * speed * 10); 
    }
    void LimitVelocity()
    {
        Vector3 currentVelocity = rb.linearVelocity;
        if(currentVelocity.sqrMagnitude >= speed * speed)
        {
            rb.linearVelocity = currentVelocity.normalized * speed;
        }
    }
    void PlayerLook()
    {
        if(isSettingUI) return;
        if(isEdit) return;
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitive;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitive;
        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation,-90,90);
        transform.rotation = Quaternion.Euler(xRotation,yRotation,0);
    }
    
    void LockCursor()
    {
        if(isLockedEdit) return;
        if(!isEdit)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
        }
    }

    public void SetIsEdit()
    {
        isEdit = !isEdit;
        OnChangeEdit();
    }

    public void OnChangeEdit()
    {
        OnEditStateChanged?.Invoke();
    }
}
