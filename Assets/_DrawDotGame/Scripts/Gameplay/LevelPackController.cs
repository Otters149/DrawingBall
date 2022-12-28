using UnityEngine;
using System.Collections;

namespace DrawDotGame
{
    public class LevelPackController : MonoBehaviour
    {

        public bool isLocked;

        public void HandleLevelPackButton()
        {
            if (!isLocked)
            {
                LevelSelectionUIManager levelSLUI = FindObjectOfType<LevelSelectionUIManager>();
                levelSLUI.HandleLevelPackButton();
                SoundManager.Instance.PlaySound(SoundManager.Instance.button);
                int level = int.Parse(GetComponentInChildren<UnityEngine.UI.Text>().text.Split('-')[0]);
                LevelScroller.levelSnapped = level;
                FindObjectOfType<LevelScroller>().ScrollToSelectedPack();
            }
        }
    }
}
