using UnityEngine;
using System.Collections;
using System;

// Add this script to the camera you want to follow the player
namespace CameraController
{
  public class CameraController : MonoBehaviour
  {
    private Transform playerTransform;
    public float damping = 0.5f;
    private Vector3 lastTargetPosition;
    private Vector3 currentVelocity;
    //public float minXPosition = -10.0f; // left border
    //public float maxXPosition = 10.0f; //  right border
    //public float minYPosition = -10.0f; // left border
    //public float maxYPosition = 10.0f; //  right border


    Transform cursorTransform;

    Transform cameraTransform;

    private void Awake()
    {
      cameraTransform = GetComponent<Transform>();
      cursorTransform = GameObject.Find("MouseCursor").transform;
      playerTransform = GameObject.FindWithTag("Player").transform;
      lastTargetPosition = playerTransform.position;
      transform.parent = null;
    }

    // Update is called once per frame
    void LateUpdate()
    {
      Vector3 targetPos = (cursorTransform.position + playerTransform.position) / 2;

      targetPos.z = cameraTransform.position.z;


      cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, targetPos, ref currentVelocity, damping);

      //cameraTransform.position = new Vector3(Mathf.Clamp(transform.position.x, minXPosition, maxXPosition), Mathf.Clamp(transform.position.y, minYPosition, maxYPosition), -10);

    }
  }
}