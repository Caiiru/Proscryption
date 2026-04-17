using Unity.Cinemachine;
using UnityEngine;

namespace proscryption
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private CinemachineStateDrivenCamera stateDrivenCamera;
        [SerializeField] private Transform _target;
        public void Initialize()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    Debug.LogError("CameraController: No camera assigned and no main camera found in the scene.");
                    return;
                }
            }

            if (stateDrivenCamera == null)
            {
                stateDrivenCamera = GetComponentInChildren<CinemachineStateDrivenCamera>();
                if (stateDrivenCamera == null)
                {
                    Debug.LogError("CameraController: No CinemachineStateDrivenCamera component found on the GameObject.");
                    return;
                }
            }

            if (_target == null)
            {
                GameObject playerObject = GameManager.Instance.GetPlayerObject();
                if (playerObject != null)
                {
                    _target = playerObject.transform;
                }
                else
                {
                    Debug.LogError("CameraController: Player object not found for camera target.");
                    return;
                }
            }

        }
        public void SetTarget(Transform target)
        {

            _target = target;
            if (stateDrivenCamera != null)
            {
                stateDrivenCamera.Follow = _target;
                stateDrivenCamera.LookAt = _target;
            }
            _target.TryGetComponent(out Animator targetAnimator);
            if (targetAnimator != null && stateDrivenCamera != null)
            {
                stateDrivenCamera.AnimatedTarget = targetAnimator;
            }
        }

    }
}
