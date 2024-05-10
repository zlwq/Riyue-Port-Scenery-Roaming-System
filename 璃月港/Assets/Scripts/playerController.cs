using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;


public class playerController : MonoBehaviour
{
    // 原神动作指令逻辑，没有体力条
    public float walkspeed = 2f;//走路速度
    public float runspeed = 10f;//每秒六米，跑步速度
    public float climbspeed = .21f;//攀爬速度
    public float rou = 0.2f;//斜面摩擦对应的加速度
    public float jumpheight = 0.8f;//预期跳跃高度
    Transform maincamera;
    float acc = 30;//玩家的加速度
    float speed;//此时刻的瞬时速度
    float targetspeed;//走路，跑步速度或0，只有这三个值
    bool dance = false; 
    CharacterController characterController;
    Animator animator;
    public float maxSlopeAngle = 50;
    Vector3 velocity;
    public LayerMask GroundLayer;
    public float Gravity = -9.8f;
    public float verticalVelocity = 0;
    Vector3 movement;
    List<float> latest_y_position = new List<float>();
    public float horizontalOffsetFromWall = 0.25f;             //水平偏移量
     
    float v;
    float h; 

    private Vector3 targetPosition; //目标位置 
    private float step; //每秒移动的距离
    public bool isclimb; 

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        Application.targetFrameRate = 60;
        maincamera = Camera.main.transform;
        animator.SetBool("true", true);
        for (int i = 0; i <= 9; i++)
        {
            latest_y_position.Add(transform.position.y);
        }
    }

    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));//wasd控制移动
        Vector2 inputdir = input.normalized;
        Debug.Log(60);
        if (verticalVelocity <= -1.0f)
        {
            animator.SetBool("falling", true);
            Debug.Log(64);

        }
        else
        {
            animator.SetBool("falling", false);
            Debug.Log(70);

        }
        if (!isonground() && !isclimb)//若不在地面上
        {
            Debug.Log(75);

            verticalVelocity += Gravity * Time.fixedDeltaTime;// 重力加速度  
            speed = changespeed(speed, targetspeed, acc / 10f);
        }
        else//若在地面上
        {
            Debug.Log(82);

            animator.SetBool("falling", false);
            verticalVelocity = 0f;//这是下落与非下落的唯一区别，下落速度需要手动设置

        }


        if (!isclimb)
        {

            Debug.Log(93);

            if (inputdir != Vector2.zero)// 输入wsad的一个或多个
            {
                dance = false;
                animator.SetBool("cross", false);
                animator.SetBool("j", false);
                animator.SetBool("walk", true);
                transform.eulerAngles = Vector3.up * (Mathf.Atan2(inputdir.x, inputdir.y) * Mathf.Rad2Deg + maincamera.eulerAngles.y);
                targetspeed = walkspeed;
                Debug.Log(103);

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    animator.SetBool("shift", true);
                    targetspeed = runspeed;
                    Debug.Log(109);

                }
                else
                {
                    animator.SetBool("shift", false);
                    Debug.Log(115);

                }
                speed = changespeed(speed, targetspeed, acc);
                Debug.Log(119);

            }
            else
            {
                targetspeed = 0f;
                speed = changespeed(speed, targetspeed, acc);
                animator.SetBool("walk", false);
                Debug.Log(127);

                if (Input.GetKey(KeyCode.J) && dance == false)
                {
                    dance = true;
                    animator.SetBool("j", true);
                }
                else
                {
                    dance = false;
                    animator.SetBool("j", false);

                }

            }
        }
        else
        {

            if (inputdir != Vector2.zero)// 输入wsad的一个或多个
            {
                //重新定义上下左右  
                //退出攀爬的方法是，爬到墙顶，检测到坡度变缓或台阶，这里没有设置主动退出攀爬的方法
                dance = false;
                animator.SetBool("j", false);
                animator.SetBool("walk", true);
                animator.SetBool("climb", true);
                animator.SetBool("cross", false);

                v = Input.GetAxis("Vertical");
                h = Input.GetAxis("Horizontal");
                RaycastHit hit;
                bool a = Physics.Raycast(transform.position + Vector3.up * 0.9f, transform.forward, out hit, 0.3f); 
                Debug.Log(160);

                if (a)
                { 
                    //设定高度
                    targetPosition = transform.position;
                    targetPosition.y += v * 2f * Time.deltaTime;

                    //旋转
                    //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.2f);
                    transform.rotation = GetRotationFromDirection(-hit.normal);
                    //角色设定到预定位置 
                    transform.position = targetPosition;
                    step = Vector3.Distance(transform.position, targetPosition) / 0.05f;
                    //上下移动
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, step * Time.deltaTime);


                    //左右移动
                    transform.Translate(Vector3.right * h * Time.deltaTime * 3);
                    Debug.Log(180);

                    if (Nowstr() == "楼梯" | Nowstr() == "可翻越陡坡")
                    {
                        Debug.Log(184);

                        characterController.Move(transform.up * detect_height_and_depth().Item1);
                        isclimb = false;
                        animator.SetBool("climb", false);
                        animator.SetBool("cross", true);

                    }
                    if (Nowstr() == "缓坡")
                    {
                        Debug.Log(194);

                        isclimb = false;
                        animator.SetBool("climb", false);

                    }

                }
                else
                {
                    isclimb = false;
                    animator.SetBool("climb", false);
                    animator.SetBool("walk", false);
                    animator.SetBool("shift", false);
                    Debug.Log(208);

                }
            }

        }






        if (!isclimb)//攀爬的逻辑和走路跑步的逻辑完完全全不一样，必须隔开
        {
            velocity = transform.forward * speed;//正常情况下算出来玩家在水平面的速度，下面只需考虑竖直方向的运动 
            movement = velocity * Time.fixedDeltaTime - Vector3.down * verticalVelocity * Time.fixedDeltaTime;
            Debug.Log(224);

            if (Nowstr() == "楼梯")
            {
                Debug.Log(228);

                characterController.Move(transform.up * detect_height_and_depth().Item1);
            }
            if (Nowstr() == "缓坡")
            {
                Debug.Log(234);


            }

            if (Nowstr() == "可翻越陡坡")
            {
                characterController.Move(transform.up * detect_height_and_depth().Item1);
                animator.SetBool("cross", true);
                Debug.Log(243);

            }
            if (Nowstr() == "墙壁")
            {
                Debug.Log(248);

                isclimb = true;

            }
            characterController.Move(movement);
            latest_y_position.Add(transform.position.y);
            latest_y_position.RemoveAt(0);
            Debug.Log(256);


        }

        if (isonground() && actual_vertical_speed()<1f)
        {
            verticalVelocity = 0f;

            Debug.Log(264);
            if (Physics.Raycast(transform.position + Vector3.up * 0.9f, transform.forward, 0.3f) && inputdir != Vector2.zero)
            {
                Debug.Log(268); 
                isclimb = true;
            }
        }
    }


    public float changespeed(float speed, float targetspeed, float used_acc)
    {
        if (speed < targetspeed)
        {

            speed += (Time.fixedDeltaTime * used_acc);
            if (speed > targetspeed)
            {
                speed = targetspeed;
            }
        }
        else
        {

            speed -= (Time.deltaTime * used_acc);
            if (speed < targetspeed)
            {
                speed = targetspeed;
            }
        }
        return speed;
    }
    public string Nowstr()
    {
        float h, d;
        h = detect_height_and_depth().Item1;
        d = detect_height_and_depth().Item2;
        if (h < 0.2f && h > 0)
        {
            return "楼梯";

        }
        if (0.2f <= h && h < 0.6f)
        {
            if (d >= 1.5f)
            {
                return "缓坡";
            }
            if (d > 0.33f)
            {
                return "陡坡";
            }
            if (d < 0.33f)
            {
                return "楼梯";
            }
        }
        if (0.6f <= h && h < 1.0f)
        {
            return "楼梯";
        }
        if (1.0f <= h && h < 1.7f)
        {
            if (d <= 0.5f)
            {
                return "可翻越陡坡";
            }

        }
        if (1.7f <= h)
        {
            return "墙壁";
        }

        return "平地";

    }
    
    public bool isonground()
    {
        if (Physics.Raycast(transform.position+ Vector3.up*0.7f, Vector3.down, 1f) |
            Physics.Raycast(transform.position + Vector3.up * 0.7f - transform.forward.normalized * 0.3f, Vector3.down, 1f) |
            Physics.Raycast(transform.position + Vector3.up * 0.7f + transform.forward.normalized * 0.3f, Vector3.down, 1f)) 
        {
            return true;
        }
        else
        {
            return false;
        } 
    }
    
    public Tuple<float, float> detect_height_and_depth()
    {
        float height = 0f;
        float depth = 0.3f;
        List<bool> isdetected = new List<bool>();
        for (float h = 0f; h <= 2f; h += 0.1f)
        {
            Vector3 rayOrigin = transform.position + h * Vector3.up;
            isdetected.Add(Physics.Raycast(rayOrigin, transform.forward, 0.3f));

        }
        for (int i = 19; i > 0; i--)
        {
            if ((!isdetected[i]) && isdetected[i - 1])
            {
                height = 0.1f * (i - 1);
            }


        }
        if (height == 0)
        {
            if (isdetected[19]) height = 2f;
            else
            {
                for (float h = 0f; h <= 0.1f; h += 0.01f)
                {
                    Vector3 rayOrigin = transform.position + h * Vector3.up;
                    if (Physics.Raycast(rayOrigin, transform.forward, 0.3f))
                    {
                        height = h;
                    }

                }
            }

        }

        for (float d = 0.3f; d <= 4f; d += 0.1f)
        {
            Vector3 rayOrigin = transform.position + 1.55f * Vector3.up; // 刻晴身高
            if (Physics.Raycast(rayOrigin, transform.forward, d))
            {
                depth = d;
                break;
            }
        }
        Tuple<float, float> myTuple = new Tuple<float, float>(height, depth);

        return myTuple;
    }
    public float actual_vertical_speed()
    {
        return (latest_y_position.Max() - latest_y_position.Min()) / 10 / Time.fixedDeltaTime;
    }


    Quaternion GetRotationFromDirection(Vector3 direction)
    {
        //根据所给的方向向量求夹角，然后在原姿态上旋转到这个角度
        float yaw = Mathf.Atan2(direction.x, direction.z);
        return Quaternion.Euler(0, yaw * Mathf.Rad2Deg, 0);
    }


    public FloatAndVector3 Gam(RaycastHit hit)
    {
        Vector3 slopeNormal = hit.normal;
        // 计算角色沿着斜面移动的方向 

        Vector3 moveDirection = Vector3.ProjectOnPlane(transform.forward, slopeNormal);

        Vector3 gradientDirection = Vector3.ProjectOnPlane(Vector3.up, slopeNormal);
        float x = gradientDirection.x;
        float y = gradientDirection.y;
        float z = gradientDirection.z;
        float gradient_angle = Mathf.Atan(Mathf.Abs(y) / Mathf.Sqrt(x * x + z * z)) * Mathf.Rad2Deg;

        x = moveDirection.x;
        y = moveDirection.y;
        z = moveDirection.z;
        float actual_angle = Mathf.Atan(Mathf.Abs(y) / Mathf.Sqrt(x * x + z * z)) * Mathf.Rad2Deg;
        return new FloatAndVector3(gradient_angle, actual_angle, moveDirection);
    }
    public struct FloatAndVector3
    {
        public float gradient_angle;
        public float actual_angle;
        public Vector3 moveDirection;

        public FloatAndVector3(float gradient_angle, float actual_angle, Vector3 moveDirection)
        {
            this.gradient_angle = gradient_angle;//射线检测点所在碰撞体的梯度向量与水平面的夹角
            this.actual_angle = actual_angle;//角色实际运动方向在法平面上的投影，可以参考能够游走在悬崖峭壁的岩羊
            this.moveDirection = moveDirection;//角色在接下来若干帧移动的方向向量
        }
    }

}


