using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Attack - o inimigo está atacando
    /// </summary>
    public class EnemyStateAttack : IEnemyState
    {
        private readonly EnemyController _controller;
        private float _attackCooldown;
        private bool _hasAttackedThisFrameSequence;

        public EnemyStateAttack(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            _controller.SetAnimationState("Attack");
            _attackCooldown = 0f;
            _hasAttackedThisFrameSequence = false;
        }

        public void Update()
        {
            _attackCooldown += Time.deltaTime;

            // Executa o ataque no tempo apropriado
            if (_attackCooldown >= _controller.attackTime && !_hasAttackedThisFrameSequence)
            {
                _controller.ExecuteAttack();
                _hasAttackedThisFrameSequence = true;
            }

            // Aguarda o ataque terminar, depois volta para Chase ou Idle
            if (_attackCooldown >= _controller.attackTime + 0.5f)
            {
                if (_controller.CanSeePlayer() && _controller.IsInAttackRange())
                {
                    // Pode fazer outro ataque ou voltar para Chase
                    _controller.StateMachine.TransitionTo(_controller.ChaseState);
                }
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
