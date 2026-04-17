using UnityEngine;

namespace proscryption.Enemy.Refactor
{
    /// <summary>
    /// Sistema de Input Reading - detecta ações do jogador e reage
    /// 
    /// Responsabilidade Única: Monitorar ações do player e dispara eventos
    /// - Detecta cura/consumo de itens
    /// - Detecta esquivas (futuro)
    /// - Detecta reset de UI (futuro)
    /// - Permite que IA reaja em tempo real
    /// 
    /// Padrão Souls-like: punição para consumo irresponsável
    /// 
    /// ESCALABILIDADE FUTURA:
    /// - Detecção de dodge rolls
    /// - Detecção de parry/block
    /// - Detecção de power-ups
    /// - Sistema de "tells" (telegraphing) do inimigo baseado em ações do player
    /// - Diferentes reações por tipo de inimigo (agressivo, defensivo, etc)
    /// - Confidence system (quanto mais ameaça, mais reações)
    /// - Machine learning: aprende padrões do player
    /// </summary>
    public class EnemyInputReader
    {
        // ============================================================
        // CONSTANTES
        // ============================================================

        private const string ANIMATION_STATE_HEALING = "isHealing";
        private const string ANIMATION_STATE_USING_ITEM = "isUsingItem";
        private const string ANIMATION_STATE_POTION = "Potion";

        // ============================================================
        // CONFIGURAÇÃO
        // ============================================================

        private readonly float _detectionRange;
        private readonly float _healingDetectionCooldown;

        // ============================================================
        // DEPENDÊNCIAS
        // ============================================================

        private Transform _playerTransform;
        private Animator _playerAnimator;

        // ============================================================
        // ESTADO INTERNO
        // ============================================================

        private float _lastHealingReactionTime = -999f;
        private float _lastDodgeReactionTime = -999f;
        private bool _wasPlayerHealingLastFrame = false;

        // ============================================================
        // EVENTOS
        // ============================================================

        /// <summary>
        /// Disparado quando player começa a se curar
        /// Permite que IA reaja agressivamente
        /// </summary>
        public event System.Action OnPlayerHealing = delegate { };

        /// <summary>
        /// Disparado quando player para de se curar
        /// </summary>
        public event System.Action OnPlayerHealingEnded = delegate { };

        /// <summary>
        /// Disparado quando player esquiva/dodges
        /// </summary>
        public event System.Action OnPlayerDodged = delegate { };

        /// <summary>
        /// Disparado quando player volta ao menu/reset
        /// </summary>
        public event System.Action OnPlayerReset = delegate { };

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        /// <summary>
        /// Inicializa o sistema de Input Reading
        /// </summary>
        public EnemyInputReader(
            Transform playerTransform,
            float detectionRange = 15f,
            float healingDetectionCooldown = 3f)
        {
            _playerTransform = playerTransform ?? throw new System.ArgumentNullException(nameof(playerTransform));
            _detectionRange = detectionRange > 0 ? detectionRange : 15f;
            _healingDetectionCooldown = healingDetectionCooldown >= 0 ? healingDetectionCooldown : 3f;

            CachePlayerAnimator();
            ValidateParameters();
        }

        // ============================================================
        // MÉTODOS PÚBLICOS - UPDATE
        // ============================================================

        /// <summary>
        /// Atualiza o sistema (chamado a cada frame)
        /// Monitora ações do player
        /// </summary>
        public void Update()
        {
            if (!IsPlayerInRange())
            {
                _wasPlayerHealingLastFrame = false;
                return;
            }

            // CheckForPlayerHealing();
            // TODO: Adicionar CheckForPlayerDodging();
            // TODO: Adicionar CheckForPlayerReset();
        }

        // ============================================================
        // MÉTODOS PRIVADOS - DETECÇÃO DE HEALING
        // ============================================================

        /// <summary>
        /// Verifica se player está se curando
        /// Dispara evento se detectado (primeiro frame)
        /// </summary>
        private void CheckForPlayerHealing()
        {
            if (!IsPlayerAnimatorValid())
                return;

            bool isHealingNow = IsPlayerCurrentlyHealing();

            // Primeiro frame de cura
            if (isHealingNow && !_wasPlayerHealingLastFrame)
            {
                ReactToPlayerHealing();
            }
            // Último frame de cura
            else if (!isHealingNow && _wasPlayerHealingLastFrame)
            {
                OnPlayerHealingEnded?.Invoke();
                DebugLogHealingEnded();
            }

            _wasPlayerHealingLastFrame = isHealingNow;
        }

        /// <summary>
        /// Verifica se player está em animação de cura
        /// Suporta multiple animation states
        /// 
        /// ESCALABILIDADE:
        /// - Adicionar suporte a diferentes animadores
        /// - Integrar com AnimationEvent system
        /// - Detectar por layer e hash para melhor performance
        /// </summary>
        private bool IsPlayerCurrentlyHealing()
        {
            if (_playerAnimator == null)
                return false;

            // Checa múltiplos estados possíveis de cura
            if (_playerAnimator.GetBool(ANIMATION_STATE_HEALING))
                return true;

            if (_playerAnimator.GetBool(ANIMATION_STATE_USING_ITEM))
                return true;

            // Checa por nome do estado (fallback)
            AnimatorStateInfo stateInfo = _playerAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(ANIMATION_STATE_POTION))
                return true;

            return false;
        }

        /// <summary>
        /// Reage ao player se curando
        /// Dispara evento se cooldown passou
        /// </summary>
        private void ReactToPlayerHealing()
        {
            // Verifica cooldown (evita reação spam)
            if (!CanReactToHealing())
                return;

            _lastHealingReactionTime = Time.time;
            OnPlayerHealing?.Invoke();
            DebugLogHealingDetected();
        }

        /// <summary>
        /// Verifica se pode reagir a cura (cooldown)
        /// </summary>
        private bool CanReactToHealing()
        {
            return Time.time >= _lastHealingReactionTime + _healingDetectionCooldown;
        }

        // ============================================================
        // MÉTODOS PRIVADOS - VALIDAÇÃO E INICIALIZAÇÃO
        // ============================================================

        /// <summary>
        /// Cacheia o Animator do player para melhor performance
        /// </summary>
        private void CachePlayerAnimator()
        {
            if (_playerTransform == null)
            {
                Debug.LogWarning("[EnemyInputReader] Player transform é null!");
                return;
            }

            _playerAnimator = _playerTransform.GetComponent<Animator>();

            if (_playerAnimator == null)
                Debug.LogWarning("[EnemyInputReader] Player não possui Animator!");
        }

        /// <summary>
        /// Verifica se player está dentro do alcance de detecção
        /// </summary>
        private bool IsPlayerInRange()
        {
            if (_playerTransform == null)
                return false;

            // Implementação placeholder (precisa receber posição do controller)
            return true; // TODO: Passar position do inimigo para verificação
        }

        /// <summary>
        /// Verifica se player animator está válido
        /// </summary>
        private bool IsPlayerAnimatorValid()
        {
            return _playerAnimator != null && _playerAnimator.gameObject.activeSelf;
        }

        /// <summary>
        /// Valida parâmetros de configuração
        /// </summary>
        private void ValidateParameters()
        {
            if (_detectionRange <= 0f)
                Debug.LogWarning("[EnemyInputReader] detectionRange deve ser maior que 0");

            if (_healingDetectionCooldown < 0f)
                Debug.LogWarning("[EnemyInputReader] healingDetectionCooldown não deve ser negativo");
        }

        // ============================================================
        // MÉTODOS PÚBLICOS - QUERIES
        // ============================================================

        /// <summary>
        /// Retorna se player está se curando AGORA
        /// </summary>
        public bool IsPlayerHealing() => _wasPlayerHealingLastFrame;

        /// <summary>
        /// Retorna tempo do último reaction a cura
        /// </summary>
        public float GetTimeSinceLastHealingReaction() => Time.time - _lastHealingReactionTime;

        /// <summary>
        /// Retorna se pode reagir novamente a cura (cooldown passou)
        /// </summary>
        public bool CanReactToHealingAgain() => CanReactToHealing();

        // ============================================================
        // MÉTODOS DE CONFIGURAÇÃO FUTURA
        // ============================================================

        /// <summary>
        /// ESCALABILIDADE: Atualizar referência do player
        /// Útil se player for destruído e reaparecido
        /// </summary>
        public void UpdatePlayerReference(Transform newPlayerTransform)
        {
            _playerTransform = newPlayerTransform;
            CachePlayerAnimator();
        }

        /// <summary>
        /// ESCALABILIDADE: Diferentes sensibilidades de reação
        /// Airbnd level 1 = não reage. Level 5 = muito agressivo
        /// </summary>
        public void SetAggressionLevel(int level)
        {
            // TODO: Implementar diferentes níveis de agressão
            // Level 1: Ignora cura
            // Level 3: (padrão) reage a cura
            // Level 5: Reage a qualquer consumível
        }

        /// <summary>
        /// ESCALABILIDADE: Desativar temporariamente input reading
        /// Útil para cinemáticas, eventos especiais, etc
        /// </summary>
        private bool _isInputReadingEnabled = true;
        public void SetInputReadingEnabled(bool enabled) => _isInputReadingEnabled = enabled;

        // ============================================================
        // MÉTODOS DE DEBUG
        // ============================================================

        private void DebugLogHealingDetected()
        {
            Debug.Log("[EnemyInputReader] Input Reading: Player está se curando! Reagindo...");
        }

        private void DebugLogHealingEnded()
        {
            Debug.Log("[EnemyInputReader] Player parou de se curar");
        }

        // ============================================================
        // MÉTODOS PLACEHOLDER PARA FUTURA IMPLEMENTAÇÃO
        // ============================================================

        /// <summary>
        /// ESCALABILIDADE: Detecção de dodge rolls
        /// Reage pulando para trás ou atacando preemptivamente
        /// </summary>
        private void CheckForPlayerDodging()
        {
            // TODO: Implementar detecção de dodge
            // - Monitorar animação de dodge/roll
            // - Disparar OnPlayerDodged quando detectado
            // - Cooldown entre reações
        }

        /// <summary>
        /// ESCALABILIDADE: Detecção de reset/pause
        /// Permite que inimigo "resete" também
        /// </summary>
        private void CheckForPlayerReset()
        {
            // TODO: Implementar detecção de UI pause/menu
            // - Monitorar se player retornou ao menu
            // - Disparar OnPlayerReset
            // - Permitir que inimigo se recupere (reset HP, poise, etc)
        }

        /// <summary>
        /// ESCALABILIDADE: Sistema de confiança do inimigo
        /// Quanto mais confiante, mais reações e agressividade
        /// </summary>
        private float _enemyConfidence = 1f; // 0-3 (intimidado, normal, confiante)
        public void SetEnemyConfidence(float confidence)
        {
            _enemyConfidence = Mathf.Clamp01(confidence / 3f);
            // TODO: Usar confidence para multiplicar frequência de reações
        }
    }
}
