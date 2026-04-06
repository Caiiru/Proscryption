using UnityEngine;
using proscryption.Enemy.Refactor;
using Unity.VisualScripting;

namespace proscryption.Enemy
{
    [RequireComponent(typeof(EnemyEntity)), RequireComponent(typeof(Rigidbody))]
    public class EnemyController : MonoBehaviour
    {
        public string currentStateName;
        [Header("Detection")]
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float combatRange = 2f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Attack")]
        public float attackTime = 1.5f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float[] attackChargeDelays = { 0.3f, 0.5f, 0.8f, 1.2f };
        private float _currentAttackChargeDelay;
        [SerializeField] private bool _isAttacking = false;
        private float _lastAttackTime = -999f;

        [Header("Poise System (Config)")]
        [SerializeField] private float maxPoise = 100f;
        [SerializeField] private float poiseRegenerationRate = 5f;
        [SerializeField] private float damageToPoiseDamageMultiplier = 0.5f;

        [Header("Input Reading (Config)")]
        [SerializeField] private float inputReadingRange = 15f;
        [SerializeField] private float healingDetectionCooldown = 3f;

        [Header("References")]
        private Transform _playerTransform;
        private Rigidbody _rigidbody;
        private Animator _animator;

        // ============================================================
        // SUBSISTEMAS REFATORADOS (Fase 1)
        // ============================================================
        private EnemyPoiseSystem _poiseSystem;
        private EnemyMovement _movementSystem;
        private EnemyInputReader _inputReaderSystem;

        // Acessores públicos para subsistemas (para possível acesso de estados)
        public EnemyPoiseSystem PoiseSystem => _poiseSystem;
        public EnemyMovement MovementSystem => _movementSystem;
        public EnemyInputReader InputReaderSystem => _inputReaderSystem;

        // State Machine
        private EnemyStateMachine _stateMachine;
        public EnemyStateMachine StateMachine => _stateMachine;

        // Estados
        public EnemyStateIdle IdleState { get; private set; }
        public EnemyStateChase ChaseState { get; private set; }
        public EnemyStateCircle CircleState { get; private set; }
        public EnemyStateAttack AttackState { get; private set; }
        public EnemyStateDamaged DamagedState { get; private set; }
        public EnemyStateStagger StaggerState { get; private set; } // Novo estado de atordoamento
        public EnemyStateDeath DeathState { get; private set; }

        // Referência à EnemyEntity
        private EnemyEntity _enemyEntity;

        private void OnEnable()
        {
            CacheComponentReferences();
            FindPlayerReference();
            InitializeSubsystems();
            InitializeStateMachine();
        }

        /// <summary>
        /// Cacheia componentes do próprio inimigo
        /// </summary>
        private void CacheComponentReferences()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            _enemyEntity = GetComponent<EnemyEntity>();
        }

        /// <summary>
        /// Encontra a referência do player no mundo
        /// </summary>
        private void FindPlayerReference()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject)
                _playerTransform = playerObject.transform;
        }

        /// <summary>
        /// Inicializa todos os subsistemas refatorados
        /// </summary>
        private void InitializeSubsystems()
        {
            // Inicializa EnemyPoiseSystem
            _poiseSystem = new EnemyPoiseSystem(
                maxPoise,
                poiseRegenerationRate,
                damageToPoiseDamageMultiplier
            );

            // Hook de eventos do Poise System
            _poiseSystem.OnPoiseBreak += HandlePoiseBreak;

            // Inicializa EnemyMovement
            _movementSystem = this.AddComponent<EnemyMovement>();
            _movementSystem.Setup(
                _rigidbody,
                moveSpeed,
                rotationSpeed
            );

            // Inicializa EnemyInputReader
            _inputReaderSystem = new EnemyInputReader(
                _playerTransform,
                inputReadingRange,
                healingDetectionCooldown
            );

            // Hook de eventos do Input Reader
            _inputReaderSystem.OnPlayerHealing += HandlePlayerHealing;
        }

        /// <summary>
        /// Inicializa a state machine e estados
        /// </summary>
        private void InitializeStateMachine()
        {
            // Cria os estados
            IdleState = new EnemyStateIdle(this);
            ChaseState = new EnemyStateChase(this);
            CircleState = new EnemyStateCircle(this);
            AttackState = new EnemyStateAttack(this);
            DamagedState = new EnemyStateDamaged(this);
            StaggerState = new EnemyStateStagger(this);
            DeathState = new EnemyStateDeath(this);

            // Inicializa a state machine
            _stateMachine = new EnemyStateMachine();
            _stateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            // Atualiza subsistemas independentes
            _poiseSystem?.Update();
            _inputReaderSystem?.Update();
            _movementSystem?.DrawDebugGizmos(transform.position, Color.red);

            // Atualiza state machine
            _stateMachine.Update();

            // Debug
            currentStateName = StateMachine.CurrentState.GetType().Name;
        }

        /// <summary>
        /// Callback disparado quando poise quebra
        /// </summary>
        private void HandlePoiseBreak()
        {
            Debug.Log($"[{gameObject.name}] Poise quebrou! Entrando em Stagger...");

            if (!_stateMachine.CurrentState.Equals(StaggerState))
            {
                _stateMachine.TransitionTo(StaggerState);
            }
        }

        /// <summary>
        /// Callback disparado quando player começa a se curar
        /// </summary>
        private void HandlePlayerHealing()
        {
            Debug.Log($"[{gameObject.name}] Input Reading: Player está se curando! Reagindo agressivamente...");

            // Se pode atacar, força ataque se estiver em alcance
            if (CanAttack() && IsInAttackRange())
            {
                if (!_stateMachine.CurrentState.Equals(AttackState))
                {
                    _stateMachine.TransitionTo(AttackState);
                }
            }
        }

        #region Estado Methods
        public void SetAnimationState(string stateName)
        {
            if (_animator)
            {
                _animator.SetTrigger(stateName);
            }
        }
        public void SetAnimationFloat(string parameterName, float value)
        {
            if (_animator)
            {
                _animator.SetFloat(parameterName, value);
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
            if (!_playerTransform || (_movementSystem == null))
                return;

            _movementSystem.MoveTowards(_playerTransform.position);
            _movementSystem.RotateTowards(_playerTransform.position);
        }

        /// <summary>
        /// Rotaciona em direção ao player
        /// Delegado via EnemyMovement
        /// </summary>
        public void RotateTowardsPlayer()
        {
            if (!_playerTransform || (_movementSystem == null))
                return;

            _movementSystem.RotateTowards(_playerTransform.position);
        }

        /// <summary>
        /// Para completamente o movimento
        /// Delegado via EnemyMovement
        /// </summary>
        public void StopMovement()
        {
            _movementSystem?.StopMovement();
        }

        /// <summary>
        /// Move para longe do player
        /// Delegado via EnemyMovement
        /// </summary>
        public void MoveAwayFromPlayer()
        {
            if (!_playerTransform || (_movementSystem == null))
                return;

            _movementSystem.MoveAwayFrom(_playerTransform.position); 
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
        /// Obtém o tempo normalizado da animação atual (0-1)
        /// </summary>
        public float GetAnimationNormalizedTime()
        {
            if (!_animator)
                return 0f;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime;
        }

        /// <summary>
        /// Verifica se pode atacar (cooldown passou)
        /// </summary>
        public bool CanAttack()
        {
            return Time.time >= _lastAttackTime + attackCooldown;
        }

        /// <summary>
        /// Registra que atacou (reinicia o cooldown)
        /// </summary>
        public void RegisterAttack()
        {
            SetIsAttacking(true);
            _lastAttackTime = Time.time;
            _enemyEntity.MakeCanHit();
        }

        /// <summary>
        /// Calcula a distância até o jogador
        /// </summary>
        public float GetDistanceToPlayer()
        {
            if (!_playerTransform)
                return float.MaxValue;

            return Vector3.Distance(transform.position, _playerTransform.position);
        }

        public bool IsTooCloseToPlayer()
        {
            return GetDistanceToPlayer() < attackRange;
        }

        public float GetAttackRange()
        {
            return attackRange;
        }

        /// <summary>
        /// Retorna a transform do jogador
        /// </summary>
        public Transform GetPlayerTransform()
        {
            return _playerTransform;
        }

        /// <summary>
        /// Move o inimigo em uma direção específica com velocidade definida
        /// Delegado via EnemyMovement
        /// </summary>
        public void MoveInDirection(Vector3 direction, float speed)
        {
            if (_movementSystem == null)
                return;

            // Calcula multiplicador baseado na velocidade
            float speedMultiplier = speed / moveSpeed;
            _movementSystem.MoveInDirection(direction, speedMultiplier);
        }
        public void RotateTowardsDirection(Vector3 direction, float? rotationSpeed = null){
            if (_movementSystem == null)
                return;

            _movementSystem.RotateTowards(direction, rotationSpeed);
        }

        /// <summary>
        /// Move circularmente ao redor de um ponto
        /// Útil para Combat Shuffle
        /// Delegado via EnemyMovement
        /// </summary>
        public void MoveCircularAround(Vector3 center, float direction = 1f)
        {
            _movementSystem?.MoveCircularAround(center, direction);
        }

        /// <summary>
        /// Retorna a distância preferida de combate
        /// </summary>
        public float GetPreferredCombatDistance()
        {
            return combatRange;
        }

        /// <summary>
        /// Retorna a velocidade de movimento do inimigo
        /// </summary>
        public float GetMoveSpeed()
        {
            return moveSpeed;
        }

        public bool GetIsAttacking() => _isAttacking;
        public void SetIsAttacking(bool value) => _isAttacking = value;
        public void AnimationAttackFinished()
        {
            SetIsAttacking(false);
        }



        // ============================================================
        // DELEGAÇÃO PARA POISE SYSTEM (REFATORADO)
        // ============================================================

        /// <summary>
        /// Aplica dano de poise ao inimigo
        /// Quando poise chega a 0, inimigo entra em estado de Stagger
        /// Delegado via EnemyPoiseSystem
        /// </summary>
        public void TakePoiseDamage(float damageAmount)
        {
            _poiseSystem?.TakePoiseDamage(damageAmount);
        }

        /// <summary>
        /// Reseta a poise após stagger
        /// Delegado via EnemyPoiseSystem
        /// </summary>
        public void ResetPoise()
        {
            _poiseSystem?.ResetPoise();
        }

        /// <summary>
        /// Retorna o valor atual de poise (para UI/debug)
        /// Delegado via EnemyPoiseSystem
        /// </summary>
        public float GetCurrentPoise() => _poiseSystem?.GetCurrentPoise() ?? 0f;

        /// <summary>
        /// Retorna a poise máxima
        /// Delegado via EnemyPoiseSystem
        /// </summary>
        public float GetMaxPoise() => _poiseSystem?.GetMaxPoise() ?? maxPoise;

        /// <summary>
        /// Converte dano recebido em dano de poise
        /// Chamada quando inimigo recebe dano (integra com EnemyEntity)
        /// Delegado via EnemyPoiseSystem
        /// </summary>
        public void ApplyPoiseDamageFromHit(float damageReceived)
        {
            _poiseSystem?.ApplyPoiseDamageFromHit(damageReceived);
        }

        // ============================================================
        // DELEGAÇÃO PARA INPUT READER (REFATORADO)
        // ============================================================

        /// <summary>
        /// Retorna se player está se curando (para queries de outros sistemas)
        /// Delegado via EnemyInputReader
        /// </summary>
        public bool IsPlayerHealing() => _inputReaderSystem?.IsPlayerHealing() ?? false;

        /// <summary>
        /// Retorna tempo desde última reação a cura
        /// Delegado via EnemyInputReader
        /// </summary>
        public float GetTimeSinceLastHealingReaction() => _inputReaderSystem?.GetTimeSinceLastHealingReaction() ?? -999f;

        // ============================================================
        // ATTACK SYSTEM (Será refatorado na Fase 2)
        // ============================================================

        /// <summary>
        /// Seleciona um delay de carregamento aleatório para o próximo ataque
        /// Cria variação e punição para \"panic rolls\"
        /// TODO: Mover para EnemyAttackManager na Fase 2
        /// </summary>
        public void SelectRandomAttackCharge()
        {
            _currentAttackChargeDelay = attackChargeDelays[Random.Range(0, attackChargeDelays.Length)];
            Debug.Log($"[{gameObject.name}] Delay de ataque selecionado: {_currentAttackChargeDelay:F2}s");
        }

        /// <summary>
        /// Retorna o delay de carregamento do ataque atual
        /// TODO: Mover para EnemyAttackManager na Fase 2
        /// </summary>
        public float GetAttackChargeDelay() => _currentAttackChargeDelay;
        #endregion

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, inputReadingRange);

        }
    }


}