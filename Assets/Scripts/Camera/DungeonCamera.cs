using UnityEngine;

public class DungeonCamera : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    GameObject playerObj;
    void Start()
    {
        if(playerObj == null)
        {
            playerObj = PlayerController.instance.Get_PlayerObj();
        }
    }

    // Update is called once per frame
    void Update()
    {
        FollowPlayer();
    }

    public void FollowPlayer()
    {
        Vector3 pos = playerObj.transform.position;
        pos.z = -10;
        this.transform.position = pos;
    }
}
