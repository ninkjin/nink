using UnityEngine;

public class CircleFollow : MonoBehaviour
{
    void Update()
    {
        
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; 
        transform.position = mouseWorldPos; 
    }
}