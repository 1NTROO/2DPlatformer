using System.Runtime.InteropServices;
using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// </summary>
    public class PlayerSpawn : Simulation.Event<PlayerSpawn>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        public override void Execute()
        {
            var player = model.player;

            player.collider2d.enabled = true;
            player.controlEnabled = false;
            if (player.audioSource && player.respawnAudio)
                AudioManager.Instance.PlayAudio(player.respawnAudio, default);
            // player.health.Increment();
            player.Teleport(model.spawnPoint);
            player.GravityDirection = model.gravityAtSpawn;
            player.jumpState = PlayerController.JumpState.Grounded;
            // player.animator.SetBool("dead", false);
            model.virtualCamera.m_Follow = player.transform;
            model.virtualCamera.m_LookAt = player.transform;
            Simulation.Schedule<EnablePlayerInput>(1f);
        }
    }
}