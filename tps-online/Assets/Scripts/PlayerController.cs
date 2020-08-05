using UnityEngine;

[RequireComponent(typeof(PlayerMotorSelf))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float lookSensitivity = 3;

    [SerializeField]
    private Camera cam;
    private PlayerMotorSelf motor;
    void Start()
    {      
        motor = GetComponent<PlayerMotorSelf>();
    }

    void FixedUpdate()
    {
        if(motor.isDead) return;
        float _xMov = Input.GetAxisRaw("Horizontal");
        float _yMov = Input.GetAxisRaw("Vertical");
        float _yRot = Input.GetAxisRaw("Mouse X");
        float _xRot = Input.GetAxisRaw("Mouse Y");
        bool _shootOn = Input.GetMouseButton(0);
        bool _isReload = Input.GetKey(KeyCode.R);
        bool _Jump = Input.GetKey(KeyCode.Space);

        #region PLAYER MOVEMENT
        Vector3 _movHorizontal = transform.right * _xMov;
        Vector3 _movVertical = transform.forward * _yMov;
        Vector3 _velocity = (_movHorizontal + _movVertical).normalized * speed;
        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * lookSensitivity;
        //Apply movement
        motor.Move(_velocity, _yMov, -_xMov, _rotation, _yRot);
        motor.Jump(_Jump);
        #endregion

        #region GUN&GUNSTAR
        RotateCamera(_xRot * 0.1f);
        motor.Shoot(_shootOn); 
        motor.Reload(_isReload);  
        #endregion
    }

    private void RotateCamera(float alpha)
    {
        cam.GetComponent<MainCameraController>().alpha += alpha;
    }
}