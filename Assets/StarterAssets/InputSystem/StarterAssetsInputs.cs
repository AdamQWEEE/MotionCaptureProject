using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool attack;
		public bool sneak;
		public bool roll;
		public bool defense;
		public bool stab;
		public bool toss;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

        public void OnAttack(InputValue value)
        {
            AttackInput(value.isPressed);
        }

        public void OnDefense(InputValue value)
        {
            DefenseInput(value.isPressed);
        }

        public void OnSneak(InputValue value)
		{
			SneakInput(value.isPressed);
		}

		public void OnRoll(InputValue value)
		{
			RollInput(value.isPressed);	
		}

		public void OnStab(InputValue value)
		{
			StabInput(value.isPressed);
		}

		public void OnToss(InputValue value)
		{
			TossInput(value.isPressed);
		}

        


#endif
        private void SneakInput(bool newSneakState)
		{
			sneak=newSneakState;
		}
        private void AttackInput(bool newAttackState)
        {
            attack = newAttackState;
        }

		private void DefenseInput(bool newDefenseState)
		{
			defense = newDefenseState;
		}

        private void RollInput(bool newRollState)
        {
            roll = newRollState;
        }

		private void TossInput(bool newTossState)
		{
			toss = newTossState;
		}

		private void StabInput(bool newStabState)
		{
			stab = newStabState;
		}

        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}