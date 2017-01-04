using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SpaceGravity2D {
	[AddComponentMenu("SpaceGravity2D/SimulationControl")]
	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	public class SimulationControl : MonoBehaviour {

		public enum NBodyCalculationType {
			Euler = 0,
			Verlet,
			RungeKutta,
		}

		/// <summary>
		/// Main constant. The real value 6.67384 * 10E-11 may not be very useful for gaming purposes
		/// </summary>
		public double gravitationalConstant = 0.0001d;

		/// <summary>
		/// Property to set new value for all bodies
		/// </summary>
		public double GravitationalConstant {
			get {
				return gravitationalConstant;
			}
			set {
				gravitationalConstant = value;
				ApplyGravConstToAllBodies();
			}
		}

		/// <summary>
		/// For changing gravitational constant and all velocity vectors proportionally. 
		/// </summary>
		public double GravitationalConstantProportional {
			get {
				return gravitationalConstant;
			}
			set {
				if (gravitationalConstant != value) {
					var deltaRatio = Mathd.Abs(gravitationalConstant) < 1e-23d ? 1d : value / gravitationalConstant;
					gravitationalConstant = value;
					ApplyGravConstToAllBodies();
					ChangeAllVelocitiesByFactor(Mathd.Sqrt(Mathd.Abs(deltaRatio)));
				}
			}
		}
		/// <summary>
		/// Global constraint for gravitational attraction range
		/// </summary>
		public double maxAttractionRange = double.PositiveInfinity;
		/// <summary>
		/// Global constraint for gravitational attraction range. It is better to set this value equal minimal body size. Not recommended to set 0 value
		/// </summary>
		public double minAttractionRange = 0.1d;
		/// <summary>
		/// TimeScale of simulation process. May be dynamicaly changed, but very large values decreasing precision of calculations
		/// </summary>
		public double timeScale = 1d;
		/// <summary>
		/// Mass threshold for body to became attractor
		/// </summary>
		public double minAttractorMass = 100d;

#if UNITY_EDITOR
		public SceneViewSettings sceneElementsDisplayParameters = new SceneViewSettings();
#endif

		/// <summary>
		/// Buffer for references. Used only in playmode
		/// </summary>
		public List<CelestialBody> bodies = new List<CelestialBody>();
		/// <summary>
		/// Optimisation cache
		/// </summary>
		public List<CelestialBody> attractorsCache = new List<CelestialBody>();
		/// <summary>
		/// Used primarily to prevent multiple copies of singleton
		/// </summary>
		public static SimulationControl instance;
		/// <summary>
		/// Current n-body simulation type
		/// </summary>
		public NBodyCalculationType calculationType = NBodyCalculationType.Verlet;

		public bool affectedByGlobalTimescale;
		public bool keepBodiesOnEclipticPlane;

		[SerializeField]
		internal Vector3d _eclipticNormal = new Vector3d(0, 0, -1);
		public Vector3d eclipticNormal {
			get {
				return _eclipticNormal;
			}
			set {
				_eclipticNormal = value.normalized;
				if (_eclipticNormal.sqrMagnitude < 0.99d || _eclipticNormal.sqrMagnitude > 1.01d) {//check if value is zero or inf
					_eclipticNormal = new Vector3d(0, 0, -1);
				}
				ApplyEclipticNormalsToAllBodies();
			}
		}

		[SerializeField]
		Vector3d _eclipticUp = new Vector3d(0, 1, 0);
		public Vector3d eclipticUp {
			get {
				return _eclipticUp;
			}
			set {
				_eclipticUp = value.normalized;
				if (_eclipticUp.magnitude < 0.9d) {//check if value is zero
					_eclipticUp = new Vector3d(0, 1, 0);
				}
				//To make sure new Up vector value is orthogonal to eclipticNormal:
				var v= CelestialBodyUtils.CrossProduct(_eclipticNormal, _eclipticUp);
				_eclipticUp = CelestialBodyUtils.CrossProduct(_eclipticNormal, v).normalized;
				ApplyEclipticNormalsToAllBodies();
			}
		}

		void OnEnable() {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				return;
			}
#endif
			//Singleton:
			if (instance && instance != this) {
				Debug.Log("SpaceGravity2D: SimulationControl already exists");
				enabled = false;
				return;
			}
			instance = this;
			SubscribeForEvents();
		}
		void OnDisable() {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				return;
			}
#endif
			UnsubscribeFromEvents();
		}

		void Update() {
			ProjectAllBodiesOnEcliptic();
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				return;
			}
#endif
			if (affectedByGlobalTimescale) {
				SimulationStep(Time.unscaledDeltaTime * timeScale);
			} else {
				SimulationStep(Time.deltaTime * timeScale);
			}
		}

		public void ProjectAllBodiesOnEcliptic() {
#if UNITY_EDITOR
			if (keepBodiesOnEclipticPlane) {
				var __bodies = GameObject.FindObjectsOfType<CelestialBody>().ToList();
				for (int i = 0; i < __bodies.Count; i++) {
					__bodies[i].ProjectOntoEclipticPlane();
				}
			}
#else
			if (keepBodiesOnEclipticPlane) {
				for (int i = 0; i < bodies.Count; i++) {
					bodies[i].ProjectOntoEclipticPlane();
				}
			}
#endif
		}

		/// <summary>
		/// Simulate gravity on scene. 
		/// Newtoninan motion and keplerian motion type
		/// </summary>
		void SimulationStep(double deltaTime) {
			//>>====
			//cache attractors to temporary list which improves performance in situations, when scene contains a lot of non-attracting low mass celestial bodies.
			attractorsCache.Clear();
			for (int i = 0; i < bodies.Count; i++) {
				if (bodies[i].isActiveAndEnabled && bodies[i].mass > minAttractorMass) {
					attractorsCache.Add(bodies[i]);
				}
			}
			//<<====
			for (int i = 0; i < bodies.Count; i++) {
				if (!bodies[i].isActiveAndEnabled || bodies[i].isFixedPosition) {
					continue;
				}
				///>>===== Keplerian motion type:
				if (bodies[i].useKeplerMotion && bodies[i].isKeplerMotion) {
					if (bodies[i].attractor != null) {
						if (bodies[i].attractor.mass < bodies[i].mass) {
							bodies[i].attractor = null;
							bodies[i].isKeplerMotion = false;
						} else {
							if (bodies[i].orbitData.isDirty) {
								bodies[i].CalculateNewOrbitData();
								bodies[i].RefreshCurrentPositionAndVelocityFromOrbitData();
							} else {
								bodies[i].UpdateObjectOrbitDynamicParameters(deltaTime);
								bodies[i].RefreshCurrentPositionAndVelocityFromOrbitData();
							}
						}
					} else {
						bodies[i].position = bodies[i].position + bodies[i].velocity * deltaTime;
					}
				}
				///<<======
			}

			///>>===================== Newtonian motion type:
			for (int i = 0; i < bodies.Count; i++) {
				if (!bodies[i].isActiveAndEnabled || bodies[i].isFixedPosition || bodies[i].useKeplerMotion && bodies[i].isKeplerMotion) {
					continue;
				}

				if (double.IsInfinity(bodies[i].velocity.x) || double.IsNaN(bodies[i].velocity.x)) {
					Debug.Log("SpaceGravity2D: Velocity is " + ( double.IsNaN(bodies[i].velocity.x) ? "NaN !" : "INF !" ) + "\nbody: " + name);
					bodies[i].velocity = new Vector3d();
				}
				switch (calculationType) {
					case NBodyCalculationType.Euler:
						CalcAccelerationEulerForBody(bodies[i], deltaTime);
						if (!bodies[i].additionalVelocity.isZero) {
							bodies[i].velocity += bodies[i].additionalVelocity;
							bodies[i].additionalVelocity = Vector3d.zero;
						}
						if (double.IsInfinity(bodies[i].velocity.x) || double.IsNaN(bodies[i].velocity.x)) {
							bodies[i].velocity = new Vector3d();
						}
						bodies[i].position = bodies[i].position + bodies[i].velocity * deltaTime;
						break;
					case NBodyCalculationType.Verlet:
						bodies[i].position += bodies[i].velocity * ( deltaTime / 2d );
						CalcAccelerationEulerForBody(bodies[i], deltaTime);
						if (!bodies[i].additionalVelocity.isZero) {
							bodies[i].velocity += bodies[i].additionalVelocity;
							bodies[i].additionalVelocity = Vector3d.zero;
						}
						if (double.IsInfinity(bodies[i].velocity.x) || double.IsNaN(bodies[i].velocity.x)) {
							bodies[i].velocity = new Vector3d();
						}
						bodies[i].position += bodies[i].velocity * ( deltaTime / 2d );
						break;
					case NBodyCalculationType.RungeKutta:
						CalcAccelerationRungeKuttaForBody(bodies[i], deltaTime);
						if (!bodies[i].additionalVelocity.isZero) {
							bodies[i].velocity += bodies[i].additionalVelocity;
							bodies[i].additionalVelocity = Vector3d.zero;
						}
						if (double.IsInfinity(bodies[i].velocity.x) || double.IsNaN(bodies[i].velocity.x)) {
							bodies[i].velocity = new Vector3d();
						}
						bodies[i].position += bodies[i].velocity * deltaTime * 0.5d;
						break;
				}
				bodies[i].orbitData.isDirty = true;
				if (bodies[i].useKeplerMotion) {
					bodies[i].isKeplerMotion = true; //transit to keplerian motion at next frame
				}
			}
			///<<=====================
			for (int i = 0; i < bodies.Count; i++) {
				if (bodies[i].orbitData.isDirty) {
					bodies[i].orbitData.isDirty = false;
					bodies[i].CalculateNewOrbitData();
				}
			}
		}

		public void CalcAccelerationEulerForBody(CelestialBody body, double dt) {
			Vector3d result = Vector3d.zero;
			for (int i = 0; i < attractorsCache.Count; i++) {
				if (attractorsCache[i] == body) {
					continue;
				}
				result += CelestialBodyUtils.AccelerationByAttractionForce(
					body.position,
					attractorsCache[i].position,
					attractorsCache[i].MG,
					minAttractionRange,
					Mathd.Min(maxAttractionRange, attractorsCache[i].maxAttractionRange)
				);
			}
			body.velocity += result * dt;
		}

		public Vector3d CalcAccelerationEulerInPoint(Vector3d pos) {
			Vector3d result = new Vector3d();
			for (int i = 0; i < attractorsCache.Count; i++) {
				if (attractorsCache[i].position == pos) {
					continue;
				}
				result += CelestialBodyUtils.AccelerationByAttractionForce(
					pos,
					attractorsCache[i].position,
					attractorsCache[i].MG,
					minAttractionRange,
					Mathd.Min(maxAttractionRange, attractorsCache[i].maxAttractionRange)
				);
			}
			return result;
		}

		public void CalcAccelerationRungeKuttaForBody(CelestialBody body, double dt) {
			Vector3d result = Vector3d.zero;

			body._position += body.velocity * ( dt / 2d );
			for (int i = 0; i < attractorsCache.Count; i++) {
				if (attractorsCache[i] == body) {
					continue;
				}
				var t1 = CelestialBodyUtils.AccelerationByAttractionForce(
					body._position,
					attractorsCache[i].position,
					attractorsCache[i].MG,
					minAttractionRange,
					Mathd.Min(maxAttractionRange, attractorsCache[i].maxAttractionRange)
				) * dt;
				var t2 = CelestialBodyUtils.AccelerationByAttractionForce(
					body._position + t1 * 0.5d,
					attractorsCache[i].position,
					attractorsCache[i].MG,
					minAttractionRange,
					Mathd.Min(maxAttractionRange, attractorsCache[i].maxAttractionRange)
				) * dt;
				var t3 = CelestialBodyUtils.AccelerationByAttractionForce(
					body._position + t2 * 0.5d,
					attractorsCache[i].position,
					attractorsCache[i].MG,
					minAttractionRange,
					Mathd.Min(maxAttractionRange, attractorsCache[i].maxAttractionRange)
				) * dt;
				var t4 = CelestialBodyUtils.AccelerationByAttractionForce(
					body._position + t3,
					attractorsCache[i].position,
					attractorsCache[i].MG,
					minAttractionRange,
					Mathd.Min(maxAttractionRange, attractorsCache[i].maxAttractionRange)
				) * dt;
				result += new Vector3d(
					( t1.x + t2.x * 2d + t3.x * 2d + t4.x ) / 6d,
					( t1.y + t2.y * 2d + t3.y * 2d + t4.y ) / 6d,
					( t1.z + t2.z * 2d + t3.z * 2d + t4.z ) / 6d);
			}
			body.velocity += result;
		}

		public CelestialBody FindMostProperAttractor(CelestialBody body) {
			if (body == null) {
				return null;
			}
			CelestialBody resultAttractor = null;
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				bodies = new List<CelestialBody>(GameObject.FindObjectsOfType<CelestialBody>());
			}
#endif
			// Search logic:
			// calculate mutual perturbation for every pair of attractors in scene and select one, 
			// which attracts the body with biggest force and is least affected by others.
			foreach (var otherBody in bodies) {
				if (otherBody == body || !otherBody.isActiveAndEnabled || otherBody.mass < minAttractorMass || ( otherBody.position - body.position ).magnitude > Mathd.Min(maxAttractionRange, otherBody.maxAttractionRange)) {
					continue;
				}
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					otherBody.FindReferences();
				}
#endif
				if (resultAttractor == null) {
					resultAttractor = otherBody;
				} else
					if (CelestialBodyUtils.RelativePerturbationRatio(body, resultAttractor, otherBody) > CelestialBodyUtils.RelativePerturbationRatio(body, otherBody, resultAttractor)) {
						resultAttractor = otherBody;
					}
			}
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				bodies.Clear(); //bodies must be empty in editor mode
			}
#endif
			return resultAttractor;
		}

		public CelestialBody FindBiggestAttractor() {
			CelestialBody[] tempBodies;
#if UNITY_EDITOR
			if (Application.isPlaying) {
				tempBodies = GameObject.FindObjectsOfType<CelestialBody>();
			} else {
#endif
				tempBodies = bodies.ToArray();
#if UNITY_EDITOR
			}
#endif
			if (tempBodies.Length == 0) {
				return null;
			}
			if (tempBodies.Length == 1) {
				return tempBodies[0];
			}
			var biggestMassIndex = 0;
			for (int i = 1; i < tempBodies.Length; i++) {
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					tempBodies[i - 1].FindReferences();
					tempBodies[i].FindReferences();
				}
#endif
				if (tempBodies[i].mass > tempBodies[biggestMassIndex].mass) {
					biggestMassIndex = i;
				}
			}
			if (biggestMassIndex >= 0 && biggestMassIndex < tempBodies.Length) {
				return tempBodies[biggestMassIndex];
			}
			return null;
		}

		public CelestialBody FindNearestAttractor(CelestialBody body) {
			if (body == null) {
				return null;
			}
			CelestialBody resultAttractor = null;
			double _minSqrDistance = 0;
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				bodies = new List<CelestialBody>(GameObject.FindObjectsOfType<CelestialBody>());
			}
#endif
			foreach (var otherBody in bodies) {
				if (otherBody == body || otherBody.mass < minAttractorMass || ( otherBody.position - body.position ).magnitude > Mathd.Min(maxAttractionRange, otherBody.maxAttractionRange)) {
					continue;
				}
				double _sqrDistance = ( body.position - otherBody.position ).sqrMagnitude;
				if (resultAttractor == null || _minSqrDistance > _sqrDistance) {
					resultAttractor = otherBody;
					_minSqrDistance = _sqrDistance;
				}
			}
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				bodies.Clear(); //_bodies must be empty in editor mode
			}
#endif
			return resultAttractor;
		}

		/// <summary>
		/// Fast and simple way to find attractor; 
		/// But note, that not always nearest attractor is most proper
		/// </summary>
		public void SetNearestAttractorForBody(CelestialBody body) {
			body.SetAttractor(FindNearestAttractor(body), false, true);
		}

		/// <summary>
		/// Find attractor which has biggest gravitational influence on body comparing to others. If fail, null will be assigned.
		/// It can be used in realtime for implementing more precise transitions beetween spheres of influence, 
		/// but performance cost is high
		/// </summary>
		public void SetMostProperAttractorForBody(CelestialBody body) {

			body.SetAttractor(FindMostProperAttractor(body), false, true);
		}

		public void SetBiggestAttractorForBody(CelestialBody body) {
			body.SetAttractor(FindBiggestAttractor(), false, true);
		}

		/// <summary>
		/// Used for changing gravitational parameter without breaking orbits.
		/// </summary>
		public void ChangeAllVelocitiesByFactor(double multiplier) {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				var __bodies = GameObject.FindObjectsOfType<CelestialBody>();
				for (int i = 0; i < __bodies.Length; i++) {
					UnityEditor.Undo.RecordObject(__bodies[i], "Proportional velocity change");
					__bodies[i].velocity *= multiplier;
					__bodies[i].orbitData.isDirty = true;
				}
				return;
			} else {
				for (int i = 0; i < bodies.Count; i++) {
					bodies[i].velocity *= multiplier;
					bodies[i].orbitData.isDirty = true;
				}
			}
#else
			for (int i = 0; i < bodies.Count; i++) {
				bodies[i].velocity *= multiplier;
				bodies[i].orbitData.isDirty = true;
			}
#endif
		}

		/// <summary>
		/// Refresh gravitational constant of orbitData of all celestial bodies
		/// </summary>
		public void ApplyGravConstToAllBodies() {
#if UNITY_EDITOR
			if (Application.isPlaying) {
				for (int i = 0; i < bodies.Count; i++) {
					bodies[i].orbitData.gravConst = gravitationalConstant;
					bodies[i].orbitData.isDirty = true;
				}
			} else {
				var __bodies = GameObject.FindObjectsOfType<CelestialBody>();
				for (int i = 0; i < __bodies.Length; i++) {
					__bodies[i].orbitData.gravConst = gravitationalConstant;
					__bodies[i].orbitData.isDirty = true;
				}
			}
#else
			for (int i = 0; i < bodies.Count; i++) {
				bodies[i].orbitData.gravConst = gravitationalConstant;
				bodies[i].orbitData.isDirty = true;
			}
#endif
		}

		/// <summary>
		/// Set ecliptic value to orbitData of all celestial bodies
		/// </summary>
		public void ApplyEclipticNormalsToAllBodies() {
#if UNITY_EDITOR
			if (Application.isPlaying) {
				for (int i = 0; i < bodies.Count; i++) {
					bodies[i].orbitData.eclipticNormal = eclipticNormal;
					bodies[i].orbitData.eclipticUp = eclipticUp;
					bodies[i].orbitData.isDirty = true;
				}
			} else {
				var __bodies = GameObject.FindObjectsOfType<CelestialBody>();
				for (int i = 0; i < __bodies.Length; i++) {
					__bodies[i].orbitData.eclipticNormal = eclipticNormal;
					__bodies[i].orbitData.eclipticUp = eclipticUp;
					__bodies[i].orbitData.isDirty = true;
				}
			}
#else
			for (int i = 0; i < bodies.Count; i++) {
				bodies[i].orbitData.eclipticNormal = eclipticNormal;
				bodies[i].orbitData.eclipticUp = eclipticUp;
				bodies[i].orbitData.isDirty = true;
			}
#endif
		}

		#region events


		void SubscribeForEvents() {
			CelestialBody.OnBodyCreatedEvent += RegisterBody;
			CelestialBody.OnBodyDestroyedEvent += UnregisterBody;
		}

		void UnsubscribeFromEvents() {
			CelestialBody.OnBodyCreatedEvent -= RegisterBody;
			CelestialBody.OnBodyDestroyedEvent -= UnregisterBody;
		}

		void RegisterBody(CelestialBody body) {
			if (instance == this) {
				bodies.Add(body);
				body.orbitData.gravConst = gravitationalConstant;
				body.orbitData.eclipticNormal = eclipticNormal;
				body.orbitData.eclipticUp = eclipticUp;
			}
		}
		void UnregisterBody(CelestialBody body) {
			if (instance == this) {
				bodies.Remove(body);
			}
		}
		#endregion
	}
}