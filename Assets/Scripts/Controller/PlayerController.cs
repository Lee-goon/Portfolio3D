using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody rigid;

    public float jumpPower = 10f;
    public float turnSpeed = 3f; // ���콺 ȸ�� �ӵ�
    public float moveSpeed = 5f; // �̵� �ӵ�
    float xRotate = 0f; // ���� ��� �� X�� ȸ������ ���� ����(ī�޶� �� �Ʒ� ����)
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
        // �ٶ󺸴� �������� ȸ�� �� �ٽ� ������ �ٶ󺸴� ������ ��������
        if (!(move_X == 0 && move_Z == 0))
        {
            // transform.Translate(movementInput * 0.02f); // �̵� �����̰� ȸ���ϰ� �����̰� ȸ����, �̵������� ȸ���ϸ� �ȵ�
            // transform.position += movementInput * moveSpeed * Time.deltaTime; // �̵����� ��ǥ�� �ݿ�
            //                         �ε巴�� ȸ���ϱ� ����
            //                                V
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movementInput), Time.deltaTime * rotateSpeed); // ȸ��
        }
        */

    }


    // �̵��� ������ �ƴ� ī�޶� ȸ������ �������� �̵�
    protected virtual void PlayerMovementBasedOnCameraRotation()
    {
        //               �¿�� ������ ���콺�� �̵��� * �ӵ��� ���� ī�޶� �¿�� ȸ���� �� ���
        float yRotateSize = Input.GetAxis("Mouse X") * turnSpeed;
        // ���� y�� ȸ������ ���� ���ο� ȸ������ ���
        float yRotate = transform.eulerAngles.y + yRotateSize;

        //              ���Ʒ��� ������ ���콺�� �̵��� * �ӵ��� ���� ī�޶� ȸ���� �� ���(�ϴ�, �ٴ��� �ٶ󺸴� ����) 
        float xRotateSize = -Input.GetAxis("Mouse Y") * turnSpeed;
        // ���Ʒ� ȸ������ ���������� -45��~80���� ����(-45 : �ϴù���, 80 : �ٴڹ���)
        // Clamp�� ���� ������ �����ϴ� �Լ�
        xRotate = Mathf.Clamp(xRotate * xRotateSize, -45, 80);

        // ī�޶� ȸ������ ī�޶� �ݿ�(X, Y�ุ ȸ��)
        transform.eulerAngles = new Vector3(xRotate, yRotate, 0);

        // Ű���忡 ���� �̵��� ����
        Vector3 playerMovementCameraRotation = (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));

        // �̵����� ��ǥ�� �ݿ�
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
