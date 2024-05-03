using UnityEngine;

namespace BwmpsTools.OtherScripts
{
    public class CameraRotator : MonoBehaviour
    {
        public GameObject cam;
        public Transform rotateCenter;
        public float distance = 1.5f;
        public float speed = 0.5f;
        void Update()
        {
            Vector3 pos = new Vector3(Mathf.Sin(Time.time * speed), 0f, Mathf.Cos(Time.time * speed)) * distance;
            cam.transform.position = pos + rotateCenter.transform.position;

            cam.transform.LookAt(rotateCenter.transform.position);
        }
    }
}
