using UnityEngine;
using System.Collections;

namespace DrawDotGame
{
    [System.Serializable]
    public enum RotateDirection
    {
        Right,
        Left
    }

    public class ConveyorController : MonoBehaviour
    {
        public float rotateSpeed;
        public RotateDirection rotateDirection;

        private Vector2 direction;
        private int dir;

        // Use this for initialization
        void Start()
        {
            if (rotateDirection == RotateDirection.Right)
            {
                dir = -1;
                direction = Vector2.right;
            }
            else
            {
                dir = 1;
                direction = Vector2.left;
            }
            Run(dir);
        }

        void Run(int dir)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                StartCoroutine(RotateObject(transform.GetChild(i).gameObject, dir));
            }
        }

        IEnumerator RotateObject(GameObject ob, int dir)
        {
            while (true)
            {
                ob.transform.Rotate(0, 0, dir * rotateSpeed * Time.deltaTime);
                yield return null;
            }
        }

        void OnCollisionStay2D(Collision2D col)
        {
            AddForceForObject(col.rigidbody);
        }

        void AddForceForObject(Rigidbody2D objectRigid)
        {
            float curVelocity = objectRigid.velocity.x;
            float desiredVelocity = rotateSpeed / 50f;
            objectRigid.AddForce(direction * (desiredVelocity - curVelocity) * objectRigid.mass);
        }
    }
}
