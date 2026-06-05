using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
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

    private const string PARAM_X_VELOCITY = "MoveX";
    private const string PARAM_Y_VELOCITY = "MoveY";
    private const string PARAM_IS_WALKING = "Is Walking";
    private const string PARAM_IS_INPUTING_TO_MOVE = "isMoveInput";
    private const string PARAM_IS_AIMING = "Is Aiming";
    private const string PARAM_IS_ATTACKING = "Shooting";
    private const string PARAM_IS_ROLLING = "isRolling";
    private const string PARAM_TAKE_DAMAGE = "Take Damage";
    private const string PARAM_IS_RELOADING = "Is Reloading";
    private const string PARAM_STOP_RELOADING = "Stop Reloading";
    private const string PARAM_INSERT_BULLET = "InsertBullet";
    private const string PARAM_DIE = "Death";

    private const string PARAM_IS_RUNNING = "IsRunning";
    private const string PARAM_TRIGGER_RUNNING_INPUT = "isRunning_Trigger";

    private const string PARAM_BLOOD_STANCE = "BloodMode";
    private const string PARAM_FAITH_STANCE = "FaithMode";

    private bool _isRunning = false;

    [Header("VFX")] public VisualEffect takeDamageVFX;
    public GameObject EnterBloodStanceVFX;
    public GameObject EnterFaithStanceVFX;
    [Header("Material")] public SkinnedMeshRenderer meshRenderer;
    public float tattooAnimationDuration = 0.5f;
    public int stancesDelayMilliseconds = 200;

    [SerializeField] private Material _bodyMaterial;
    [SerializeField] private Material _detailsMaterial;

    // parameter material
    private const string PARAM_TATTO_ID = "_Tattoo_ID";
    private const string PARAM_ANIMATION_FACTOR = "_Animation_Factor";
    private const string PARAM_EYE_ID = "_Eye_ID";


    void Awake()
    {
        _animator = GetComponent<Animator>();
        _model = GetComponent<PlayerModel>();
    }

    void OnEnable()
    {
        // Listen to state and event changes
        PlayerEvents.OnPlayerStateChanged += HandleStateChanged;
        PlayerEvents.OnPlayerStanceChanged += OnPlayerStanceChangedEvent;
    }

    void Unregister()
    {
        PlayerEvents.OnPlayerStateChanged -= HandleStateChanged;
        PlayerEvents.OnPlayerStanceChanged -= OnPlayerStanceChangedEvent;
        _animator = null;
    }

    void OnDisable()
    {
        Unregister();
    }

    private void OnDestroy()
    {
        Unregister();
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
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _bodyMaterial = meshRenderer.materials[1];
        _detailsMaterial = meshRenderer.materials[0];

        if (_bodyMaterial != null)
            _bodyMaterial.SetFloat(PARAM_TATTO_ID, 0);
        if (_bodyMaterial != null)
            _bodyMaterial.SetFloat(PARAM_ANIMATION_FACTOR, 0);
        if (_detailsMaterial != null)
            _detailsMaterial.SetFloat(PARAM_EYE_ID, 0);
    }
    // ===== STATE CHANGE HANDLERS =====

    private void OnPlayerStanceChangedEvent(PlayerStance oldStance, PlayerStance newStance)
    {
        HandleStanceChanged(oldStance, newStance).Forget();
    }

    /// <summary>
    /// React to player state changes with appropriate animations
    /// </summary>
    private void HandleStateChanged(PlayerState prev, PlayerState next)
    {
        // Clear all state animations first 
        _animator.SetBool(PARAM_IS_ATTACKING, false);
        if (prev == PlayerState.Reloading)
        {
            // _animator.SetTrigger(PARAM_STOP_RELOADING);
        }

        // Set new animation state
        switch (next)
        {
            case PlayerState.Idle:
                // No animation needed, idle is default
                break;

            case PlayerState.Moving:
                break;

            case PlayerState.Attacking:

                _animator.SetBool(PARAM_IS_ATTACKING, true);
                break;

            case PlayerState.Running:
                break;

            case PlayerState.Dead:
                _animator.SetTrigger(PARAM_DIE);
                //this.gameObject.SetActive(false);
                break;


            case PlayerState.Reloading:
                _animator.SetBool(PARAM_IS_RELOADING, true);
                break;
        }
    }


    public void TakeDamageAnimation()
    {
        Debug.Log("VIEW - TAKE DAMAGE");
        if (_model.CurrentState != PlayerState.Attacking)
            _animator.SetTrigger(PARAM_TAKE_DAMAGE);

        if (takeDamageVFX)
        {
            takeDamageVFX.Play();
        }
    }

    public void UpdateInputAnimation(Vector2 moveInput, Vector3 lookingDirection)
    {
        Vector3 worldMoveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 localLookDir = transform.InverseTransformDirection(worldMoveDirection);

        bool isWalking = moveInput.x != 0 || moveInput.y != 0;
        _animator.SetBool(PARAM_IS_WALKING, isWalking);
        _animator.SetFloat(PARAM_X_VELOCITY, localLookDir.x);
        _animator.SetFloat(PARAM_Y_VELOCITY, localLookDir.z);
    }

    public async UniTask HandleStanceChanged(PlayerStance oldStance, PlayerStance newStance)
    {
        if (newStance == PlayerStance.Standard)
        {
            //DisableTattoo
            if (_bodyMaterial == null) return;
            if (_detailsMaterial == null) return;

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
            _animator.SetTrigger(PARAM_BLOOD_STANCE);
            if (EnterBloodStanceVFX == null) return;
            EnterBloodStanceVFX.SetActive(true);
            VisualEffect vfx = GetVisualEffect(EnterBloodStanceVFX);
            if (vfx)
            {
                vfx.Play();
            }
        }
        else
        {
            _detailsMaterial.SetFloat(PARAM_EYE_ID, 1);
            _bodyMaterial.SetFloat(PARAM_TATTO_ID, 1);
            _animator.SetTrigger(PARAM_FAITH_STANCE);
            if (EnterFaithStanceVFX == null) return;
            EnterFaithStanceVFX.SetActive(true);
            VisualEffect vfx = GetVisualEffect(EnterFaithStanceVFX);
            if (vfx)
            {
                vfx.Play();
            }
        }
    }

    public void SetAiming(bool aiming)
    {
        if (_animator)
            _animator.SetBool(PARAM_IS_AIMING, aiming);
    }

    public void InsertBulletVisual()
    {
        //_animator.SetTrigger(PARAM_INSERT_BULLET);
    }

    public void StopReloading()
    {
        _animator.SetBool(PARAM_IS_RELOADING, false);
    }

    public void SetRunning(bool isRunning)
    {
        //_isRunning is just to save the trigger to animator override
        //cause is any state - so need trigger just when first run input
        if (!_isRunning && isRunning)
        {
            _animator.SetTrigger(PARAM_TRIGGER_RUNNING_INPUT);
        }

        if (!isRunning)
        {
            _isRunning = false;
        }

        _animator.SetBool(PARAM_IS_RUNNING, isRunning);
    }

    public void RollAnimation(Vector2 moveInput)
    {
        // Vector3 worldMoveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        // Vector3 localLookDir = transform.InverseTransformDirection(worldMoveDirection);
        //
        // Debug.Log(localLookDir);
        //
        // // Caso não tenha nenhum input, evita rodar a lógica (ou define um padrão)
        // if (localLookDir == Vector3.zero)
        // {
        //     _animator.SetTrigger(PARAM_FORWARD_DASH);
        //     return;
        // }
        //
        // // 1. Verifica se o movimento horizontal (X) é maior que o vertical (Y)
        // if (Mathf.Abs(localLookDir.x) > Mathf.Abs(localLookDir.z))
        // {
        //     // Movimento predominantemente Horizontal
        //     if (localLookDir.x > 0)
        //     {
        //         _animator.SetTrigger(PARAM_RIGHT_DASH);
        //     }
        //     else
        //     {
        //         _animator.SetTrigger(PARAM_LEFT_DASH);
        //     }
        // }
        // else
        // {
        //     // Movimento predominantemente Vertical
        //     if (localLookDir.z > 0)
        //     {
        //         _animator.SetTrigger(PARAM_FORWARD_DASH);
        //     }
        //     else
        //     {
        //         _animator.SetTrigger(PARAM_BACKWRD_DASH);
        //     }
        // }
    }

    [CanBeNull]
    VisualEffect GetVisualEffect(GameObject vfxHolder)
    {
        vfxHolder.TryGetComponent<VisualEffect>(out VisualEffect vfx);
        if (vfx != null)
        {
            return vfx;
        }

        VisualEffect vfx2 = vfxHolder.GetComponentInChildren<VisualEffect>();
        return vfx2 != null ? vfx2 : null;
    }

    [CanBeNull]
    ParticleSystem GetParticleSystem(GameObject vfxHolder)
    {
        vfxHolder.TryGetComponent<ParticleSystem>(out ParticleSystem vfx);
        if (vfx != null)
        {
            return vfx;
        }

        ParticleSystem vfx2 = vfxHolder.GetComponentInChildren<ParticleSystem>();
        return vfx2 != null ? vfx2 : null;
    }
}