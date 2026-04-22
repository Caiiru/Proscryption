using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using proscryption;
using UnityEngine;
using UnityEngine.VFX;

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
    private const string PARAM_TAKE_DAMAGE = "TakeDamage";
    private const string PARAM_IS_RELOADING = "isReloading";
    private const string PARAM_DIE = "die";



    [Header("VFX")]
    public VisualEffect takeDamageVFX;

    [Header("Material")]
    public SkinnedMeshRenderer meshRenderer;
    public float tattooAnimationDuration = 0.5f;
    public int stancesDelayMilliseconds = 200;

    [SerializeField] private Material _bodyMaterial;
    [SerializeField] private Material _detailsMaterial;
    // parameter material
    private const string PARAM_TATTO_ID = "_Tattoo_ID";
    private const string PARAM_ANIMATION_FACTOR = "_Animation_Factor";
    private const string PARAM_EYE_ID = "_Eye_ID";

    //Color




    void Awake()
    {
        _animator = GetComponent<Animator>();
        _model = GetComponent<PlayerModel>();
    }

    void OnEnable()
    {
        // Listen to state and event changes
        PlayerEvents.OnPlayerStateChanged += HandleStateChanged;
        PlayerEvents.OnPlayerAttack += HandleAttackPlayed;
        EventManager.OnEntityDamaged += HandleDamageTaken;
        EventManager.OnHitDetected += HandleHitDetected;

        PlayerEvents.OnPlayerStanceChanged += (oldStance, newStance) => { HandleStanceChanged(oldStance, newStance).Forget(); };
    }


    void OnDisable()
    {
        EventManager.OnHitDetected -= HandleHitDetected;
        PlayerEvents.OnPlayerStateChanged -= HandleStateChanged;
        PlayerEvents.OnPlayerAttack -= HandleAttackPlayed;
        EventManager.OnEntityDamaged -= HandleDamageTaken;
        PlayerEvents.OnPlayerStanceChanged -= (oldStance, newStance) => { HandleStanceChanged(oldStance, newStance).Forget(); };
    }
    void Start()
    {
        if (takeDamageVFX)
        {
            takeDamageVFX.Stop();
        }
        SetupStart();


    }

    private void SetupStart()
    {

        _bodyMaterial.SetFloat(PARAM_TATTO_ID, 0);
        _bodyMaterial.SetFloat(PARAM_ANIMATION_FACTOR, 0);
        _detailsMaterial.SetFloat(PARAM_EYE_ID, 0);
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
        _animator.SetBool(PARAM_IS_RELOADING, false);

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
                this.gameObject.SetActive(false);
                break;


            case PlayerState.Reloading:
                _animator.SetBool(PARAM_IS_RELOADING, true);
                break;
        }
    }

    /// <summary>
    /// React to player attack event
    /// </summary>
    private void HandleAttackPlayed()
    {
        // VFX, SFX, and animations handled via state change above
        // Debug.Log($"[PlayerView] Attack played - {damage} damage", gameObject);
    }

    /// <summary>
    /// React to damage received
    /// </summary>
    private void HandleDamageTaken(int damage, GameObject damageSource)
    {
        // Only if damage was to this player
        // if (damageSource == gameObject || damageSource.GetComponent<BaseWeapon>()?.transform.parent != transform)
        //     return;



        // Debug.Log($"[PlayerView] Hit animation played", gameObject);
    }
    private void HandleHitDetected(Vector3 hitPos, int damage, GameObject target)
    {

        if (target != gameObject) return;
        if (!_model.IsAlive) return;
        if (_model.IsInvulnerable) return;

        Debug.Log("VIEW - TAKE DAMAGE");
        if (_model.CurrentState != PlayerState.Attacking)
            _animator.SetTrigger(PARAM_TAKE_DAMAGE);

        if (takeDamageVFX)
        {
            takeDamageVFX.Play();
        }


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

    public async UniTask HandleStanceChanged(PlayerStance oldStance, PlayerStance newStance)
    {
        if (newStance == PlayerStance.Standard)
        {
            //DisableTattoo
            _bodyMaterial.DOFloat(0, PARAM_ANIMATION_FACTOR, tattooAnimationDuration).onComplete = () =>
            {
                _bodyMaterial.SetFloat(PARAM_TATTO_ID, 0);
                _detailsMaterial.SetFloat(PARAM_EYE_ID, 0);
            };
            return;
        }


        if (oldStance == PlayerStance.Standard)
        {
            // _bodyMaterial.SetFloat(PARAM_ANIMATION_FACTOR, 1);
            //Enable Tattoo
            _bodyMaterial.DOFloat(1, PARAM_ANIMATION_FACTOR, tattooAnimationDuration);
        }
        else
        {
            _bodyMaterial.DOFloat(0, PARAM_ANIMATION_FACTOR, tattooAnimationDuration / 2);
            _detailsMaterial.SetFloat(PARAM_EYE_ID, 0);
        }
        await UniTask.Delay(stancesDelayMilliseconds / 2);

        _bodyMaterial.DOFloat(1, PARAM_ANIMATION_FACTOR, tattooAnimationDuration);

        if (newStance == PlayerStance.Blood)
        {
            _bodyMaterial.SetFloat(PARAM_TATTO_ID, 2);
            _detailsMaterial.SetFloat(PARAM_EYE_ID, 2);
        }
        else
        {
            _detailsMaterial.SetFloat(PARAM_EYE_ID, 1);
            _bodyMaterial.SetFloat(PARAM_TATTO_ID, 1);

        }
    }

}
