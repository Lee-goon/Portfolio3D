using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float YAxis;
    public float XAxis;

    public Transform target;
    Transform cameraPosition;

    float rotationSensitive = 2f; // 카메라 회전 감도
    float distance = 6f; // 카메라와 플레이어 사이의 거리
    float rotationMin = -50f; // 카메라 회전각도 최소
    float rotationMax = 80f; // 카메라 회전각도 최대
    float smoothTime = 0.12f; // 카메라 회전하는데 걸리는 시간
    // 위 5개는 value는 개발자의 취향껏 조정

    Vector3 targetRotation;
    Vector3 currentVel;

    void Start()
    {
        cameraPosition = Camera.main.transform;
        // 마우스의 커서를 윈도우 정중앙에 고정시킨 후 보이지 않게 해주는 코드
        // FPS에 유용(커서 고정)
        Cursor.lockState = CursorLockMode.Locked; 
        // 마우스 커서가 표시가 되지않게 하는 명령어(빌드를 해야 확인가능)
        Cursor.visible = false;
    }

    private void LateUpdate() // player가 움직이고 그 후 카메라가 따라가야 함
    {
        CameraTurnSmooth();
    }

    protected virtual void CameraTurnSmooth()
    {
        YAxis = YAxis + Input.GetAxis("Mouse X") * rotationSensitive; // 마우스 죄우움직임을 입력받아서 카메라의 Y축을 회전
        XAxis = XAxis - Input.GetAxis("Mouse Y") * rotationSensitive; // 마우스 상하움직임을 입력받아서 카메라의 X축을 회전
        // XAxis는 마우스를 아래로 했을때(음수값이 입력 받아질때) 값이 더해져야 카메라가 아래로 회전

        // X축 회전에 한계치를 남지않게 제한
        // 카메라가 360도로 도는걸 막기위해
        XAxis = Mathf.Clamp(XAxis, rotationMin, rotationMax);

        targetRotation = Vector3.SmoothDamp(targetRotation, new Vector3(XAxis, YAxis), ref currentVel, smoothTime);
        this.transform.eulerAngles = targetRotation; // smoothDamp를 통해 카메라 회전을 부드럽게 함

        // 카메라의 위치는 플레이어보다 설정한 값만큼 떨어져있게 계속 변경        카메라가 살짝 위로 위치하게 변경
        transform.position = target.position - (transform.forward * distance) + (cameraPosition.up * 2);

    }


}
