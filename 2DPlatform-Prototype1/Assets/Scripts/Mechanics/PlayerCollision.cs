using UnityEngine;

namespace Platformer.Mechanics
{
    public class PlayerCollision : MonoBehaviour
    {
        [SerializeField] TMPro.TextMeshProUGUI lapsText;
        private float laps = -1;

        private Rigidbody2D rb;
        private Collider2D coll;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            if (lapsText != null)
            {
                lapsText.text = "0 Laps";
            }
        }

        void Update()
        {
            
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("DNI"))
            {
                collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(2, 5), Random.Range(3, 8)), ForceMode2D.Impulse);
            }
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("FinishLine"))
            {
                if (lapsText != null)
                {
                    laps++;
                    lapsText.text = laps.ToString() + " Laps";
                }
            }
        }
    }

}
