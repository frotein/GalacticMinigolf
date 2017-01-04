using UnityEngine;

public class TutorialGUI : MonoBehaviour {

	public GameObject[] slides = new GameObject[0];
	public GameObject BG;
	int current = -1;

	void Start() {
		if (PlayerPrefs.GetInt("Tutorial", 0) == 0) {
			if (BG == null) {
				BG = transform.Find("ImageBG").gameObject;
			}
			BG.SetActive(true);
			for (int i = 0; i < slides.Length; i++) {
				slides[i].SetActive(false);
			}
			ShowNext();
		} else {
			gameObject.SetActive(false);
		}
	}

	public void ShowNext() {
		if (current >= 0 && current < slides.Length) {
			slides[current].SetActive(false);
		}
		current++;
		if (current < slides.Length) {
			slides[current].SetActive(true);
		} else {
			PlayerPrefs.SetInt("Tutorial", 1);
			gameObject.SetActive(false);
		}
	}

	[ContextMenu("Reset Tutorial")]
	public void ResetTutorial() {
		PlayerPrefs.SetInt("Tutorial", 0);
	}
}
