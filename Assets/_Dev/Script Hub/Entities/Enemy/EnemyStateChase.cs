using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Chase - o inimigo persegue o jogador
    /// </summary>
    public class EnemyStateChase : IEnemyState
    {
        private readonly EnemyController _controller;

        public EnemyStateChase(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            _controller.SetAnimationState("Run");
        }

        public void Update()
        {
            // Se o jogador está ao alcance de ataque
            if (_controller.IsInAttackRange())
            {
                _controller.StateMachine.TransitionTo(_controller.AttackState);
                return;
            }

            // Se perdeu o jogador de vista
            if (!_controller.CanSeePlayer())
            {
                _controller.StateMachine.TransitionTo(_controller.IdleState);
                return;
            }

            // Persegue o jogador
            _controller.MoveTowardsPlayer();
        }

        public void Exit()
        {
            _controller.StopMovement();
        }
    }
}
