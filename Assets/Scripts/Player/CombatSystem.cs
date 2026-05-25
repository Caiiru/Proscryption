using System;
using Cysharp.Threading.Tasks;
using proscryption;
using UnityEngine;

namespace proscryption
{
    /// <summary>
    /// CombatSystem - Handles all attack logic
    /// Separated from movement (PlayerController) and animations (PlayerView)
    /// Subscribes to attack input and manages damage
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        [SerializeField] private const int ATTACK_STAMINA_COST = 10;


        // ===== REFERENCES =====
        private PlayerController _controller;
        private PlayerModel _model;
        [SerializeField] private BaseWeapon _currentWeapon;

        [Header("Attack Settings")] public LayerMask enemyMask;
        public float attackRaycastLimit = 5f;
        public Transform raycastStartTransform;

        public RaycastHit[] _raycastHits = new RaycastHit[3];

        // ===== STATE =====
        private float _lastAttackTime = 0f;

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _model = GetComponent<PlayerModel>();
            _currentWeapon = GetComponentInChildren<BaseWeapon>();

            if (_model == null)
                Debug.LogError("[CombatSystem] PlayerModel not found!", gameObject);

            if (_currentWeapon == null)
                Debug.LogWarning("[CombatSystem] BaseWeapon not found on children!", gameObject);


            _currentWeapon.GetComponent<BaseWeapon>().Setup(this);
        }


        /// <summary>
        /// Triggered by animation event, ensures damage is applied at the correct time in the animation
        /// </summary>
        private void ExecuteAttack()
        {
            _lastAttackTime = Time.time;

            PlayerEvents.BroadcastPlayerAttack();

            // Ray attackRay = new Ray(raycastStartTransform.position, raycastStartTransform.forward * attackRaycastLimit);
            // // Debug.DrawLine(raycastStartTransform.position,
            // //     raycastStartTransform.position + raycastStartTransform.forward * attackRaycastLimit,
            // //     Color.red, 1f);
            // Debug.DrawRay(raycastStartTransform.position, raycastStartTransform.forward * attackRaycastLimit);
            // if (Physics.Raycast(attackRay, attackRaycastLimit))
            // {
            //     Debug.Log("HIT RAYCAST");
            //     return;
            // }

            _currentWeapon.OnAttack();
        }

        public bool HasBullets()
        {
            return _currentWeapon.CanConsumeBullet();
        }

        public BaseWeapon GetWeapon()
        {
            return _currentWeapon;
        }

        public PlayerModel GetModel()
        {
            return _model;
        }

        public void InsertBullet()
        {
            Debug.Log("Insert Bullet");
            _currentWeapon.ReloadOneBullet();
        }
    }
}