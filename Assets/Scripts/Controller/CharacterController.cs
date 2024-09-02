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

        // ī�޶� ���� �������� ���� ���콺 �̵����� ���̷� ī�޶���� ���Ϸ� �󸶳� ȸ���ؾ� �ϴ��� �����ϴ� ����
        // x���� y�� ���� ������ ī�޶� ���� Player������Ʈ�� �پ������� ���� ������ x���̱� ����
        float difference = camAngle.x - mouseDelta.y;

        // Clamp�� ���� �����ϱ� ���� ���
        // ������ 0�� �̱� ������ �̸� ������ ���� 180���� ���� ���� ū ��쿡 ���� ������ ����
        if(difference < 180f)
        {
            // ī�޶� ���������� ���� ���
            difference = Mathf.Clamp(difference, -10f, 60f);
        }
        else
        {
            difference = Mathf.Clamp(difference, 320f, 361f);
        }

        // ī�޶� ���� ȸ������ ���������� ���� x, y���� ������ ����, z���� ���Ծ���.
        modelCameraArm.rotation = Quaternion.Euler(difference, camAngle.y + mouseDelta.x, camAngle.z);

    }

    protected virtual void Move()
    {
        // Update()���� ��� �� modelMoveInput�� ũ�⸦ �����Ͽ� 0�� �ƴ� ���(�Է��� �ִ� ���) ���� ó���� �̾ ��.
        // �Է��� ���� �� ���ʿ��� ����� ���̱� ���� ���
        bool isMove = mMoveInput.magnitude != 0;
        

        if(isMove)
        {
            // ī�޶���� ���� ����� ���� ���⿡ ���� ������ �����ϴ� ����
            // �÷��̾� ĳ������ �� ������ �ƴ� ī�޶� �ٶ󺸰� �ִ� ������ �������� ȸ���ϱ� ������ ���
            Vector3 lookForward = new Vector3(modelCameraArm.forward.x, 0f, modelCameraArm.forward.z).normalized;
            Vector3 lookRight   = new Vector3(modelCameraArm.right.x, 0f, modelCameraArm.right.z).normalized;

            // ���� ���꿡 ���� ī�޶� ���� ������� 'y�� �̵��Է� + ī�޶� ���� �������' x�� �̵��Է��� ���Ͽ� ������ ����.
            Vector3 moveDirection;

            if (isAttach)
            {
                //                                          wallRight�� -�� ���� ����
                //                                          => �÷��̾��� �Է¹���� �����̴� ������ ���� �ϱ� ����
                moveDirection = Vector3.up * mMoveInput.y + (-wallRight) * mMoveInput.x;
            }
            else 
            {
                moveDirection = lookForward * mMoveInput.y + lookRight * mMoveInput.x;

                if (!OnPulling)
                {
                    // �̵������ ������ �÷��̾��� ����� ����ϴ� PlayerModel�� ������ �����̴� �������� ȸ����Ű�� ����
                    // �ٶ󺸴� ������ ����
                    viewRotation = Quaternion.LookRotation(moveDirection.normalized);

                    // ���� ��涧�� �ؿ� ������ �ۿ��� �ȵǰ�
                    //                            ��������     �ε巯�� ��ȯ�� ����                            ������ �ٶ󺸴� ȸ���ӵ�
                    transform.rotation = Quaternion.Lerp(modelPlayerObject.transform.rotation, viewRotation, Time.fixedDeltaTime * 20);
                }

            }
            
            // ���������� ����� �ӵ��� deltaTime�� ���� �÷��̾� ĳ���͸� �̵���Ŵ
            transform.position += moveDirection * modelSpeed * Time.fixedDeltaTime;
        }

        if (OnPulling)
        {
            // viewRotation = Quaternion.LookRotation(spot);

            // ó���� ��ġ�� ����ϰ� ������ ���¿��� ����
            // ȸ������ ��� ���� �� ������, ȸ������ ��� ���� �� ������ 
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
            // Debug.Log("����");

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
        // duration : �׷����� ������ ����, 0���� �� �ٸ� ����.
        // Debug.DrawRay(modelPlayerObject.transform.position, modelPlayerObject.transform.forward * 5f, Color.red);
        if (Physics.Raycast(modelPlayerObject.transform.position, modelPlayerObject.transform.forward, out raycastHit, 10f, LayerMask.GetMask("Building")))
        {
            // ���� ���� �ؾ��ϴ°� �÷��̾ ���� �ٴ°� ���� ��������.
            // �߷��� ���� �Ƚ��� ����Ʈ�� ���� �޶�ٰ� �� ����
            // �߷��� �������� �÷��̾ ���� �ö󰡴°� �����ϱ� ���� ����.
            rigid.velocity = Vector3.zero;
            rigid.useGravity = false;

            // sticktothewall�̸� ���� �پ����.
            // �ٵ� �Ⱥ��� 
            // ���� �ٿ������.(���ٰ� ����)
            // Debug.Log("X");

            // �߷��� ���ּ� �÷��̾ �ǹ��� ���� �� �ְ� ����.
            

            // �÷��̾ �ǹ��� ���ƴٴ� �� �ְ� ����.
            // �÷��̾��� rotation�� x���� 90���� �ٲ�����.
            // RotateAround : �ڽ��� �ִ� ��ġ�� Ư�������� �Ÿ��� ���������� �ϴ� ���� �׸��鼭 ȸ���ϴ� ������
            // ���� �������� �ϴ� ������ ���ÿ� ���� 
            //                  �÷��̾ �������� ȸ���ϰ� ����.
            // transform.RotateAround(modelPlayerObject.transform.position, modelPlayerObject.transform.right, -45f);

            // ����ִ� ������ ������ �˾ƾ���.... 
            // �÷��̾ ���� �ִ� ���� => �÷��̾��� ����, ������
            // �÷��̾ ���� �پ��ִ� ���� => �÷��̾��� �Ʒ���
            // ���� ���� �÷��̾��� ���� : �������� �����Ѱ� x��ǥ, z��ǥ
            // ���� ����
            // Vector3 cross = Vector3.Cross(modelPlayerObject.transform.position, modelCameraArm.transform.position);
            // 
            // if(cross.z > 0)
            // {
            //     Debug.Log("������");
            // }
            // else if(cross.z < 0)
            // {
            //     Debug.Log("����");
            // }
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ���� ���� ����
        // if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Building") { isGround = true; }

        if (collision.gameObject.tag == "Building")
        {
            // �±׿� �ش��ϴ� ������Ʈ�� ������ �÷��̾ ������� 
            // �� ������Ʈ�� ������ �˷���.
            wallForward = collision.GetContact(0).normal;
            
            // ��ǥ�� �������� ����������
            // ��ǥ�� �������� ������ ���͸� ã�ƾ���.
            // ���� �����Ͷ� ���븻���͸� ������ ������ ���͸� ã�ƾ���.
            // (-x, y, z)
            wallRight = new Vector3(wallForward.z, wallForward.y, -wallForward.x);

            angle = Vector3.Angle(-Physics.gravity, wallForward);
            if (angle > 80f && angle < 100f)
            {
                isAttach = true;
                rigid.velocity = Vector3.zero;
                rigid.useGravity = false;
                OnPulling = false;
                // if (OnPulling == false) { Debug.Log("�ٴ�"); }
            }

            // UI�� ���� ��ǥ ǥ��
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

    protected virtual bool CheckGround() // ĳ���Ͱ� �ٴڿ� �ִ��� üũ!
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
            lineRenderer.positionCount = 2; // ���η������� ���� ����
            lineRenderer.SetPosition(0, this.transform.position); // ù���� ���� �÷��̾��� ��ġ�� ����
            lineRenderer.SetPosition(1, raycastHit.point); // �ι�° ���� ����ĳ��Ʈ�� ��ġ�� ����
            
            springJoint = modelPlayerObject.AddComponent<SpringJoint>();
            // autoConfigureConnectedAnchor : ����Ƽ�� ����� ��Ŀ ����Ʈ�� �������� �ڵ����� ����ؾ� �ϴ���
            // ����� ��Ŀ �ڵ����� ��Ȱ��ȭ
            springJoint.autoConfigureConnectedAnchor = false;
            // connectedAnchor : ����� ������Ʈ�� ���� �������� ����Ʈ�� ���� �� ����Ʈ
            // ����� ��Ŀ�� ���������� ����
            springJoint.connectedAnchor = spot;

            // �÷��̾�� ���� �������� �Ÿ� ���
            //                                   �÷��̾��� ��ġ        ����ĳ��Ʈ�� ������ ���� ��
            float distance = Vector3.Distance(this.transform.position, spot);

            // maxDistance : �������� ��� ���� �ۿ���� �ʴ� �ִ�Ÿ� �Ѱ�
            springJoint.maxDistance = distance;
            // minDistance : �������� ��� ���� �ۿ���� �ʴ� �ּҰŸ� �Ѱ�
            springJoint.minDistance = distance * 0.2f;
            // spring : �������� ����
            springJoint.spring = 2f;
            // damper : Ȱ��ȭ �Ǿ� ������ �������� �پ��� ����
            springJoint.damper = 5f;
            // massScale : ������ ���� ����
            springJoint.massScale = 5f;

        }
        
    }

    protected virtual void EndShoot()
    {
        OnGrappling = false;
        // ���� ����
        lineRenderer.positionCount = 0;
        rigid.AddForce(Vector3.up * modelSpeed, ForceMode.Force); 
        Destroy(springJoint);
    }

    protected virtual void DrawRope()
    {
        if (OnGrappling)
        {
            lineRenderer.SetPosition(0, this.transform.position);
            // ������ �÷��̾� ���� ��� �������� �׸���
            // �÷��̾�� ���� �׷��� ������ �ٶ�.
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
