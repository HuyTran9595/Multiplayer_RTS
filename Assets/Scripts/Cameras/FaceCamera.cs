using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Cameras
{
    public class FaceCamera : MonoBehaviour
    {
        Transform mainCameraTransform;

        private void Start()
        {
            mainCameraTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            //if camera moves, we rotate
            transform.LookAt(
                transform.position + mainCameraTransform.rotation * Vector3.forward, //forwards
                mainCameraTransform.rotation * Vector3.up); //and up
        }

    }
}