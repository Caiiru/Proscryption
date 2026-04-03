using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Attack - o inimigo está atacando
    /// </summary>
    public class EnemyStateAttack : IEnemyState
    {
        private readonly EnemyController _controller;
        private float _attackStartTime;
        private bool _hasAttackedThisFrameSequence;

        public EnemyStateAttack(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            _controller.SetAnimationState("Attack");
            _attackStartTime = Time.time;
            _hasAttackedThisFrameSequence = false;
        }

        public void Update()
        {
            float elapsedTime = Time.time - _attackStartTime;

            // Executa o ataque no tempo apropriado (trigger de dano no meio da animação)
            if (elapsedTime >= _controller.attackTime && !_hasAttackedThisFrameSequence)
            {
                _controller.ExecuteAttack();
                _hasAttackedThisFrameSequence = true;
            }

            // Mantém rotação durante o ataque (soulslike)
            _controller.RotateTowardsPlayer();

            // Aguarda a animação terminar completamente (normalizedTime >= 1.0)
            if (_controller.IsAnimationFinished())
            {
                if (_controller.CanSeePlayer() && _controller.IsInAttackRange())
                {
                    // Pode fazer outro ataque se está perto o suficiente
                    _controller.StateMachine.TransitionTo(_controller.ChaseState);
                }
                else if (_controller.CanSeePlayer())
                {
                    // Volta a perseguir se o jogador está visível mas longe
                    _controller.StateMachine.TransitionTo(_controller.ChaseState);
                }
                else
                {
                    // Volta ao idle se perdeu o jogador
                    _controller.StateMachine.TransitionTo(_controller.IdleState);
                }
            }
        }

        public void Exit()
        {
        }
    }
}
