using UnityEngine;

namespace proscryption.Enemy.Refactor
{
    /// <summary>
    /// Sistema de movimento baseado em CharacterController (CC)
    /// Substitui EnemyMovement para suportar melhor transição para ragdoll
    /// </summary>
    public class EnemyMovementCC : MonoBehaviour
    {
        // ============================================================
        // CONSTANTES
        // ============================================================

        private const float MOVEMENT_INPUT_THRESHOLD = 0.01f;
        private const float ROTATION_MIN_DISTANCE = 0.1f;
        private const float GRAVITY = -9.81f;

        // ============================================================
        // DEPENDÊNCIAS
        // ============================================================

        private CharacterController _characterController;
        private float _baseSpeed;
        private float _rotationSpeed;

        // ============================================================
        // ESTADO INTERNO
        // ============================================================

        private Vector3 _currentVelocity = Vector3.zero;
        private Vector3 _targetDirection = Vector3.zero;
        private float _currentSpeedMultiplier = 1f;
        private float _verticalVelocity = 0f;

        // ============================================================
        // EVENTOS
        // ============================================================

        public event System.Action OnMovementStarted = delegate { };
        public event System.Action OnMovementStopped = delegate { };

        // ============================================================
        // SETUP
        // ============================================================

        public void Setup(
            CharacterController characterController,
            float baseSpeed = 5f,
            float rotationSpeed = 10f)
        {
            _characterController = characterController ?? throw new System.ArgumentNullException(nameof(characterController));
            _baseSpeed = baseSpeed > 0 ? baseSpeed : 5f;
            _rotationSpeed = rotationSpeed > 0 ? rotationSpeed : 10f;

            ValidateParameters();
        }

        // ============================================================
        // MOVIMENTO
        // ============================================================

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

        // ============================================================
        // ROTAÇÃO
        // ============================================================

        public void RotateTowards(Vector3 targetPosition, float? tempRotationSpeed = null)
        {
            Vector3 directionToTarget = (targetPosition - GetPosition());

            if (directionToTarget.magnitude < ROTATION_MIN_DISTANCE)
                return;

            directionToTarget.y = 0f;
            directionToTarget = directionToTarget.normalized;

            if (directionToTarget == Vector3.zero)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            Quaternion newRotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * (tempRotationSpeed ?? _rotationSpeed)
            );

            transform.rotation = newRotation;
        }

        public void RotateTowardsInstant(Vector3 targetPosition)
        {
            Vector3 directionToTarget = (targetPosition - GetPosition());
            directionToTarget.y = 0f;

            if (directionToTarget.magnitude > ROTATION_MIN_DISTANCE)
            {
                transform.rotation = Quaternion.LookRotation(directionToTarget.normalized);
            }
        }

        // ============================================================
        // QUERIES
        // ============================================================

        public Vector3 GetPosition() => transform.position;

        public Quaternion GetRotation() => transform.rotation;

        public Vector3 GetForwardDirection() => transform.forward;

        public float GetCurrentSpeed() => _currentVelocity.magnitude;

        public bool IsMoving() => GetCurrentSpeed() > MOVEMENT_INPUT_THRESHOLD;

        public Vector3 GetTargetDirection() => _targetDirection;

        // ============================================================
        // APLICAR MOVIMENTO
        // ============================================================

        private void ApplyMovement(Vector3 direction)
        {
            _currentVelocity = direction * _baseSpeed * _currentSpeedMultiplier;

            // Aplica gravidade
            if (!_characterController.isGrounded)
            {
                _verticalVelocity += GRAVITY * Time.deltaTime;
            }
            else
            {
                _verticalVelocity = -0.5f; // Pequeno valor negativo para manter no chão
            }

            _currentVelocity.y = _verticalVelocity;

            _characterController.Move(_currentVelocity * Time.deltaTime);

            if (direction.magnitude > MOVEMENT_INPUT_THRESHOLD)
            {
                RotateTowards(transform.position + direction, _rotationSpeed * 2f);
                if (IsMoving())
                    OnMovementStarted?.Invoke();
            }
        }

        // ============================================================
        // CONFIGURAÇÃO
        // ============================================================

        private void ValidateParameters()
        {
            if (_baseSpeed <= 0f)
                Debug.LogWarning("[EnemyMovementCC] baseSpeed deve ser maior que 0");

            if (_rotationSpeed <= 0f)
                Debug.LogWarning("[EnemyMovementCC] rotationSpeed deve ser maior que 0");
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _currentSpeedMultiplier = Mathf.Max(0f, multiplier);
        }

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

        public enum MovementStage
        {
            Stop = 0,
            Walk = 1,
            Run = 2,
            Sprint = 3
        }

        // ============================================================
        // DEBUG
        // ============================================================

        public void DrawDebugGizmos(Vector3 position, Color directionColor, float arrowLength = 2f)
        {
            if (_targetDirection.magnitude > 0.01f)
            {
                Debug.DrawLine(position, position + _targetDirection * arrowLength, directionColor);
            }
        }
    }
}
