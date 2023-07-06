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
            public KeyCode moveUp;
            public KeyCode moveDown;
            public KeyCode moveLeft;
            public KeyCode moveRight;
        }

        [System.Serializable]
        private struct ArrowShootingInputKeys
        {
            public KeyCode shootUp;
            public KeyCode shootDown;
            public KeyCode shootLeft;
            public KeyCode shootRight;
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

            inputMoveEvent.callerGameObject = gameObject;
            inputArrowShotEvent.callerGameObject = gameObject;

            gameEventsManager = GameEventsManager.Instance;
        }

        void Update()
        {
            inputMoveEvent.direction = GetCurrentInputMovementDirection();
            if (inputMoveEvent.direction != Direction.None)
                gameEventsManager.DispatchGameEvent(inputMoveEvent);

            inputArrowShotEvent.direction = GetCurrentArrowShotDirection();
            if (inputArrowShotEvent.direction != Direction.None)
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
            if (Input.GetKeyDown(movementInputs.moveUp))
                return Direction.Up;
            if (Input.GetKeyDown(movementInputs.moveDown))
                return Direction.Down;
            if (Input.GetKeyDown(movementInputs.moveLeft))
                return Direction.Left;
            if (Input.GetKeyDown(movementInputs.moveRight))
                return Direction.Right;

            return Direction.None;
        }

        /// <summary>
        /// Returns the current player input arrow shooting direction
        /// </summary>
        /// <returns>The arrow shooting direction the player gave as input, None if player didn't input anything</returns>
        private Direction GetCurrentArrowShotDirection()
        {
            if (Input.GetKeyDown(arrowShootingInputs.shootUp))
                return Direction.Up;
            if (Input.GetKeyDown(arrowShootingInputs.shootDown))
                return Direction.Down;
            if (Input.GetKeyDown(arrowShootingInputs.shootLeft))
                return Direction.Left;
            if (Input.GetKeyDown(arrowShootingInputs.shootRight))
                return Direction.Right;

            return Direction.None;
        }
    }
}
