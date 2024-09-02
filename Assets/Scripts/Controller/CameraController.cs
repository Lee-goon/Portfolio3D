using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float YAxis;
    public float XAxis;

    public Transform target;
    Transform cameraPosition;

    float rotationSensitive = 2f; // ī�޶� ȸ�� ����
    float distance = 6f; // ī�޶�� �÷��̾� ������ �Ÿ�
    float rotationMin = -50f; // ī�޶� ȸ������ �ּ�
    float rotationMax = 80f; // ī�޶� ȸ������ �ִ�
    float smoothTime = 0.12f; // ī�޶� ȸ���ϴµ� �ɸ��� �ð�
    // �� 5���� value�� �������� ���ⲯ ����

    Vector3 targetRotation;
    Vector3 currentVel;

    void Start()
    {
        cameraPosition = Camera.main.transform;
        // ���콺�� Ŀ���� ������ ���߾ӿ� ������Ų �� ������ �ʰ� ���ִ� �ڵ�
        // FPS�� ����(Ŀ�� ����)
        Cursor.lockState = CursorLockMode.Locked; 
        // ���콺 Ŀ���� ǥ�ð� �����ʰ� �ϴ� ��ɾ�(���带 �ؾ� Ȯ�ΰ���)
        Cursor.visible = false;
    }

    private void LateUpdate() // player�� �����̰� �� �� ī�޶� ���󰡾� ��
    {
        CameraTurnSmooth();
    }

    protected virtual void CameraTurnSmooth()
    {
        YAxis = YAxis + Input.GetAxis("Mouse X") * rotationSensitive; // ���콺 �˿�������� �Է¹޾Ƽ� ī�޶��� Y���� ȸ��
        XAxis = XAxis - Input.GetAxis("Mouse Y") * rotationSensitive; // ���콺 ���Ͽ������� �Է¹޾Ƽ� ī�޶��� X���� ȸ��
        // XAxis�� ���콺�� �Ʒ��� ������(�������� �Է� �޾�����) ���� �������� ī�޶� �Ʒ��� ȸ��

        // X�� ȸ���� �Ѱ�ġ�� �����ʰ� ����
        // ī�޶� 360���� ���°� ��������
        XAxis = Mathf.Clamp(XAxis, rotationMin, rotationMax);

        targetRotation = Vector3.SmoothDamp(targetRotation, new Vector3(XAxis, YAxis), ref currentVel, smoothTime);
        this.transform.eulerAngles = targetRotation; // smoothDamp�� ���� ī�޶� ȸ���� �ε巴�� ��

        // ī�޶��� ��ġ�� �÷��̾�� ������ ����ŭ �������ְ� ��� ����        ī�޶� ��¦ ���� ��ġ�ϰ� ����
        transform.position = target.position - (transform.forward * distance) + (cameraPosition.up * 2);

    }


}
