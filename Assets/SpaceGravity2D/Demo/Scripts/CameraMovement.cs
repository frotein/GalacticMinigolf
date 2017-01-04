using UnityEngine;

namespace SpaceGravity.Demo {
	public class CameraMovement : MonoBehaviour {

		Transform _transform;
		bool isRotating;
		bool isMoving;
		Vector2 lastMousePos;
		public float rotatingSpeed = 1f;
		public float movingSpeed = 1f;
		public float scalingSpeed = 1f;
		[Space]
		public float currentScale = 20;
		public bool keepZeroYCoord = true;
		[Space]
		public float camMinScale = 10;
		public float camMaxScale = 2000;

		void Start() {
			_transform = GetComponent<Transform>();
			Subscribe();
		}

		void OnDestroy() {
			Unsubscribe();
		}

		void Subscribe() {
			Unsubscribe();
			InputProvider.OnPointerDown += OnMouseDown;
			InputProvider.OnPointerUp += OnMouseUp;
			InputProvider.OnPointerStayDown += OnMouseStay;
			InputProvider.OnScroll += OnMouseScroll;
		}

		void Unsubscribe() {
			InputProvider.OnPointerDown -= OnMouseDown;
			InputProvider.OnPointerUp -= OnMouseUp;
			InputProvider.OnPointerStayDown -= OnMouseStay;
			InputProvider.OnScroll -= OnMouseScroll;

		}

		void OnMouseDown(Vector2 pos, int btn) {
			if (btn == 1) {
				isRotating = true;
			}
			if (btn == 2) {
				isMoving = true;
			}
			lastMousePos = pos;
		}

		void OnMouseStay(Vector2 pos, int btn) {
			if (btn == 1 && isRotating) {
				var delta = ( lastMousePos - pos ) * rotatingSpeed * 100f * Time.deltaTime;
				var rot = _transform.localRotation.eulerAngles;
				_transform.localRotation = Quaternion.Euler(rot.x + delta.y, rot.y + delta.x, rot.z);
			}
			if (btn == 2 && isMoving) {
				var delta = ( lastMousePos - pos ) * movingSpeed * 100 * Time.deltaTime * ( currentScale / 20f );
				_transform.position += _transform.right * delta.x + _transform.up * delta.y;
				if (keepZeroYCoord) {
					_transform.position = new Vector3(_transform.position.x, 0, _transform.position.z);
				}
			}
			lastMousePos = pos;
		}

		void OnMouseUp(Vector2 pos, int btn) {
			if (btn == 1) {
				isRotating = false;
			}
		}

		void OnMouseScroll(float x) {
			if (_transform.childCount > 0) {
				var cam = _transform.GetChild(0);
				cam.localPosition = new Vector3(0, 0, Mathf.Clamp(cam.localPosition.z + x * ( -cam.localPosition.z ), -camMaxScale, -camMinScale));
				if (cam.localPosition.z >= -camMinScale) {
					_transform.position += cam.forward * x;
					_transform.position = new Vector3(_transform.position.x, 0, _transform.position.z);
				}
				currentScale = Mathf.Abs(cam.localPosition.z);
			}
		}


	}
}