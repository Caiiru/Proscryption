using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Gerenciador da máquina de estados para inimigos
    /// </summary>
    public class EnemyStateMachine
    {
        private IEnemyState _currentState;
        private IEnemyState _previousState;

        public IEnemyState CurrentState => _currentState;
        public IEnemyState PreviousState => _previousState;

        /// <summary>
        /// Inicializa a state machine com um estado inicial
        /// </summary>
        public void Initialize(IEnemyState initialState)
        {
            _currentState = initialState;
            _currentState.Enter();
        }

        /// <summary>
        /// Transiciona para um novo estado
        /// </summary>
        public void TransitionTo(IEnemyState newState)
        {
            if (_currentState == newState)
                return;

            // Debug.Log($"Transição de estado: {_currentState?.GetType().Name ?? "NULO"} -> {newState.GetType().Name}");

            _previousState = _currentState;
            _currentState.Exit();

            _currentState = newState;
            _currentState.Enter();

            
        }

        /// <summary>
        /// Atualiza o estado atual
        /// </summary>
        public void Update()
        {
            _currentState?.Update();
        }
    }
}
