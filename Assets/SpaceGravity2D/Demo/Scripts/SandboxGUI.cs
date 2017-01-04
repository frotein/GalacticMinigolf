using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace SpaceGravity2D.Demo {

	[ExecuteInEditMode]
	public class SandboxGUI : MonoBehaviour {

		public Button addButton;
		public Button delButton;
		public Button autoOrbitButton;
		public Slider bodyMassSlider;
		public Button orbitsToggle;
		public Button vectorsToggle;
		public Button keplerToggle;
		[Space]
		public SandboxManager managerRef;

		void Start() {
			if (Application.isPlaying) {
				SubscribeButtons();
				autoOrbitButton.transform.parent.gameObject.SetActive(false);
				bodyMassSlider.transform.parent.gameObject.SetActive(false);
			}
		}

		void OnEnable() {
			FindReferences();
		}

		void FindReferences() {
			if (addButton == null || delButton == null || autoOrbitButton == null || orbitsToggle == null || vectorsToggle == null || keplerToggle == null) {
				var buttons = GetComponentsInChildren<Button>();
				if (addButton == null) {
					addButton = buttons.FirstOrDefault(b => b.name.ToLower().Contains("add"));
				}
				if (delButton == null) {
					delButton = buttons.FirstOrDefault(b => b.name.ToLower().Contains("del"));
				}
				if (autoOrbitButton == null) {
					autoOrbitButton = buttons.FirstOrDefault(b => b.name.ToLower().Contains("auto"));
				}
				if (orbitsToggle == null) {
					orbitsToggle = buttons.FirstOrDefault(b => b.name.ToLower().Contains("orbitstoggle"));
				}
				if (vectorsToggle == null) {
					vectorsToggle = buttons.FirstOrDefault(b => b.name.ToLower().Contains("vectors"));
				}
				if (keplerToggle == null) {
					keplerToggle = buttons.FirstOrDefault(b => b.name.ToLower().Contains("kepler"));
				}
			}
			if (bodyMassSlider == null) {
				var sliders = GetComponentsInChildren<Slider>();
				bodyMassSlider = sliders.FirstOrDefault(s => s.name.ToLower().Contains("mass"));
			}
			if (managerRef == null) {
				var mngr = GameObject.FindObjectOfType<SandboxManager>();
				if (mngr) {
					managerRef = mngr;
				}
			}
		}

		void SubscribeButtons() {
			addButton.onClick.AddListener(() => {
				managerRef.isAdding = !managerRef.isAdding;
				ToggleFramesColor(addButton.transform.Find("FrameBG"), managerRef.isAdding);
				autoOrbitButton.transform.parent.gameObject.SetActive(managerRef.isAdding);
				bodyMassSlider.transform.parent.gameObject.SetActive(managerRef.isAdding);
				if (managerRef.isAdding) {
					ToggleFramesColor(delButton.transform.Find("FrameBG"), false);
				}
			});
			delButton.onClick.AddListener(() => {
				managerRef.isDeleting = !managerRef.isDeleting;
				if (managerRef.isDeleting) {
					autoOrbitButton.transform.parent.gameObject.SetActive(false);
					bodyMassSlider.transform.parent.gameObject.SetActive(false);
					ToggleFramesColor(addButton.transform.Find("FrameBG"), false);
				}
				ToggleFramesColor(delButton.transform.Find("FrameBG"), managerRef.isDeleting);
			});
			autoOrbitButton.onClick.AddListener(() => {
				managerRef.isAutoOrbit = !managerRef.isAutoOrbit;
				ToggleFramesColor(autoOrbitButton.transform.Find("FrameBG"), managerRef.isAutoOrbit);
			});
			managerRef.isAutoOrbit = true;
			ToggleFramesColor(autoOrbitButton.transform.Find("FrameBG"), true);

			bodyMassSlider.value = managerRef.initMass;
			bodyMassSlider.onValueChanged.AddListener((f) => {
				managerRef.initMass = f;
			});
			orbitsToggle.onClick.AddListener(() => {
				managerRef.isDrawOrbits = !managerRef.isDrawOrbits;
				ToggleFramesColor(orbitsToggle.transform.Find("FrameBG"), managerRef.isDrawOrbits);
			});
			vectorsToggle.onClick.AddListener(() => {
				managerRef.isDrawVectors = !managerRef.isDrawVectors;
				ToggleFramesColor(vectorsToggle.transform.Find("FrameBG"), managerRef.isDrawVectors);
			});
			keplerToggle.onClick.AddListener(() => {
				managerRef.isKeplerMotion = !managerRef.isKeplerMotion;
				ToggleFramesColor(keplerToggle.transform.Find("FrameBG"), managerRef.isKeplerMotion);
			});
		}

		void ToggleFramesColor(Transform root, bool green) {
			if (root != null) {
				var images = root.GetComponentsInChildren<Image>();
				for (int i = 0; i < images.Length; i++) {
					images[i].color = green ? Color.green : Color.white;
				}
			}
		}

	}
}