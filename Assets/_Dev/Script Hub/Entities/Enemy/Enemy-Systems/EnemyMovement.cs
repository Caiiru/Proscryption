using UnityEngine;

namespace proscryption.Enemy.Refactor
{

    public class EnemyMovement : MonoBehaviour
    {
        // ============================================================
        // CONSTANTES
        // ============================================================

        private const float MOVEMENT_INPUT_THRESHOLD = 0.01f; // Evita drift de movimento
        private const float ROTATION_MIN_DISTANCE = 0.1f;    // Não rotaciona se muito perto

        // ============================================================
        // DEPENDÊNCIAS
        // ============================================================

        private Rigidbody _rigidbody;
        private float _baseSpeed;
        private float _rotationSpeed;

        // ============================================================
        // ESTADO INTERNO
        // ============================================================

        private Vector3 _currentVelocity = Vector3.zero;
        private Vector3 _targetDirection = Vector3.zero;
        private float _currentSpeedMultiplier = 1f;

        // ============================================================
        // EVENTOS
        // ============================================================

        /// <summary>
        /// Disparado quando inimigo começa a se mover
        /// </summary>
        public event System.Action OnMovementStarted = delegate { };

        /// <summary>
        /// Disparado quando inimigo para de se mover
        /// </summary>
        public event System.Action OnMovementStopped = delegate { };

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        /// <summary>
        /// Inicializa o sistema de movimento
        /// </summary>
        public void Setup(
            Rigidbody rigidbody,
            float baseSpeed = 5f,
            float rotationSpeed = 10f)
        {
            _rigidbody = rigidbody ?? throw new System.ArgumentNullException(nameof(rigidbody));
            _baseSpeed = baseSpeed > 0 ? baseSpeed : 5f;
            _rotationSpeed = rotationSpeed > 0 ? rotationSpeed : 10f;

            ValidateParameters();
        }

        public void MoveInDirection(Vector3 direction, float speedMultiplier = 1f)
        {
            if (direction.magnitude < MOVEMENT_INPUT_THRESHOLD)
            {
                StopMovement();
                return;
            }

            direction = direction.normalized;
            _targetDirection = direction;
            _currentSpeedMultiplier = Mathf.Max(0f, speedMultiplier);
            ApplyMovement(direction);
        }

        /// <summary>
        /// Move inimigo em direção ao alvo
        /// </summary>
        public void MoveTowards(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - GetPosition()).normalized;
            MoveInDirection(direction, 1f);
        }

        public void MoveAwayFrom(Vector3 targetPosition)
        {
            Vector3 direction = (GetPosition() - targetPosition).normalized;
            MoveInDirection(direction, 1f);
            RotateTowards(direction);
        }


        public void MoveCircularAround(Vector3 centerPosition, float circleDirection = 1f)
        {
            Vector3 positionRelativeToCenter = GetPosition() - centerPosition;

            // Calcula direção tangencial (perpendicular ao vetor para center)
            Vector3 tangentialDirection = Vector3.Cross(Vector3.up, positionRelativeToCenter).normalized;
            tangentialDirection *= circleDirection;

            MoveInDirection(tangentialDirection, 1f);
        }

        public void StopMovement()
        {
            if (_currentVelocity.magnitude > MOVEMENT_INPUT_THRESHOLD)
            {
                OnMovementStopped?.Invoke();
            }

            _currentVelocity = Vector3.zero;
            _targetDirection = Vector3.zero;
            _currentSpeedMultiplier = 0f;

            ApplyMovement(Vector3.zero);
        }

        public void RotateTowards(Vector3 targetPosition, float? tempRotationSpeed = null)
        {
            Vector3 directionToTarget = (targetPosition - GetPosition());

            // Ignora distância muito pequena
            if (directionToTarget.magnitude < ROTATION_MIN_DISTANCE)
                return;

            // Apenas rotaciona no eixo Y (horizontal)
            directionToTarget.y = 0f;
            directionToTarget = directionToTarget.normalized;

            if (directionToTarget == Vector3.zero)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            Quaternion newRotation = Quaternion.Slerp(
                _rigidbody.rotation,
                targetRotation,
                Time.deltaTime * (tempRotationSpeed ?? _rotationSpeed)
            );

            _rigidbody.rotation = newRotation;
        }

        /// <summary>
        /// Rotaciona inimigo instantaneamente para target
        /// (sem suavização)
        /// </summary>
        public void RotateTowardsInstant(Vector3 targetPosition)
        {
            Vector3 directionToTarget = (targetPosition - GetPosition());
            directionToTarget.y = 0f;

            if (directionToTarget.magnitude > ROTATION_MIN_DISTANCE)
            {
                _rigidbody.rotation = Quaternion.LookRotation(directionToTarget.normalized);
            }
        }

        // ============================================================
        // MÉTODOS PÚBLICOS - QUERIES
        // ============================================================

        /// <summary>
        /// Retorna posição atual do inimigo
        /// </summary>
        public Vector3 GetPosition() => _rigidbody.position;

        /// <summary>
        /// Retorna rotação atual do inimigo
        /// </summary>
        public Quaternion GetRotation() => _rigidbody.rotation;

        /// <summary>
        /// Retorna direção "forward" do inimigo
        /// </summary>
        public Vector3 GetForwardDirection() => _rigidbody.transform.forward;

        /// <summary>
        /// Retorna velocidade atual em magnitude
        /// </summary>
        public float GetCurrentSpeed() => _currentVelocity.magnitude;

        /// <summary>
        /// Retorna se está em movimento
        /// </summary>
        public bool IsMoving() => GetCurrentSpeed() > MOVEMENT_INPUT_THRESHOLD;

        /// <summary>
        /// Retorna direção de movimento alvo (normalizada)
        /// </summary>
        public Vector3 GetTargetDirection() => _targetDirection;

        // ============================================================
        // MÉTODOS PRIVADOS - APLICAR MOVIMENTO
        // ============================================================

        /// <summary>
        /// Aplica movimento via Rigidbody velocity
        /// 
        /// ESCALABILIDADE:
        /// - Adicionar parâmetro de cheque de obstáculos
        /// - Usar Rigidbody.AddForce ao invés de velocity (mais realista)
        /// - Integrar com animações
        /// </summary>
        private void ApplyMovement(Vector3 direction)
        {
            _currentVelocity = direction * _baseSpeed * _currentSpeedMultiplier;

            // Preserva apenas velocidade vertical (gravidade/saltos)
            _currentVelocity.y = _rigidbody.linearVelocity.y;

            _rigidbody.linearVelocity = _currentVelocity;

            RotateTowards(transform.position + direction, _rotationSpeed * 2f); // Rotaciona mais rápido durante movimento
            // Disparar evento se começou a se mover
            if (IsMoving())
                OnMovementStarted?.Invoke();
        }

        /// <summary>
        /// Valida parâmetros de configuração
        /// </summary>
        private void ValidateParameters()
        {
            if (_baseSpeed <= 0f)
                Debug.LogWarning("[EnemyMovement] baseSpeed deve ser maior que 0");

            if (_rotationSpeed <= 0f)
                Debug.LogWarning("[EnemyMovement] rotationSpeed deve ser maior que 0");
        }

        // ============================================================
        // MÉTODOS DE CONFIGURAÇÃO DINÂMICA
        // ============================================================

        /// <summary>
        /// ESCALABILIDADE: Modificar speed temporariamente (buff, debuff, etc)
        /// Exemplo: "Slow" reduz speed em 50% por 3 segundos
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            _currentSpeedMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// ESCALABILIDADE: Sistema de velocidade com stages
        /// Exemplo: Walk -> Run -> Sprint com diferentes speeds
        /// </summary>
        public void SetMovementStage(MovementStage stage)
        {
            switch (stage)
            {
                case MovementStage.Walk:
                    SetSpeedMultiplier(0.5f);
                    break;
                case MovementStage.Run:
                    SetSpeedMultiplier(1.0f);
                    break;
                case MovementStage.Sprint:
                    SetSpeedMultiplier(1.5f);
                    break;
            }
        }

        // ============================================================
        // TIPOS E ENUMS
        // ============================================================

        /// <summary>
        /// Diferentes stages de movimento
        /// 
        /// ESCALABILIDADE: Pode expandir para incluir:
        /// - Dodge, Dash, Stagger
        /// - Diferentes animações para cada stage
        /// - Diferentes custos de stamina/poise
        /// </summary>
        public enum MovementStage
        {
            Stop = 0,
            Walk = 1,
            Run = 2,
            Sprint = 3
        }

        // ============================================================
        // MÉTODOS DE DEBUG
        // ============================================================

        /// <summary>
        /// Desenha gizmos para visualizar movimento no editor
        /// </summary>
        public void DrawDebugGizmos(Vector3 position, Color directionColor, float arrowLength = 2f)
        {
            if (_targetDirection.magnitude > 0.01f)
            {
                Debug.DrawLine(position, position + _targetDirection * arrowLength, directionColor);
            }
        }
    }
}
