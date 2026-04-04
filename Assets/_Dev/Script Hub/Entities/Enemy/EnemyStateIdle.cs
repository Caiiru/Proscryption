using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Estado Idle - o inimigo está parado/descansando
    /// 
    /// CARACTERÍSTICAS:
    /// - Estado inicial do inimigo
    /// - Aguarda detecção do jogador (visão)
    /// - Agressivo "Input Reading": reage se player tenta se aproximar ou curar
    /// - Transição rápida para Chase quando detecta ameaça
    /// 
    /// Este é um estado "esperando" onde a IA repousa mas permanece
    /// vigilante de sinais de perigo
    /// </summary>
    public class EnemyStateIdle : IEnemyState
    {
        private readonly EnemyController _controller;
        private float _idleTimer;
        private float _alertnessLevel = 0f; // Nível de alerta antes de atacar preventivamente

        public EnemyStateIdle(EnemyController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            _controller.SetAnimationState("Idle");
            _idleTimer = 0f;
            _alertnessLevel = 0f;
            Debug.Log($"[{_controller.gameObject.name}] Estado Idle - aguardando jogador");
        }

        public void Update()
        {
            _idleTimer += Time.deltaTime;

            // TRANSIÇÃO PRIORIDADE 1: Pode ver o jogador
            // Tempo de reação rápido - inimigos soulslike são sempre vigilantes
            if (_controller.CanSeePlayer())
            {
                Debug.Log($"[{_controller.gameObject.name}] Deu! Detectou jogador - transitando para Chase!");
                _controller.StateMachine.TransitionTo(_controller.ChaseState);
                return;
            }

            // Comportamento passivo: permanecer em idle
            // Incrementar alertness poderia ser usado para comportamentos futuros
            // (ex: detecção de som, vibração do terreno, etc.)
        }

        public void Exit()
        {
            // Reset do alertness quando sai do idle
            _alertnessLevel = 0f;
        }
    }
}
