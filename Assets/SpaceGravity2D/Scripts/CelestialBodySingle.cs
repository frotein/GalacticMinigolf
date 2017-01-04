using UnityEngine;

namespace SpaceGravity2D {
	[AddComponentMenu("SpaceGravity2D/CelestialBodySingle")]
	[SelectionBase]
	public class CelestialBodySingle : MonoBehaviour {
		public Transform attractorObject;
		public Vector3 attractorPosition;
		public Transform velocityHandle;
		public float velocityMlt = 1f;
		public float attractorMass = 1000;
		public float G = 0.1f;
		public int orbitPointsCount = 50;
		public float maxDistForHyperbolicCase = 100f;

		public OrbitData orbitData = new OrbitData();
		public LineRenderer linerend;

		void Start() {
			linerend = GetComponent<LineRenderer>();
		}

		void Update() {
			if (attractorObject != null) {
				attractorPosition = attractorObject.position;
			}
			if (linerend != null) {
				var points = orbitData.GetOrbitPoints(orbitPointsCount, 1000f);
				linerend.SetVertexCount(points.Length);
				for (int i = 0; i < points.Length; i++) {
					linerend.SetPosition(i, attractorPosition + (Vector3)points[i]);
				}
			}

			orbitData.UpdateOrbitDataByTime(Time.deltaTime);
			transform.position = attractorPosition + (Vector3)orbitData.position;
			if (velocityHandle != null) {
				velocityHandle.position = transform.position + (Vector3)orbitData.velocity * velocityMlt;
			}
		}

		void UpdateOrbitData() {
			orbitData.attractorMass = attractorMass;
			orbitData.gravConst = G;
			orbitData.position = new Vector3d(transform.position - attractorPosition);
			orbitData.velocity = new Vector3d(( velocityHandle.position - transform.position ) * velocityMlt);
			orbitData.CalculateNewOrbitData();

		}

		void OnDrawGizmos() {
			if (velocityHandle != null) {
				if (!Application.isPlaying) {
					if (attractorObject != null) {
						attractorPosition = attractorObject.position;
					}
					UpdateOrbitData();
				}
				ShowVelocity();
				ShowOrbit();
				ShowNodes();
			}
		}

		void ShowVelocity() {
			Gizmos.DrawLine(transform.position, transform.position + (Vector3)orbitData.GetVelocityAtEccentricAnomaly(orbitData.eccentricAnomaly));
		}

		void ShowOrbit() {
			var points = orbitData.GetOrbitPoints(orbitPointsCount, (double)maxDistForHyperbolicCase);
			Gizmos.color = new Color(1, 1, 1, 0.3f);
			for (int i = 0; i < points.Length - 1; i++) {
				Gizmos.DrawLine(attractorPosition + (Vector3)points[i], attractorPosition + (Vector3)points[i + 1]);
			}
		}

		void ShowNodes() {
			Vector3 asc;
			if (orbitData.GetAscendingNode(out asc)) {
				Gizmos.color = new Color(0.9f, 0.4f, 0.2f, 0.5f);
				Gizmos.DrawLine(attractorPosition, attractorPosition + asc);
			}
			Vector3 desc;
			if (orbitData.GetDescendingNode(out desc)) {
				Gizmos.color = new Color(0.2f, 0.4f, 0.78f, 0.5f);
				Gizmos.DrawLine(attractorPosition, attractorPosition + desc);
			}
		}
	}
}