using UnityEngine;

public class NetworkPlayerInformation : MonoBehaviour
{
    private static NetworkPlayerInformation instance;
    public string playerName;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Xóa object nếu đã tồn tại
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Giữ object khi chuyển scene
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetPlayerName(string name)
    {
        playerName = name;
    }
}
