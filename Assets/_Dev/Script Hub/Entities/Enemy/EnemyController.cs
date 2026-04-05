using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Controlador do inimigo com state machine
    /// Integra-se com EnemyEntity para combater sistema existente
    /// </summary>
    [RequireComponent(typeof(EnemyEntity)), RequireComponent(typeof(Rigidbody))]
    public class EnemyController : MonoBehaviour
    {

        public string currentStateName;
        [Header("Stats")]
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float moveSpeed = 5f;
        public float attackTime = 1.5f;
        [SerializeField] private float attackCooldown = 2f;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Poise System")]
        [SerializeField] private float maxPoise = 100f; // Quantidade total de poise antes de quebrar
        [SerializeField] private float poiseRegenerationRate = 5f; // Poise regenerado por segundo sem dano
        [SerializeField] private float damageToPoiseDamageMultiplier = 0.5f; // Quanto dano recebido vira dano de poise
        private float _currentPoise;

        [Header("Attack Delays")]
        [SerializeField] private float[] attackChargeDelays = { 0.3f, 0.5f, 0.8f, 1.2f }; // Variação de delays de carregamento
        private float _currentAttackChargeDelay; // Delay aplicado ao ataque atual
        private bool _isAttacking = false;

        [Header("Input Reading")]
        [SerializeField] private float inputReadingRange = 15f; // Alcance para detectar ações do jogador
        [SerializeField] private float healingDetectionCooldown = 3f; // Cooldown entre reações a cura
        private float _lastHealingReactionTime = -999f;

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
        public EnemyStateCircle CircleState { get; private set; }
        public EnemyStateAttack AttackState { get; private set; }
        public EnemyStateDamaged DamagedState { get; private set; }
        public EnemyStateStagger StaggerState { get; private set; } // Novo estado de atordoamento
        public EnemyStateDeath DeathState { get; private set; }

        // Referência à EnemyEntity
        private EnemyEntity _enemyEntity;

        // Cooldown de ataque
        private float _lastAttackTime = -999f;

        private void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            _enemyEntity = GetComponent<EnemyEntity>();

            // Procura o jogador
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject)
            {
                _playerTransform = playerObject.transform;
            }

            // Inicializa o sistema de poise
            _currentPoise = maxPoise;

            // Criaos estados
            IdleState = new EnemyStateIdle(this);
            ChaseState = new EnemyStateChase(this);
            CircleState = new EnemyStateCircle(this);
            AttackState = new EnemyStateAttack(this);
            DamagedState = new EnemyStateDamaged(this);
            StaggerState = new EnemyStateStagger(this); // Inicializa novo estado
            DeathState = new EnemyStateDeath(this);

            // Inicializa a state machine
            _stateMachine = new EnemyStateMachine();
            _stateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            _stateMachine.Update();

            // Sistema de regeneração de poise (quando não está em combate ativo)
            RegeneratePoise();

            // Verifica constantemente por Input Reading (reações a cura do jogador)
            CheckForPlayerHealing();

            currentStateName = StateMachine.CurrentState.GetType().Name;
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

        public void MoveAwayFromPlayer()
        {
            if (!_playerTransform || !_rigidbody)
                return;

            Vector3 directionAwayFromPlayer = (transform.position - _playerTransform.position).normalized;
            _rigidbody.linearVelocity = new Vector3(
                directionAwayFromPlayer.x * moveSpeed,
                _rigidbody.linearVelocity.y,
                directionAwayFromPlayer.z * moveSpeed
            );
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
        /// </summary>
        public void MoveInDirection(Vector3 direction, float speed)
        {
            if (!_rigidbody)
                return;

            direction = direction.normalized;
            _rigidbody.linearVelocity = new Vector3(
                direction.x * speed,
                _rigidbody.linearVelocity.y,
                direction.z * speed
            );
        }

        /// <summary>
        /// Retorna a distância preferida de combate
        /// </summary>
        public float GetPreferredCombatDistance()
        {
            return attackRange;
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



        #region Poise System 

        /// <summary>
        /// Aplica dano de poise ao inimigo
        /// Quando poise chega a 0, inimigo entra em estado de Stagger
        /// </summary>
        public void TakePoiseDamage(float damageAmount)
        {
            _currentPoise -= damageAmount;
            Debug.Log($"[{gameObject.name}] Poise: {_currentPoise:F1}/{maxPoise:F1} (- {damageAmount:F1})");

            // Se poise quebrou, transiciona para Stagger
            if (_currentPoise <= 0f && !_stateMachine.CurrentState.Equals(StaggerState))
            {
                _stateMachine.TransitionTo(StaggerState);
            }
        }

        /// <summary>
        /// Reseta a poise após stagger
        /// </summary>
        public void ResetPoise()
        {
            _currentPoise = maxPoise;
            Debug.Log($"[{gameObject.name}] Poise resetada para {maxPoise}");
        }

        /// <summary>
        /// Regenera poise gradualmente (soulslike: poise se regenera lentamente)
        /// </summary>
        private void RegeneratePoise()
        {
            if (_currentPoise < maxPoise)
            {
                _currentPoise += poiseRegenerationRate * Time.deltaTime;
                _currentPoise = Mathf.Min(_currentPoise, maxPoise);
            }
        }

        /// <summary>
        /// Retorna o valor atual de poise (para UI/debug)
        /// </summary>
        public float GetCurrentPoise() => _currentPoise;

        /// <summary>
        /// Retorna a poise máxima
        /// </summary>
        public float GetMaxPoise() => maxPoise;

        #endregion

        #region Input Reading

        /// <summary>
        /// Verifica se o jogador está tentando se curar ou usar itens
        /// Se sim, e o inimigo está no cooldown, ele avança agressivamente
        /// Comportamento Soulslike: punir uso de consumíveis
        /// </summary>
        private void CheckForPlayerHealing()
        {
            if (!_playerTransform || !CanSeePlayer())
                return;

            float distanceToPlayer = GetDistanceToPlayer();
            if (distanceToPlayer > inputReadingRange)
                return;

            // Detecta animações de cura (verifica se player está em animação de poção/item)
            // Nota: Adapte os parâmetros abaixo conforme suas animações do player
            Animator playerAnimator = _playerTransform.GetComponent<Animator>();
            if (playerAnimator)
            {
                AnimatorStateInfo playerStateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

                // Verifica se player está em estado de "Healing", "Potion", "Item", etc.
                if (playerAnimator.GetBool("isHealing"))
                {
                    // Se pode atacar (cooldown passou), reage agressivamente
                    if (CanAttack() && Time.time >= _lastHealingReactionTime + healingDetectionCooldown)
                    {
                        Debug.Log($"[{gameObject.name}] Input Reading: Player está se curando! Atacando agora!");
                        _lastHealingReactionTime = Time.time;

                        // Se está muito longe, persegue o player agressivamente
                        if (_stateMachine.CurrentState.Equals(ChaseState) ||
                            _stateMachine.CurrentState.Equals(CircleState) ||
                            _stateMachine.CurrentState.Equals(DamagedState))
                        {
                            // Força transição para ataque se possível
                            if (IsInAttackRange())
                            {
                                _stateMachine.TransitionTo(AttackState);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Attack Delays  

        /// <summary>
        /// Seleciona um delay de carregamento aleatório para o próximo ataque
        /// Cria variação e punição para "panic rolls"
        /// </summary>
        public void SelectRandomAttackCharge()
        {
            _currentAttackChargeDelay = attackChargeDelays[Random.Range(0, attackChargeDelays.Length)];
            Debug.Log($"[{gameObject.name}] Delay de ataque selecionado: {_currentAttackChargeDelay:F2}s");
        }

        /// <summary>
        /// Retorna o delay de carregamento do ataque atual
        /// Usado por EnemyStateAttack para sincronizar com animação
        /// </summary>
        public float GetAttackChargeDelay() => _currentAttackChargeDelay;

        #endregion

        #region Poise Damage from External Sources

        /// <summary>
        /// Converte dano recebido em dano de poise
        /// Chamada quando inimigo recebe dano (integra com EnemyEntity)
        /// </summary>
        public void ApplyPoiseDamageFromHit(float damageReceived)
        {
            float poiseDamage = damageReceived * damageToPoiseDamageMultiplier;
            TakePoiseDamage(poiseDamage);
        }

        #endregion
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
