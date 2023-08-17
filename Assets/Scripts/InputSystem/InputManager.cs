using DBGA.Common;
using DBGA.EventSystem;
using UnityEngine;

namespace DBGA.InputSystem
{
    [DisallowMultipleComponent]
    public class InputManager : MonoBehaviour
    {
        [System.Serializable]
        private struct MovementInputKeys
        {
            [SerializeField]
            private KeyCode moveUp;
            [SerializeField]
            private KeyCode moveDown;
            [SerializeField]
            private KeyCode moveLeft;
            [SerializeField]
            private KeyCode moveRight;

            public KeyCode MoveUp { get => moveUp; }
            public KeyCode MoveDown { get => moveDown; }
            public KeyCode MoveLeft { get => moveLeft; }
            public KeyCode MoveRight { get => moveRight; }
        }

        [System.Serializable]
        private struct ArrowShootingInputKeys
        {
            [SerializeField]
            private KeyCode shootUp;
            [SerializeField]
            private KeyCode shootDown;
            [SerializeField]
            private KeyCode shootLeft;
            [SerializeField]
            private KeyCode shootRight;

            public KeyCode ShootUp { get => shootUp; }
            public KeyCode ShootDown { get => shootDown; }
            public KeyCode ShootLeft { get => shootLeft; }
            public KeyCode ShootRight { get => shootRight; }
        }

        [SerializeField]
        private MovementInputKeys movementInputs;
        [SerializeField]
        private ArrowShootingInputKeys arrowShootingInputs;
        [SerializeField]
        private KeyCode toggleFogVisibilityKey;

        private GameEvent inputMoveEvent;
        private GameEvent inputArrowShotEvent;
        private GameEventsManager gameEventsManager;

        private Direction currentMoveDirection;
        private Direction currentArrowDirection;

        void Awake()
        {
            inputMoveEvent = new GameEvent("InputMoveEvent", new GameEventParameter("Direction", Direction.Up));
            inputArrowShotEvent = new GameEvent("InputArrowShotEvent", new GameEventParameter("Direction", Direction.Up));
            gameEventsManager = GameEventsManager.Instance;
        }

        void Update()
        {
            currentMoveDirection = GetCurrentInputMovementDirection();
            if (currentMoveDirection != Direction.None)
            {
                inputMoveEvent.TrySetParameter("Direction", currentMoveDirection);
                gameEventsManager.DispatchGameEvent(inputMoveEvent);
            }

            currentArrowDirection = GetCurrentArrowShotDirection();
            if (currentArrowDirection != Direction.None)
            {
                inputArrowShotEvent.TrySetParameter("Direction", currentArrowDirection);
                gameEventsManager.DispatchGameEvent(inputArrowShotEvent);
            }

            if (Input.GetKeyDown(toggleFogVisibilityKey))
                gameEventsManager.DispatchGameEvent(new GameEvent("InputToggleFogVisibilityEvent"));
        }

        /// <summary>
        /// Returns the current player input movement direction
        /// </summary>
        /// <returns>The movement direction the player gave as input, None if player didn't input anything</returns>
        private Direction GetCurrentInputMovementDirection()
        {
            if (Input.GetKeyDown(movementInputs.MoveUp))
                return Direction.Up;
            if (Input.GetKeyDown(movementInputs.MoveDown))
                return Direction.Down;
            if (Input.GetKeyDown(movementInputs.MoveLeft))
                return Direction.Left;
            if (Input.GetKeyDown(movementInputs.MoveRight))
                return Direction.Right;

            return Direction.None;
        }

        /// <summary>
        /// Returns the current player input arrow shooting direction
        /// </summary>
        /// <returns>The arrow shooting direction the player gave as input, None if player didn't input anything</returns>
        private Direction GetCurrentArrowShotDirection()
        {
            if (Input.GetKeyDown(arrowShootingInputs.ShootUp))
                return Direction.Up;
            if (Input.GetKeyDown(arrowShootingInputs.ShootDown))
                return Direction.Down;
            if (Input.GetKeyDown(arrowShootingInputs.ShootLeft))
                return Direction.Left;
            if (Input.GetKeyDown(arrowShootingInputs.ShootRight))
                return Direction.Right;

            return Direction.None;
        }
    }
}
