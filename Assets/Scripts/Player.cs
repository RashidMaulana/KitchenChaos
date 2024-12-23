using System;
using UnityEngine;

public class Player : MonoBehaviour {

	public static Player Instance { get; private set; }

	public EventHandler<OnSelectedCounterChangedEventArgs> onSelectedCounterChanged;
	public class OnSelectedCounterChangedEventArgs : EventArgs {
		public ClearCounter selectedCounter;
	}

	[SerializeField] private float moveSpeed = 7f;
	[SerializeField] private GameInput gameInput;
	[SerializeField] private LayerMask countersLayerMask;

	private ClearCounter selectedCounter;
	private bool isWalking;

	private Vector3 lastInteractDir;


	private void Awake() {
		if (Instance != null) {
			Debug.LogError("There is more than one player instance");
		}

		Instance = this;
	}

	private void Start() {
		gameInput.OnInteractAction += GameInput_OnInteractAction;
	}

	private void GameInput_OnInteractAction(object sender, System.EventArgs e) {
		selectedCounter?.Interact();
	}

	private void Update() {
		HandleMovement();
		HandleInteractions();
	}

	public bool IsWalking() {
		return isWalking;
	}

	private void HandleInteractions() {
		Vector2 inputVector = gameInput.GetMovementVectorNormalized();

		Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

		if (moveDir != Vector3.zero) {
			lastInteractDir = moveDir;
		}

		float interactDistance = 2f;
		if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)) {
			if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter)) {
				if (clearCounter != selectedCounter) {
					SetSelectedCounter(clearCounter);
				}
			} else {
				SetSelectedCounter(null);
			}
		} else {
			SetSelectedCounter(null);
		};
	}

	private void HandleMovement() {
		Vector2 inputVector = gameInput.GetMovementVectorNormalized();

		Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

		float playerRadius = .7f;
		float playerHeight = 2f;
		float moveDistance = moveSpeed * Time.deltaTime;
		bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

		if (!canMove) {
			Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
			canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

			if (canMove) {
				moveDir = moveDirX;
			} else {
				Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
				canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

				if (canMove) {
					moveDir = moveDirZ;
				} else {

				}
			}
		}


		if (canMove) {
			transform.position += moveDir * moveDistance;
		}

		float rotateSpeed = 10f;

		transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
		isWalking = moveDir != Vector3.zero;
	}

	private void SetSelectedCounter(ClearCounter selectedCounter) {
		this.selectedCounter = selectedCounter;

		onSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs {
			selectedCounter = selectedCounter
		});
	}
}
