using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public Camera mainCamera;
    public float minZoom;
    public float maxZoom;
    public float zoomSpeed = 0.05f;
    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - player.transform.position;
        mainCamera = GetComponent<Camera>();
    }

    void Update()
    {

        if (Input.touchCount == 2)
        {
            
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);
            Vector2 touchZeroOriginalPosition = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOneOriginalPosition = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroOriginalPosition - touchOneOriginalPosition).magnitude;
            float currMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currMagnitude - prevMagnitude;

            Zoom(difference * zoomSpeed);
        }
    }

    void Zoom(float increment)
    {
        mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView - increment, minZoom, maxZoom);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //transform.position = player.transform.position + offset;
        transform.SetPositionAndRotation(player.transform.position + offset, transform.rotation);
    }
}
