using UnityEngine;
using System.Collections;

namespace SpaceGravity2D {
	[AddComponentMenu("SpaceGravity2D/OrbitDisplay")]
	public class OrbitDisplay : MonoBehaviour {

		CelestialBody _body;
		LineRenderer _lineRenderer;

		public Material OrbitLineMaterial;
		public float Width = 0.1f;
		public int OrbitPointsCount = 50;
		public float maxHyperbolicDist= 100;

		void OnEnable() {
			if (_lineRenderer == null) {
				CreateLineRend();
			}
			if (_body == null) {
				_body = GetComponentInParent<CelestialBody>();
				if (_body == null) {
					Debug.Log("SpaceGravity2D: Orbit Display can't find celestial body on " + name);
					enabled = false;
				}
			}
		}
		void OnDisable() {
			HideOrbit();
		}

		void Update() {
			DrawOrbit();
		}

		public void DrawOrbit() {
			if (!_lineRenderer) {
				CreateLineRend();
			}
			_lineRenderer.enabled = true;
			_lineRenderer.SetWidth(Width, Width);
			if (_lineRenderer.material != OrbitLineMaterial) {
				_lineRenderer.material = OrbitLineMaterial;
			}
			Vector3[] points = _body.GetOrbitPoints(OrbitPointsCount, false, maxHyperbolicDist);
			
			_lineRenderer.SetVertexCount(points.Length);
			for (int i = 0; i < points.Length; i++) {
				_lineRenderer.SetPosition(i, points[i]);
			}
		}

		public void HideOrbit() {
			if (_lineRenderer) {
				_lineRenderer.enabled = false;
			}
		}

		void CreateLineRend() {
			GameObject lineRendObj = new GameObject("OrbitLineRenderer");
			lineRendObj.transform.SetParent(transform);
			lineRendObj.transform.position = Vector3.zero;
			_lineRenderer = lineRendObj.AddComponent<LineRenderer>();
			_lineRenderer.material = OrbitLineMaterial;
		}
	}
}