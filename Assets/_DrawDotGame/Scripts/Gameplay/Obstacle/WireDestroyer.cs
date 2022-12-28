using UnityEngine;
using System.Collections;

namespace DrawDotGame
{
    public class WireDestroyer : MonoBehaviour
    {
        private bool isScaled;
        void OnCollisionEnter2D(Collision2D col)
        {
            if ((col.gameObject.CompareTag("Ball") && !isScaled))
            {
                isScaled = true;
                WireController[] wireControllers = FindObjectsOfType<WireController>();
                foreach (WireController o in wireControllers)
                {
                    Destroy(o.gameObject);
                }
                StartCoroutine(ScaleDown());
            }
        }

        IEnumerator ScaleDown()
        {
            Vector2 startScale = transform.localScale;
            Vector2 endScale = new Vector2(startScale.x, 0);
            float t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float fraction = t / 0.5f;
                transform.localScale = Vector2.Lerp(startScale, endScale, fraction);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
