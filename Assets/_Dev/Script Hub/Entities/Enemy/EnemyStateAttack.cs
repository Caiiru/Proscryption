using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Attack - o inimigo está atacando com padrão Soulslike
    /// 
    /// CARACTERÍSTICAS SOULSLIKE:
    /// - Delays variados de carregamento (punição para panic rolls)
    /// - Rotação durante ataque mantida (não é imobilizado)
    /// - Sincronização com animação (trigger de dano no meio)
    /// - Cooldown após ataque
    /// - Combat shuffle: após atacar, retorna ao circulamento
    /// </summary>
    public class EnemyStateAttack : IEnemyState
    {
        private readonly EnemyController _controller;
        private float _attackStartTime;
        private float _chargeDelayTime;
        private bool _hasAttackedThisSequence;
        private bool _isCharging; // Se está no período de "wind-up" do ataque
        private bool _alreadyAttack;

        public EnemyStateAttack(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            // Transição para animação de ataque
            _controller.SetAnimationState("Attack");
            _attackStartTime = Time.time;
            _hasAttackedThisSequence = false;
            _isCharging = true;

            // Seleciona um delay aleatório para este ataque
            // Isto cria variação e punição para o jogador "panic rolling" cegamente
            _controller.SelectRandomAttackCharge();
            _chargeDelayTime = _controller.GetAttackChargeDelay();

            // Registra que atacou (para cooldown)
            _controller.RegisterAttack();
            _alreadyAttack = false;
            Debug.Log($"[{_controller.gameObject.name}] Iniciando ataque com charge delay: {_chargeDelayTime:F2}s");
        }

        public void Update()
        {
            float elapsedTime = Time.time - _attackStartTime;
            // FASE 1: Wind-up/Charge
            // Durante este período, o inimigo carrega seu ataque
            // O jogador tem tempo para esquivar ou bloquear
            if (_isCharging && elapsedTime < _chargeDelayTime)
            {
                // Mantém rotação voltada para o jogador durante carregamento
                _controller.RotateTowardsPlayer();
                // Podería adicionar efeitos visuais/sonoros de carga aqui
                return;
            }

            // FASE 2: Execute Attack
            // Fim do carregamento - executa o dano
            if (_isCharging && elapsedTime >= _chargeDelayTime)
            {
                _isCharging = false;
            }

            // Executa o ataque no tempo sincronizado com a animação
            // Nota: _controller.attackTime é o tempo em que o hit box se ativa
            if (elapsedTime >= (_chargeDelayTime + _controller.attackTime) && !_hasAttackedThisSequence && !_alreadyAttack)
            {
                _controller.ExecuteAttack();
                _hasAttackedThisSequence = true;
                _alreadyAttack = true;
                // Debug.Log($"[{_controller.gameObject.name}] Ataque executado em {elapsedTime:F2}s");
            }

            // Sempre mantém rotação durante o ataque  
            _controller.RotateTowardsPlayer();

            // Aguarda a animação terminar completamente
            if (!_controller.GetIsAttacking())
            {
                // TRANSIÇÃO PÓS-ATAQUE (Combat Shuffle)
                // Após atacar, retorna a um estado de movimento
                if (_controller.CanSeePlayer())
                {
                    // Se consegue ver o jogador, volta a circular/perseguir
                    // Isto evita que o inimigo ataque infinitamente sem parar
                    _controller.StateMachine.TransitionTo(_controller.CircleState);
                }
                else
                {
                    // Se perdeu visão, volta ao idle
                    _controller.StateMachine.TransitionTo(_controller.IdleState);
                }
            }
            else
            {
                Debug.Log("Aguardando animação de ataque terminar...");
            }
        }

        public void Exit()
        {
            _isCharging = false;
            _alreadyAttack = false;
     
        }
    }
}
