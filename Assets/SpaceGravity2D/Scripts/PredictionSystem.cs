using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpaceGravity2D {

	/// <summary>
	/// basic prediction orbits calculator
	/// </summary>
	[AddComponentMenu("SpaceGravity2D/PredictionSystem")]
	public class PredictionSystem : MonoBehaviour {

		struct BodyPoints {
			public Vector3 pos;
			public Vector3 v;
			public float m;
			public bool isFixed;
			public bool isVisible;
			public Material material;
			public float width;
			public Vector3[] points;
		}

		public SimulationControl SimControl;

		[Tooltip("Larger - better")]
		/// <summary>
		/// larger - better
		/// </summary>
		public float CalcStep = 1f;

		public int PointsCount = 50;
		public Material LinesMaterial;
		public float LinesWidth = 0.05f;
		public bool ShowTraceForDisabledBodies = false;

		private BodyPoints[] bodies = new BodyPoints[0];
		List<LineRenderer> lineRends = new List<LineRenderer>();

		void Start() {
			if (SimControl == null) {
				SimControl = GameObject.FindObjectOfType<SimulationControl>();
			}
			if (SimControl == null) {
				enabled = false;
			}
		}

		void Update() {
			if (Mathd.Abs(SimControl.timeScale) > 1e-6) {
				Calc();
			}
			ShowPredictOrbit();
		}

		void OnDisable() {
			HideAllOrbits();
		}

		void Calc() {
			//>===== Filter disabled bodies
			List<CelestialBody> targets = new List<CelestialBody>();
			for (int i = 0; i < SimControl.bodies.Count; i++) {
				if (SimControl.bodies[i].isActiveAndEnabled) {
					targets.Add(SimControl.bodies[i]);
				} else {
					if (ShowTraceForDisabledBodies && SimControl.bodies[i].gameObject.activeSelf) {
						targets.Add(SimControl.bodies[i]);
					}
				}
			}
			//<=====

			//>=====Check is any body visible
			if (!targets.Any(t => {
				var targetComponent = t.GetComponent<PredictionSystemTarget>();
				return targetComponent == null || targetComponent.enabled;
			})) {
				for (int i = 0; i < lineRends.Count; i++) {
					lineRends[i].enabled = false;
				}
				bodies = new BodyPoints[0];
				HideAllOrbits();
				return;//Don't calculate
			}
			//<=====

			//>===== Create working data array from bodies.
			bodies = new BodyPoints[targets.Count];
			for (int i = 0; i < bodies.Length; i++) {
				var targetComponent = targets[i].GetComponent<PredictionSystemTarget>();
				bool isVisibleOrb = targetComponent == null || targetComponent.enabled;
				Material mat = targetComponent == null ? LinesMaterial : targetComponent.OrbitMaterial;

				bodies[i] = new BodyPoints() {
					pos = targets[i].transformRef.position,
					v = (Vector3)targets[i].velocity,
					m = (float)targets[i].mass,
					isFixed = targets[i].isFixedPosition,
					isVisible = isVisibleOrb,
					material = mat,
					width = targetComponent == null ? LinesWidth : targetComponent.OrbitWidth,
					points = new Vector3[PointsCount]
				};
			}
			//<=====
			//>===== Calculate scene motion progress and record points into arrays.
			for (int i = 0; i < PointsCount; i++) {
				//>===== calculate next step velocities for each body
				for (int j = 0; j < bodies.Length; j++) {
					if (bodies[j].isFixed) {
						continue;
					}
					Vector3 acceleration = Vector3.zero;
					for (int n = 0; n < bodies.Length; n++) {
						if (n != j) {
							acceleration += Acceleration(bodies[j].pos, bodies[n].pos, bodies[n].m * (float)SimControl.gravitationalConstant, 0.5f, (float)SimControl.maxAttractionRange);
						}
					}
					bodies[j].v += acceleration * CalcStep;
				}
				//<=====
				//>===== move bodies and store current step positions
				for (int j = 0; j < bodies.Length; j++) {
					if (bodies[j].isFixed) {
						continue;
					}
					bodies[j].points[i] = bodies[j].pos;
					bodies[j].pos += bodies[j].v * CalcStep;
				}
				//<=====
			}
			//<======
		}

		void ShowPredictOrbit() {
			int t = 0;
			while (lineRends.Count < bodies.Length && t < 1000) {
				CreateLineRenderer();
				t++;
			}
			var i = 0;
			for (i = 0; i < bodies.Length; i++) {
				if (bodies[i].isVisible) {
					lineRends[i].SetVertexCount(PointsCount + 1);
					lineRends[i].SetWidth(bodies[i].width, bodies[i].width);
					lineRends[i].material = bodies[i].material ?? LinesMaterial;
					for (int j = 0; j < bodies[i].points.Length; j++) {
						lineRends[i].SetPosition(j, bodies[i].points[j]);
					}
					lineRends[i].SetPosition(PointsCount, bodies[i].pos); //last point is not in array;
					lineRends[i].enabled = true;
				} else {
					lineRends[i].enabled = false;
				}
			}
			for (; i < lineRends.Count; i++) {
				lineRends[i].enabled = false;
			}
		}

		void CreateLineRenderer() {
			var o = new GameObject("prediction orbit " + lineRends.Count);
			o.transform.SetParent(transform);
			var lineRend = o.AddComponent<LineRenderer>();
			lineRends.Add(lineRend);
		}

		public void HideAllOrbits() {
			for (int i = 0; i < lineRends.Count; i++) {
				lineRends[i].enabled = false;
			}
		}

		static Vector3 Acceleration(Vector3 pos, Vector3 attractorPos, float attractorMG, float minRange, float maxRange) {
			Vector3 distanceVector =  attractorPos - pos;
			if (maxRange != 0 && distanceVector.sqrMagnitude > maxRange * maxRange || distanceVector.sqrMagnitude < minRange * minRange) {
				return Vector3.zero;
			}
			var distanceMagnitude = distanceVector.magnitude;
			return distanceVector * attractorMG / distanceMagnitude / distanceMagnitude / distanceMagnitude;
		}
	}
}