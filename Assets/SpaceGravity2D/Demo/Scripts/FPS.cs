using UnityEngine;
using UnityEngine.UI;
namespace SpaceGravity2D.Demo {
	public class FPScounter {

		int frames=0;
		float frequency=0;
		float timerAscend=0;
		float timerDescend=0;
		public float FPS { get; private set; }

		public FPScounter( float freq ) {
			frequency = freq;
			timerDescend = freq;
		}

		public void Update( float deltatime ) {
			timerAscend += deltatime;
			timerDescend -= deltatime;
			frames++;
			if ( timerDescend <= 0 ) {
				FPS = frames / timerAscend;
				frames = 0;
				timerDescend = frequency;
				timerAscend = 0f;
			}
		}
	}

	/// <summary>
	/// attach to UI text component to display FPS
	/// </summary>
	public class FPS : MonoBehaviour {

		public float updateFreq = 0.1f;
		private FPScounter _counter = new FPScounter(0.1f);
		private Text displayText;

		void Start() {
			displayText = GetComponentInChildren<Text>();
			if ( !displayText ) {
				enabled = false;
			}
			_counter = new FPScounter( updateFreq );
		}

		void Update() {
			if (displayText != null) {
				_counter.Update(Time.deltaTime);
				displayText.text = _counter.FPS.ToString("FPS:0.00");
			}
		}
	}
}
