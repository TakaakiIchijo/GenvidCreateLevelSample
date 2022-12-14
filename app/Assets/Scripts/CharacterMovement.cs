using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float speed = 3.0F;
    public float rotateSpeed = 3.0F;
    private CharacterController controller;
    
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 左右キー Yを軸として回転
        transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed, 0);
 
        // 上下キー 前後移動
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        float curSpeed = speed * Input.GetAxis("Vertical");
        controller.SimpleMove(forward * curSpeed);
    }
}
