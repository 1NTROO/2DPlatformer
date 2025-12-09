using UnityEngine;

namespace Platformer.Mechanics
{
    public class PlayerCollision : MonoBehaviour
    {
        private Rigidbody2D rb;
        private Collider2D coll;
        void Start()
        {
            
        }

        void Update()
        {
            
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            print("collision detected");
            if (collision.gameObject.CompareTag("DNI"))
            {
                collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(2, 7), Random.Range(3, 11)), ForceMode2D.Impulse);
                print("DNI hit");
            }
        }
    }

}
