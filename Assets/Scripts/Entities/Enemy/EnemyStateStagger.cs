using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Stagger - o inimigo ficou travado/desequilibrado após quebra de poise
    /// Comportamento Soulslike: período vulnerável após acumular muitos golpes
    /// Inimigo não pode se mover ou atacar durante este estado
    /// </summary>
    public class EnemyStateStagger : IEnemyState
    {
        private readonly EnemyController _controller;
        private float _staggerStartTime;
        private const float STAGGER_DURATION = 1.2f; // Tempo em segundos que fica travado

        public EnemyStateStagger(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            // Transição para animação de atordoamento
            _controller.SetAnimationState("Stagger");
            _staggerStartTime = Time.time;
            
            // Para todos os movimentos
            _controller.StopMovement();
            
            // Reset da poise para o próximo ciclo
            _controller.ResetPoise();
            
            Debug.Log($"[{_controller.name}] Entrou em Stagger - será travado por {STAGGER_DURATION}s");
        }

        public void Update()
        {
            float elapsedTime = Time.time - _staggerStartTime;

            // Mantém o inimigo parado durante o stagger
            _controller.StopMovement();

            // Aguarda a duração do stagger
            if (elapsedTime >= STAGGER_DURATION)
            {
                // Transiciona de volta ao estado apropriado
                if (_controller.CanSeePlayer())
                {
                    if (_controller.IsInAttackRange() && _controller.CanAttack())
                    {
                        _controller.StateMachine.TransitionTo(_controller.AttackState);
                    }
                    else
                    {
                        _controller.StateMachine.TransitionTo(_controller.ChaseState);
                    }
                }
                else
                {
                    _controller.StateMachine.TransitionTo(_controller.IdleState);
                }
            }
        }

        public void Exit()
        {
            _controller.StopMovement();
        }
    }
}
