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

        private InputMoveEvent inputMoveEvent;
        private InputArrowShotEvent inputArrowShotEvent;
        private GameEventsManager gameEventsManager;

        void Awake()
        {
            inputMoveEvent = new InputMoveEvent();
            inputArrowShotEvent = new InputArrowShotEvent();
            gameEventsManager = GameEventsManager.Instance;
        }

        void Update()
        {
            inputMoveEvent.Direction = GetCurrentInputMovementDirection();
            if (inputMoveEvent.Direction != Direction.None)
                gameEventsManager.DispatchGameEvent(inputMoveEvent);

            inputArrowShotEvent.Direction = GetCurrentArrowShotDirection();
            if (inputArrowShotEvent.Direction != Direction.None)
                gameEventsManager.DispatchGameEvent(inputArrowShotEvent);

            if (Input.GetKeyDown(toggleFogVisibilityKey))
                gameEventsManager.DispatchGameEvent(new InputToggleFogVisibilityEvent());
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
