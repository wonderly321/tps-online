using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    public int playerID;
    private Vector3 rotation = Vector3.zero;
    private Rigidbody rb;
    private Animator animator;

    public Vector3 logicPosition;
    public Vector3 lastLogicPosition;

    private GameManager gameMgr;
    private StateManager stateMgr;

    [SerializeField]
    private Camera cam;
    public bool isDead;
    public bool isJumping = false;
    public bool last_isShooting;
    
    private AudioSource audioPlayer;

    void Awake()
    {
        gameMgr = GameObject.Find("GameManager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        stateMgr = GameObject.Find("StateManager").GetComponent<StateManager>();
        logicPosition = rb.position;
        lastLogicPosition = rb.position;
        isDead = false;
        audioPlayer = GetComponent<AudioSource>();
    }

    public void HandleMsg(byte[] msg)
    { 
        // Debug.Log("Motor  " + msg.Length);
        int msg_type = System.BitConverter.ToInt32(msg, 0);
        // Debug.Log(msg_type+ " " + ProtoSettings.MSG_PLAYER_SHOOT );
        switch(msg_type) {
            case ProtoSettings.MSG_PLAYER_MOVE:
                lastLogicPosition = logicPosition;
                Vector3 new_pos = MessageFactory.Utils.Decode_Vector3(msg, 4);
                float X = System.BitConverter.ToInt32(msg, 16) * 0.001f;
                float Y = System.BitConverter.ToInt32(msg, 20) * 0.001f;
                
                float isRunning = Mathf.Abs(X) + Mathf.Abs(Y);
                animator.SetBool("isRunning", isRunning > 1e-3);
                animator.SetFloat("forward", X);
                animator.SetFloat("left", Y);
                if(isRunning > 1e-3) logicPosition = new_pos;
                
                Vector3 _rotation = MessageFactory.Utils.Decode_Vector3(msg, 24);
                float y = System.BitConverter.ToInt32(msg, 36) * 0.001f;
                rotation = _rotation;
                break;
            case ProtoSettings.MSG_PLAYER_SHOOT:
                int playerID = System.BitConverter.ToInt32(msg, 4);            
                int bullets = System.BitConverter.ToInt32(msg, 8);
                //animate
                // Debug.Log("playerID " + playerID + " bullets " + bullets);
                if (animator.GetBool("isRunning")) animator.Play("Base Layer.RunShooting");
                else animator.Play("Base Layer.Shooting");
                audioPlayer.Play(0);
                stateMgr.gunBullet = bullets;
                break;
            case ProtoSettings.MSG_PLAYER_HURT:
                int health = System.BitConverter.ToInt32(msg, 4);
                Debug.Log(health);
                stateMgr.HP = health;
                if(stateMgr.HP == 0){
                    Die();
                }
                break;
            case ProtoSettings.MSG_PLAYER_RELOAD:
                int gun = System.BitConverter.ToInt32(msg, 4);
                int bag = System.BitConverter.ToInt32(msg, 8);
                animator.Play("Base Layer.Reloading");
                stateMgr.gunBullet = gun;
                stateMgr.bagBullet = bag;
                break;
            case ProtoSettings.MSG_PLAYER_JUMP:
                animator.Play("Base Layer.JumpStart");
                isJumping = true;
                break;
        }
    }

    public void Move(Vector3 _velocity, float X, float Y, Vector3 _rotation, float mouseX)
    {
        float isRunning = Mathf.Abs(X) + Mathf.Abs(Y);
        var newPos = logicPosition;
        if(isRunning > 1e-3) newPos += _velocity * Time.fixedDeltaTime;
        byte[] msg = MessageFactory.SetMoveMsg(
            MessageFactory.Utils.Encode_Vector3(newPos),
            X, Y,
            MessageFactory.Utils.Encode_Vector3(_rotation),
            mouseX
        );
        gameMgr.SendMsg(msg);
    }
    public void Jump(bool _jump)
    {
        if(_jump == false || isJumping == true) return;
        Debug.Log("jump!");
        byte[] msg = MessageFactory.SetJumpMsg();
        gameMgr.SendMsg(msg);
    }

    public void Shoot(bool isShooting) 
    {     
        if(!isShooting || stateMgr.gunBullet <= 0) {
            last_isShooting = isShooting;
            return;
        }

        if(last_isShooting == false && isShooting == true)
        {
            //  if(Time.deltaTime < 1) return;
            if(stateMgr.gunBullet <= 0)
            {
                return;
            }
            //获得屏幕中心位置，转化为世界坐标
            Vector3 point = cam.ScreenToWorldPoint (new Vector3 (Screen.width / 2, Screen.height / 2,0));
            //定义射线
            RaycastHit hitinfo;
            //发射射线
            bool isCollider = Physics.Raycast (point, cam.transform.forward, out hitinfo);
            if (isCollider) {
                //如果射线撞击到了物体
                int hurt = 10;
                var obj = hitinfo.collider.gameObject;
                if(obj.tag == "Player") {
                    int hurtPlayerID = obj.GetComponent<PlayerMotor>().playerID;
                    Debug.Log(obj + " " + hurtPlayerID);
                    byte[] msg = MessageFactory.SetShootMsg(hurtPlayerID, 1, hurt);
                    gameMgr.SendMsg(msg);
                }
                else {
                    Debug.Log("no hit");
                    byte[] msg = MessageFactory.SetShootMsg(0, 1, hurt);
                    gameMgr.SendMsg(msg);
                }
            } else {
                Debug.Log("no hit");
                byte[] msg = MessageFactory.SetShootMsg(0, 1, 0);
                gameMgr.SendMsg(msg);
                
            }
            last_isShooting = isShooting;
        }
    }
    public void Die()
    {
        Debug.Log("Game over");
        isDead = true;
        stateMgr.GameOver();
        rb.MoveRotation(Quaternion.LookRotation(Vector3.up));
    }

    public void OtherDie()
    {
        rb.MoveRotation(Quaternion.LookRotation(Vector3.up));
    }
    public void Reload(bool isReload)
    {
        //animation
        if(isReload == false) return;
        Debug.Log("reload bullets");
        byte[] msg = MessageFactory.SetReloadMsg();
        gameMgr.SendMsg(msg);
    }

    void Update()
    {
        var animatorInfo = animator.GetCurrentAnimatorStateInfo(0);
        if(animatorInfo.IsName("JumpStart") || animatorInfo.IsName("JumpAir") || animatorInfo.IsName("JumpEnd")) {
        }
        else isJumping = false;
        PerformMovement();
        PerformRotation();
    }
    void PerformMovement()
    {
        var renderPos = Vector3.Lerp(lastLogicPosition, logicPosition, gameMgr.Interpolation);
        rb.MovePosition(renderPos);
    }
    void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
    }
}
