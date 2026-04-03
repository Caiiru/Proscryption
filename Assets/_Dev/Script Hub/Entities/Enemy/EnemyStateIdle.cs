using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Idle - o inimigo está parado
    /// </summary>
    public class EnemyStateIdle : IEnemyState
    {
        private readonly EnemyController _controller;
        private float _idleTimer;

        public EnemyStateIdle(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            _controller.SetAnimationState("Idle");
            _idleTimer = 0f;
        }

        public void Update()
        {
            // Verifica se deve perseguir o jogador
            if (_controller.CanSeePlayer())
            {
                _controller.StateMachine.TransitionTo(_controller.ChaseState);
                return;
            }

            // Se recebeu dano enquanto em Idle, vai para Idle mesmo (ou pode fazer algo especial)
            _idleTimer += Time.deltaTime;
        }

        public void Exit()
        {
        }
    }
}
