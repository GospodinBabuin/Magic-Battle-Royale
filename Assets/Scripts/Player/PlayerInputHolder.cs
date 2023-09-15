using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class PlayerInputHolder : MonoBehaviour
	{
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool aim;
		public bool walk;
		public bool useSpell;
		public byte currentSpell;

		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

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
    
		public void OnAim(InputValue value)
		{
			AimInput(value.isPressed);
		}
    
		public void OnWalk()
		{
			WalkInput();
		}
    
		public void OnUseSpell(InputValue value)
		{
			UseSpellInput(value.isPressed);
		}
    
		public void OnSelectSpell(InputValue value)
		{
			SelectSpellInput(value.Get<Vector2>());
	    
		}

		private void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 
    
		private void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}
    
		private void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}
    
		private void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
    
		private void AimInput(bool newAimState)
		{
			aim = newAimState;
		}
    
		private void WalkInput()
		{
			walk = !walk;
		}
    
		private void UseSpellInput(bool newUseSpellState)
		{
			useSpell = newUseSpellState;
		}
    
		private void SelectSpellInput(Vector2 newSelectSpellState)
		{
			switch (newSelectSpellState.normalized.x, newSelectSpellState.normalized.y)
			{
				case (0f, 1f) :
					currentSpell = 0;
					break;
				case (0f, -1f) :
					currentSpell = 1;
					break;
				case (-1f, 0f) :
					currentSpell = 2;
					break;
				case (1, 0) :
					currentSpell = 3;
					break;
			}
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
