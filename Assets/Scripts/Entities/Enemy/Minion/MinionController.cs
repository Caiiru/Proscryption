using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.VirtualTexturing;

namespace proscryption
{
    public class MinionController : MonoBehaviour
    {
        public EnemyState currentState;
        private EnemyEntity _enemyEntity;

        #region Vision Settings

        [Space] [Header("Vision")] [SerializeField]
        private LayerMask _playerMask = 1 << 3;

        #endregion

        #region Movement Settings

        [SerializeField] private float _moveSpeed = 2;
        [SerializeField] private float _turnRate = 2;
        private NavMeshAgent _navMeshAgent;

        #endregion

        #region Combat State

        [Space] [Header("Attack")] [SerializeField]
        bool _isAttacking;

        [SerializeField] float _attackRange = 2f;
        [Range(0, 100)] [SerializeField] float _critChance = 10;

        [Tooltip("Delay para ele se recuperar e voltar a se mexer")]
        public float _takeDamageDelay = 0.1f;

        private bool _canBeStunned = true;
        public float stunDelay = 1f;

        [Tooltip("Após a antecipação, o minion vai dar um salto e se deslocará com certa força em linha reta")]
        public float jumpForce = 5;


        [SerializeField] Transform _playerTransform;
        [SerializeField] BoxCollider[] _clawsHitBox;


        //References 

        #endregion

        #region Visual and Ragdoll

        CapsuleCollider _takeDamageCollider;
        [SerializeField] Collider[] _ragdollColliders;
        [SerializeField] Rigidbody[] _ragdollRigidbodies;

        [SerializeField] SkinnedMeshRenderer _bodyRenderer;
        [SerializeField] SkinnedMeshRenderer _eyesRenderer;

        public Material DissolveMaterial;

        Rigidbody _rigidbody;
        Transform _transform;

        #endregion

        #region Animation

        Animator _animator;
        private const string ANIM_SPEED = "Speed"; // float
        private const string ANIM_ATTACK = "Attack"; // trigger 
        private const string ANIM_ANTECIPATION = "Attack"; // trigger 
        private const string ANIM_DEATH = "Die"; // trigger 
        private const string ANIM_TAKE_DAMAGE = "TakeDamage"; // trigger 
        private const string ANIM_IS_STUNNED = "MinionIsStunned"; // boolean 

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
            _navMeshAgent.acceleration = 1f;
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
            _enemyEntity.OnDeath += (force, mode) => { HandleDeath(force, mode).Forget(); };
        }


        void OnDestroy()
        {
            _enemyEntity.OnDeath -= (force, mode) => { HandleDeath(force, mode).Forget(); };
            _enemyEntity.OnTakeDamage -= HandleTakeDamage;
        }

        void Update()
        {
            HandleCurrentState();
        }


        private UniTask EnterCurrentState()
        {
            switch (currentState)
            {
                case EnemyState.TakingDamage:
                    HandleTakeDamageState().Forget();
                    return UniTask.CompletedTask;
                    break;
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
            if (newState == currentState) return;
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
                SetVelocity(0, 0);
                _isAttacking = true;
                _animator.SetTrigger(ANIM_ANTECIPATION);
            }
        }

        private async UniTask HandleTakeDamageState()
        {
            //Stun Enemy
            if (!_canBeStunned) return;
            _canBeStunned = false;


            _animator.SetTrigger(ANIM_TAKE_DAMAGE);

            SetVelocity(0, 0);

            await UniTask.WaitForEndOfFrame();
            await UniTask.WaitForSeconds(_animator.GetCurrentAnimatorClipInfo(0).Length);
            await UniTask.Delay((int)(_takeDamageDelay * 500));

            HandleStunDelay().Forget();
            await UniTask.Delay((int)(_takeDamageDelay * 1000));

            if (_enemyEntity.IsDead) return;

            ChangeState(EnemyState.Attacking);

            _navMeshAgent.speed = _moveSpeed;
        }

        #endregion

        #region Movement

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
                return;
            }

            _navMeshAgent.SetDestination(_playerTransform.position);
            // _rigidbody.MovePosition(transform.position + transform.forward * Time.fixedDeltaTime * _moveSpeed);
            SetVelocity(_moveSpeed, 0.75f);
        }

        private void SetVelocity(float velocity, float animSpeed)
        {
            _navMeshAgent.speed = velocity;
            _animator.SetFloat(ANIM_SPEED, animSpeed);
        }

        private void RotateTowardsPlayer()
        {
            if (_isAttacking) return;
            // Vector3 _distance = _playerTransform.position - _transform.position;
            Vector3 _distance = _transform.InverseTransformPoint(_playerTransform.position);
            float angle = Mathf.Atan2(_distance.x, _distance.z) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0f, _transform.eulerAngles.y + angle, 0f);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, Time.deltaTime * _turnRate);

            // _transform.Rotate(rotation.eulerAngles);
        }

        #endregion

        #region Combat

        private bool CanAttack()
        {
            if (_isAttacking) return false;

            Vector3 _distance = _playerTransform.position - _transform.position;
            float distance = _distance.sqrMagnitude;
            return distance <= _attackRange * _attackRange;
        }

        public void EnableClawsHitBox()
        {
            if (_clawsHitBox.Length == 0) return;

            foreach (BoxCollider claw in _clawsHitBox)
            {
                claw.enabled = true;
            }
        }

        public void DisableClawsHitbox()
        {
            if (_clawsHitBox.Length == 0) return;

            foreach (BoxCollider claw in _clawsHitBox)
            {
                claw.enabled = false;
            }
        }

        public async void AntecipationDone()
        {
            _animator.SetTrigger(ANIM_ATTACK);

            // _rigidbody.isKinematic = true;
            _navMeshAgent.enabled = false;

            float duration = _animator.GetCurrentAnimatorClipInfo(0).Length;


            await UniTask.WaitForSeconds(duration / 2);
            _rigidbody.AddForce(transform.forward * jumpForce, ForceMode.Impulse); 
            // await UniTask.Delay(1000);
            await UniTask.Delay(Mathf.FloorToInt(duration * 2000));

            _rigidbody.isKinematic = false;
            _navMeshAgent.enabled = true;
            _isAttacking = false;
        }

        public int GetAttackDamage()
        {
            return _enemyEntity.GetAttackDamage();
        }

        public bool IsAttackCritical()
        {
            return _enemyEntity.IsCritical();
        }

        private async void HandleTakeDamage(Vector3? directionForce, ForceMode? forceMode)
        {
            _navMeshAgent.enabled = false;
            ChangeState(EnemyState.TakingDamage);
            await UniTask.WaitForEndOfFrame();

            AddKnockback(directionForce, forceMode);

            await UniTask.Delay(Mathf.FloorToInt(_takeDamageDelay * 1000));

            _rigidbody.isKinematic = true;
            _navMeshAgent.enabled = enabled;
        }

        private void AddKnockback(Vector3? directionForce, ForceMode? forceMode)
        {
            if (directionForce == null) return;
            if (forceMode == null) return;

            _rigidbody.isKinematic = false;
            _rigidbody.AddForce((Vector3)directionForce, (ForceMode)forceMode);
        }

        private void AddKnockbackOnRagdoll(Vector3? directionForce, ForceMode? forceMode)
        {
            if (directionForce == null) return;
            if (forceMode == null) return;

            foreach (Rigidbody r in _ragdollRigidbodies)
            {
                r.isKinematic = false;
                r.AddForce((Vector3)directionForce, (ForceMode)forceMode);
            }
        }

        private async UniTask HandleStunDelay()
        {
            await UniTask.WaitForSeconds(stunDelay);
            _canBeStunned = true;
        }

        #endregion

        #region Death & Ragdoll

        private async UniTask HandleDeath(Vector3? directionForce, ForceMode? forceMode)
        {
            _navMeshAgent.enabled = false;
            _animator.enabled = false;
            EnableRagdoll();
            _takeDamageCollider.enabled = false;

            //Apply Force
            await UniTask.WaitForEndOfFrame();

            AddKnockbackOnRagdoll(directionForce, forceMode);


            await UniTask.Delay(Mathf.FloorToInt(5000));
            //Dissolve Minion
            _eyesRenderer.enabled = false;
            _bodyRenderer.material = DissolveMaterial;
            float dissolveDuration = 3;
            _bodyRenderer.material.DOFloat(1, "_DissolveAmount", dissolveDuration);
            await UniTask.Delay(Mathf.FloorToInt(Mathf.FloorToInt(dissolveDuration) * 1000));
            Destroy(this.gameObject);
        }

        private void ToggleColliders(bool isActivate)
        {
            foreach (Collider c in _ragdollColliders)
            {
                c.enabled = isActivate;
            }
        }

        private void ToggleRigidbodies(bool isRagdollActivate)
        {
            foreach (Rigidbody r in _ragdollRigidbodies)
            {
                r.isKinematic = !isRagdollActivate;
            }
        }

        private void DisableRagdoll()
        {
            ToggleColliders(false);
            ToggleRigidbodies(false);
        }

        private void EnableRagdoll()
        {
            ToggleColliders(true);
            ToggleRigidbodies(true);

            DisableClawsHitbox();
            //My Rigidbody
            _rigidbody.isKinematic = true;
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