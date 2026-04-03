using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Death - o inimigo está morto
    /// </summary>
    public class EnemyStateDeath : IEnemyState
    {
        private readonly EnemyController _controller;

        public EnemyStateDeath(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            _controller.SetAnimationState("Death");
            _controller.DisableCollision();
            _controller.StopMovement();
        }

        public void Update()
        {
            // O inimigo está morto, não faz mais nada
        }

        public void Exit()
        {
        }
    }
}
