using UnityEngine;

namespace DrawDotGame
{
    public class LevelButtonController : MonoBehaviour
    {

        public bool isLocked;

        public void HandleOnClick()
        {
            if (!isLocked)
            {
                GameManager.LevelLoaded = int.Parse(GetComponentInChildren<UnityEngine.UI.Text>().text);
                DrawDotGame.SoundManager.Instance.PlaySound(DrawDotGame.SoundManager.Instance.button);
                LevelScroller.levelSnapped = (int)((Mathf.Ceil(GameManager.LevelLoaded / (float)LevelScroller.LEVELS_PER_PACK) - 1) * LevelScroller.LEVELS_PER_PACK + 1);
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            }
        }
    }
}
