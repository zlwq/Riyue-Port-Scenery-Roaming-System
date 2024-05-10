using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class thirdpersoncamera : MonoBehaviour
{
    // Start is called before the first frame update
    float yaw = 0;
    float pitch = 0;
    public float MouseSensitivity = 10;
    public float offset = 2;
    public Transform target;
    public Vector2 pitchmnmax = new Vector2(-40, 70); 
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    // Update is called once per frame
    void Update()
    {
        
        yaw += Input.GetAxis("Mouse X") * MouseSensitivity;
        pitch += Input.GetAxis("Mouse Y") * MouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchmnmax.x, pitchmnmax.y); 
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // 计算相机位置
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -offset);
        Vector3 position = rotation * negDistance + target.position;

        // 设置相机的位置和旋转
        transform.rotation = rotation;
        transform.position = position;
    }
}
