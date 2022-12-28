using UnityEngine;
using System.Collections.Generic;

namespace DrawDotGame
{
    [System.Serializable]
    public class LevelData
    {
        public int levelNumber;
        public Vector2 pinkBallPosition;
        public Vector2 blueBallPosition;
        public HintData hintData;
        public List<ObstacleData> listObstacleData = new List<ObstacleData>();
    }
}