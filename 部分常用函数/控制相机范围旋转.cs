using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ceshi : MonoBehaviour
{
    public float angleXLeft=30;
    public float angleXRight=45;
    public float angleYUp=60;
    public float angleYDown=25;
    public float speed=2;

    float angleX;
    float angleY;

    float nowAngleX;
    float nowAngleY;

    private void Start()
    {
        Reset();
    }



    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            angleY += Input.GetAxis("Mouse Y")* speed;
            angleX += Input.GetAxis("Mouse X")* speed;

            angleX = Mathf.Clamp(angleX, -angleXLeft, angleXRight);

            angleY = Mathf.Clamp(angleY, -angleYDown, angleYUp);

            Quaternion targetRotation = Quaternion.Euler(-angleY+ nowAngleX, angleX+ nowAngleY, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * speed);
            Debug.Log("angleX " + angleX + "  " + "angleY " + angleY);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Reset();
            Debug.Log("angleX " + angleX + "  " + "angleY " + angleY);
        }
    }



    private void Reset()
    {
        angleX = 0;
        angleY = 0;
        nowAngleX = transform.localEulerAngles.x;
        nowAngleY = transform.localEulerAngles.y;
    }
}
