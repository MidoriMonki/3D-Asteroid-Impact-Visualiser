using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class CameraBehaviour : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float wasdSpeed = 10.0f;
    [SerializeField] private float panSpeed = 175.0f;
    [SerializeField] private float rotationSpeed = 10.0f;
    [SerializeField] private float zoomSpeed = 300.0f;

    [Header("Focus Object")]
    [SerializeField] private float focusLimit = 100f;
    private float doubleClickTime = .25f;
    private float cooldown = 0;

    [SerializeField] private KeyCode frontKey = KeyCode.W;
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    [SerializeField] private KeyCode backKey = KeyCode.S;
    [SerializeField] private KeyCode anchoredMoveKey = KeyCode.Mouse2;
    [SerializeField] private KeyCode anchoredRotateKey = KeyCode.Mouse1;


    // Update is called once per frame
    void Update()
    {
        if (cooldown > 0 && Input.GetKeyDown(KeyCode.Mouse0))
            FocusObject();
        if (Input.GetKeyDown(KeyCode.Mouse0))
            cooldown = doubleClickTime;
        cooldown -= Time.deltaTime;
    }

    private void FocusObject()
    {
        //if looking at obj in scene, goto obj pos
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, focusLimit))
        {
            GameObject target = hit.collider.gameObject;
            Vector3 targetPos = target.transform.position;
            Vector3 targetSize = hit.collider.bounds.size;

            transform.position = targetPos;
        }
    }

    private void FixedUpdate()
    {
        Vector3 move = Vector3.zero;
        //move and rotate

        //move the camera with forward, left, right, back
        if (Input.GetKey(frontKey))
            move += Vector3.up * wasdSpeed;
        if (Input.GetKey(backKey))
            move -= Vector3.up * wasdSpeed;
        if (Input.GetKey(leftKey))
            move -= Vector3.right * wasdSpeed;
        if (Input.GetKey(rightKey))
            move += Vector3.right * wasdSpeed;

        //rotate when anchored
        float mouseMoveY = Input.GetAxis("Mouse Y");
        float mouseMoveX = Input.GetAxis("Mouse X");

        if (Input.GetKey(anchoredRotateKey))
        {
            transform.RotateAround(transform.position, transform.right, mouseMoveY * rotationSpeed);
            transform.RotateAround(transform.position, Vector3.up, mouseMoveX * -rotationSpeed);
        }

        //move when anchored
        if (Input.GetKey(anchoredMoveKey))
        {
            move -= Vector3.up * mouseMoveY * panSpeed;
            move -= Vector3.right * mouseMoveX * panSpeed;
        }


        transform.Translate(move);
    }

    private void LateUpdate()
    {
        //scroll to zoom
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.forward * mouseScroll * zoomSpeed);
    }
}
