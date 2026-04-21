using UnityEngine;

namespace proscryption.Enemy.Refactor
{
    /// <summary>
    /// Sistema de Poise (Postura) - implementação Souls-like
    /// 
    /// Responsabilidade Única: Gerenciar a vida de poise do inimigo
    /// - Receber dano de poise
    /// - Regenerar poise gradualmente
    /// - Disparar eventos quando poise quebra
    /// 
    /// ESCALABILIDADE FUTURA:
    /// - Adicionar diferentes tipos de poise (equipment, buffs)
    /// - Sistema de "armor" que reduz poise damage
    /// - Poise stage progression (15% -> 30% -> 50% -> stagger)
    /// - Resistência temporal (poise regenera mais rápido com tempo)
    /// - Multiplicadores baseados em tipo de dano (blunt, slash, etc)
    /// </summary>
    public class EnemyPoiseSystem
    {
        // ============================================================
        // CONSTANTES CONFIGURÁVEIS
        // ============================================================

        private readonly float _maxPoise;
        private readonly float _poiseRegenerationRate;
        private readonly float _damageToPoiseDamageMultiplier;

        // ============================================================
        // ESTADO INTERNO
        // ============================================================

        private float _currentPoise;
        private float _lastDamageTime = -999f;
        private const float POISE_REGENERATION_DELAY = 0f; // delay antes de começar regenerar (0 = imediato)

        // ============================================================
        // EVENTOS
        // ============================================================

        /// <summary>
        /// Disparado quando poise chega a 0 (antes de transição para Stagger)
        /// Permite que outros sistemas reajam (feedback de audio, visual, etc)
        /// </summary>
        public event System.Action OnPoiseBreak = delegate { };

        /// <summary>
        /// Disparado quando poise é parcialmente danificado (útil para UI)
        /// </summary>
        public event System.Action<float, float> OnPoiseDamaged = delegate { };

        /// <summary>
        /// Disparado quando poise é regenerado
        /// </summary>
        public event System.Action<float, float> OnPoiseRegenerated = delegate { };

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        /// <summary>
        /// Inicializa o sistema de poise com valores configuráveis
        /// </summary>
        public EnemyPoiseSystem(
            float maxPoise = 100f,
            float poiseRegenerationRate = 5f,
            float damageToPoiseDamageMultiplier = 0.5f)
        {
            _maxPoise = maxPoise;
            _poiseRegenerationRate = poiseRegenerationRate;
            _damageToPoiseDamageMultiplier = damageToPoiseDamageMultiplier;

            // Inicia com poise máxima
            _currentPoise = _maxPoise;

            ValidateParameters();
        }

        // ============================================================
        // MÉTODOS PÚBLICOS - APLICAR DANO
        // ============================================================

        /// <summary>
        /// Aplica dano de poise direto ao inimigo
        /// 
        /// ESCALABILIDADE:
        /// - Adicionar parâmetro de tipo de dano (DamageType enum)
        /// - Multiplicadores baseados em tipo de armadura
        /// </summary>
        public void TakePoiseDamage(float damageAmount)
        {
            if (damageAmount < 0f)
                damageAmount = 0f;

            _currentPoise -= damageAmount;
            _lastDamageTime = Time.time;

            // Clamp para não ficar com poise negativo
            if (_currentPoise < 0f)
                _currentPoise = 0f;

            OnPoiseDamaged?.Invoke(_currentPoise, _maxPoise);

            // Log detalhado para debugging
            DebugLogPoiseDamage(damageAmount);

            // Verifica se poise quebrou
            if (IsPoiseBreached())
                OnPoiseBreak?.Invoke();
        }

        /// <summary>
        /// Converte dano recebido em dano de poise
        /// Utilizado quando inimigo recebe hit do jogador
        /// 
        /// ESCALABILIDADE:
        /// - Passar DamageInfo com tipo de dano, weapon, etc
        /// - Aplicar resistências/fraquezas baseadas em tipo
        /// </summary>
        public void ApplyPoiseDamageFromHit(float damageReceived)
        {
            if (damageReceived < 0f)
                return;

            float poiseDamage = damageReceived * _damageToPoiseDamageMultiplier;
            TakePoiseDamage(poiseDamage);
        }

        // ============================================================
        // MÉTODOS PÚBLICOS - RECUPERAÇÃO
        // ============================================================

        /// <summary>
        /// Reseta poise para o máximo (após Stagger, break, etc)
        /// </summary>
        public void ResetPoise()
        {
            _currentPoise = _maxPoise;
            _lastDamageTime = -999f;

            DebugLogPoiseReset();
        }

        /// <summary>
        /// Atualiza o sistema (chamado a cada frame)
        /// Responsável por regenerar poise gradualmente
        /// </summary>
        public void Update()
        {
            RegeneratePoise();
        }

        // ============================================================
        // MÉTODOS PÚBLICOS - QUERIES
        // ============================================================

        /// <summary>
        /// Retorna poise atual (0-maxPoise)
        /// </summary>
        public float GetCurrentPoise() => _currentPoise;

        /// <summary>
        /// Retorna poise máxima
        /// </summary>
        public float GetMaxPoise() => _maxPoise;

        /// <summary>
        /// Retorna percentual de poise (0-100)
        /// Útil para UI
        /// </summary>
        public float GetPoisePercentage() => (_currentPoise / _maxPoise) * 100f;

        /// <summary>
        /// Verifica se poise foi completamente quebrado
        /// </summary>
        public bool IsPoiseBreached() => _currentPoise <= 0f;

        /// <summary>
        /// Verifica se poise está em estado crítico (20% ou menos)
        /// 
        /// ESCALABILIDADE:
        /// - Adicionar threshold configurável
        /// - Adicionar estados intermediários de poise
        /// </summary>
        public bool IsPoiseInCriticalState() => GetPoisePercentage() <= 20f;

        /// <summary>
        /// Retorna tempo desde último dano recebido
        /// Útil para regeneração com delay
        /// </summary>
        public float GetTimeSinceLastDamage() => Time.time - _lastDamageTime;

        // ============================================================
        // MÉTODOS PRIVADOS - LÓGICA INTERNA
        // ============================================================

        /// <summary>
        /// Regenera poise gradualmente (padrão Souls-like)
        /// 
        /// ESCALABILIDADE:
        /// - Adicionar curva de regeneração (suave, rápida, etc)
        /// - Regeneração condicional (só fora de combate, etc)
        /// - Buffs que modificam taxa de regeneração
        /// </summary>
        private void RegeneratePoise()
        {
            // Se poise está quebrado, não regenera (precisa de reset)
            if (IsPoiseBreached())
                return;

            // Se já tem poise máxima, não faz nada
            if (_currentPoise >= _maxPoise)
                return;

            // Verifica delay antes de regenerar (se implementado)
            if (GetTimeSinceLastDamage() < POISE_REGENERATION_DELAY)
                return;

            _currentPoise += _poiseRegenerationRate * Time.deltaTime;

            // Garante que não ultrapassa máximo
            if (_currentPoise > _maxPoise)
                _currentPoise = _maxPoise;

            OnPoiseRegenerated?.Invoke(_currentPoise, _maxPoise);
        }

        /// <summary>
        /// Valida parâmetros de configuração
        /// Previne valores inválidos no construtor
        /// </summary>
        private void ValidateParameters()
        {
            if (_maxPoise <= 0f)
                Debug.LogWarning("[EnemyPoiseSystem] maxPoise deve ser maior que 0");

            if (_poiseRegenerationRate < 0f)
                Debug.LogWarning("[EnemyPoiseSystem] poiseRegenerationRate não deve ser negativo");

            if (_damageToPoiseDamageMultiplier < 0f)
                Debug.LogWarning("[EnemyPoiseSystem] damageToPoiseDamageMultiplier não deve ser negativo");
        }

        // ============================================================
        // MÉTODOS DE DEBUG
        // ============================================================

        private void DebugLogPoiseDamage(float damageAmount)
        {
            Debug.Log($"[EnemyPoiseSystem] Poise danificado: {_currentPoise:F1}/{_maxPoise:F1} (- {damageAmount:F1})");
        }

        private void DebugLogPoiseReset()
        {
            Debug.Log($"[EnemyPoiseSystem] Poise resetada para {_maxPoise}");
        }

        // ============================================================
        // MÉTODOS DE CONFIGURAÇÃO FUTURA
        // ============================================================

        /// <summary>
        /// ESCALABILIDADE: Modificadores temporários de poise
        /// Exemplo: buff que aumenta regeneração por 10 segundos
        /// </summary>
        private float GetRegenerationMultiplier()
        {
            // TODO: Implementar sistema de buffs/debuffs
            return 1f;
        }

        /// <summary>
        /// ESCALABILIDADE: Resistência baseada em tipo de dano
        /// </summary>
        private float GetDamageToDamageResistance(string damageType = "generic")
        {
            // TODO: Implementar sistema de resistências por tipo de dano
            return _damageToPoiseDamageMultiplier;
        }
    }
}
