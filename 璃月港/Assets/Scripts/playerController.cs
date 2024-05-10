using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;


public class playerController : MonoBehaviour
{
    // ԭ����ָ���߼���û��������
    public float walkspeed = 2f;//��·�ٶ�
    public float runspeed = 10f;//ÿ�����ף��ܲ��ٶ�
    public float climbspeed = .21f;//�����ٶ�
    public float rou = 0.2f;//б��Ħ����Ӧ�ļ��ٶ�
    public float jumpheight = 0.8f;//Ԥ����Ծ�߶�
    Transform maincamera;
    float acc = 30;//��ҵļ��ٶ�
    float speed;//��ʱ�̵�˲ʱ�ٶ�
    float targetspeed;//��·���ܲ��ٶȻ�0��ֻ��������ֵ
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
    public float horizontalOffsetFromWall = 0.25f;             //ˮƽƫ����
     
    float v;
    float h; 

    private Vector3 targetPosition; //Ŀ��λ�� 
    private float step; //ÿ���ƶ��ľ���
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
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));//wasd�����ƶ�
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
        if (!isonground() && !isclimb)//�����ڵ�����
        {
            Debug.Log(75);

            verticalVelocity += Gravity * Time.fixedDeltaTime;// �������ٶ�  
            speed = changespeed(speed, targetspeed, acc / 10f);
        }
        else//���ڵ�����
        {
            Debug.Log(82);

            animator.SetBool("falling", false);
            verticalVelocity = 0f;//����������������Ψһ���������ٶ���Ҫ�ֶ�����

        }


        if (!isclimb)
        {

            Debug.Log(93);

            if (inputdir != Vector2.zero)// ����wsad��һ������
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

            if (inputdir != Vector2.zero)// ����wsad��һ������
            {
                //���¶�����������  
                //�˳������ķ����ǣ�����ǽ������⵽�¶ȱ仺��̨�ף�����û�����������˳������ķ���
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
                    //�趨�߶�
                    targetPosition = transform.position;
                    targetPosition.y += v * 2f * Time.deltaTime;

                    //��ת
                    //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.2f);
                    transform.rotation = GetRotationFromDirection(-hit.normal);
                    //��ɫ�趨��Ԥ��λ�� 
                    transform.position = targetPosition;
                    step = Vector3.Distance(transform.position, targetPosition) / 0.05f;
                    //�����ƶ�
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, step * Time.deltaTime);


                    //�����ƶ�
                    transform.Translate(Vector3.right * h * Time.deltaTime * 3);
                    Debug.Log(180);

                    if (Nowstr() == "¥��" | Nowstr() == "�ɷ�Խ����")
                    {
                        Debug.Log(184);

                        characterController.Move(transform.up * detect_height_and_depth().Item1);
                        isclimb = false;
                        animator.SetBool("climb", false);
                        animator.SetBool("cross", true);

                    }
                    if (Nowstr() == "����")
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






        if (!isclimb)//�������߼�����·�ܲ����߼�����ȫȫ��һ�����������
        {
            velocity = transform.forward * speed;//�������������������ˮƽ����ٶȣ�����ֻ�迼����ֱ������˶� 
            movement = velocity * Time.fixedDeltaTime - Vector3.down * verticalVelocity * Time.fixedDeltaTime;
            Debug.Log(224);

            if (Nowstr() == "¥��")
            {
                Debug.Log(228);

                characterController.Move(transform.up * detect_height_and_depth().Item1);
            }
            if (Nowstr() == "����")
            {
                Debug.Log(234);


            }

            if (Nowstr() == "�ɷ�Խ����")
            {
                characterController.Move(transform.up * detect_height_and_depth().Item1);
                animator.SetBool("cross", true);
                Debug.Log(243);

            }
            if (Nowstr() == "ǽ��")
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
            return "¥��";

        }
        if (0.2f <= h && h < 0.6f)
        {
            if (d >= 1.5f)
            {
                return "����";
            }
            if (d > 0.33f)
            {
                return "����";
            }
            if (d < 0.33f)
            {
                return "¥��";
            }
        }
        if (0.6f <= h && h < 1.0f)
        {
            return "¥��";
        }
        if (1.0f <= h && h < 1.7f)
        {
            if (d <= 0.5f)
            {
                return "�ɷ�Խ����";
            }

        }
        if (1.7f <= h)
        {
            return "ǽ��";
        }

        return "ƽ��";

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
            Vector3 rayOrigin = transform.position + 1.55f * Vector3.up; // �������
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
        //���������ķ���������нǣ�Ȼ����ԭ��̬����ת������Ƕ�
        float yaw = Mathf.Atan2(direction.x, direction.z);
        return Quaternion.Euler(0, yaw * Mathf.Rad2Deg, 0);
    }


    public FloatAndVector3 Gam(RaycastHit hit)
    {
        Vector3 slopeNormal = hit.normal;
        // �����ɫ����б���ƶ��ķ��� 

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
            this.gradient_angle = gradient_angle;//���߼���������ײ����ݶ�������ˮƽ��ļн�
            this.actual_angle = actual_angle;//��ɫʵ���˶������ڷ�ƽ���ϵ�ͶӰ�����Բο��ܹ������������ͱڵ�����
            this.moveDirection = moveDirection;//��ɫ�ڽ���������֡�ƶ��ķ�������
        }
    }

}


