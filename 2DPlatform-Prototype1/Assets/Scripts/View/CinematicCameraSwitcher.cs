using Platformer.Model;
using Platformer.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer.View
{
    public class CinematicCameraSwitcher : MonoBehaviour
    {
        [Header("Cinematic Camera Settings")]
        [SerializeField] private Cinemachine.CinemachineVirtualCamera vcam1;
        [SerializeField] private Cinemachine.CinemachineVirtualCamera vcam2;
        private Animator animator;

        public enum CameraState
        {
            vcam1,
            vcam2
        }

        public CameraState currentCameraState { get; private set; } = CameraState.vcam1;
        public PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            
        }

        void Update()
        {
            
        }

        public void SwitchState()
        {
            switch (currentCameraState)
            {
                case CameraState.vcam1:
                    currentCameraState = CameraState.vcam2;
                    animator.Play("vcam2");
                    model.virtualCamera = vcam2;
                    break;
                case CameraState.vcam2:
                    currentCameraState = CameraState.vcam1;
                    animator.Play("vcam1");
                    model.virtualCamera = vcam1;
                    break;
            }
        }
    }
}
