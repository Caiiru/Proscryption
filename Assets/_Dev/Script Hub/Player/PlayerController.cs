using UnityEngine;

namespace proscryption
{
    public class PlayerController : MonoBehaviour
    {


        [Header("Player Settings")]
        [SerializeField] private float movementSpeed = 6;
        [SerializeField] private float rollForce = 10;
        [SerializeField] private float rollCooldown = 1f;

        #region Internal Variables
        private bool _isRolling;
        private float _rollCooldownTimer;
        #endregion

        #region Components Refs
        protected Transform _transform;
        protected CharacterInput _input;
        protected Transform _mainCameraTransform;

        protected Rigidbody _rigidbody;

        protected Animator _animator;
        #endregion
        void Start()
        {
            Setup();
        }

        void Setup()
        {
            _transform = GetComponent<Transform>();
            _input = GetComponent<CharacterInput>();
            _mainCameraTransform = Camera.main.transform;
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
        }

        protected Vector3 CalculateMovementDirection()
        {
            if (!_input)
            {
                return Vector3.zero;
            }


            Vector3 _velocity = Vector3.zero;

            _velocity += Vector3.ProjectOnPlane(_mainCameraTransform.transform.right, _transform.up).normalized * _input.MoveInput.x;
            _velocity += Vector3.ProjectOnPlane(_mainCameraTransform.transform.forward, _transform.up).normalized * _input.MoveInput.y;

            if (_velocity.magnitude > 1)
            {
                _velocity = _velocity.normalized;
            }


            return _velocity;
        }
        protected void MovementAnimation(Vector3 _velocity, float movementSpeed)
        {
            if (_velocity.magnitude == 0)
            {
                _animator.SetBool("isMoving", false);
                _animator.speed = 1;
                return;
            }
            else
                _animator.SetBool("isMoving", true);

            _animator.SetFloat("velocityX", _input.MoveInput.x);
            _animator.SetFloat("velocityY", _input.MoveInput.y);

            _animator.speed = movementSpeed / 2;
        }

        protected Vector3 CalculateMovementVelocity()
        {
            Vector3 _velocity = CalculateMovementDirection();

            MovementAnimation(_velocity, movementSpeed);


            _velocity *= movementSpeed;
            return _velocity;

        }

        protected Vector3 CalculateRollDirection()
        {
            if (!_input || !_isRolling) return Vector3.zero;

            Vector3 _rollDirection = Vector3.zero;

            _rollDirection += Vector3.ProjectOnPlane(_mainCameraTransform.transform.right, _transform.up).normalized * _input.MoveInput.x;
            _rollDirection += Vector3.ProjectOnPlane(_mainCameraTransform.transform.forward, _transform.up).normalized * _input.MoveInput.y;

            if (_rollDirection.magnitude == 0)
            {
                _rollDirection = _mainCameraTransform.forward;
            }

            return _rollDirection.normalized;
        }

        protected Vector3 CalculateRollVelocity()
        {
            Vector3 _rollVelocity = CalculateRollDirection();
            _rollVelocity *= rollForce;
            return _rollVelocity;
        }
        void FixedUpdate()
        {
            UpdateRollCooldown();
            _rigidbody.linearVelocity = CalculateMovementVelocity();

            // Verificar se deve fazer roll
            if (_input.RollInput && _rollCooldownTimer <= 0)
            {
                _isRolling = true;
                _rollCooldownTimer = rollCooldown;
            }

            if (_isRolling)
            {
                ExecuteRoll();
            }
        }

        private void UpdateRollCooldown()
        {
            if (_rollCooldownTimer > 0)
            {
                _rollCooldownTimer -= Time.fixedDeltaTime;
            }
        }

        private void ExecuteRoll()
        {
            Vector3 rollVelocity = CalculateRollVelocity();
            _rigidbody.linearVelocity = new Vector3(rollVelocity.x, _rigidbody.linearVelocity.y, rollVelocity.z);
            _isRolling = false;
        }

    }
}
