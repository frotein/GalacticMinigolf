using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
namespace SpaceGravity2D.Inspector {

	[CustomEditor(typeof(CelestialBody))]
	[CanEditMultipleObjects()]
	public class CelestialBodyEditor : Editor {

		CelestialBody _body;
		CelestialBody[] _bodies;
		SerializedProperty dynamicChangeIntervalProp;
		SerializedProperty attractorProp;
		SerializedProperty maxInfRangeProp;
		SerializedProperty velocityProp;

		bool preferDegrees;


		[MenuItem("GameObject/SpaceGravity2D/CelestialBody")]
		public static void CreateGameObject() {
			var go = new GameObject("CelestialBody");
			Undo.RegisterCreatedObjectUndo(go, "new CelestialBody");
			go.AddComponent<CelestialBody>();
			Selection.activeObject = go;
		}

		void OnEnable() {
			//initialize properties to display
			_body = target as CelestialBody;
			var celestials = new List<CelestialBody>();
			for (int i = 0; i < Selection.gameObjects.Length; i++) {
				var gocb = Selection.gameObjects[i].GetComponent<CelestialBody>();
				if (gocb != null) {
					celestials.Add(gocb);
				}
			}
			_bodies = celestials.ToArray();
			for (int i = 0; i < _bodies.Length; i++) {
				_bodies[i].FindReferences();
			}
			if (!_body.simControlRef) {
				_body.simControlRef = SimulationParametersWindow.FindSimulationControlGameObject() ??
				                      SimulationParametersWindow.CreateSimulationControl();
			}
			dynamicChangeIntervalProp = serializedObject.FindProperty("searchAttractorInterval");
			attractorProp = serializedObject.FindProperty("attractor");
			maxInfRangeProp = serializedObject.FindProperty("maxAttractionRange");
			velocityProp = serializedObject.FindProperty("velocity");

			AssignIconImage();
		}

		void AssignIconImage() {
			Texture2D icon = (Texture2D)Resources.Load("Textures/icon");
			if (icon != null) {
				typeof(EditorGUIUtility).InvokeMember(
					"SetIconForObject",
					BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic,
					null,
					null,
					new object[] { _body, icon }
				);
			}
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			ShowActionsButtons();
			ShowToggles();
			ShowGravityProperties();
			ShowVectors();
			ShowOrbitParameters();

			if (GUI.changed) {
				for (int i = 0; i < _bodies.Length; i++) {
					_bodies[i].orbitData.isDirty = true;
					EditorUtility.SetDirty(_bodies[i]);
				}
			}
		}

		void ShowToggles() {
			EditorGUILayout.LabelField("Toggles:", EditorStyles.boldLabel);
			var isFixedPropValue = GUILayout.Toggle(_body.isFixedPosition, new GUIContent("Is fixed position", "Relative to attractor"), "Button");
			if (isFixedPropValue != _body.isFixedPosition) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Toggle fixed position");
					_bodies[i].isFixedPosition = isFixedPropValue;
				}
			}
			//===============
			var useRailValue = GUILayout.Toggle(_body.useKeplerMotion, new GUIContent("Use RailMotion", "Keplerian/Newtonian motion type toggle"), "Button");
			if (useRailValue != _body.useKeplerMotion) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Toggle keplerian motion");
					_bodies[i].useKeplerMotion = useRailValue;
				}
			}
			//===============
			var drawOrbitValue = GUILayout.Toggle(_body.isDrawOrbit, new GUIContent("Draw Orbit", "Drawing orbits depends on global settings"), "Button");
			if (drawOrbitValue != _body.isDrawOrbit) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Toggle object orbit draw");
					_bodies[i].isDrawOrbit = drawOrbitValue;
				}
			}
			//===============
			var dynamicChangingValue = GUILayout.Toggle(_body.isAttractorSearchActive, new GUIContent("Dynamic attractor changing", "search most proper attractor continiously. It is recommended not to use on many objects due to performance. For large amount of bodies better to use spheres-of-influence colliders"), "Button");
			if (dynamicChangingValue != _body.isAttractorSearchActive) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Toggle attractor searching");
					_bodies[i].isAttractorSearchActive = dynamicChangingValue;
				}
			}
			//===============
			if (dynamicChangingValue) {
				EditorGUI.showMixedValue = dynamicChangeIntervalProp.hasMultipleDifferentValues;
				var intervalValue = EditorGUILayout.FloatField(new GUIContent("search interval", "in seconds"), dynamicChangeIntervalProp.floatValue);
				if (intervalValue != dynamicChangeIntervalProp.floatValue) {
					for (int i = 0; i < _bodies.Length; i++) {
						Undo.RecordObject(_bodies[i], "Change search interval");
						_bodies[i].searchAttractorInterval = intervalValue;
					}
				}
				EditorGUI.showMixedValue = false;
			}
		}

		void ShowGravityProperties() {
			var mixedMass = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].mass != _body.mass) {
					mixedMass = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedMass;
			var massValue = EditorGUILayout.Slider("Mass", (float)_body.mass, 1e-3f, 1e6f);
			if (massValue != _body.mass) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Mass change");
					_bodies[i].mass = massValue;
				}
			}
			/// ==============
			EditorGUI.showMixedValue = maxInfRangeProp.hasMultipleDifferentValues;
			var maxInfValue = EditorGUILayout.FloatField(
				new GUIContent("influence range:", "Body's own max influence range of attraction force. this option competes with the same global property"),
				maxInfRangeProp.floatValue
				);
			if (maxInfValue != maxInfRangeProp.floatValue) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Body gravity range change");
					_bodies[i].maxAttractionRange = maxInfValue;
				}
			}
			/// ==============
			EditorGUI.showMixedValue = attractorProp.hasMultipleDifferentValues;
			var attrValue = EditorGUILayout.ObjectField(new GUIContent("Attractor"), attractorProp.objectReferenceValue, typeof(CelestialBody), true) as CelestialBody;
			if (attrValue != attractorProp.objectReferenceValue) {
				for (int i = 0; i < _bodies.Length; i++) {
					if (_bodies[i] != attrValue) {
						Undo.RecordObject(_bodies[i], "Attractor ref change");
						_bodies[i].attractor = attrValue;
					}
				}
			}
			if (GUILayout.Button("Remove attractor")) {
				for (int i = 0; i < _bodies.Length; i++) {
					if (_bodies[i].attractor != null) {
						Undo.RecordObject(_bodies[i], "Removing attractor ref");
						_bodies[i].attractor = null;
						_bodies[i].TerminateKeplerMotion();
					}
				}
			}
		}
		void ShowVectors() {
			EditorGUI.showMixedValue = velocityProp.hasMultipleDifferentValues;
			EditorGUILayout.PropertyField(velocityProp, new GUIContent("Velocity(?):", "World space Velocity"));
			if (GUI.changed) {
				serializedObject.ApplyModifiedProperties();
			}
			/// ==============
			var mixedLen = false;
			var lenSqr = _body.velocity.sqrMagnitude;
			for (int i = 0; i < _bodies.Length; i++) {
				if (lenSqr != _bodies[i].velocity.sqrMagnitude) {
					mixedLen = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedLen;
			var startLen = _body.velocity.magnitude;
			var lenValue = EditorGUILayout.FloatField(new GUIContent("Velocity magnitude"), (float)startLen);
			if (lenValue != startLen) {
				for (int i = 0; i < _bodies.Length; i++) {
					var bodyVelocityLen = _bodies[i].velocity.magnitude;
					if (!Mathd.Approximately(lenValue, bodyVelocityLen)) {
						Undo.RecordObject(_bodies[i], "Velocity magnitude change");
						_bodies[i].velocity = _bodies[i].velocity * ( Mathd.Approximately(bodyVelocityLen, 0) ? lenValue : lenValue / bodyVelocityLen );
					}
				}
			}
			/// ==============
			EditorGUILayout.Separator();
			var mixedRelVelocity = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i] != _body && _bodies[i].relativeVelocity != _body.relativeVelocity) {
					mixedRelVelocity = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedRelVelocity;
			var relVelocityValue = Vector3dField(new GUIContent("RelV"), _body.relativeVelocity);// EditorGUILayout.Vector3Field("Relative velocity:", (Vector3)_body.RelativeVelocity);
			if (relVelocityValue != _body.relativeVelocity) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Rel.Velocity change");
					_bodies[i].relativeVelocity = relVelocityValue;
				}
			}
			/// ==============
			var mixedRelLen = false;
			var relLenSqr = _body.relativeVelocity.sqrMagnitude;
			for (int i = 0; i < _bodies.Length; i++) {
				if (relLenSqr != _bodies[i].relativeVelocity.sqrMagnitude) {
					mixedRelLen = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedRelLen;
			var startRelLen = _body.relativeVelocity.magnitude;
			var relLenValue = EditorGUILayout.FloatField(new GUIContent("Rel.Velocity magn."), (float)startRelLen);
			if (relLenValue != startRelLen) {
				for (int i = 0; i < _bodies.Length; i++) {
					var bodyRelVelocityLen = _bodies[i].relativeVelocity.magnitude;
					if (!Mathd.Approximately(relLenValue, bodyRelVelocityLen)) {
						Undo.RecordObject(_bodies[i], "Rel.Velocity magnitude change");
						_bodies[i].relativeVelocity = _bodies[i].relativeVelocity * ( Mathd.Approximately(bodyRelVelocityLen, 0) ? relLenValue : relLenValue / bodyRelVelocityLen );
					}
				}
			}
			/// ==============
			EditorGUILayout.Separator();
			//var relPositionInit = _body.relativePosition;

			EditorGUI.BeginChangeCheck();
			var relPositionValue = Vector3dField(new GUIContent("RelPos"), _body.relativePosition);
			if (EditorGUI.EndChangeCheck()) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Rel.Position change");
					_bodies[i].relativePosition = relPositionValue;
				}
			}
		}

		void ShowActionsButtons() {
			EditorGUILayout.LabelField("Actions:", EditorStyles.boldLabel);
			if (GUILayout.Button("Reset velocity")) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Reset Velocity");
					_bodies[i].relativeVelocity = Vector3d.zero;
					_bodies[i].TerminateKeplerMotion();
				}

			}
			if (GUILayout.Button(new GUIContent("Find nearest attractor", "Note: nearest attractor not always most proper"))) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Nearest attractor assign");
					_bodies[i].FindAndSetNearestAttractor();
					_bodies[i].TerminateKeplerMotion();
				}
			}
			if (GUILayout.Button(new GUIContent("Find biggest attractor"))) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Biggest attractor assign");
					_bodies[i].FindAndSetBiggestAttractor();
					_bodies[i].TerminateKeplerMotion();
				}
			}
			if (GUILayout.Button(new GUIContent("Find most proper attractor", "Choose most realistic attractor for this body at current position"))) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Most proper attractor assign");
					_bodies[i].FindAndSetMostProperAttractor();
					_bodies[i].TerminateKeplerMotion();
				}
			}
			if (!_body.attractor) {
				GUI.enabled = false; //turn button off if attractor object is not assigned
			}
			if (GUILayout.Button("Make Orbit Circle")) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Rounding orbit");
					_bodies[i].MakeOrbitCircle();
					_bodies[i].TerminateKeplerMotion();
				}
			}
			if (!_body.attractor) {
				GUI.enabled = true;
			}
			if (GUILayout.Button("Project onto ecliptic plane")) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i], "Projection onto ecliptic");
					_bodies[i].ProjectOntoEclipticPlane();
				}
			}
			//===============
			if (SceneViewDisplayManager.Instance != null) {
				var b = GUILayout.Toggle(SceneViewDisplayManager.Instance.IsOrbitRotating, new GUIContent("Rotate Orbit Tool"), "Button");
				if (b != SceneViewDisplayManager.Instance.IsOrbitRotating) {
					SceneViewDisplayManager.Instance.IsOrbitRotating = b;
				}
			}

			//===============
			if (SceneViewDisplayManager.Instance != null) {
				var b = GUILayout.Toggle(SceneViewDisplayManager.Instance.IsVelocityRotating, new GUIContent("Rotate Velocity Tool"), "Button");
				if (b != SceneViewDisplayManager.Instance.IsVelocityRotating) {
					SceneViewDisplayManager.Instance.IsVelocityRotating = b;
				}
			}
		}

		Vector3d Vector3dField(GUIContent content, Vector3d vector) {
			var rect = EditorGUILayout.GetControlRect();
			EditorGUILayout.GetControlRect();
			EditorGUILayout.GetControlRect(); //free space for 3 lines;
			var xyzWidth = 15f;
			var tempWidth = EditorGUIUtility.labelWidth;
			EditorGUI.LabelField(new Rect(rect.x, rect.y, tempWidth - xyzWidth, rect.height), content);
			EditorGUIUtility.labelWidth = xyzWidth;
			double x = EditorGUI.DoubleField(new Rect(rect.x + tempWidth - xyzWidth, rect.y, rect.width - tempWidth + xyzWidth, rect.height), "x", vector.x);
			double y = EditorGUI.DoubleField(new Rect(rect.x + tempWidth - xyzWidth, rect.y + rect.height, rect.width - tempWidth + xyzWidth, rect.height), "y", vector.y);
			double z = EditorGUI.DoubleField(new Rect(rect.x + tempWidth - xyzWidth, rect.y + rect.height * 2, rect.width - tempWidth + xyzWidth, rect.height), "z", vector.z);
			EditorGUIUtility.labelWidth = tempWidth;
			return new Vector3d(x, y, z);
		}

		void ShowOrbitParameters() {
			EditorGUILayout.LabelField("Current state:", EditorStyles.boldLabel);
			if (_body.attractor == null) {
				EditorGUILayout.LabelField("Eccentricity", "-");
				EditorGUILayout.LabelField("Mean Anomaly", "-");
				EditorGUILayout.LabelField("True Anomaly", "-");
				EditorGUILayout.LabelField("Eccentric Anomaly", "-");
				EditorGUILayout.LabelField("Argument of Periapsis", "-");
				EditorGUILayout.LabelField("Apoapsis", "-");
				EditorGUILayout.LabelField("Periapsis", "-");
				EditorGUILayout.LabelField("Period", "-");
				EditorGUILayout.LabelField("Energy", "-");
				EditorGUILayout.LabelField("Distance to focus", "-");
				EditorGUILayout.LabelField("Semi major axys", "-");
				EditorGUILayout.LabelField("Semi minor axys", "-");
				return;
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Angle display:");
			var __preferDegrees = preferDegrees;
			preferDegrees = !GUILayout.Toggle(!preferDegrees, "Radians", "Button");
			preferDegrees = GUILayout.Toggle(preferDegrees, "Degrees", "Button");
			EditorGUILayout.EndHorizontal();
			///==================== Editable values:
			///====================
			bool mixedEcc = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].eccentricity != _body.eccentricity) {
					mixedEcc = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedEcc;
			var eccValue = EditorGUILayout.DoubleField(new GUIContent("Eccentricity"), _body.eccentricity);
			if (eccValue != _body.eccentricity) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i].transformRef, "Eccentricity change");
					Undo.RecordObject(_bodies[i], "Eccentricity change");
					_bodies[i].eccentricity = eccValue;
				}
			}
			///====================
			EditorGUILayout.Space();
			bool mixedAnomaly_m = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].meanAnomaly != _body.meanAnomaly) {
					mixedAnomaly_m = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedAnomaly_m;
			var anomalyInput_m = __preferDegrees ? _body.meanAnomaly * Mathd.Rad2Deg : _body.meanAnomaly;
			var anomalyValue_m = EditorGUILayout.DoubleField(new GUIContent("Mean Anomaly(rad)"), anomalyInput_m);
			if (anomalyValue_m != anomalyInput_m) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i].transformRef, "Mean anomaly change");
					Undo.RecordObject(_bodies[i], "Mean anomaly change");
					_bodies[i].meanAnomaly = __preferDegrees ? anomalyValue_m * Mathd.Deg2Rad : anomalyValue_m;
				}
			}
			///====================
			EditorGUILayout.Space();
			bool mixedAnomaly_t = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].trueAnomaly != _body.trueAnomaly) {
					mixedAnomaly_t = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedAnomaly_t;
			var anomalyInput_t = __preferDegrees ? _body.trueAnomaly * Mathd.Rad2Deg : _body.trueAnomaly;
			var anomalyValue_t = EditorGUILayout.DoubleField(new GUIContent("True Anomaly(rad)"), anomalyInput_t);
			
			if (anomalyValue_t != anomalyInput_t) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i].transformRef, "True anomaly change");
					Undo.RecordObject(_bodies[i], "True anomaly change");
					_bodies[i].trueAnomaly = __preferDegrees ? anomalyValue_t * Mathd.Deg2Rad : anomalyValue_t;
				}
			}
			///====================
			EditorGUILayout.Space();
			bool mixedAnomaly_e = false;
			for (int i = 0; i < _bodies.Length; i++) {
				if (_bodies[i].eccentricAnomaly != _body.eccentricAnomaly) {
					mixedAnomaly_e = true;
					break;
				}
			}
			EditorGUI.showMixedValue = mixedAnomaly_e;
			var anomalyInput_e = __preferDegrees ? _body.eccentricAnomaly * Mathd.Rad2Deg : _body.eccentricAnomaly;
			var anomalyValue_e = EditorGUILayout.DoubleField(new GUIContent("Eccentric Anomaly(rad)"), anomalyInput_e);
			if (anomalyValue_e != anomalyInput_e) {
				for (int i = 0; i < _bodies.Length; i++) {
					Undo.RecordObject(_bodies[i].transformRef, "Eccentric anomaly change");
					Undo.RecordObject(_bodies[i], "Eccentric anomaly change");
					_bodies[i].eccentricAnomaly = __preferDegrees ? anomalyValue_e * Mathd.Deg2Rad : anomalyValue_e;
				}
			}
			EditorGUI.showMixedValue = false;
			///====================
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Apoapsis", _body.orbitData.apoapsisDistance.ToString());
			EditorGUILayout.LabelField("Periapsis", _body.orbitData.periapsisDistance.ToString());
			EditorGUILayout.LabelField("Period", _body.orbitData.period.ToString());
			EditorGUILayout.LabelField("Energy", _body.orbitData.energyTotal.ToString());
			EditorGUILayout.LabelField("Distance to focus", _body.orbitData.attractorDistance.ToString("0.000"));
			EditorGUILayout.LabelField("Semi major axis", _body.orbitData.semiMajorAxis.ToString("0.000"));
			EditorGUILayout.LabelField("Semi minor axis", _body.orbitData.semiMinorAxis.ToString("0.000"));
			EditorGUILayout.LabelField("Semi minor axis normal", _body.orbitData.semiMinorAxisBasis.ToString("0.000"));
			EditorGUILayout.LabelField("Semi major axis normal", _body.orbitData.semiMajorAxisBasis.ToString("0.000"));
			EditorGUILayout.LabelField("Orbit normal", _body.orbitData.orbitNormal.ToString("0.000"));
			EditorGUILayout.LabelField("Inclination", __preferDegrees ? ( _body.orbitData.inclination * Mathd.Rad2Deg ).ToString("0.000") : _body.orbitData.inclination.ToString("0.000"));

		}
	}

}