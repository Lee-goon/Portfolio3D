using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public struct ContactInfo
{
    public GameObject other;
    public ContactPoint contact;
    public float time;
    public ContactInfo(GameObject wantOther, ContactPoint wantContact, float wantTime)
    {
        other = wantOther;
        contact = wantContact;
        time = wantTime;
    }
}

public class CharacterController : MonoBehaviour
{
    public const float coyoteTime = 0f;

    Rigidbody rigid;
    LineRenderer lineRenderer;
    SpringJoint springJoint;
    public GameObject modelPlayerObject;
    public Transform modelCameraArm;
    // public TextMeshProUGUI contactText;

    public float modelSpeed;
    public float jumpPower;
    private float jumpTime;
    float angle;

    protected List<ContactInfo> collisionList = new List<ContactInfo>();
    
    bool isGround;
    bool isAttach;
    bool OnGrappling;
    bool OnPulling;
    
    Vector3 mMoveInput;
    Vector3 spot;
    Vector3 wallRight;
    Vector3 wallForward;

    RaycastHit raycastHit;
    Quaternion viewRotation;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        mMoveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        LookAround();
        Dash();
        Jump();
        // StickToTheWall();

        if (Input.GetMouseButtonDown(0)) { RopeShoot(); }
        else if (Input.GetMouseButtonUp(0)) { EndShoot(); }
        else if (Input.GetKeyDown(KeyCode.F)) { PullRope(); }
        else if (Input.GetKeyUp(KeyCode.F)) { EndPullRope(); }

        DrawRope();

    }

    private void FixedUpdate()
    {
        Move();

        isGround = CheckGround();
    }

    protected virtual void LookAround()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 camAngle = modelCameraArm.rotation.eulerAngles;

        // 카메라 암의 각도에서 상하 마우스 이동값의 차이로 카메라암이 상하로 얼마나 회전해야 하는지 저장하는 변수
        // x에서 y를 빼는 이유는 카메라 암이 Player오브젝트에 붙어있을때 상하 방향이 x축이기 때문
        float difference = camAngle.x - mouseDelta.y;

        // Clamp는 각도 제한하기 위해 사용
        // 정면이 0도 이기 때문에 이를 반으로 나눠 180보다 작은 경우와 큰 경우에 따라 각도를 제한
        if(difference < 180f)
        {
            // 카메라 각도제한을 위해 사용
            difference = Mathf.Clamp(difference, -10f, 60f);
        }
        else
        {
            difference = Mathf.Clamp(difference, 320f, 361f);
        }

        // 카메라 암의 회전값을 최종적으로 계산된 x, y축의 각도로 변경, z축은 변함없음.
        modelCameraArm.rotation = Quaternion.Euler(difference, camAngle.y + mouseDelta.x, camAngle.z);

    }

    protected virtual void Move()
    {
        // Update()에서 계산 된 modelMoveInput의 크기를 측정하여 0이 아닌 경우(입력이 있는 경우) 다음 처리를 이어서 함.
        // 입력이 없을 때 불필요한 계산을 줄이기 위해 사용
        bool isMove = mMoveInput.magnitude != 0;
        

        if(isMove)
        {
            // 카메라암의 정면 방향과 측면 방향에 대한 방향을 저장하는 변수
            // 플레이어 캐릭터의 현 방향이 아닌 카메라가 바라보고 있는 방향을 기준으로 회전하기 때문에 사용
            Vector3 lookForward = new Vector3(modelCameraArm.forward.x, 0f, modelCameraArm.forward.z).normalized;
            Vector3 lookRight   = new Vector3(modelCameraArm.right.x, 0f, modelCameraArm.right.z).normalized;

            // 벡터 연산에 의해 카메라 암의 정면방향 'y축 이동입력 + 카메라 암의 측면방향' x축 이동입력을 더하여 방향을 결정.
            Vector3 moveDirection;

            if (isAttach)
            {
                //                                          wallRight에 -를 붙인 이유
                //                                          => 플레이어의 입력방향과 움직이는 방향이 같게 하기 위해
                moveDirection = Vector3.up * mMoveInput.y + (-wallRight) * mMoveInput.x;
            }
            else 
            {
                moveDirection = lookForward * mMoveInput.y + lookRight * mMoveInput.x;

                if (!OnPulling)
                {
                    // 이동방향과 별개로 플레이어의 모습을 담당하는 PlayerModel의 방향을 움직이는 방향으로 회전시키기 위해
                    // 바라보는 방향을 구함
                    viewRotation = Quaternion.LookRotation(moveDirection.normalized);

                    // 줄을 당길때는 밑에 문장이 작용이 안되게
                    //                            션형구간     부드러운 전환이 가능                            방향을 바라보는 회전속도
                    transform.rotation = Quaternion.Lerp(modelPlayerObject.transform.rotation, viewRotation, Time.fixedDeltaTime * 20);
                }

            }
            
            // 최종적으로 방향과 속도와 deltaTime을 곱해 플레이어 캐릭터를 이동시킴
            transform.position += moveDirection * modelSpeed * Time.fixedDeltaTime;
        }

        if (OnPulling)
        {
            // viewRotation = Quaternion.LookRotation(spot);

            // 처음의 위치를 계산하고 저장한 상태에서 유지
            // 회전값을 어디에 저장 할 것인지, 회전값을 어떻게 유지 할 것인지 
            transform.rotation = viewRotation;
        }
    }

    protected virtual void Dash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) { modelSpeed *= 2f; }
        else if (Input.GetKeyUp(KeyCode.LeftShift)) { modelSpeed /= 2f; }
    }

    protected virtual void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            // Debug.Log("점프");

            if (Time.time - jumpTime < coyoteTime * Time.time) return;
            jumpTime = Time.time;
            // rigid.velocity = Vector3.zero;

            if (isAttach)
            {
                rigid.MovePosition(transform.position + wallForward);
                rigid.AddForce(wallForward * jumpPower, ForceMode.Impulse);
                // rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);

            }
            else
            {
                rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }
        }
    }

    protected virtual void StickToTheWall()
    {
        // duration : 그려지는 레이의 갯수, 0개면 한 줄만 나옴.
        // Debug.DrawRay(modelPlayerObject.transform.position, modelPlayerObject.transform.forward * 5f, Color.red);
        if (Physics.Raycast(modelPlayerObject.transform.position, modelPlayerObject.transform.forward, out raycastHit, 10f, LayerMask.GetMask("Building")))
        {
            // 가장 먼저 해야하는건 플레이어가 벽에 붙는걸 먼저 만들어야함.
            // 중력을 끄고 픽스드 조인트를 쓰면 달라붙게 될 것임
            // 중력이 꺼졌을때 플레이어가 위로 올라가는걸 방지하기 위해 넣음.
            rigid.velocity = Vector3.zero;
            rigid.useGravity = false;

            // sticktothewall이면 벽에 붙어야함.
            // 근데 안붙음 
            // 벽에 붙여줘야함.(가다가 멈춤)
            // Debug.Log("X");

            // 중력을 없애서 플레이어가 건물에 붙을 수 있게 만듦.
            

            // 플레이어가 건물을 돌아다닐 수 있게 만듦.
            // 플레이어의 rotation의 x축이 90도로 바뀌어야함.
            // RotateAround : 자신이 있는 위치와 특정점과의 거리를 반지름으로 하는 원을 그리면서 회전하는 공전과
            // 축을 기준으로 하는 자전을 동시에 수행 
            //                  플레이어를 기준으로 회전하게 만듦.
            // transform.RotateAround(modelPlayerObject.transform.position, modelPlayerObject.transform.right, -45f);

            // 닿아있는 벽면의 방향을 알아야함.... 
            // 플레이어가 땅에 있는 기준 => 플레이어의 왼쪽, 오른쪽
            // 플레이어가 벽에 붙어있는 기준 => 플레이어의 아래쪽
            // 벽에 붙은 플레이어의 기준 : 움직임이 가능한건 x좌표, z좌표
            // 외적 사용법
            // Vector3 cross = Vector3.Cross(modelPlayerObject.transform.position, modelCameraArm.transform.position);
            // 
            // if(cross.z > 0)
            // {
            //     Debug.Log("오른쪽");
            // }
            // else if(cross.z < 0)
            // {
            //     Debug.Log("왼쪽");
            // }
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 닿은 땅이 위로
        // if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Building") { isGround = true; }

        if (collision.gameObject.tag == "Building")
        {
            // 태그에 해당하는 오브젝트의 정보를 플레이어가 닿았을때 
            // 그 오브젝트의 정보를 알려줌.
            wallForward = collision.GetContact(0).normal;
            
            // 좌표를 기준으로 움직여야함
            // 좌표를 기준으로 오른쪽 벡터를 찾아야함.
            // 절대 위벡터랑 벽노말벡터를 가지고 오른쪽 벡터를 찾아야함.
            // (-x, y, z)
            wallRight = new Vector3(wallForward.z, wallForward.y, -wallForward.x);

            angle = Vector3.Angle(-Physics.gravity, wallForward);
            if (angle > 80f && angle < 100f)
            {
                isAttach = true;
                rigid.velocity = Vector3.zero;
                rigid.useGravity = false;
                OnPulling = false;
                // if (OnPulling == false) { Debug.Log("붙다"); }
            }

            // UI로 닿은 좌표 표시
            // Debug.Log(result);
            // contactText.text = wallForward.ToString();
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contacts = new ContactPoint[collision.contactCount];
        collision.GetContacts(contacts);
        ContactPoint myContacts = System.Array.Find(contacts, target => target.otherCollider.gameObject == gameObject);
        int collisionIndex = collisionList.FindIndex(target => target.other == collision.gameObject);
        if (collisionIndex < 0)
        {
            collisionList.Add(new ContactInfo(collision.gameObject, myContacts, Time.time));
        }
        else
        {
            collisionList[collisionIndex] = new ContactInfo(collision.gameObject, myContacts, Time.time);
        }
    }
     
    private void OnCollisionExit(Collision collision)
    {
        rigid.useGravity = true;
        if(collision.gameObject.tag == "Building")
        {
            isAttach = false;
            rigid.useGravity = true;
        }
    }

    protected virtual bool CheckGround() // 캐릭터가 바닥에 있는지 체크!
    {
        collisionList.RemoveAll(target => Time.time - target.time > coyoteTime);
        if (rigid.velocity.y == 0) return true;
        return collisionList.FindIndex(target => Vector3.Angle(-Physics.gravity, target.contact.normal) < 80f) >= 0;
    }

    protected virtual bool CheckWall()
    {
        return collisionList.FindIndex(target => (target.contact.normal.x) * (transform.position.x) < 80f) >= 0;
    }

    protected virtual void RopeShoot()
    {
        if (Physics.Raycast(modelCameraArm.transform.position, modelCameraArm.transform.forward, out raycastHit, 100f, LayerMask.GetMask("Building")))
        {
            OnGrappling = true;

            spot = raycastHit.point;
            lineRenderer.positionCount = 2; // 라인랜더러의 점의 개수
            lineRenderer.SetPosition(0, this.transform.position); // 첫번쨰 점을 플레이어의 위치로 설정
            lineRenderer.SetPosition(1, raycastHit.point); // 두번째 점을 레이캐스트의 위치로 설정
            
            springJoint = modelPlayerObject.AddComponent<SpringJoint>();
            // autoConfigureConnectedAnchor : 유니티가 연결된 앵커 포인트의 포지션을 자동으로 계산해야 하는지
            // 연결된 앵커 자동설정 비활성화
            springJoint.autoConfigureConnectedAnchor = false;
            // connectedAnchor : 연결된 오브젝트의 로컬 공간에서 조인트가 고정 된 포인트
            // 연결된 앵커를 훅지점으로 설정
            springJoint.connectedAnchor = spot;

            // 플레이어와 연결 지점간의 거리 계산
            //                                   플레이어의 위치        레이캐스트를 쐈을때 생긴 점
            float distance = Vector3.Distance(this.transform.position, spot);

            // maxDistance : 스프링이 어떠한 힘도 작용되지 않는 최대거리 한계
            springJoint.maxDistance = distance;
            // minDistance : 스프링이 어떠한 힘도 작용되지 않는 최소거리 한계
            springJoint.minDistance = distance * 0.2f;
            // spring : 스프링의 강도
            springJoint.spring = 2f;
            // damper : 활성화 되어 있을때 스프링이 줄어드는 강도
            springJoint.damper = 5f;
            // massScale : 스프링 질량 설정
            springJoint.massScale = 5f;

        }
        
    }

    protected virtual void EndShoot()
    {
        OnGrappling = false;
        // 선을 지움
        lineRenderer.positionCount = 0;
        rigid.AddForce(Vector3.up * modelSpeed, ForceMode.Force); 
        Destroy(springJoint);
    }

    protected virtual void DrawRope()
    {
        if (OnGrappling)
        {
            lineRenderer.SetPosition(0, this.transform.position);
            // 라인을 플레이어 기준 어느 방향으로 그리든
            // 플레이어는 선이 그려진 방향을 바라봄.
            this.transform.LookAt(spot);
        }
    }

    protected virtual void PullRope()
    {
        if (Physics.Raycast(modelCameraArm.transform.position, modelCameraArm.transform.forward, out raycastHit, 50f, LayerMask.GetMask("Building")))
        {
            spot = raycastHit.point;
            spot.y = transform.position.y;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, this.transform.position);
            lineRenderer.SetPosition(1, raycastHit.point);

            OnPulling = true;

            viewRotation = Quaternion.LookRotation(spot - transform.position);
            transform.rotation = viewRotation;

            float distance = Vector3.Distance(this.transform.position, Vector3.forward);

            rigid.AddForce(modelCameraArm.transform.forward * distance, ForceMode.VelocityChange);

        }

    }

    protected virtual void EndPullRope()
    {
        lineRenderer.positionCount = 0;
        Destroy(springJoint);
    }
}
