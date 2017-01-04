using UnityEngine;
using System.Linq;

namespace SpaceGravity2D.Demo {
	[System.Serializable]
	public class BodySaveData {
		public string name = "name";
		public Vector3d position;
		public Vector3d velocity;
		public Color color;
		public double mass;
		public float scale;
	}

	[System.Serializable]
	public class SceneSaveData {
		public double gravConst = 0.0001f;
		public bool isKeplerMotion = false;
		public double timeScale = 1f;
		public float scaleMlt = 1f;
		public BodySaveData[] bodies = new BodySaveData[0];
	}

	[ExecuteInEditMode]
	public class SandboxManager : MonoBehaviour {

		public SimulationControl simControl;
		public Camera cam;
		public BodiesPool pool;
		public Material bodyMat;
		public Material emitMat;
		public PredictionSystem predictionSystem;
		public double maxBodyDistance = 10000f;
		public float ambientIntensity = 1f;
		public TextAsset serializedJsonData;
		public float velocitiesScale = 1f;
		public Color velocitiesColor = Color.yellow;

		public PooledBody capturedBody;
		public float maxScale = 10f;
		public float minScale = 1f;
		public float illumMassTreshold = 5000;
		public Color[] bodiesColors = new Color[]{
			Color.red,
			Color.green,
			Color.blue,
			Color.yellow
		};
		public bool isShiftDown = false;
		public Vector3 worldPosOnXZPlane = new Vector3();

		#region computation properties
		bool _isAdding;
		public bool isAdding {
			get {
				return _isAdding;
			}
			set {
				_isAdding = value;
				if (value) {
					_isDeleting = false;
				}
			}
		}
		bool _isDeleting;
		public bool isDeleting {
			get {
				return _isDeleting;
			}
			set {
				_isDeleting = value;
				if (value) {
					_isAdding = false;
				}
			}
		}
		bool _isAutoOrbit;
		public bool isAutoOrbit {
			get {
				return _isAutoOrbit;
			}
			set {
				_isAutoOrbit = value;
			}
		}
		public float initMass = 1;

		bool _isDrawOrbits;
		public bool isDrawOrbits {
			get {
				return _isDrawOrbits;
			}
			set {
				_isDrawOrbits = value;
				for (int i = 0; i < simControl.bodies.Count; i++) {
					if (value) {
						simControl.bodies[i].GetComponent<OrbitDisplay>().enabled = _isKeplerMotion;
						simControl.bodies[i].GetComponent<PredictionSystemTarget>().enabled = !_isKeplerMotion;
					} else {
						simControl.bodies[i].GetComponent<OrbitDisplay>().enabled = false;
						simControl.bodies[i].GetComponent<PredictionSystemTarget>().enabled = false;
					}
				}
			}
		}

		bool _isDrawVectors;
		public bool isDrawVectors {
			get {
				return _isDrawVectors;
			}
			set {
				_isDrawVectors = value;
				for (int i = 0; i < pool.Count; i++) {
					//pool[i].velocityHandle.gameObject.SetActive(value);
					pool[i].velocityLine.enabled = value;
				}
			}
		}

		bool _isKeplerMotion;
		public bool isKeplerMotion {
			get {
				return _isKeplerMotion;
			}
			set {
				_isKeplerMotion = value;
				for (int i = 0; i < simControl.bodies.Count; i++) {
					if (_isDrawOrbits) {
						simControl.bodies[i].GetComponent<OrbitDisplay>().enabled = value;
						simControl.bodies[i].GetComponent<PredictionSystemTarget>().enabled = !value;
					}
					simControl.bodies[i].useKeplerMotion = value;
					//if (value) {
					//	simControl.bodies[i].CalculateNewOrbitData();
					//}
					simControl.bodies[i].TerminateKeplerMotion();
				}
			}
		}
		#endregion

		void Start() {
			if (Application.isPlaying) {
				if (serializedJsonData != null) {
					LoadSceneFromJson(serializedJsonData.text);
				}
			}
		}

		void LoadSceneFromJson(string json) {
			SceneSaveData data;
			try {
				data = JsonUtility.FromJson<SceneSaveData>(json);
			}
			catch {
				return;
			}
			simControl.GravitationalConstant = data.gravConst;
			simControl.timeScale = data.timeScale;
			_isKeplerMotion = data.isKeplerMotion;
			for (int i = 0; i < data.bodies.Length; i++) {
				var b = pool.GetReadyOne();
				InitCreatedBody(b, data.bodies[i].name, data.bodies[i].mass, data.bodies[i].color);
				b.body.position = data.bodies[i].position;
				b.body.velocity = data.bodies[i].velocity;
				b.body.useKeplerMotion = _isKeplerMotion;
				b.body.transformRef.localScale = new Vector3(1, 1, 1) * data.bodies[i].scale * data.scaleMlt;
				b.body.enabled = true;
			}
			for (int i = 0; i < pool.Count; i++) {
				pool[i].body.FindAndSetMostProperAttractor();
			}
			OnActiveBodiesCountChanged();
		}

		[ContextMenu("Save Scene To Json")]
		public void SaveSceneToJson() {
#if UNITY_EDITOR
			SceneSaveData data = new SceneSaveData
			{
				gravConst = simControl.GravitationalConstant,
				isKeplerMotion = _isKeplerMotion,
				scaleMlt = 1f,
				timeScale = simControl.timeScale
			};
			if (Application.isPlaying) {
				data.bodies = new BodySaveData[pool.Count];
				for (int i = 0; i < data.bodies.Length; i++) {
					if (!pool[i].isReady) {
						data.bodies[i] = new BodySaveData() {
							scale = pool[i].body.transformRef.localScale.x,
							position = pool[i].body.position,
							velocity = pool[i].body.velocity,
							color = pool[i].meshRend.material.color,
							mass = pool[i].body.mass,
							name = pool[i].go.name
						};
					}
				}
				data.bodies = data.bodies.Where(b => b != null).ToArray();
			} else {
				var _bodies = GameObject.FindObjectsOfType<CelestialBody>();
				data.bodies = new BodySaveData[_bodies.Length];
				for (int i = 0; i < data.bodies.Length; i++) {
					if (_bodies[i].gameObject.activeSelf) {
						var col = _bodies[i].GetComponent<MeshRenderer>() != null ? _bodies[i].GetComponent<MeshRenderer>().sharedMaterial.color : new Color();
						data.bodies[i] = new BodySaveData() {
							position = _bodies[i].position,
							velocity = _bodies[i].velocity,
							mass = _bodies[i].mass,
							name = _bodies[i].name,
							scale = _bodies[i].transform.localScale.x,
							color = col
						};
					}
				}
				data.bodies = data.bodies.Where(b => b != null).ToArray();
			}
			string filename = "SavedScene";
			string path = Application.dataPath + "/SpaceGravity2D/Demo/JsonData/";
			int t = 0;
			while (t < 100 && System.IO.File.Exists(path + filename + ".txt")) {
				t++;
				filename = "SavedScene (" + t + ")";
			}
			System.IO.File.WriteAllText(path + filename + ".txt", JsonUtility.ToJson(data, true));
			UnityEditor.AssetDatabase.Refresh();
			Debug.Log("SpaceGravity2D: Objects saved to " + filename);
#endif
		}

		void Update() {
			if (Application.isPlaying) {
				for (int i = 0; i < pool.Count; i++) {
					if (!pool[i].isReady) {
						if (double.IsNaN(pool[i].body.position.x) ||
							double.IsInfinity(pool[i].body.position.x) ||
							double.IsNaN(pool[i].body.velocity.x) ||
							double.IsInfinity(pool[i].body.velocity.x) ||
							pool[i].body.position.sqrMagnitude > maxBodyDistance * maxBodyDistance) {
							pool.ReleaseOne(pool[i].body);
						}
					}
				}
				ShiftInput();
				RefreshVelocities();
			}
		}

		void ShiftInput() {
			if (isShiftDown) {
				if (Input.GetKeyUp(KeyCode.LeftShift)) {
					isShiftDown = false;
				}
			} else {
				if (Input.GetKeyDown(KeyCode.LeftShift) && capturedBody != null) {
					isShiftDown = true;
					worldPosOnXZPlane = capturedBody.body.transform.position;
				}
			}
		}

		void RefreshVelocities() {
			if (_isDrawVectors) {
				for (int i = 0; i < pool.Count; i++) {
					if (!pool[i].isReady) {
						//pool[i].velocityHandle.transform.position = (Vector3)( pool[i].body.position + pool[i].body.velocity );
						var startPoint = pool[i].body.transformRef.position + (Vector3)pool[i].body.velocity.normalized * pool[i].body.transformRef.localScale.x * 0.5f;
						pool[i].velocityLine.SetPositions(new Vector3[]{
							startPoint,
							startPoint + (Vector3)pool[i].body.velocity,
						});
					}
				}
			}
		}

		void OnEnable() {
			FindReferences();
			if (Application.isPlaying) {
				Subscribe();
			}
		}

		void OnDestroy() {
			Unsubscribe();
		}
		void FindReferences() {
			if (simControl == null) {
				simControl = GameObject.FindObjectOfType<SimulationControl>();
			}
			if (cam == null) {
				cam = Camera.main ?? GameObject.FindObjectOfType<Camera>();
			}
			if (pool == null) {
				pool = GetComponent<BodiesPool>() ?? GameObject.FindObjectOfType<BodiesPool>();
			}
			if (predictionSystem == null) {
				predictionSystem = GameObject.FindObjectOfType<PredictionSystem>();
			}
		}

		void Subscribe() {
			Unsubscribe();
			InputProvider.OnPointerDown += OnMouseDown;
			InputProvider.OnPointerStayDown += OnMouseStay;
			InputProvider.OnPointerUp += OnMouseUp;
			InputProvider.OnClick += OnMouseClick;
		}


		void Unsubscribe() {
			InputProvider.OnPointerDown -= OnMouseDown;
			InputProvider.OnPointerStayDown -= OnMouseStay;
			InputProvider.OnPointerUp -= OnMouseUp;
			InputProvider.OnClick -= OnMouseClick;
		}

		void OnActiveBodiesCountChanged() {
			RenderSettings.ambientIntensity = pool.IsAnyLightActive() ? 0.05f : ambientIntensity;
		}

		void OnMouseDown(Vector2 pos, int btn) {
			if (isAdding && btn == 0 && capturedBody == null) {
				capturedBody = pool.GetReadyOne();
				if (_isKeplerMotion) {
					capturedBody.orbitDisplay.enabled = _isDrawOrbits;
					capturedBody.predictionDisplay.enabled = false;
				} else {
					capturedBody.orbitDisplay.enabled = false;
					capturedBody.predictionDisplay.enabled = _isDrawOrbits;
				}
				InitCreatedBody(capturedBody, "body_" + pool.ActiveCount, initMass, bodiesColors[Random.Range(0, bodiesColors.Length)]);
				capturedBody.body.enabled = true;
				//capturedBody.body.mass = 1;
				capturedBody.body.position = new Vector3d(GetWorldRaycastPos(pos));
				capturedBody.body.transformRef.localScale = new Vector3(1, 1, 1) * GetSizeOfBody(initMass);
				capturedBody.body.useKeplerMotion = _isKeplerMotion;
				if (_isAutoOrbit) {
					capturedBody.body.MakeOrbitCircle(true);
				} else {
					capturedBody.body.velocity = new Vector3d();
				}
				OnActiveBodiesCountChanged();
			}
		}

		float GetSizeOfBody(float mass) {
			return minScale + ( mass / 1000000f ) * maxScale;
		}

		void OnMouseStay(Vector2 pos, int btn) {
			if (btn == 0) {
				if (capturedBody != null) {
					if (capturedBody.isReady) {
						capturedBody = null;
					} else {
						capturedBody.body.enabled = false;
						capturedBody.body.position = new Vector3d(GetWorldRaycastPos(pos));
						capturedBody.body.FindAndSetMostProperAttractor();
						if (_isAutoOrbit) {
							capturedBody.body.MakeOrbitCircle(true);
						}
						if (_isKeplerMotion) {
							capturedBody.orbitDisplay.DrawOrbit();
						}
						capturedBody.body.orbitData.isDirty = true;
					}
				}
			}
		}

		void OnMouseUp(Vector2 pos, int btn) {
			if (btn == 0) {
				if (capturedBody != null) {
					capturedBody.body.enabled = true;
					//capturedBody.body.mass = initMass;
					capturedBody.body.useKeplerMotion = _isKeplerMotion;
					capturedBody.body.orbitData.isDirty = true;
					capturedBody = null;
				}
			}
		}

		void OnMouseClick(Vector2 pos, int btn) {
			if (isDeleting) {
				RaycastHit hit;
				if (Physics.Raycast(cam.ScreenPointToRay(pos), out hit)) {
					var cb = hit.collider.GetComponent<CelestialBody>();
					if (cb != null) {
						pool.ReleaseOne(cb);
						OnActiveBodiesCountChanged();
					}
				}
			}
		}

		void InitCreatedBody(PooledBody b, string name, double mass, Color col) {
			b.body.orbitData.eclipticNormal = new Vector3d();
			b.body.orbitData.orbitNormal = new Vector3d();
			b.go.SetActive(true);
			b.body.enabled = false;
			b.go.name = name;
			b.body.mass = mass;
			b.meshRend.material.color = col;
			b.orbitDisplay.OrbitLineMaterial = predictionSystem == null ? null : predictionSystem.LinesMaterial;
			b.light.enabled = mass > illumMassTreshold;
			var mat = Instantiate(mass > illumMassTreshold ? emitMat : bodyMat);
			mat.color = col;
			b.meshRend.material = mat;
			if (b.velocityLine.material.name.ToLower().Contains("default")) {
				b.velocityLine.material = Instantiate(emitMat);
				b.velocityLine.material.color = velocitiesColor;
			}
			var linesW = GetSizeOfBody((float)mass) * 0.1f;
			b.velocityLine.SetWidth(linesW, linesW);
			b.velocityLine.enabled = _isDrawVectors;
		}

		Vector3 GetWorldRaycastPos(Vector2 screenPos) {
			var ray = cam.ScreenPointToRay(screenPos);
			if (isShiftDown) {
				var normal = new Vector3(-ray.direction.x, 0, -ray.direction.z);
				var hitPos = CelestialBodyUtils.GetRayPlaneIntersectionPoint(worldPosOnXZPlane, normal, ray.origin, ray.direction);
				return new Vector3(worldPosOnXZPlane.x, hitPos.y, worldPosOnXZPlane.z);
			} else {
				return CelestialBodyUtils.GetRayPlaneIntersectionPoint(Vector3.zero, Vector3.up, ray.origin, ray.direction);
			}
		}
	}
}