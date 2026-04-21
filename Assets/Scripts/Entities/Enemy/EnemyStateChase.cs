using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Chase - o inimigo persegue o jogador
    /// 
    /// CARACTERÍSTICAS SOULSLIKE:
    /// - Rotação contínua na direção do jogador (nunca vira as costas)
    /// - Perseguição agressiva quando pode atacar
    /// - Mantém distância estratégica antes de atacar proximidade
    /// - Transiciona para Circle quando em cooldown (evita ataque infinito)
    /// - Reage a mudanças de distância dinamicamente
    /// 
    /// FLUXO:
    /// - Se longe: Segue o jogador
    /// - Se na distância ideal: Para e segue
    /// - Se muito perto: Recua um pouco
    /// - Se pode atacar: Transiciona para Attack
    /// - Se não pode atacar: Transiciona para Circle
    /// </summary>
    public class EnemyStateChase : IEnemyState
    {
        private readonly EnemyController _controller;
        private float _circleTransitionDelay = 0.8f; // Espera antes de circular
        private float _circleTransitionTimer = 0f;

        public EnemyStateChase(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            // _controller.SetAnimationState("Run");
            _controller.SetAnimationFloat("Speed", 0.5f); // Assume que 1 é a velocidade de corrida
            _circleTransitionTimer = 0f;
            // Debug.Log($"[{_controller.gameObject.name}] Iniciando Chase - perseguindo player");
        }

        public void Update()
        {
            // TRANSIÇÃO PRIORIDADE 1: Perdeu visão do jogador
            if (!_controller.CanSeePlayer())
            {
                _controller.StateMachine.TransitionTo(_controller.IdleState);
                Debug.Log($"[{_controller.gameObject.name}] Perdeu visão - voltando ao Idle");
                return;
            }

            float distanceToPlayer = _controller.GetDistanceToPlayer();
            float attackRange = _controller.GetAttackRange();
            float preferredDistance = _controller.GetPreferredCombatDistance();

            // COMPORTAMENTO 1: Muito perto (recua defensivamente)
            // Evita ficar "grudado" no player (bug comum de IA)
            if (distanceToPlayer < attackRange * 0.75f)
            {
                _controller.MoveAwayFromPlayer();
                _controller.RotateTowardsPlayer();
                _circleTransitionTimer = 0f;
                return;
            }

            // COMPORTAMENTO 2: Longe demais (persegue o jogador)
            if (distanceToPlayer > attackRange)
            {
                _controller.MoveTowardsPlayer();
                _controller.RotateTowardsPlayer();
                _circleTransitionTimer = 0f;

                return;
            }

            // COMPORTAMENTO 3: Distância ideal de combate
            // Nesta zona, o inimigo decide atacar ou circular
            _controller.StopMovement();
            _controller.RotateTowardsPlayer();

            // TRANSIÇÃO PRIORIDADE 2: Pode atacar (cooldown passou)
            if (_controller.CanAttack() && distanceToPlayer <= attackRange)
            {
                _circleTransitionTimer = 0f;
                _controller.StateMachine.TransitionTo(_controller.AttackState);
                Debug.Log($"[{_controller.gameObject.name}] Pode atacar! Transitando para Attack");
                return;
            }

            // COMPORTAMENTO 4: Está em cooldown - começa a circular
            // Transição suave para Circle state (Combat Shuffle)
            _circleTransitionTimer += Time.deltaTime;
            if (_circleTransitionTimer >= _circleTransitionDelay)
            {
                _controller.StateMachine.TransitionTo(_controller.CircleState);
                Debug.Log($"[{_controller.gameObject.name}] Entrando em cooldown - transitando para Circle (Combat Shuffle)");
                return;
            }
        }

        public void Exit()
        {
            _controller.StopMovement();
        }
    }
}
