using System;
using proscryption;
using UnityEngine;

/// <summary>
/// PlayerView - MVC View Layer
/// Handles all animations and visual feedback
/// NEVER initiates actions - only reacts to events
/// Uses constants for animator parameters (no magic strings)
/// </summary>
public class PlayerView : MonoBehaviour
{
    private Animator _animator;
    private PlayerModel _model;

    // Animator parameter constants (no magic strings!) 
    private const string PARAM_IS_MOVING = "isMoving";
    private const string PARAM_IS_INPUTING_TO_MOVE = "isMoveInput";
    private const string PARAM_IS_ATTACKING = "isAttacking";
    private const string PARAM_IS_ROLLING = "isRolling";
    private const string PARAM_TAKE_DAMAGE = "takeDamage";
    private const string PARAM_DIE = "die";

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _model = GetComponent<PlayerModel>();
    }

    void OnEnable()
    {
        // Listen to state and event changes
        EventManager.OnPlayerStateChanged += HandleStateChanged;
        EventManager.OnPlayerAttack += HandleAttackPlayed;
        EventManager.OnEntityDamaged += HandleDamageTaken;
        EventManager.OnPlayerHealthChanged += HandleHealthChanged;
    }

    void OnDisable()
    {
        EventManager.OnPlayerStateChanged -= HandleStateChanged;
        EventManager.OnPlayerAttack -= HandleAttackPlayed;
        EventManager.OnEntityDamaged -= HandleDamageTaken;
        EventManager.OnPlayerHealthChanged -= HandleHealthChanged;
    }
    // ===== STATE CHANGE HANDLERS =====

    /// <summary>
    /// React to player state changes with appropriate animations
    /// </summary>
    private void HandleStateChanged(PlayerState prev, PlayerState next)
    {
        // Clear all state animations first
        _animator.SetBool(PARAM_IS_MOVING, false);
        _animator.SetBool(PARAM_IS_ATTACKING, false);
        _animator.SetBool(PARAM_IS_ROLLING, false);

        // Set new animation state
        switch (next)
        {
            case PlayerState.Idle:
                // No animation needed, idle is default
                break;

            case PlayerState.Moving:
                _animator.SetBool(PARAM_IS_MOVING, true);
                break;

            case PlayerState.Attacking:

                _animator.SetBool(PARAM_IS_ATTACKING, true);
                break;

            case PlayerState.Rolling:
                _animator.SetBool(PARAM_IS_ROLLING, true);
                break;

            case PlayerState.Dead:
                _animator.SetTrigger(PARAM_DIE);
                break;

            case PlayerState.Stunned:
                // Could add stun animation here
                break;
        }
    }

    /// <summary>
    /// React to player attack event
    /// </summary>
    private void HandleAttackPlayed(int damage)
    {
        // VFX, SFX, and animations handled via state change above
        Debug.Log($"[PlayerView] Attack played - {damage} damage", gameObject);
    }

    /// <summary>
    /// React to damage received
    /// </summary>
    private void HandleDamageTaken(int damage, GameObject damageSource)
    {
        // Only if damage was to this player
        if (damageSource == gameObject || damageSource.GetComponent<BaseWeapon>()?.transform.parent != transform)
            return;

        _animator.SetTrigger(PARAM_TAKE_DAMAGE);
        Debug.Log($"[PlayerView] Hit animation played", gameObject);
    }

    /// <summary>
    /// React to health changes (could change UI color, visual indicator)
    /// </summary>
    private void HandleHealthChanged(int newHealth, int maxHealth)
    {
        float healthPercent = (float)newHealth / maxHealth;
        Debug.Log($"[PlayerView] Health changed: {newHealth}/{maxHealth} ({healthPercent:P0})", gameObject);
    }

    public void UpdateInputAnimation(Vector2 moveInput)
    {
        // Debug.Log(moveInput);
        if (moveInput.magnitude > 0.1f)
        {
            _animator.SetBool(PARAM_IS_INPUTING_TO_MOVE, true);
        }
        else
        {
            _animator.SetBool(PARAM_IS_INPUTING_TO_MOVE, false);
        }
    }

    // ===== PUBLIC METHODS (Controllers can call these) =====

    /// <summary>
    /// Update movement animation based on velocity
    /// Called from PlayerController with world velocity
    /// </summary>
    // public void UpdateMovementAnimation(Vector3 worldVelocity)
    // {
    //     if (_animator == null) return;

    //     // Convert world velocity to local coordinates for animation blending
    //     Vector3 localVelocity = transform.parent != null ? 
    //         transform.parent.TransformDirection(worldVelocity) : worldVelocity;

    //     // Project onto local axes
    //     float velocityX = Vector3.Dot(localVelocity, transform.right);
    //     float velocityY = Vector3.Dot(localVelocity, transform.forward);

    //     _animator.SetFloat(PARAM_VELOCITY_X, velocityX);
    //     _animator.SetFloat(PARAM_VELOCITY_Y, velocityY);
    // }
}
