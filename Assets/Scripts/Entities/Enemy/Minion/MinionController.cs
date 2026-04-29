
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [Range(0, 100)]
        [SerializeField] float _critChance = 10;

        [Tooltip("Delay para ele se recuperar e voltar a se mexer")]
        public float _takeDamageDelay = 0.1f;

        [SerializeField] Transform _playerTransform;
        [SerializeField] BoxCollider[] _clawsHitBox;



        //References 
        #endregion
        CapsuleCollider _takeDamageCollider;
        [SerializeField] Collider[] _ragdollColliders;
        [SerializeField] Rigidbody[] _ragdollRigidbodies;

        [SerializeField] SkinnedMeshRenderer _bodyRenderer;
        [SerializeField] SkinnedMeshRenderer _eyesRenderer;

        public Material DissolveMaterial;

        Rigidbody _rigidbody;
        Transform _transform;

        #region Animation
        Animator _animator;
        private const string ANIM_SPEED = "Speed"; // float
        private const string ANIM_ATTACK = "Attack"; // trigger 
        private const string ANIM_DEATH = "Die"; // trigger 
        private const string ANIM_TAKE_DAMAGE = "TakeDamage"; // trigger 


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
        void OnEnable()
        {
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

            _ragdollColliders = _transform.GetComponentsInChildren<Collider>();
            _ragdollRigidbodies = _transform.GetComponentsInChildren<Rigidbody>();
            DisableRagdoll();
            _takeDamageCollider = _transform.GetComponent<CapsuleCollider>();

            _takeDamageCollider.enabled = true;

            SubscribeEvents();

        }
        private void SubscribeEvents()
        {
            _enemyEntity.OnTakeDamage += HandleTakeDamage;
        }


        void OnDestroy()
        {
            _enemyEntity.OnTakeDamage -= HandleTakeDamage;

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
                case EnemyState.TakingDamage:
                    HandleTakeDamageState().Forget();
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
        private async UniTask HandleTakeDamageState()
        {
            _animator.SetTrigger(ANIM_TAKE_DAMAGE);

            await UniTask.Delay(1000);

            ChangeState(EnemyState.Attacking);
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
            if (SeePlayer() && currentState == EnemyState.Attacking)
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
        private async void HandleTakeDamage(Vector3? directionForce, ForceMode? forceMode)
        {
            if (directionForce == null) return;

            if (forceMode == null) return;


            _navMeshAgent.enabled = false;
            ChangeState(EnemyState.TakingDamage);
            _rigidbody.isKinematic = false;
            _rigidbody.AddForce((Vector3)directionForce, (ForceMode)forceMode);
            // _animator.SetFloat(ANIM_SPEED, 0);
            // Debug.Log($"before delay {directionForce}");
            // Debug.DrawRay(_transform.position, _transform.position + (Vector3)directionForce, Color.red, 2f);
            await UniTask.Delay(Mathf.FloorToInt(_takeDamageDelay * 1000));
            // Debug.Log("after delay");

            _rigidbody.isKinematic = true;
            _navMeshAgent.enabled = enabled;
            _animator.SetFloat(ANIM_SPEED, 0.5f);
        }


        #endregion

        #region Death & Ragdoll
        private async UniTask HandleDeath()
        {
            // _animator.SetTrigger(ANIM_DEATH);
            // _navMeshAgent.isStopped = true;
            _navMeshAgent.enabled = false;
            _animator.enabled = false;
            EnableRagdoll();
            _takeDamageCollider.enabled = false;

            // float _duration = _animator.GetCurrentAnimatorClipInfo(0).Length;
            await UniTask.Delay(Mathf.FloorToInt(5000));
            _eyesRenderer.enabled = false;
            _bodyRenderer.material = DissolveMaterial;
            _bodyRenderer.material.DOFloat(1, "_DissolveAmount", 3);
            await UniTask.Delay(Mathf.FloorToInt(1000));
            Destroy(this.gameObject);

        }

        private void DisableRagdoll()
        {
            foreach (Collider collider in _ragdollColliders)
            {
                collider.enabled = false;
            }
            foreach (Rigidbody rigidbody in _ragdollRigidbodies)
            {
                rigidbody.isKinematic = true;
            }
        }
        private void EnableRagdoll()
        {
            foreach (Collider collider in _ragdollColliders)
            {
                collider.enabled = true;
            }

            foreach (Rigidbody rigidbody in _ragdollRigidbodies)
            {
                rigidbody.isKinematic = false;
            }
            DesactivateClawsHitBox();
            _takeDamageCollider.enabled = false;
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
        TakingDamage,
        Attacking,
        Dead,
    }
}
