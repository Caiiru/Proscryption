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



        }


        /// <summary>
        /// Triggered by animation event, ensures damage is applied at the correct time in the animation
        /// </summary>
        private void ExecuteAttack()
        {
            _lastAttackTime = Time.time;
            _currentWeapon.OnAttack();

            PlayerEvents.BroadcastPlayerAttack();

        }
        /// <summary>
        /// Triggered by animation event 
        /// </summary>
        private void ExecuteReload()
        {
            _currentWeapon.ReloadInput();
        }

        public BaseWeapon GetWeapon()
        {
            return _currentWeapon;
        }
    }
}
