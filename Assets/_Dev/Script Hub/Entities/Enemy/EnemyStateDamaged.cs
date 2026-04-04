using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Damaged - o inimigo foi atingido
    /// 
    /// CARACTERÍSTICAS SOULSLIKE:
    /// - Knockback/flinch brief
    /// - Acúmulo de poise damage (dano de equilíbrio)
    /// - Se poise quebra durante este estado, transiciona para Stagger
    /// - Período de invulnerabilidade curto (frames)
    /// - Retorna ao combate após recuperação
    /// </summary>
    public class EnemyStateDamaged : IEnemyState
    {
        private readonly EnemyController _controller;
        private float _damageRecoveryTimer;
        private const float DAMAGE_RECOVERY_TIME = 0.5f;
        private float _damageTakenThisFrame;
        private bool _poiseWasBroken;

        public EnemyStateDamaged(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            // Transição para animação de dano flinch
            _controller.SetAnimationState("TakeDamage");
            _damageRecoveryTimer = 0f;
            _poiseWasBroken = false;

            Debug.Log($"[{_controller.gameObject.name}] Entered Damaged state. Poise: {_controller.GetCurrentPoise():F1}/{_controller.GetMaxPoise():F1}");
        }

        public void Update()
        {
            _damageRecoveryTimer += Time.deltaTime;

            // Durante o período de dano, o inimigo está vulnerável mas imobilizado
            // Isto simula o "flinch" ou "stun" de um golpe em Soulslike

            // Se a poise foi quebrada durante este estado, vai para Stagger
            // (verificação contínua em caso de receber mais dano)
            if (_controller.GetCurrentPoise() <= 0f)
            {
                _controller.StateMachine.TransitionTo(_controller.StaggerState);
                return;
            }

            // Após o tempo de recuperação, volta para o estado apropriado
            if (_damageRecoveryTimer >= DAMAGE_RECOVERY_TIME)
            {
                // Prioridade: Stagger > Attack > Chase > Idle
                // Se o jogador está ao alcance e pode atacar, volta para Attack
                if (_controller.IsInAttackRange() && _controller.CanAttack() && _controller.CanSeePlayer())
                {
                    _controller.StateMachine.TransitionTo(_controller.AttackState);
                    Debug.Log($"[{_controller.gameObject.name}] Recovered from damage -> Attacking");
                }
                // Se consegue ver o jogador, volta para Chase
                else if (_controller.CanSeePlayer())
                {
                    _controller.StateMachine.TransitionTo(_controller.ChaseState);
                    Debug.Log($"[{_controller.gameObject.name}] Recovered from damage -> Chasing");
                }
                // Caso contrário, volta para Idle
                else
                {
                    _controller.StateMachine.TransitionTo(_controller.IdleState);
                    Debug.Log($"[{_controller.gameObject.name}] Recovered from damage -> Idle");
                }
            }
        }

        public void Exit()
        {
            _damageRecoveryTimer = 0f;
        }
    }
}
