using UnityEngine;
using System.Collections;

namespace DrawDotGame
{
    public class BallController : MonoBehaviour
    {
        [SerializeField] private Color particleColor;
        private GameManager gameManager;
        private Rigidbody2D rigid2D;

        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            rigid2D = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!gameManager.gameOver)
            {
                float x = Camera.main.WorldToViewportPoint(transform.position).x;
                float y = Camera.main.WorldToViewportPoint(transform.position).y;

                if (x < -0.015f || x > 1.015f || y < -0.015f)
                {
                    ParticleSystem particle = Instantiate(gameManager.explores, transform.position, Quaternion.identity) as ParticleSystem;
#if UNITY_5_5_OR_NEWER
                    var main = particle.main;
                    main.startColor = particleColor;
                    particle.Play();
                    Destroy(particle.gameObject, main.startLifetimeMultiplier);
#else
                particle.startColor = particleColor;
                particle.Play();
                Destroy(particle.gameObject, particle.startLifetime);
#endif
                    gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    rigid2D.isKinematic = true;
                    gameManager.GameOver();
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Obstacle"))
            {
                return;
            }

            if (!gameManager.gameOver)
            {
                if (LayerMask.NameToLayer("Dead") == other.gameObject.layer)
                {
                    ParticleSystem particle = Instantiate(gameManager.explores, transform.position, Quaternion.identity) as ParticleSystem;
#if UNITY_5_5_OR_NEWER
                    var main = particle.main;
                    main.startColor = particleColor;
                    particle.Play();
                    Destroy(particle.gameObject, main.startLifetimeMultiplier);
#else
                particle.startColor = particleColor;
                particle.Play();
                Destroy(particle.gameObject, particle.startLifetime);
#endif
                    gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    rigid2D.isKinematic = true;
                    gameManager.GameOver();
                }
            }
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (!gameManager.win && !gameManager.gameOver)
            {
                if (col.gameObject.CompareTag("Ball"))
                {
                    gameManager.Win();

                    Vector3 thisPos = this.transform.position;
                    Vector3 thatPos = col.transform.position;
                    Vector3 midPoint = thisPos + (thatPos - thisPos) / 2;

                    ParticleSystem particle = Instantiate(gameManager.winning, midPoint, Quaternion.identity) as ParticleSystem;
                    particle.Play();
#if UNITY_5_5_OR_NEWER
                    Destroy(particle.gameObject, particle.main.startLifetimeMultiplier);
#else
                Destroy(particle.gameObject, particle.startLifetime);
#endif

                }
                else if (col.gameObject.CompareTag("Obstacle"))
                {
                    if (!gameManager.gameOver)
                    {
                        if (LayerMask.NameToLayer("Dead") == col.gameObject.layer)
                        {
                            ParticleSystem particle = Instantiate(gameManager.explores, transform.position, Quaternion.identity) as ParticleSystem;
#if UNITY_5_5_OR_NEWER
                            var main = particle.main;
                            main.startColor = particleColor;
                            particle.Play();
                            Destroy(particle.gameObject, main.startLifetimeMultiplier);
#else
                        particle.startColor = particleColor;
                        particle.Play();
                        Destroy(particle.gameObject, particle.startLifetime);
#endif
                            gameObject.GetComponent<SpriteRenderer>().enabled = false;
                            rigid2D.isKinematic = true;
                            gameManager.GameOver();
                        }
                    }
                }
            }
        }
    }
}
