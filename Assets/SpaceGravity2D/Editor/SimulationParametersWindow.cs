using UnityEngine;
using UnityEditor;

namespace SpaceGravity2D.Inspector {

	[InitializeOnLoad]
	public class SimulationParametersWindow : EditorWindow {
		SimulationControl _simControl;
		SerializedObject _simControlSerialized;
		static SceneViewDisplayManager _displayManager;

		#region serialized properties

		SerializedProperty inflRangeProp;
		SerializedProperty inflRangeMinProp;
		SerializedProperty timeScaleProp;
		SerializedProperty minMassProp;
		SerializedProperty calcTypeProp;
		SerializedProperty eclipticNormalProp;
		SerializedProperty eclipticUpProp;
		SerializedProperty sceneViewDisplayParametersProp;

		#endregion

		#region initialization
		[MenuItem("Window/Space Gravity 2D Window")]
		public static void ShowWindow() {
			EditorWindow.GetWindow<SimulationParametersWindow>();
		}

		static SimulationParametersWindow() {
			if (_displayManager == null) {
				_displayManager = new SceneViewDisplayManager();
			}
		}

		void OnEnable() {
			if (_displayManager == null) {
				_displayManager = new SceneViewDisplayManager();
			}
		}

		public static SimulationControl FindSimulationControlGameObject() {
			if (SimulationControl.instance != null) {
				return SimulationControl.instance;
			}
			var simControl = GameObject.FindObjectOfType<SimulationControl>();
			return simControl;
		}

		[MenuItem("GameObject/SpaceGravity2D/Simulation Control")]
		public static SimulationControl CreateSimulationControl() {
			var obj = new GameObject("SimulationControl");
			Undo.RegisterCreatedObjectUndo(obj, "SimControl creation");
			//Debug.Log("SpaceGravity2D: Simulation Control created");
			return Undo.AddComponent<SimulationControl>(obj);
		}
		/// <summary>
		/// creating serialized properties:
		/// </summary>
		void InitializeProperties() {
			_simControlSerialized = new SerializedObject(_simControl);
			inflRangeProp = _simControlSerialized.FindProperty("maxAttractionRange");
			inflRangeMinProp = _simControlSerialized.FindProperty("minAttractionRange");
			timeScaleProp = _simControlSerialized.FindProperty("timeScale");
			minMassProp = _simControlSerialized.FindProperty("minAttractorMass");
			calcTypeProp = _simControlSerialized.FindProperty("calculationType");
			eclipticNormalProp = _simControlSerialized.FindProperty("_eclipticNormal");
			eclipticUpProp = _simControlSerialized.FindProperty("_eclipticUp");
			sceneViewDisplayParametersProp = _simControlSerialized.FindProperty("sceneElementsDisplayParameters");
		}
		#endregion

		#region scenewindow and editorwindow GUI
		Vector2 scrollPos;
		/// <summary>
		/// General window onGUI
		/// </summary>
		void OnGUI() {
			if (!_simControl || _simControlSerialized == null) {
				_simControl = FindSimulationControlGameObject();
				if (!_simControl) {//check if creation process failed
					return;
				}
				InitializeProperties();
			}
			if (SimulationControl.instance == null) {
				SimulationControl.instance = _simControl;
			}
			_simControlSerialized.Update();
			scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.MinHeight(200), GUILayout.MaxHeight(1000), GUILayout.ExpandHeight(true));
			EditorGUILayout.LabelField("Tools:", EditorStyles.boldLabel);
			if (GUILayout.Button("Inverse velocity for all selected celestial bodies")) {
				InverseVelocityFor(Selection.gameObjects); //Undo functionality is supported
			}
			EditorGUILayout.LabelField("Global Parameters:", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(calcTypeProp, new GUIContent("N-Body algorithm(?)", "Euler - fastest performance, \nVerlet - fast and more stable, \nRungeKutta - more precise"));

			var gravConst = EditorGUILayout.DoubleField(new GUIContent("Gravitational Constant(?)", "Main constant. The real value 6.67384 * 10E-11 may not be very useful for gaming purposes"), _simControl.gravitationalConstant);
			if (gravConst != _simControl.gravitationalConstant) {
				Undo.RecordObject(_simControl, "Grav.Const change");
				_simControl.GravitationalConstant = gravConst;
			}
			gravConst = EditorGUILayout.DoubleField(new GUIContent("Grav.Const.Proportional(?)", "Change gravitational constant AND keep all orbits unaffected"), _simControl.gravitationalConstant);
			if (gravConst != _simControl.gravitationalConstant) {
				Undo.RecordObject(_simControl, "Grav.Const change");
				_simControl.GravitationalConstantProportional = gravConst;
			}
			EditorGUILayout.PropertyField(inflRangeProp, new GUIContent("Max influence range(?)", "global max range of n-body attraction"));
			EditorGUILayout.PropertyField(inflRangeMinProp, new GUIContent("Min influence range(?)", "global min range of n-body attraction"));
			EditorGUILayout.PropertyField(timeScaleProp, new GUIContent("Time Scale(?)", "Time multiplier. Note: high value will decrease n-body calculations precision"));
			EditorGUILayout.PropertyField(minMassProp, new GUIContent("Min attractor mass(?)", "Mass threshold for body to became attractor"));

			EditorGUILayout.PropertyField(eclipticNormalProp, new GUIContent("Ecliptic Normal Vector(?)", "Perpendicular to ecliptic plane"));
			EditorGUILayout.PropertyField(eclipticUpProp, new GUIContent("Ecliptic Up Vector(?)", "Up vector on ecliptic plane. used for rotation tool"));

			var keep2d = EditorGUILayout.Toggle(new GUIContent("Keep ALL bodies on ecliptic plane(?)", "2d mode. force all bodies to project positions and velocities onto ecliptic plane"), _simControl.keepBodiesOnEclipticPlane);
			if (keep2d != _simControl.keepBodiesOnEclipticPlane) {
				Undo.RecordObject(_simControl, "2d mode toggle");
				_simControl.keepBodiesOnEclipticPlane = keep2d;
				if (keep2d) {
					_simControl.ProjectAllBodiesOnEcliptic();
				}
			}

			var scaledTime = EditorGUILayout.Toggle(new GUIContent("Affected by global timescale(?)", "toggle ignoring of Time.timeScale"), _simControl.affectedByGlobalTimescale);
			if (scaledTime != _simControl.affectedByGlobalTimescale) {
				Undo.RecordObject(_simControl, "affected by timescale toggle");
				_simControl.affectedByGlobalTimescale = scaledTime;
			}

			EditorGUILayout.LabelField("Set ecliptic normal along axis:", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("X")) {
				SetEclipticNormal(new Vector3d(1, 0, 0), new Vector3d(0, 0, 1));
			}
			GUILayout.Space(6);
			if (GUILayout.Button("-X")) {
				SetEclipticNormal(new Vector3d(-1, 0, 0), new Vector3d(0, 0, -1));
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Y")) {
				SetEclipticNormal(new Vector3d(0, 1, 0), new Vector3d(0, 0, 1));
			}
			GUILayout.Space(6);
			if (GUILayout.Button("-Y")) {
				SetEclipticNormal(new Vector3d(0, -1, 0), new Vector3d(0, 0, -1));
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Z")) {
				SetEclipticNormal(new Vector3d(0, 0, 1), new Vector3d(1, 0, 0));
			}
			GUILayout.Space(6);
			if (GUILayout.Button("-Z")) {
				SetEclipticNormal(new Vector3d(0, 0, -1), new Vector3d(-1, 0, 0));
			}
			GUILayout.EndHorizontal();
			bool eclipticRotateTool = GUILayout.Toggle(_displayManager.IsEclipticRotating, "Rotate Ecliptic Plane", "Button");
			if (eclipticRotateTool != _displayManager.IsEclipticRotating) {
				_displayManager.IsEclipticRotating = eclipticRotateTool;
			}
			bool orbitRotateTool = GUILayout.Toggle(_displayManager.IsOrbitRotating, "Rotate Orbit Of Selected Obj.", "Button");
			if (orbitRotateTool != _displayManager.IsOrbitRotating) {
				_displayManager.IsOrbitRotating = orbitRotateTool;
			}
			EditorGUIUtility.labelWidth = 250f;
			EditorGUILayout.PropertyField(sceneViewDisplayParametersProp, true);
			GUILayout.Space(15);
			GUILayout.EndScrollView();
			if (GUI.changed) {
				_simControlSerialized.ApplyModifiedProperties();
				SceneView.RepaintAll();
			}
		}

		#endregion

		void SetEclipticNormal(Vector3d normal, Vector3d up) {
			Undo.RecordObject(_simControl, "Ecliptic normal change");
			_simControl.eclipticNormal = normal;
			_simControl.eclipticUp = up;
			EditorUtility.SetDirty(_simControl);
		}

		/// <summary>
		/// Tool: inverse velocity of selection
		/// </summary>
		static public void InverseVelocityFor(GameObject[] objects) {
			foreach (var obj in objects) {
				var cBody = obj.GetComponent<CelestialBody>();
				if (cBody) {
					Undo.RecordObject(cBody, "Inverse velocity");
					cBody.relativeVelocity = -cBody.relativeVelocity;
					cBody.TerminateKeplerMotion();
					Undo.IncrementCurrentGroup();
					EditorUtility.SetDirty(cBody);
				}
			}
		}

	}

	public class SceneViewDisplayManager {
		/// <summary>
		/// this reference is used to access global properties
		/// </summary>
		SimulationControl _simControl;

		public static SceneViewDisplayManager Instance;
		/// <summary>
		/// stores all bodies references in scene and is updated every onGUI frame
		/// </summary>
		CelestialBody[] _bodies = new CelestialBody[0];
		/// <summary>
		/// sceneView buttons texture, which is autoloaded at init time from Assets/Resources folder
		/// </summary>
		Texture2D _arrowsBtnImage;
		Texture2D _orbitsBtnImage;
		GUIStyle _buttonActiveStyle;
		GUIStyle _buttonInactiveStyle;

		const double _waitDuration = 1;
		double _waitTime = 0;
		bool _isEclipticRotating;
		public bool IsEclipticRotating {
			get {
				return _isEclipticRotating;
			}
			set {
				_isEclipticRotating = value;
				_isOrbitRotating = false;
				_isVelocityRotating = false;
				if (value) {
					_simControl.sceneElementsDisplayParameters.drawEclipticMark = true;
					Selection.activeGameObject = null;
				}
			}
		}
		bool _isOrbitRotating;
		public bool IsOrbitRotating {
			get {
				return _isOrbitRotating;
			}
			set {
				_isEclipticRotating = false;
				_isOrbitRotating = value;
				_isVelocityRotating = false;
				if (value) {
					_simControl.sceneElementsDisplayParameters.drawOrbits = true;
				}
			}
		}
		bool _isVelocityRotating;
		public bool IsVelocityRotating {
			get {
				return _isVelocityRotating;
			}
			set {
				_isEclipticRotating = false;
				_isOrbitRotating = false;
				_isVelocityRotating = value;
				if (value) {
					_simControl.sceneElementsDisplayParameters.drawOrbits = true;
					_simControl.sceneElementsDisplayParameters.drawVelocityVectors = true;
				}
			}
		}

		public SceneViewDisplayManager() {
			EditorApplication.update += StartupUpdate;
			Instance = this;
		}

		/// <summary>
		/// Editor update delegate. 
		/// Subscribe OnSceneGUI delegate if _simControl is not null. If SimulationControl is not exists on scene, continuously retry untill it's not created
		/// </summary>
		void StartupUpdate() {
			if (_waitTime > EditorApplication.timeSinceStartup) {
				return; ///Wait for check time
			}
			if (_simControl == null) {
				if (SimulationControl.instance != null) {
					_simControl = SimulationControl.instance;
				} else {
					var simControl = GameObject.FindObjectOfType<SimulationControl>();
					if (simControl != null) {
						SimulationControl.instance = simControl;
						_simControl = simControl;
					} else {
						_waitTime = EditorApplication.timeSinceStartup + _waitDuration;
						return; ///If simulation control is not created, exit and wait for next check time
					}
				}
			}
			_arrowsBtnImage = Resources.Load("Textures/arrowsBtn") as Texture2D;
			_orbitsBtnImage = Resources.Load("Textures/orbitsBtn") as Texture2D;
			CreateButtonStyle();
			// subscribe our OnSceneGUI for updates callbacks
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
			//Instance = this;
			SceneView.RepaintAll();
			EditorApplication.update -= StartupUpdate; //Don't call this function anymore.
		}

		/// <summary>
		/// Create background textures and styles for sceneview buttons
		/// </summary>
		void CreateButtonStyle() {
			///Texture2D image parameters:
			int width = 50;
			int height = 50;
			int borderWidth = 1;

			///****************** Active style
			Color32 normalBGcolor_enabled = new Color32();
			Color32 normalBGcolorBorder_enabled = Color.green;

			Texture2D normalTex_enabled = new Texture2D(width, height, TextureFormat.ARGB32, false);
			Color32[] cols = new Color32[width * height];
			for (int i = 0; i < height; i++) {
				for (int j = 0; j < width; j++) {
					cols[i * width + j] = i < borderWidth || i >= height - borderWidth || j < borderWidth || j >= width - borderWidth ? normalBGcolorBorder_enabled : normalBGcolor_enabled;
				}
			}
			normalTex_enabled.SetPixels32(cols);
			normalTex_enabled.Apply();

			_buttonActiveStyle = new GUIStyle();
			_buttonActiveStyle.padding = new RectOffset(5, 5, 5, 5);
			_buttonActiveStyle.fontSize = 20;
			_buttonActiveStyle.alignment = TextAnchor.MiddleCenter;
			_buttonActiveStyle.normal.background = normalTex_enabled;
			_buttonActiveStyle.normal.textColor = Color.white;
			_buttonActiveStyle.active.background = normalTex_enabled;
			_buttonActiveStyle.active.textColor = Color.white;
			_buttonActiveStyle.hover.background = normalTex_enabled;
			_buttonActiveStyle.hover.textColor = Color.white;
			_buttonActiveStyle.focused.background = normalTex_enabled;
			_buttonActiveStyle.focused.textColor = Color.white;

			///******************* Inactive style
			Color32 normalBGcolor_disabled = new Color32();
			Color32 normalBGcolorBorder_disabled = Color.grey;

			Texture2D normalTex_disabled = new Texture2D(width, height, TextureFormat.ARGB32, false);
			for (int i = 0; i < height; i++) {
				for (int j = 0; j < width; j++) {
					cols[i * width + j] = i < borderWidth || i >= height - borderWidth || j < borderWidth || j >= width - borderWidth ? normalBGcolorBorder_disabled : normalBGcolor_disabled;
				}
			}
			normalTex_disabled.SetPixels32(cols);
			normalTex_disabled.Apply();

			_buttonInactiveStyle = new GUIStyle();
			_buttonInactiveStyle.padding = new RectOffset(5, 5, 5, 5);
			_buttonInactiveStyle.fontSize = 20;
			_buttonInactiveStyle.alignment = TextAnchor.MiddleCenter;
			_buttonInactiveStyle.normal.background = normalTex_disabled;
			_buttonInactiveStyle.normal.textColor = Color.white;
			_buttonInactiveStyle.active.background = normalTex_disabled;
			_buttonInactiveStyle.active.textColor = Color.white;
			_buttonInactiveStyle.hover.background = normalTex_disabled;
			_buttonInactiveStyle.hover.textColor = Color.white;
			_buttonInactiveStyle.focused.background = normalTex_disabled;
			_buttonInactiveStyle.focused.textColor = Color.white;
		}

		/// <summary>
		/// Draw all velocitiy vectors and orbits in scene window and process mouse dragging events.
		/// </summary>
		public void OnSceneGUI(SceneView sceneView) {
			if (!_simControl) {
				//sim control was destroyed?
				SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
				EditorApplication.update += StartupUpdate;
				return;
			}
			//cache scene celestial bodies
			_bodies = GameObject.FindObjectsOfType<CelestialBody>();

			DisplayCirclesOverBodies(sceneView.rotation);
			DrawOrbitElementsAndLabels();
			DrawSceneOrbitsAndVectors();

			//Buttons drawing block:
			Handles.BeginGUI();
			var positionRect = new Rect(_simControl.sceneElementsDisplayParameters.sceneViewButtonsPosition + new Vector2(5, 5), new Vector2(40, 40) * _simControl.sceneElementsDisplayParameters.sceneViewButtonsScale);
			if (GUI.Button(positionRect, _arrowsBtnImage, _simControl.sceneElementsDisplayParameters.drawVelocityVectors ? _buttonActiveStyle : _buttonInactiveStyle)) {
				Undo.RecordObject(_simControl, "toggle velocity arrows display");
				_simControl.sceneElementsDisplayParameters.drawVelocityVectors = !_simControl.sceneElementsDisplayParameters.drawVelocityVectors;
				EditorUtility.SetDirty(_simControl);
			}
			positionRect.y += 45 * _simControl.sceneElementsDisplayParameters.sceneViewButtonsScale;
			if (GUI.Button(positionRect, _orbitsBtnImage, _simControl.sceneElementsDisplayParameters.drawOrbits ? _buttonActiveStyle : _buttonInactiveStyle)) {
				Undo.RecordObject(_simControl, "toggle orbits display");
				_simControl.sceneElementsDisplayParameters.drawOrbits = !_simControl.sceneElementsDisplayParameters.drawOrbits;
				EditorUtility.SetDirty(_simControl);
			}
			Handles.EndGUI();
		}

		#region different stuff

		void DrawOrbitElementsAndLabels() {
			var prms = _simControl.sceneElementsDisplayParameters;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].isValidOrbit) {
					if (prms.drawPeriapsisPoint) {
						//===== Periapsis
						DrawX((Vector3)_bodies[i].orbitPeriapsisPoint, prms.circlesScale, Color.green, (Vector3)_bodies[i].orbitData.orbitNormal, (Vector3)_bodies[i].orbitData.semiMajorAxisBasis);
					}
					if (prms.drawPeriapsisLabel) {
						DrawLabelScaled((Vector3)_bodies[i].orbitPeriapsisPoint, "P", Color.white, 10f);
					}
					if (prms.drawApoapsisPoint || prms.drawApoapsisLabel) {
						//===== Apoapsis
						if (!double.IsInfinity(_bodies[i].orbitApoapsisPoint.x) && !double.IsNaN(_bodies[i].orbitApoapsisPoint.x)) {
							if (prms.drawApoapsisPoint) {
								DrawX((Vector3)_bodies[i].orbitApoapsisPoint, prms.circlesScale, Color.green, (Vector3)_bodies[i].orbitData.orbitNormal, (Vector3)_bodies[i].orbitData.semiMajorAxisBasis);
							}
							if (prms.drawApoapsisLabel) {
								DrawLabelScaled((Vector3)_bodies[i].orbitApoapsisPoint, "A", Color.white, 10f);
							}
						}
					}
					if (prms.drawCenterOfMassPoint) {
						//===== Center of mass
						Handles.color = Color.white;
						Handles.DrawWireDisc((Vector3)_bodies[i].centerOfMass, (Vector3)_bodies[i].orbitData.orbitNormal, 1f);
					}
					Vector3 asc;
					if (( prms.drawAscendingNodeLabel || prms.drawAscendingNodeLine || prms.drawAscendingNodePoint ) && _bodies[i].GetAscendingNode(out asc)) {
						//===== Ascending node
						asc = asc + (Vector3)_bodies[i].attractor.position;
						if (prms.drawAscendingNodePoint) {
							DrawX(asc, prms.circlesScale, Color.blue, (Vector3)_bodies[i].orbitData.orbitNormal, (Vector3)_bodies[i].orbitData.semiMajorAxisBasis);
						}
						if (prms.drawAscendingNodeLine) {
							Handles.color = new Color(0.1f, 0.3f, 0.8f, 0.8f);
							Handles.DrawLine((Vector3)_bodies[i].attractor.position, asc);
						}
						if (prms.drawAscendingNodeLabel) {
							DrawLabelScaled(asc, "ASC", Color.white, 10f);
						}
					}
					Vector3 desc;
					if (( prms.drawDescendingNodeLabel || prms.drawDescendingNodeLine || prms.drawDescendingNodePoint ) && _bodies[i].GetDescendingNode(out desc)) {
						//===== Descending node
						desc = desc + (Vector3)_bodies[i].attractor.position;
						if (prms.drawDescendingNodePoint) {
							DrawX(desc, prms.circlesScale, Color.blue, (Vector3)_bodies[i].orbitData.orbitNormal, (Vector3)_bodies[i].orbitData.semiMajorAxisBasis);
						}
						if (prms.drawDescendingNodeLine) {
							Handles.color = new Color(0.8f, 0.3f, 0.1f, 0.8f);
							Handles.DrawLine((Vector3)_bodies[i].attractor.position, desc);
						}
						if (prms.drawDescendingNodeLabel) {
							DrawLabelScaled(desc, "DESC", Color.white, 10f);
						}
					}
					if (prms.drawInclinationLabel) {
						//===== Inclination
						DrawInclinationMarkForBody(_bodies[i], prms.normalAxisScale);
					}
					if (prms.drawRadiusVector) {
						//===== Radius vector
						Handles.color = Color.gray;
						Handles.DrawLine((Vector3)_bodies[i].attractor.position, (Vector3)_bodies[i].position);
					}
					if (prms.drawOrbitsNormal) {
						//===== Orbit normal
						Handles.color = new Color(0.16f, 0.92f, 0.88f, 0.8f);
						Handles.DrawLine((Vector3)_bodies[i].orbitCenterPoint, (Vector3)_bodies[i].orbitCenterPoint + (Vector3)_bodies[i].orbitData.orbitNormal * prms.normalAxisScale * 5f);
						Handles.DrawWireDisc((Vector3)_bodies[i].orbitCenterPoint, (Vector3)_bodies[i].orbitData.orbitNormal, prms.normalAxisScale * 2f);
					}
					if (prms.drawSemiAxis) {
						//===== SemiMinor axis normal
						Handles.color = Color.blue;
						Handles.DrawLine((Vector3)_bodies[i].orbitCenterPoint, (Vector3)_bodies[i].orbitCenterPoint + (Vector3)_bodies[i].orbitData.semiMinorAxisBasis * prms.normalAxisScale * 5f);
						//===== SemiMajor axis normal
						Handles.color = Color.red;
						Handles.DrawLine((Vector3)_bodies[i].orbitCenterPoint, (Vector3)_bodies[i].orbitCenterPoint + (Vector3)_bodies[i].orbitData.semiMajorAxisBasis * prms.normalAxisScale * 5f);
					}
				}//if valid orbit


				if (_simControl.sceneElementsDisplayParameters.drawBodiesEclipticProjection) {
					Handles.color = Color.yellow;
					var ProjectionPos = _bodies[i].position - _simControl.eclipticNormal * CelestialBodyUtils.DotProduct(_bodies[i].position, _simControl.eclipticNormal);
					if (( ProjectionPos - _bodies[i].position ).sqrMagnitude > 1e-003d) {
						Handles.DrawDottedLine((Vector3)_bodies[i].position, (Vector3)ProjectionPos, 2f);
						Handles.DrawWireDisc((Vector3)ProjectionPos, (Vector3)_simControl.eclipticNormal, _simControl.sceneElementsDisplayParameters.circlesScale);
					}
				}
			} //for _bodies
		}

		void DrawSceneOrbitsAndVectors() {
			DrawAllOrbitsInEditor();
			ProcessVelocityArrows();
			DisplayEcliptic();
			EclipticRotationTool();
			OrbitRotationTool();
			VelocityRotationTool();
		}

		void DisplayCirclesOverBodies(Quaternion rotation) {
			if (_simControl.sceneElementsDisplayParameters.drawCirclesOverBodies) {
				Handles.color = new Color(0.56f, 0.89f, 0.4f, 0.6f);
				var selectedCB = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<CelestialBody>() : null;
				for (int i = 0; i < _bodies.Length; i++) {
					if (selectedCB == _bodies[i]) {
						continue;
					}
					if (Handles.Button((Vector3)_bodies[i].position, rotation, (float)_simControl.sceneElementsDisplayParameters.circlesScale, (float)_simControl.sceneElementsDisplayParameters.circlesScale, Handles.CircleCap)) {
						Selection.activeGameObject = _bodies[i].gameObject;
						break;
					}
				}
			}
		}

		void DisplayEcliptic() {
			if (_simControl.sceneElementsDisplayParameters.drawEclipticMark) {
				DrawArrow(Vector3.zero, (Vector3)_simControl.eclipticNormal * _simControl.sceneElementsDisplayParameters.eclipticMarkScale, Color.magenta, (Vector3)_simControl.eclipticUp);
				Handles.color = Color.magenta;
				Handles.DrawWireDisc(Vector3.zero, (Vector3)_simControl.eclipticNormal, _simControl.sceneElementsDisplayParameters.eclipticMarkScale);
				Handles.color = Color.gray;
				Handles.DrawLine(Vector3.zero, (Vector3)_simControl.eclipticUp * _simControl.sceneElementsDisplayParameters.eclipticMarkScale);
			}
		}

		void EclipticRotationTool() {
			if (_isEclipticRotating) {
				if (Selection.activeGameObject != null) {
					IsEclipticRotating = false;
					return;
				}
				var currentRot = Quaternion.LookRotation((Vector3)_simControl.eclipticNormal, (Vector3)_simControl.eclipticUp);

				var rot = Handles.RotationHandle(currentRot, Vector3.zero);
				if (GUI.changed && currentRot != rot) {
					Undo.RecordObject(_simControl, "Ecliptic plane orientation change");
					var rotFromTo = rot * Quaternion.Inverse(currentRot);
					_simControl.eclipticNormal = new Vector3d(rotFromTo * (Vector3)_simControl.eclipticNormal);
					_simControl.eclipticUp = new Vector3d(rotFromTo * (Vector3)_simControl.eclipticUp);
					EditorUtility.SetDirty(_simControl);
				}

			}
		}


		void OrbitRotationTool() {
			if (_isOrbitRotating) {
				if (Selection.activeGameObject == null) {
					IsOrbitRotating = false;
					return;
				}
				var cb = Selection.activeGameObject.GetComponent<CelestialBody>();
				if (cb == null || cb.attractor == null) {
					IsOrbitRotating = false;
					return;
				}
				var currentRot = Quaternion.LookRotation((Vector3)cb.orbitData.orbitNormal, (Vector3)cb.orbitData.semiMinorAxisBasis);
				var rot = Handles.RotationHandle(currentRot, (Vector3)cb.attractor.position);
				if (GUI.changed && currentRot != rot) {
					Undo.RecordObject(cb, "Orbit rotation change");
					cb.RotateOrbitAroundFocus(rot * Quaternion.Inverse(currentRot));
					EditorUtility.SetDirty(cb);
				}
			}
		}

		void VelocityRotationTool() {
			if (_isVelocityRotating) {
				if (Selection.activeGameObject == null) {
					IsVelocityRotating = false;
					return;
				}
				var cb = Selection.activeGameObject.GetComponent<CelestialBody>();
				if (cb == null) {
					IsOrbitRotating = false;
					return;
				}
				var currentRot = Quaternion.LookRotation((Vector3)cb.velocity, (Vector3)cb.orbitData.orbitNormal);
				var rot = Handles.RotationHandle(currentRot, (Vector3)cb.position);
				if (GUI.changed && currentRot != rot) {
					Undo.RecordObject(cb, "Velocity change");
					var rotFromTo = rot * Quaternion.Inverse(currentRot);
					cb.velocity = new Vector3d(rotFromTo * (Vector3)cb.velocity);
					cb.orbitData.isDirty = true;
					EditorUtility.SetDirty(cb);
				}
			}
		}

		/// <summary>
		/// Process mouse drag and hover events and draw velocity vectors if enabled
		/// </summary>
		void ProcessVelocityArrows() {
			if (!_simControl.sceneElementsDisplayParameters.drawVelocityVectors) {
				return;
			}
			Vector3d velocity = new Vector3d();
			Vector3 hpos = new Vector3();
			Vector3 pos = new Vector3();
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].isActiveAndEnabled) {
					velocity = _simControl.sceneElementsDisplayParameters.editGlobalVelocity ?
						_bodies[i].velocity :
						_bodies[i].relativeVelocity;
					pos = (Vector3)( _bodies[i].position + velocity * _simControl.sceneElementsDisplayParameters.velocitiesArrowsScale );
					Handles.DrawCapFunction capFunc;
					switch (_simControl.sceneElementsDisplayParameters.velocityHandlerType) {
						case VelocityHandlerType.Circle:
							capFunc = Handles.CircleCap;
							break;
						case VelocityHandlerType.Sphere:
							capFunc = Handles.SphereCap;
							break;
						case VelocityHandlerType.Dot:
							capFunc = Handles.DotCap;
							break;
						default:
							continue;
					}
					Handles.color = Color.white;
					hpos = Handles.FreeMoveHandle(pos, Quaternion.identity, _simControl.sceneElementsDisplayParameters.handleScale * HandleUtility.GetHandleSize(pos), Vector3.zero, capFunc);
					if (pos != hpos) {
						//===== Project onto orbit plane
						if (_bodies[i].attractor != null && !_simControl.sceneElementsDisplayParameters.editGlobalVelocity && ( _simControl.sceneElementsDisplayParameters.keepOrbitPlaneWhileChangeVelocity || _simControl.keepBodiesOnEclipticPlane )) {
							Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
							hpos = CelestialBodyUtils.GetRayPlaneIntersectionPoint((Vector3)_bodies[i].position, (Vector3)_bodies[i].orbitData.orbitNormal, ray.origin, ray.direction);
						}
						//=====
						velocity = ( new Vector3d(hpos) - _bodies[i].position ) / _simControl.sceneElementsDisplayParameters.velocitiesArrowsScale;
						Undo.RecordObject(_bodies[i], "Velocity change");
						if (_simControl.sceneElementsDisplayParameters.editGlobalVelocity) {
							_bodies[i].velocity = velocity;
						} else {
							_bodies[i].relativeVelocity = velocity;
						}
						_bodies[i].orbitData.isDirty = true;
						if (_simControl.sceneElementsDisplayParameters.selectBodyWhenDraggingVelocity) {
							Selection.activeGameObject = _bodies[i].gameObject;
						}
					}
				}
			}
			ShowAllVelocitiesVectors();
		}


		/// <summary>
		/// Draw velocity vectors in scene view for all active celestial bodies
		/// </summary>
		void ShowAllVelocitiesVectors() {
			foreach (var body in _bodies) {
				if (body.isActiveAndEnabled) {
					if (_simControl.sceneElementsDisplayParameters.drawArrowsHead) {
						DrawArrow(
							body.position,
							body.position + ( _simControl.sceneElementsDisplayParameters.editGlobalVelocity ? body.velocity : body.relativeVelocity ) * _simControl.sceneElementsDisplayParameters.velocitiesArrowsScale,
							Selection.activeTransform != null && Selection.activeTransform == body.transformRef ? Color.cyan : Color.green,
							body.isValidOrbit ? body.orbitData.orbitNormal : _simControl.eclipticNormal
						);
					} else {
						Handles.color = Selection.activeTransform == body.transformRef ? Color.cyan : Color.green;
						Handles.DrawLine(
							(Vector3)body.position,
							(Vector3)( body.position + ( _simControl.sceneElementsDisplayParameters.editGlobalVelocity ? body.velocity : body.relativeVelocity ) * _simControl.sceneElementsDisplayParameters.velocitiesArrowsScale ));
					}
				}
			}
		}

		void DrawArrow(Vector3d from, Vector3d to, Color col, Vector3d normal) {
			DrawArrow((Vector3)from, (Vector3)to, col, (Vector3)normal);
		}

		/// <summary>
		/// Draw simple arrow in scene window at given world coordinates
		/// </summary>
		void DrawArrow(Vector3 from, Vector3 to, Color col, Vector3 normal) {
			var dir = to - from;
			float dist = dir.magnitude;
			var dirNorm = dir / dist; //normalized vector
			float headSize = dist / 6f;
			var _colBefore = Handles.color;
			Handles.color = col;
			Vector3 sideNormal = CelestialBodyUtils.CrossProduct(dir, normal).normalized;
			Handles.DrawLine(from, from + dirNorm * ( dist - headSize ));
			Handles.DrawLine(from + dirNorm * ( dist - headSize ) + sideNormal * headSize / 2f, from + dirNorm * ( dist - headSize ) - sideNormal * headSize / 2f);
			Handles.DrawLine(from + dirNorm * ( dist - headSize ) + sideNormal * headSize / 2f, from + dir);
			Handles.DrawLine(from + dirNorm * ( dist - headSize ) - sideNormal * headSize / 2f, from + dir);
			Handles.color = _colBefore;
		}

		/// <summary>
		/// Draw orbits for bodies which has drawing orbit enabled
		/// </summary>
		void DrawAllOrbitsInEditor() {
			if (_simControl.sceneElementsDisplayParameters.drawOrbits) {
				foreach (var body in _bodies) {
					if (body.isDrawOrbit) {
						DrawOrbitInEditorFor(body);
					}
				}
			}
		}

		void DrawOrbitInEditorFor(CelestialBody body) {
			int pointsCount = _simControl.sceneElementsDisplayParameters.orbitPointsCount;
			if (body.isActiveAndEnabled) {
				if (!Application.isPlaying && body.attractor != null && body.orbitData.isDirty) {
					if (body.attractor.mass <= 0) {
						body.attractor.mass = 1e-007;//to avoid div by zero
					}
					body.CalculateNewOrbitData();
				}
				Handles.color = Color.white;
				var points = body.GetOrbitPointsDouble(pointsCount, false, _simControl.sceneElementsDisplayParameters.maxOrbitDistance);
				for (int i = 1; i < points.Length; i++) {
					Handles.DrawLine((Vector3)points[i - 1], (Vector3)points[i]);
				}
				if (_simControl.sceneElementsDisplayParameters.drawOrbitsEclipticProjection && points.Length > 0) {
					var point1 = points[0] - _simControl.eclipticNormal * CelestialBodyUtils.DotProduct(points[0], _simControl.eclipticNormal);
					var point2 = Vector3d.zero;
					Handles.color = Color.gray;
					for (int i = 1; i < points.Length; i++) {
						point2 = points[i] - _simControl.eclipticNormal * CelestialBodyUtils.DotProduct(points[i], _simControl.eclipticNormal);
						Handles.DrawLine((Vector3)point1, (Vector3)point2);
						point1 = point2;
					}
				}
			}
		}

		void DrawInclinationMarkForBody(CelestialBody body, float scale) {
			var norm = CelestialBodyUtils.CrossProduct((Vector3)body.orbitData.orbitNormal, (Vector3)_simControl.eclipticNormal);
			Handles.color = Color.white;
			var p = CelestialBodyUtils.CrossProduct(norm, (Vector3)_simControl.eclipticNormal).normalized;
			Handles.DrawLine((Vector3)body.orbitFocusPoint, (Vector3)body.orbitFocusPoint + p * 3f * scale);
			Handles.DrawLine((Vector3)body.orbitFocusPoint, (Vector3)body.orbitFocusPoint + CelestialBodyUtils.RotateVectorByAngle(p, (float)body.orbitData.inclination, -norm.normalized) * 3f * scale);
			Handles.DrawWireArc((Vector3)body.orbitFocusPoint, -norm, p, (float)( body.orbitData.inclination * Mathd.Rad2Deg ), 1f * scale);
			DrawLabelScaled((Vector3)body.orbitFocusPoint + p * scale, ( body.orbitData.inclination * Mathd.Rad2Deg ).ToString("0") + "\u00B0", Color.white, 10);
		}

		/// <summary>
		/// Draw two crossing lines in scene view
		/// </summary>
		static void DrawX(Vector3 pos, float size, Color col, Vector3 normal, Vector3 up) {
			Handles.color = col;
			Vector3 right = CelestialBodyUtils.CrossProduct(up, normal).normalized;
			Handles.DrawLine(pos + up * size, pos - up * size);
			Handles.DrawLine(pos + right * size, pos - right * size);
		}

		static void DrawLabel(Vector3 pos, string text, Color color, float sizeMlt) {
			GUIStyle style = new GUIStyle();
			style.normal.textColor = color;
			style.fontSize = (int)sizeMlt;
			Handles.BeginGUI();
			GUI.Label(new Rect(HandleUtility.WorldToGUIPoint(pos), new Vector2(100, 100)), text, style);
			Handles.EndGUI();
		}

		void DrawLabelScaled(Vector3 pos, string text, Color color, float sizeMlt) {
			if (_simControl.sceneElementsDisplayParameters.drawLabels) {
				DrawLabel(pos, text, color, sizeMlt * _simControl.sceneElementsDisplayParameters.labelsScale);
			}
		}

		#endregion
	}


}