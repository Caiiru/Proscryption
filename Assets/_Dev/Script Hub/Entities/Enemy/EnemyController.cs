using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Controlador do inimigo com state machine
    /// Integra-se com EnemyEntity para combater sistema existente
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float moveSpeed = 5f;
        public float attackTime = 1.5f;

        [Header("Rotation (Soulslike)")]
        [SerializeField] private float rotationSpeed = 10f;

        [Header("References")]
        private Transform _playerTransform;
        private Rigidbody _rigidbody;
        private Animator _animator;

        // State Machine
        private EnemyStateMachine _stateMachine;
        public EnemyStateMachine StateMachine => _stateMachine;

        // Estados
        public EnemyStateIdle IdleState { get; private set; }
        public EnemyStateChase ChaseState { get; private set; }
        public EnemyStateAttack AttackState { get; private set; }
        public EnemyStateDamaged DamagedState { get; private set; }
        public EnemyStateDeath DeathState { get; private set; }

        // Referência à EnemyEntity
        private EnemyEntity _enemyEntity;

        private void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _enemyEntity = GetComponent<EnemyEntity>();

            // Procura o jogador
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject)
            {
                _playerTransform = playerObject.transform;
            }

            // Cria os estados
            IdleState = new EnemyStateIdle(this);
            ChaseState = new EnemyStateChase(this);
            AttackState = new EnemyStateAttack(this);
            DamagedState = new EnemyStateDamaged(this);
            DeathState = new EnemyStateDeath(this);

            // Inicializa a state machine
            _stateMachine = new EnemyStateMachine();
            _stateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        #region Estado Methods
        public void SetAnimationState(string stateName)
        {
            if (_animator)
            {
                _animator.SetTrigger(stateName);
            }
        }

        public bool CanSeePlayer()
        {
            if (!_playerTransform)
                return false;

            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
            return distanceToPlayer <= detectionRange;
        }

        public bool IsInAttackRange()
        {
            if (!_playerTransform)
                return false;

            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
            return distanceToPlayer <= attackRange;
        }

        public void MoveTowardsPlayer()
        {
            if (!_playerTransform || !_rigidbody)
                return;

            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            Debug.DrawLine(transform.position, transform.position + direction, Color.red);
            _rigidbody.linearVelocity = new Vector3(direction.x * moveSpeed, _rigidbody.linearVelocity.y, direction.z * moveSpeed);

            // Rotaciona na direção do movimento
            RotateTowardsPlayer();
        }

        public void RotateTowardsPlayer()
        {
            if (!_playerTransform)
                return;

            Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
            directionToPlayer.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        public void StopMovement()
        {
            if (_rigidbody)
            {
                _rigidbody.linearVelocity = Vector3.zero;
            }
        }

        public void ExecuteAttack()
        {
            if (_enemyEntity)
            {
                _animator.SetTrigger("Attack");
                // A lógica de dano é tratada pela EnemyEntity ou por eventos
            }
        }

        public void DisableCollision()
        {
            var collider = GetComponent<Collider>();
            if (collider)
            {
                collider.enabled = false;
            }
        }

        public void OnDamageTaken()
        {
            if (!_stateMachine.CurrentState.Equals(DeathState))
            {
                _stateMachine.TransitionTo(DamagedState);
            }
        }

        public void OnDeath()
        {
            _stateMachine.TransitionTo(DeathState);
        }

        /// <summary>
        /// Verifica se a animação atual terminou (normalizedTime >= 1.0)
        /// </summary>
        public bool IsAnimationFinished()
        {
            if (!_animator)
                return false;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime >= 1f;
        }

        /// <summary>
        /// Obtém o tempo normalizado da animação atual (0-1)
        /// </summary>
        public float GetAnimationNormalizedTime()
        {
            if (!_animator)
                return 0f;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime;
        }
        #endregion
    }
}
