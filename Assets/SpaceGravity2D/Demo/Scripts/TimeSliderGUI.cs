using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

namespace SpaceGravity2D.Demo{
public class TimeSliderGUI : Slider {

	public bool snapInMiddle=true;
	public RectTransform leftFillArea;
	public RectTransform rightFillArea;
	SimulationControl simControl;

	protected override void Start() {
		base.Start();
		var transforms = GetComponentsInChildren<RectTransform>();
		if (Application.isPlaying) {
			leftFillArea = transforms.FirstOrDefault(t => t.name == "LeftFillArea");
			if (leftFillArea != null) {
				leftFillArea.anchorMin = new Vector2(leftFillArea.anchorMax.x, 0);
				leftFillArea.pivot = new Vector2(1f, 0.5f);
			}
			rightFillArea = transforms.FirstOrDefault(t => t.name == "RightFillArea");
			if (rightFillArea != null) {
				rightFillArea.anchorMax = new Vector2(rightFillArea.anchorMin.x, 1);
				rightFillArea.pivot = new Vector2(0f, 0.5f);
			}
			RefreshFillAreas();
			if (simControl == null) {
				simControl = GameObject.FindObjectOfType<SimulationControl>();
			}
			if (simControl != null) {
				simControl.timeScale = value;
			}
		}
	}

	public override void OnDrag(PointerEventData eventData) {
		base.OnDrag(eventData);
		RefreshFillAreas();
		if (simControl != null) {
			simControl.timeScale = value;
		}
	}

	public override void OnPointerDown(PointerEventData eventData) {
		base.OnPointerDown(eventData);
	}

	public override void OnPointerUp(PointerEventData eventData) {
		base.OnPointerUp(eventData);
		if (snapInMiddle) {
			if (Mathd.Abs(m_Value) < 0.1f) {
				value = 0f;
				RefreshFillAreas();
			}
		}
		if (simControl != null) {
			simControl.timeScale = value;
		}
	}

	void RefreshFillAreas() {
		if (handleRect != null) {
			if (leftFillArea != null) {
				leftFillArea.sizeDelta = new Vector2(leftFillArea.position.x - handleRect.position.x, leftFillArea.sizeDelta.y);
			}
			if (rightFillArea != null) {
				rightFillArea.sizeDelta = new Vector2(handleRect.position.x - rightFillArea.position.x, rightFillArea.sizeDelta.y);
			}
		}
	}
}
}