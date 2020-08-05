using System.Collections;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    //target 
    public Transform target;
    public Transform gun;
    public float alpha;  
    //vertical translation  
    public float distanceUp=1.5f;  
    //horizontal translation
    public float distanceAway = 3f;  
    //position smooth 
    public float posSmooth = 2f;  
    //Fov smooth 
    public float fovSmooth = 5f;

    void Start() {
        transform.LookAt(gun.position + target.forward * 1000);
    }  
    void Update () {  
    // mouse scroller control distance 
    if ((Input.mouseScrollDelta.y < 0 && Camera.main.fieldOfView >= 3) || Input.mouseScrollDelta.y > 0 && Camera.main.fieldOfView <= 80)  
    {  
    Camera.main.fieldOfView += Input.mouseScrollDelta.y * fovSmooth * Time.deltaTime;  
    }  
    }  
    void LateUpdate()  
    {  
    // //camera position  
    // Vector3 targetPos = target.position + Vector3.up * distanceUp - target.forward * distanceAway;  
    // transform.position=Vector3.Lerp(transform.position,targetPos,Time.deltaTime*posSmooth);  
    // //camera lookat  
    // transform.LookAt(target.position+new Vector3(0, 2, 0));  
        Vector3 targetPos = target.position - target.forward * distanceAway * 2 + target.right * distanceAway + Vector3.up * distanceUp;
        transform.position=Vector3.Lerp(transform.position,targetPos,Time.deltaTime*posSmooth);
        transform.LookAt(gun.position + target.forward * 1000 * Mathf.Cos(alpha) + target.up * 1000 * Mathf.Sin(alpha));
        //transform.rotation = Quaternion.Euler(new Vector3(gun.rotation.z, 0, 0));
    }  
}
