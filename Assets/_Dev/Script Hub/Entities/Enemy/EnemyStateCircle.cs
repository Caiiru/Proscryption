using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Circle - o inimigo rodeia o jogador (Combat Shuffle)
    /// 
    /// CARACTERÍSTICAS SOULSLIKE:
    /// - Circunda o player pelas laterais em vez de atacar sem parar
    /// - Alternância dinâmica entre lado esquerdo e direito
    /// - Mantém rotação voltada para o player (pronto para reagir)
    /// - Reage a movimento do player (mantém distância ideal)
    /// - Nunca fica completamente imobilizado (sempre em movimento estratégico)
    /// 
    /// Este estado é fundamental para punição e criação de tensão:
    /// - Permite que o jogador recupere stamina
    /// - Deixa o inimigo se aproximar para próximo ataque
    /// - Cria momentos de "tensão" antes de ataques
    /// </summary>
    public class EnemyStateCircle : IEnemyState
    {
        private readonly EnemyController _controller;
        private float _circleDirection = 1f; // 1 para direita, -1 para esquerda
        private float _directionChangeTimer = 0f;
        private float _directionChangeCooldown = 2f; // Muda de lado a cada 2 segundos
        private float _nextDirectanceAdjustmentTime = 0f; // Timer para ajustar distância dinamicamente

        public EnemyStateCircle(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            _controller.SetAnimationState("Run");
            _directionChangeTimer = 0f;
            _nextDirectanceAdjustmentTime = Time.time + 0.5f;
            
            // Randomiza a direção inicial (50/50 esquerda/direita)
            _circleDirection = Random.value > 0.5f ? 1f : -1f;

            Debug.Log($"[{_controller.gameObject.name}] Entrando em Circle - iniciando Combat Shuffle (direção: {(_circleDirection > 0 ? "DIREITA" : "ESQUERDA")})");
        }

        public void Update()
        {
            // TRANSIÇÃO 1: Perdeu visão do jogador
            if (!_controller.CanSeePlayer())
            {
                _controller.StateMachine.TransitionTo(_controller.IdleState);
                return;
            }

            float distanceToPlayer = _controller.GetDistanceToPlayer();
            float preferredDistance = _controller.GetPreferredCombatDistance();

            // TRANSIÇÃO 2: Ficou muito perto (deve recuar)
            if (distanceToPlayer < preferredDistance * 0.6f)
            {
                _controller.MoveAwayFromPlayer();
                _controller.RotateTowardsPlayer();
                Debug.Log($"[{_controller.gameObject.name}] Muito perto - recuando");
                return;
            }

            // TRANSIÇÃO 3: Ficou muito longe (volta a perseguir)
            if (distanceToPlayer > preferredDistance * 1.4f)
            {
                _controller.StateMachine.TransitionTo(_controller.ChaseState);
                Debug.Log($"[{_controller.gameObject.name}] Muito longe - voltando ao Chase");
                return;
            }

            // TRANSIÇÃO 4: Cooldown passou - pode atacar
            if (_controller.CanAttack())
            {
                _controller.StateMachine.TransitionTo(_controller.AttackState);
                Debug.Log($"[{_controller.gameObject.name}] Cooldown terminou - atacando!");
                return;
            }

            // COMPORTAMENTO: Circunda o player durante o cooldown de ataque
            CircleAroundPlayer();
        }

        public void Exit()
        {
            _controller.StopMovement();
        }

        /// <summary>
        /// Move o inimigo em círculo ao redor do jogador
        /// Combina movimento tangencial (lateral) com pequenos ajustes radiais (distância)
        /// </summary>
        private void CircleAroundPlayer()
        {
            Transform playerTransform = _controller.GetPlayerTransform();
            if (!playerTransform)
                return;

            // Calcula a posição relativa do inimigo em relação ao player
            Vector3 positionRelativeToPlayer = _controller.transform.position - playerTransform.position;
            positionRelativeToPlayer.y = 0f; // Ignora altura para movimento 2D

            float distanceToPlayer = positionRelativeToPlayer.magnitude;
            if (distanceToPlayer < 0.1f) // Proteção contra divisão por zero
                distanceToPlayer = 0.1f;

            // Normaliza para obter direção radial (player para inimigo)
            Vector3 directionFromPlayer = positionRelativeToPlayer.normalized;

            // Calcula o vetor tangencial (movimento lateral)
            // Perpendicular ao vetor radial, rotaciona 90 graus em torno do Y
            Vector3 tangentialDirection = new Vector3(
                -directionFromPlayer.z * _circleDirection,  // Movimento lateral
                0f,
                directionFromPlayer.x * _circleDirection
            ).normalized;

            // Mistura movimento tangencial com pequeno ajuste radial
            // 85% movimento lateral (entorno), 15% ajuste radial (distância)
            Vector3 moveDirection = (tangentialDirection * 0.85f) + (directionFromPlayer * 0.15f);
            moveDirection = moveDirection.normalized;

            // Aplica o movimento de circundação
            _controller.MoveInDirection(moveDirection, _controller.GetMoveSpeed());

            // Sempre mantém o rosto voltado para o player (ready stance)
            _controller.RotateTowardsPlayer();

            // ALTERNÂNCIA DINÂMICA: Muda de direção periodicamente
            // Cria padrão mais natural e menos previsível
            _directionChangeTimer += Time.deltaTime;
            if (_directionChangeTimer >= _directionChangeCooldown)
            {
                _circleDirection *= -1f; // Inverte direção (esquerda ↔ direita)
                _directionChangeTimer = 0f;
                Debug.Log($"[{_controller.gameObject.name}] Alternando direção - agora circulando pela {(_circleDirection > 0 ? "DIREITA" : "ESQUERDA")}");
            }
        }
    }
}
