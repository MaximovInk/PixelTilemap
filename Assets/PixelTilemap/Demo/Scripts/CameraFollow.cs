using UnityEngine;

namespace MaximovInk.PixelTilemap
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;

        public float FollowSpeed = 2f;

        void FixedUpdate()
        {
            if (target != null)
            {
                //transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);
                Vector3 newPosition = target.position;
                newPosition.z = -10;
                transform.position = Vector3.Slerp(transform.position, newPosition, FollowSpeed * Time.deltaTime);
            }
        }
    }
}