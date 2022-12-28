using UnityEngine;
namespace DrawDotGame
{
    [System.Serializable]
    public class ObstacleData
    {
        public string id;
        public Vector2 position;
        public Quaternion rotation;
        public Vector2 scale;

        public RotateDirection rotateDirection;
        public float rotatingSpeed;
    }
}
