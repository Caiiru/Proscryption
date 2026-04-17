using UnityEngine;

namespace proscryption.Enemy
{
    /// <summary>
    /// Interface base para estados de inimigo
    /// </summary>
    public interface IEnemyState
    {
        /// <summary>
        /// Chamado quando o inimigo entra neste estado
        /// </summary>
        void Enter();

        /// <summary>
        /// Lógica de atualização do estado
        /// </summary>
        void Update();

        /// <summary>
        /// Chamado quando o inimigo sai deste estado
        /// </summary>
        void Exit();
    }
}
