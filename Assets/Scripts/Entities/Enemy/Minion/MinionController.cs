
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace proscryption
{
    public class MinionController : MonoBehaviour
    {
        public EnemyState currentState;
        #region Vision
        [Space]
        [Header("Vision")]
        [SerializeField] private float _lineOfSight = 4f;
        [SerializeField] private LayerMask _playerMask = 1 << 3;

        #endregion
        #region Movement
        [SerializeField] private float _moveSpeed = 2;
        [SerializeField] private float _turnRate = 2;

        #endregion
        #region Attack State
        [Space]
        [Header("Attack")]
        [SerializeField] bool _isAttacking;
        [SerializeField] float _attackRange = 2f;

        [SerializeField] Transform _playerTransform;


        #endregion
        #region Animation
        Animator _animator;
        private const string ANIM_SPEED = "Speed"; // float
        private const string ANIM_ATTACK = "Attack"; // trigger
        private const string ANIM_DIE = "MinionDied"; //trigger 


        #endregion
        //References
        public CharacterController _characterController;

        Rigidbody _rigidbody;
        Transform _transform;

        private void GetReferences()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
            _transform = this.transform;
        }
        void OnValidate()
        {
            _transform = this.transform;

        }
        void OnEnable()
        {
            Setup();
        }
        private void Setup()
        {
            GetReferences();
        }
        void Update()
        {
            HandleCurrentState();
        }


        private UniTask EnterCurrentState()
        {

            return UniTask.CompletedTask;
        }
        private UniTask LeaveCurrentState()
        {


            return UniTask.CompletedTask;
        }
        private void HandleCurrentState()
        {
            switch (currentState)
            {
                case EnemyState.Roaming:
                    Roam();
                    break;
                case EnemyState.Attacking:
                    Attack();
                    break;
                case EnemyState.Dead:
                    Dead();
                    break;
            }
        }
        private async void ChangeState(EnemyState newState)
        {
            await LeaveCurrentState();
            currentState = newState;
            await EnterCurrentState();
        }

        #region States

        private void Roam()
        {
            if (SeePlayer())
            {
                ChangeState(EnemyState.Attacking);
                return;

            }
            // _characterController.SimpleMove(Vector3.forward);
        }
        private UniTask Attack()
        {
            RotateTowardsPlayer();
            WalkTowardsPlayer();
            if (CanAttack())
            {
                _isAttacking = true;
                _animator.SetTrigger(ANIM_ATTACK);
            }
            UniTask.Delay(100);
            float _duration = _animator.GetCurrentAnimatorClipInfo(0).Length;
            UniTask.Delay(Mathf.FloorToInt(_duration * 1000));
            _isAttacking = false;
            return UniTask.CompletedTask;

        }
        private void Dead()
        {
        }

        #endregion

        private bool SeePlayer()
        {
            Vector3 centerPosition = _transform.position;
            centerPosition.y = 1;
            if (Physics.Raycast(centerPosition, transform.forward, out RaycastHit hitInfo, _lineOfSight, _playerMask))
            {
                _playerTransform = hitInfo.transform;
                return true;
            }
            return false;


        }
        private bool CanAttack()
        {
            if (_isAttacking) return false;

            Vector3 _distance = _playerTransform.position - _transform.position;
            float distance = _distance.sqrMagnitude;
            return distance <= _attackRange * _attackRange;
        }
        private void WalkTowardsPlayer()
        {
            if (_isAttacking) return;
            _rigidbody.linearVelocity = transform.forward * _moveSpeed * Time.deltaTime;
        }
        private void RotateTowardsPlayer()
        {
            // Vector3 _distance = _playerTransform.position - _transform.position;
            Vector3 _distance = _transform.InverseTransformPoint(_playerTransform.position);
            float angle = Mathf.Atan2(_distance.x, _distance.z) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0f, _transform.eulerAngles.y + angle, 0f);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, Time.deltaTime * _turnRate);

            // _transform.Rotate(rotation.eulerAngles);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = SeePlayer() ? Color.green : Color.red;
            Vector3 _gizmosOrigin = _transform.position;
            _gizmosOrigin.y = 0.5f;
            Gizmos.DrawRay(_gizmosOrigin, transform.forward * _lineOfSight);

        }
    }


    public enum EnemyState
    {
        Roaming,
        Attacking,
        Dead,
    }
}
