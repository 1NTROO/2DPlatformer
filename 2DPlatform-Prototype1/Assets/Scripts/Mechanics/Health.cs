using System;
using System.Collections;
using Platformer.Gameplay;
using Unity.Tutorials.Core;
using UnityEngine;
using UnityEngine.UI;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Represebts the current vital statistics of some game entity.
    /// </summary>
    public class Health : MonoBehaviour
    {
        /// <summary>
        /// The UI canvas to show on game over.
        /// </summary>
        public GameObject gameOverCanvas;

        public GameObject healthUI;

        /// <summary>
        /// The maximum hit points for the entity.
        /// </summary>
        public int maxHP = 1;

        /// <summary>
        /// The starting hit points for the entity.
        /// </summary>
        public int startingHP = 1;

        /// <summary>
        /// Indicates if the entity should be considered 'alive'.
        /// </summary>
        public bool IsAlive => currentHP > 0;

        int currentHP;

        /// <summary>
        /// Increment the HP of the entity.
        /// </summary>
        public void Increment()
        {
            currentHP = Mathf.Clamp(currentHP + 1, 0, maxHP);
            UpdateUI();
        }

        /// <summary>
        /// Decrement the HP of the entity. Will trigger a HealthIsZero event when
        /// current HP reaches 0.
        /// </summary>
        public void Decrement()
        {
            currentHP = Mathf.Clamp(currentHP - 1, 0, maxHP);
            if (currentHP == 0)
            {
                var ev = Schedule<HealthIsZero>();
                ev.health = this;
            }
            UpdateUI();
        }

        public void UpdateUI()
        {
            for (int i = 0; i < healthUI.transform.childCount; i++)
            {
                var heart = healthUI.transform.GetChild(i).gameObject;
                if (i < currentHP)
                    heart.GetComponent<Image>().color = Color.white;
                else
                    heart.GetComponent<Image>().color = Color.black;
            }
        }

        /// <summary>
        /// Game over logic when health reaches zero.
        /// </summary>
        public void GameOver()
        {
            gameOverCanvas.SetActive(true);
            gameOverCanvas.GetComponentInChildren<Button>().enabled = false;
            SceneObjectGuidManager.Instance.Unregister(gameObject.GetComponent<SceneObjectGuid>());
            StartCoroutine(GameOverButtonDelay());
        }

        private IEnumerator GameOverButtonDelay()
        {
            yield return new WaitForSeconds(1f);

            gameOverCanvas.GetComponentInChildren<Button>().enabled = true;
        }

        void Awake()
        {
            currentHP = startingHP;
            UpdateUI();
        }
    }
}
