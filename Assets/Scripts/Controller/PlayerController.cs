using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody rigid;

    public float jumpPower = 10f;
    public float turnSpeed = 3f; // 마우스 회전 속도
    public float moveSpeed = 5f; // 이동 속도
    float xRotate = 0f; // 내부 사용 할 X축 회전량은 별도 정의(카메라 위 아래 방향)
    float move_X;
    float move_Z;
    float rotateSpeed = 30f;
    
    bool isGround;
    
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        PlayerMovementBasedOnCameraRotation();
        // Move();
        Jump();
    }

    protected virtual void Move()
    {
        move_Z = Input.GetAxis("Vertical");
        move_X = Input.GetAxis("Horizontal");

        Vector3 movementInput = new Vector3(move_X, 0f, move_Z);

        transform.position += movementInput * moveSpeed * Time.deltaTime;

        /*
        // 바라보는 방향으로 회전 후 다시 정면을 바라보는 현상을 막기위해
        if (!(move_X == 0 && move_Z == 0))
        {
            // transform.Translate(movementInput * 0.02f); // 이동 움직이고 회전하고 움직이고 회전함, 이동때문에 회전하면 안됨
            // transform.position += movementInput * moveSpeed * Time.deltaTime; // 이동량을 죄표에 반영
            //                         부드럽게 회전하기 위해
            //                                V
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movementInput), Time.deltaTime * rotateSpeed); // 회전
        }
        */

    }


    // 이동은 본인이 아닌 카메라 회전값을 기준으로 이동
    protected virtual void PlayerMovementBasedOnCameraRotation()
    {
        //               좌우로 움직인 마우스의 이동량 * 속도에 따라 카메라가 좌우로 회전할 양 계산
        float yRotateSize = Input.GetAxis("Mouse X") * turnSpeed;
        // 현재 y축 회전값에 더한 새로운 회전각도 계산
        float yRotate = transform.eulerAngles.y + yRotateSize;

        //              위아래로 움직인 마우스의 이동량 * 속도에 따라 카메라가 회전할 양 계산(하늘, 바닥을 바라보는 동작) 
        float xRotateSize = -Input.GetAxis("Mouse Y") * turnSpeed;
        // 위아래 회전량을 더해주지만 -45도~80도로 제한(-45 : 하늘방향, 80 : 바닥방향)
        // Clamp는 값의 범위를 제한하는 함수
        xRotate = Mathf.Clamp(xRotate * xRotateSize, -45, 80);

        // 카메라 회전량을 카메라에 반영(X, Y축만 회전)
        transform.eulerAngles = new Vector3(xRotate, yRotate, 0);

        // 키보드에 따른 이동량 측정
        Vector3 playerMovementCameraRotation = (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));

        // 이동량을 죄표에 반영
        transform.position += playerMovementCameraRotation * moveSpeed * Time.deltaTime;
    }

    protected virtual void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            isGround = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Untagged")
        {
            isGround = true;
        }
    }


}
