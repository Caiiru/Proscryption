
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

namespace proscryption
{
    public class MinionController : MonoBehaviour
    {
        public EnemyState currentState;
        private EnemyEntity _enemyEntity;
        #region Vision
        [Space]
        [Header("Vision")]
        [SerializeField] private float _lineOfSight = 48f;
        [SerializeField] private LayerMask _playerMask = 1 << 3;

        #endregion
        #region Movement
        [SerializeField] private float _moveSpeed = 2;
        [SerializeField] private float _turnRate = 2;
        private NavMeshAgent _navMeshAgent;

        #endregion
        #region Attack State
        [Space]
        [Header("Attack")]
        [SerializeField] bool _isAttacking;
        [SerializeField] float _attackRange = 2f;
        [SerializeField] int _minAttackDamage = 3;
        [SerializeField] int _maxAttackDamage = 6;
        [Range(0, 100)]
        [SerializeField] float _critChance = 10;


        [SerializeField] Transform _playerTransform;
        [SerializeField] BoxCollider[] _clawsHitBox;


        #endregion

        //References 

        Rigidbody _rigidbody;
        Transform _transform;

        #region Animation
        Animator _animator;
        private const string ANIM_SPEED = "Speed"; // float
        private const string ANIM_ATTACK = "Attack"; // trigger 
        private const string ANIM_DEATH = "Die"; // trigger 


        #endregion

        private void GetReferences()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
            _transform = this.transform;
            _enemyEntity = GetComponent<EnemyEntity>();
            _navMeshAgent = GetComponent<NavMeshAgent>();

            _playerTransform = GameManager.Instance.GetPlayerObject().transform;
        }
        void OnValidate()
        {
            _transform = this.transform;

        }
        void Start()
        {
            Setup();
        }
        private void Setup()
        {
            GetReferences();
            foreach (BoxCollider claw in _clawsHitBox)
            {
                claw.enabled = false;
                claw.GetComponent<MinionClaw>().Setup(this);
            }
            _navMeshAgent.speed = _moveSpeed;
            _navMeshAgent.angularSpeed = _turnRate;
        }
        void Update()
        {
            if (_enemyEntity.IsDead && currentState != EnemyState.Dead)
            {
                ChangeState(EnemyState.Dead);
                return;
            }
            HandleCurrentState();
        }


        private UniTask EnterCurrentState()
        {
            switch (currentState)
            {
                case EnemyState.Roaming:
                    return UniTask.CompletedTask;
                case EnemyState.Attacking:
                    return UniTask.CompletedTask;
                case EnemyState.Dead:
                    HandleDeath().Forget();
                    return UniTask.CompletedTask;
            }
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
                    Attack().Forget();
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
        private async UniTask Attack()
        {
            if (_isAttacking) await UniTask.CompletedTask;
            if (!CanAttack())
            {
                RotateTowardsPlayer();
            }
            else
            {

                _isAttacking = true;
                _animator.SetTrigger(ANIM_ATTACK);
                await UniTask.Delay(1000);
                float _duration = _animator.GetCurrentAnimatorClipInfo(0).Length;
                await UniTask.Delay(Mathf.FloorToInt(_duration * 2000));
                _isAttacking = false;
            }

        }
        private void Dead()
        {
        }

        #endregion

        private bool SeePlayer()
        {
            Vector3 centerPosition = _transform.position;
            centerPosition.y = 1;
            if (Physics.Linecast(centerPosition, _playerTransform.position, _playerMask))
            {
                return true;
            }

            return false;


        }
        void FixedUpdate()
        {
            if (SeePlayer())
            {

                WalkTowardsPlayer();
            }
        }
        private void WalkTowardsPlayer()
        {
            if (_isAttacking)
            {
                _navMeshAgent.SetDestination(transform.position);
                return;
            }
            ;
            _navMeshAgent.SetDestination(_playerTransform.position);
            // _rigidbody.MovePosition(transform.position + transform.forward * Time.fixedDeltaTime * _moveSpeed);
            _animator.SetFloat(ANIM_SPEED, 0.5f);
            // Debug.Log("Walking forward");
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


        #region Attack State
        private bool CanAttack()
        {
            if (_isAttacking) return false;

            Vector3 _distance = _playerTransform.position - _transform.position;
            float distance = _distance.sqrMagnitude;
            return distance <= _attackRange * _attackRange;
        }

        public void ActivateClawsHitBox()
        {
            if (_clawsHitBox.Length == 0) return;

            foreach (BoxCollider claw in _clawsHitBox)
            {
                claw.enabled = true;
            }
        }
        public int GetAttackDamage()
        {
            return _enemyEntity.GetAttackDamage();
        }
        public bool IsAttackCritical()
        {
            return _enemyEntity.IsCritical();
        }


        public void DesactivateClawsHitBox()
        {
            if (_clawsHitBox.Length == 0) return;

            foreach (BoxCollider claw in _clawsHitBox)
            {
                claw.enabled = false;
            }
        }

        #endregion

        #region Death
        private async UniTask HandleDeath()
        {
            _animator.SetTrigger(ANIM_DEATH);
            _navMeshAgent.isStopped = true;
            await UniTask.Delay(1000);
            float _duration = _animator.GetCurrentAnimatorClipInfo(0).Length;
            await UniTask.Delay(Mathf.FloorToInt(_duration * 2000));

            Destroy(this.gameObject);

        }

        #endregion

        void OnDrawGizmosSelected()
        {

            if (_playerTransform == null) return;
            if (!_navMeshAgent) return;


            Vector3 _centerPosition = _transform.position;
            _centerPosition.y = 0.5f;
            if (Vector3.Distance(_transform.position, _playerTransform.position) > _attackRange)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;

            }
            Gizmos.DrawLine(_centerPosition, _centerPosition + _transform.forward * _attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_centerPosition, _navMeshAgent.stoppingDistance);
        }
    }


    public enum EnemyState
    {
        Roaming,
        Attacking,
        Dead,
    }
}
