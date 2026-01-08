using UnityEngine;
using Platformer.Core;
using Platformer.Model;


namespace Platformer.Mechanics
{
    /// <summary>
    /// HealthPickup components mark a collider which will increment
    /// the player's health when the player enters the trigger.
    /// </summary>
    public class HealthPickup : MonoBehaviour
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        [SerializeField] private AudioClip pickupAudio;
        void Start()
        {
            
        }

        void Update()
        {
            
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            var player = collision.gameObject.GetComponent<PlayerController>();
            Debug.Log("Collision detected with object : " + collision.gameObject.name);
            if (player != null)
            {
                player.health.Increment();
                model.spawnPoint = transform.position;
                // model.gravityAtSpawn = player.GravityDirection;
                Debug.Log("Health pickup collected. New spawn point set.");
                AudioManager.Instance.PlayAudio(pickupAudio, transform.position);
                Destroy(gameObject);
            }
        }
    }
}