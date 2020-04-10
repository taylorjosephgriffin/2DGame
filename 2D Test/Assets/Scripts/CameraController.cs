using UnityEngine;
using System.Collections;
using System;

// Add this script to the camera you want to follow the player
namespace CameraController
{
    public class CameraController : MonoBehaviour
    {
        private Transform player;
        public float constantYposition;
        public float damping = 0.5f;
        public bool YMinEnabled = false;
        public float YMinValue = 0;
        public bool YMaxEnabled = false;
        public float YMaxValue = 0;
        public bool XMinEnabled = false;
        public float XMinValue = 0;
        public bool XMaxEnabled = false;
        public float XMaxValue = 0;

        private Vector3 lastTargetPosition;
        private Vector3 currentVelocity;


        // Use this for initialization
        void Start()
        {

            player = GameObject.FindWithTag("Player").transform;

            // Save player position as last player position
            lastTargetPosition = player.position;

            // This will prevent the camera from following a parent - in case you added the camera as somethings child.
            // (which makes the camera follow whatever parent)
            transform.parent = null;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Vector3 targetPos = player.position;

            if (YMinEnabled && YMaxEnabled)
            {
                targetPos.y = Mathf.Clamp(player.position.y, YMinValue, YMaxValue);
            }

            if (XMinEnabled && XMaxEnabled)
            {
                targetPos.x = Mathf.Clamp(player.position.x, XMinValue, XMaxValue);
            }
        
            targetPos.z = transform.position.z;

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, damping);

        }
    }   
}