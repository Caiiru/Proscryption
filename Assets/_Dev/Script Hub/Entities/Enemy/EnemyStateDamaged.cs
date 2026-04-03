using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Damaged - o inimigo foi atingido
    /// Típico em Soulslike: knockback e estado de recuperação
    /// </summary>
    public class EnemyStateDamaged : IEnemyState
    {
        private readonly EnemyController _controller;
        private float _damageRecoveryTimer;
        private const float DAMAGE_RECOVERY_TIME = 0.5f;

        public EnemyStateDamaged(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            _controller.SetAnimationState("TakeDamage");
            _damageRecoveryTimer = 0f;
        }

        public void Update()
        {
            _damageRecoveryTimer += Time.deltaTime;

            // Após o tempo de recuperação, volta para o estado anterior adequado
            if (_damageRecoveryTimer >= DAMAGE_RECOVERY_TIME)
            {
                // Se o jogador está ao alcance, volta para Attack
                if (_controller.IsInAttackRange() && _controller.CanSeePlayer())
                {
                    _controller.StateMachine.TransitionTo(_controller.AttackState);
                }
                // Se consegue ver o jogador, volta para Chase
                else if (_controller.CanSeePlayer())
                {
                    _controller.StateMachine.TransitionTo(_controller.ChaseState);
                }
                // Caso contrário, volta para Idle
                else
                {
                    _controller.StateMachine.TransitionTo(_controller.IdleState);
                }
            }
        }

        public void Exit()
        {
        }
    }
}
