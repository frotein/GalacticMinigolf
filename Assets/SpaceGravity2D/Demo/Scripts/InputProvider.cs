using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class InputProvider : MonoBehaviour {

	class InputState {
		public bool isPressed = false;
		public Vector2 lastPos = new Vector2();
		public int tapsCount = 0;
		public float lastPressedTime = 0;
		public float lastReleasedTime = 0;
	}

	public static event Action<Vector2, int> OnPointerDown;
	public static event Action<Vector2, int> OnPointerStayDown;
	public static event Action<Vector2, int> OnPointerUp;
	public static event Action<Vector2, int> OnClick;
	public static event Action<float> OnScroll;
	public float clickTime = 0.2f;

	const int inputsCount = 3;
	InputState[] inputStates = new InputState[] { new InputState(), new InputState(), new InputState() };


	void Update() {
		MouseInputHandler();
	}

	void MouseInputHandler() {
		bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
		for (int i = 0; i < inputsCount; i++) {
			if (Input.GetMouseButtonDown(i)) {
				if (!overUI){
					if (inputStates[i].isPressed) {
						RaiseEvent(OnPointerUp, inputStates[i].lastPos, i);
					}
					RaiseEvent(OnPointerDown, Input.mousePosition, i);
					inputStates[i].isPressed = true;
					inputStates[i].lastPos = Input.mousePosition;
					inputStates[i].lastPressedTime = Time.time;
					if (inputStates[i].tapsCount>0 && inputStates[i].lastPressedTime - inputStates[i].lastReleasedTime >= clickTime) {
						inputStates[i].tapsCount = 0;
					}
				}
			} else
				if (Input.GetMouseButton(i)) {
					if (inputStates[i].isPressed) {
						RaiseEvent(OnPointerStayDown, Input.mousePosition, i);
						inputStates[i].lastPos = Input.mousePosition;
					}
				}
			if (Input.GetMouseButtonUp(i)) {
				if (inputStates[i].isPressed) {
					RaiseEvent(OnPointerUp, Input.mousePosition, i);
					inputStates[i].lastPos = Input.mousePosition;
					inputStates[i].isPressed = false;
					inputStates[i].lastReleasedTime = Time.time;
					if (inputStates[i].lastReleasedTime - inputStates[i].lastPressedTime < clickTime) {
						inputStates[i].tapsCount++;
						if (inputStates[i].tapsCount == 1) {
							RaiseEvent(OnClick, Input.mousePosition, i);
						}
					}
				}
			}
		}
		var scroll = Input.GetAxis("Mouse ScrollWheel");
		if (Mathf.Abs(scroll) > 1e-5f && OnScroll!=null) {
			OnScroll(scroll);
		}
	}

	void RaiseEvent(Action<Vector2, int> handler, Vector2 prm0, int prm1) {
		if (handler != null) {
			handler(prm0, prm1);
		}
	}
}
