using UnityEngine;
using UnityEngine.InputSystem;

namespace proscryption
{
    public class PlayerAttack : MonoBehaviour
    {
        public bool isAttacking = false;
        #region Components Reference
        private CharacterInput _input;
        private Animator _animator;

        public BaseWeapon _currentWeapon;
        #endregion

        void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _input = GetComponent<CharacterInput>();

        }
        void Update()
        {
            isAttacking = _input.Attackinput;
            if (isAttacking)
            {
                OnAttack();
            }
        }

        private void OnAttack()
        {
            _animator.SetBool("isAttacking", true);
            _currentWeapon.OnAttack();
        }
        public void OnCanHit()
        {
            _currentWeapon.SetCanHitState(true);
        }
        public void OnAttackEnd()
        {
            _animator.SetBool("isAttacking", false);
            _currentWeapon.SetCanHitState(false);
            _currentWeapon.StopAttacking();
        }

    }
}
