using UnityEngine;
using System.Collections;

namespace DrawDotGame
{
    public class WireController : MonoBehaviour
    {


        private int counter = -1;
        // Use this for initialization
        void Start()
        {
            StartCoroutine(Loop());
        }

        IEnumerator Loop()
        {
            while (true)
            {
                counter = counter * (-1);
                if (counter < 0)
                {
                    transform.GetChild(0).gameObject.SetActive(false);
                    transform.GetChild(1).gameObject.SetActive(true);
                }
                else
                {
                    transform.GetChild(0).gameObject.SetActive(true);
                    transform.GetChild(1).gameObject.SetActive(false);
                }

                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}
