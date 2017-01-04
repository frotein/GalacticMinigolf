using UnityEngine;
using System.Collections.Generic;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SpaceGravity2D {
	[AddComponentMenu("SpaceGravity2D/CelestialBody")]
	[ExecuteInEditMode]
	[SelectionBase]
	public sealed class CelestialBody : MonoBehaviour {
		public static event System.Action<CelestialBody> OnBodyCreatedEvent;
		public static event System.Action<CelestialBody> OnBodyDestroyedEvent;

		#region fields and properties

		public Transform transformRef;
		public SimulationControl simControlRef;
		[SerializeField]
		internal Vector3d _position;
		public Vector3d position {
			get {
				return _position;
			}
			set {
				_position = value;
				transformRef.position = (Vector3)value;
			}
		}
		public Vector3d focalPosition {
			get {
				if (attractor != null) {
					return orbitData.position;
				}
				return new Vector3d();
			}
			set {
				if (attractor != null) {
					position = attractor.position + value;
				}
			}
		}

		public Vector3d centralPosition {
			get {
				if (attractor != null) {
					return orbitData.position - orbitData.centerPoint;
				}
				return new Vector3d();
			}
		}

		[SerializeField]
		public double mass = 1f;
		public double MG {
			get {
				return mass * simControlRef.gravitationalConstant;
			}
		}
		public double maxAttractionRange = double.PositiveInfinity;
		public CelestialBody attractor;
		public Vector3d velocity;
		public Vector3d relativeVelocity {
			get {
				if (attractor) {
					return velocity - attractor.velocity;
				} else {
					return velocity;
				}
			}
			set {
				if (attractor) {
					velocity = attractor.velocity + value;
				} else {
					velocity = value;
				}
			}
		}
		public Vector3d relativePosition {
			get {
				if (attractor) {
					return _position - attractor._position;
				} else {
					return _position;
				}
			}
			set {
				if (attractor) {
					_position = value + attractor._position;
					transformRef.position = (Vector3)_position;
				} else {
					_position = value;
					transformRef.position = (Vector3)_position;
				}
			}
		}
		public bool isFixedPosition;
#if UNITY_EDITOR
		/// <summary>
		/// Draw orbit in editor view
		/// </summary>
		public bool isDrawOrbit=true;
#endif
		/// <summary>
		/// is currently kepler motion.
		/// Don't change this manually. modify useKeplerMotion instead
		/// </summary>
		public bool isKeplerMotion=true;

		/// <summary>
		/// motion type switch
		/// </summary>
		public bool useKeplerMotion;

		private Coroutine _attrSearch;

		private bool _isAttractorSearchActive = false;

		/// <summary>
		/// Dynamic search of most proper attractor
		/// </summary>
		public bool isAttractorSearchActive {
			get {
				return _isAttractorSearchActive;
			}
			set {
				if (_isAttractorSearchActive != value) {
					_isAttractorSearchActive = value;
#if UNITY_EDITOR
					if (Application.isPlaying) {
#endif
						if (_isAttractorSearchActive) {
							_attrSearch = StartCoroutine(ContiniousAttractorSearch());
						} else {
							if (_attrSearch != null) {
								StopCoroutine(_attrSearch);
							}
						}
#if UNITY_EDITOR
					}
#endif
				}
			}
		}

		public float searchAttractorInterval = 1.0f;


		private Queue<CelestialBody> _newAttractorsBuffer = new Queue<CelestialBody>(4);

		public Vector3d additionalVelocity;

		public OrbitData orbitData = new OrbitData();

		public Vector3d orbitFocusPoint {
			get {
				if (attractor != null) {
					return attractor.position;
				}
				return position;
			}
		}

		public Vector3d orbitCenterPoint {
			get {
				if (attractor != null) {
					return attractor.position + orbitData.centerPoint;
				}
				return position;
			}
		}

		public Vector3d orbitPeriapsisPoint {
			get {
				if (attractor != null) {
					return attractor.position + orbitData.periapsis;
				}
				return position;
			}
		}

		public Vector3d orbitApoapsisPoint {
			get {
				if (attractor != null) {
					return attractor.position + orbitData.apoapsis;
				}
				return position;
			}
		}

		public bool isValidOrbit {
			get {
				return attractor != null && orbitData.isValidOrbit;
			}
		}
		public Vector3d centerOfMass {
			get {
				if (attractor != null) {
					return CelestialBodyUtils.CalcCenterOfMass(_position, mass, attractor._position, attractor.mass);
				} else {
					return _position;
				}
			}
		}

		public double eccentricity {
			get {
				return orbitData.eccentricity;
			}
			set {
				orbitData.SetEccentricity(value);
				RefreshCurrentPositionAndVelocityFromOrbitData();
			}
		}

		public double trueAnomaly {
			get {
				return orbitData.trueAnomaly;
			}
			set {
				orbitData.SetTrueAnomaly(value);
				RefreshCurrentPositionAndVelocityFromOrbitData();
			}
		}

		public double eccentricAnomaly {
			get {
				return orbitData.eccentricAnomaly;
			}
			set {
				orbitData.SetEccentricAnomaly(value);
				RefreshCurrentPositionAndVelocityFromOrbitData();
			}
		}

		public double meanAnomaly {
			get {
				return orbitData.meanAnomaly;
			}
			set {
				orbitData.SetMeanAnomaly(value);
				RefreshCurrentPositionAndVelocityFromOrbitData();
			}
		}

		#endregion

		void Start() {

#if UNITY_EDITOR
			if (Application.isPlaying) {
#endif
				if (OnBodyCreatedEvent != null) {
					OnBodyCreatedEvent(this);
				}
				if (simControlRef == null) {
					if (SimulationControl.instance == null) {
						Debug.Log("SpaceGravity2D: Simulation Control not found");
						enabled = false;
						return;
					} 
					simControlRef = SimulationControl.instance;
				}
#if UNITY_EDITOR
			} else {
				if (simControlRef == null) {
					simControlRef = GameObject.FindObjectOfType<SimulationControl>();
				}
				if (simControlRef != null) {
					orbitData.gravConst = simControlRef.gravitationalConstant;
					orbitData.eclipticNormal = simControlRef.eclipticNormal;
					orbitData.eclipticUp = simControlRef.eclipticUp;
				}
			}
#endif
		}

		void OnEnable() {
			FindReferences();

#if UNITY_EDITOR
			if (!Application.isPlaying) {
				return;
			}
#endif
			if (_isAttractorSearchActive) {
				StartCoroutine(ContiniousAttractorSearch());
			}
		}

		void OnDestroy() {
			if (OnBodyDestroyedEvent != null) {
				OnBodyDestroyedEvent(this);
			}
		}

#if UNITY_EDITOR
		float _attractorUpdateTime = 0;
		void Update() {
			//>==== Make FindMostProperAttractor working in editormode
			if (!Application.isPlaying) {
				position = new Vector3d(transformRef.position);
				if (_isAttractorSearchActive) {
					if (Time.realtimeSinceStartup > _attractorUpdateTime) {
						_attractorUpdateTime = Time.realtimeSinceStartup + searchAttractorInterval;
						FindAndSetMostProperAttractor();
					}
				}
			}
			//<=====
		}
#endif

		IEnumerator ContiniousAttractorSearch() {
			yield return null;
			float timer = 0;
			while (isActiveAndEnabled && _isAttractorSearchActive) {
				timer += Time.deltaTime;
				if (timer >= searchAttractorInterval) {
					timer = 0;
					FindAndSetMostProperAttractor();
					yield return null;
				}
			}
		}

		/// <summary>
		/// find components and simControl references
		/// </summary>
		public void FindReferences() {
			if (transformRef == null || simControlRef == null) {
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					Undo.RecordObject(this, "FindReferences");
				}
#endif
				if (transformRef == null) {
					transformRef = transform;
				}
				if (simControlRef == null) {
					simControlRef = SimulationControl.instance ?? GameObject.FindObjectOfType<SimulationControl>();
				}
			}
		}

		/// <summary>
		/// When first time called this method makes orbit clockwise, next time - oposite
		/// Orbit plane will be unchanged.
		/// </summary>
		public void MakeOrbitCircle() {
			if (attractor) {
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					FindReferences();
					attractor.FindReferences();
					Undo.RecordObject(this, "Round orbit");
				}
#endif
				var v = CelestialBodyUtils.CalcCircleOrbitVelocity(
					attractor._position,
					_position,
					attractor.mass,
					mass,
					orbitData.orbitNormal,
					simControlRef.gravitationalConstant
				);
				if (relativeVelocity == v) {
					relativeVelocity = -v;
				} else {
					relativeVelocity = v;
				}
				orbitData.isDirty = true;
			}
#if UNITY_EDITOR
 else {
				Debug.Log("SpaceGravity2D: Can't round orbit. " + name + " has no attractor");
			}
#endif
		}

		/// <summary>
		/// Orbit plane will be unchanged.
		/// </summary>
		public void MakeOrbitCircle(bool clockwise) {
			if (attractor) {
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					FindReferences();
					attractor.FindReferences();
					Undo.RecordObject(this, "Round orbit");
				}
#endif
				var dotProduct = CelestialBodyUtils.DotProduct(orbitData.orbitNormal, simControlRef.eclipticNormal); //sign of this value determines orbit orientation
				if (Mathd.Abs(orbitData.orbitNormal.sqrMagnitude - 1d) > 0.5d) {
					orbitData.orbitNormal = simControlRef.eclipticNormal;
				}
				var v = CelestialBodyUtils.CalcCircleOrbitVelocity(
					attractor._position,
					_position,
					attractor.mass,
					mass,
					orbitData.orbitNormal * ( clockwise && dotProduct >= 0 || !clockwise && dotProduct < 0 ? 1 : -1 ),
					simControlRef.gravitationalConstant
				);
				if (relativeVelocity != v) {
					relativeVelocity = v;
					orbitData.isDirty = true;
				}
			}
		}

		public void SetAttractor(CelestialBody attr) {
			if (attr == null || ( attr != attractor && attr.mass > mass && attr != this )) {
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					UnityEditor.Undo.RecordObject(this, "attractor ref change");
				}
#endif
				attractor = attr;
				orbitData.isDirty = true;
			}
		}

		/// <summary>
		/// Set new attractor at the end of frame or instant
		/// </summary>
		public void SetAttractor(CelestialBody attr, bool checkIsInRange, bool instant = false) {
			if (( attr == null || ( attr != attractor && attr.mass > mass ) ) && attr != this) {
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					UnityEditor.Undo.RecordObject(this, "attractor ref change");
				}

				if (!Application.isPlaying || instant) {
#else
				if (instant) {
#endif
					attractor = attr;
					orbitData.isDirty = true;
					return;
				}

				if (_newAttractorsBuffer.Count == 0) {
					StartCoroutine(SetNearestAttractor(checkIsInRange));
				}
				_newAttractorsBuffer.Enqueue(attr);
			}
		}

		IEnumerator SetNearestAttractor(bool checkIsInRange) {
			yield return new WaitForEndOfFrame();
			if (_newAttractorsBuffer.Count == 0) {
				yield break;
			}
			if (_newAttractorsBuffer.Count == 1) {
				if (checkIsInRange) {
					var attr = _newAttractorsBuffer.Dequeue();
					if (attr == null || ( attr._position - _position ).magnitude < Mathd.Min(attr.maxAttractionRange, simControlRef.maxAttractionRange)) {
						attractor = attr;
						orbitData.isDirty = true;
					}
				} else {
					attractor = _newAttractorsBuffer.Dequeue();
					orbitData.isDirty = true;
				}
				_newAttractorsBuffer.Clear();
				yield break;
			}
			CelestialBody nearest = _newAttractorsBuffer.Dequeue();
			var sqrDistance = nearest != null ? ( nearest._position - _position ).sqrMagnitude : double.MaxValue;
			while (_newAttractorsBuffer.Count > 0) {
				var cb = _newAttractorsBuffer.Dequeue();
				if (cb == nearest) {
					continue;
				}
				if (cb != null && ( cb._position - _position ).sqrMagnitude < sqrDistance) {
					nearest = cb;
					sqrDistance = ( cb._position - _position ).sqrMagnitude;
				}
			}
			attractor = nearest;
			orbitData.isDirty = true;
		}

		public void TerminateKeplerMotion() {
			isKeplerMotion = false;
		}

		[ContextMenu("Find nearest attractor")]
		public void FindAndSetNearestAttractor() {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				FindReferences();
			}
#endif
			if (simControlRef) {
				simControlRef.SetNearestAttractorForBody(this);
			}
#if UNITY_EDITOR
 else {
				Debug.Log("SpaceGravity2D: Simulation Control not found");
			}
#endif
		}

		[ContextMenu("Find Most Proper Attractor")]
		public void FindAndSetMostProperAttractor() {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				FindReferences();
			}
#endif
			if (simControlRef) {
				simControlRef.SetMostProperAttractorForBody(this);
			}
#if UNITY_EDITOR
 else {
				Debug.Log("SpaceGravity2D: Simulation Control not found");
			}
#endif
		}

		public void FindAndSetBiggestAttractor() {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				FindReferences();
			}
#endif
			if (simControlRef) {
				simControlRef.SetBiggestAttractorForBody(this);
			}
#if UNITY_EDITOR
 else {
				Debug.Log("SpaceGravity2D: Simulation Control not found");
			}
#endif
		}

		[ContextMenu("Project (pos and v) onto ecliptic plane")]
		public void ProjectOntoEclipticPlane() {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				Undo.RecordObject(this, "ecliptic projection");
				Undo.RecordObject(transformRef, "ecliptic projection");
			}
#endif
			var projectedPos = _position - simControlRef.eclipticNormal * CelestialBodyUtils.DotProduct(_position, simControlRef.eclipticNormal);
			var projectedV = velocity - simControlRef.eclipticNormal * CelestialBodyUtils.DotProduct(velocity, simControlRef.eclipticNormal);
			_position = projectedPos;
			transformRef.position = (Vector3)projectedPos;
			velocity = projectedV;
			orbitData.isDirty = true;
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				EditorUtility.SetDirty(this);
			}
#endif
		}

		[ContextMenu("Recalculate orbit data")]
		public void CalculateNewOrbitData() {
			if (attractor) {
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					FindReferences();
					attractor.FindReferences();
				}
#endif
				orbitData.attractorMass = attractor.mass;
				orbitData.eclipticNormal = simControlRef.eclipticNormal;
				orbitData.eclipticUp = simControlRef.eclipticUp;
				orbitData.position = _position - attractor._position;
				orbitData.velocity = velocity - attractor.velocity;
				orbitData.CalculateNewOrbitData();
			}
		}

		public void RotateOrbitAroundFocus(Quaternion rotation) {
			if (attractor != null) {
				orbitData.position = _position - attractor._position;
				orbitData.velocity = velocity - attractor.velocity;
				orbitData.RotateOrbit(rotation);
				position = attractor.position + orbitData.position;
				velocity = attractor.velocity + orbitData.velocity;
				orbitData.isDirty = true;
			}
		}

		public Vector3[] GetOrbitPoints(int pointsCount = 50, bool localSpace = false, float maxDistance = 1000f) {
			if (!attractor) {
				if (velocity.sqrMagnitude < 1e-004) {
					return new Vector3[0];
				}
				var normal = (Vector3)velocity.normalized;
				if (localSpace) {
					return new Vector3[]{
						-normal * maxDistance,
						new Vector3(),
						normal * maxDistance
					};
				} else {
					return new Vector3[]{
						transformRef.position - normal * maxDistance, 
						transformRef.position, 
						transformRef.position + normal * maxDistance
					};
				}
			}
			return orbitData.GetOrbitPoints(pointsCount, localSpace ? new Vector3() : attractor.transformRef.position, maxDistance);
		}

		public Vector3d[] GetOrbitPointsDouble(int pointsCount = 50, bool localSpace = false, double maxDistance = 1000d) {
			if (!attractor) {
				if (velocity.sqrMagnitude < 1e-004) {
					return new Vector3d[0];
				}
				var normal = velocity.normalized;
				if (localSpace) {
					return new Vector3d[]{
						-normal * maxDistance,
						new Vector3d(),
						normal * maxDistance
					};
				} else {
					return new Vector3d[]{
					_position - normal * maxDistance,
					_position,
					_position + normal * maxDistance
				};
				}
			}
			return orbitData.GetOrbitPoints(pointsCount, localSpace ? new Vector3d() : attractor.position, maxDistance);
		}

		public void AddExternalVelocity(Vector3d deltaVelocity) {
			additionalVelocity += deltaVelocity;
			TerminateKeplerMotion();
			orbitData.isDirty = true;
		}

		public void AddExternalVelocity(Vector3 deltaVelocity) {
			AddExternalVelocity(new Vector3d(deltaVelocity));
		}

		public void AddExternalForce(Vector3d forceVector) {
			additionalVelocity += forceVector / mass;
			TerminateKeplerMotion();
			orbitData.isDirty = true;
		}

		public void AddExternalForce(Vector3 forceVector) {
			AddExternalForce(new Vector3d(forceVector));
		}


		public void SetPosition(Vector3d newPosition) {
			position = newPosition;
			orbitData.isDirty = true;
		}

		public void SetPosition(Vector3 newPosition) {
			SetPosition(new Vector3d(newPosition));
		}

		public Vector3d GetCentralPositionAtEccentricAnomaly(double eccentricAnomaly) {
			return orbitData.GetCentralPositionAtEccentricAnomaly(eccentricAnomaly);
		}

		public Vector3d GetCentralPositionAtTrueAnomaly(double trueAnomaly) {
			return orbitData.GetCentralPositionAtTrueAnomaly(trueAnomaly);
		}

		public Vector3d GetFocalPositionAtEccentricAnomaly(double eccentricAnomaly) {
			return orbitData.GetFocalPositionAtEccentricAnomaly(eccentricAnomaly);
		}

		public Vector3d GetFocalPositionAtTrueAnomaly(double trueAnomaly) {
			return orbitData.GetFocalPositionAtTrueAnomaly(trueAnomaly);
		}

		public Vector3d GetRelVelocityAtEccentricAnomaly(double eccentricAnomaly) {
			return orbitData.GetVelocityAtEccentricAnomaly(eccentricAnomaly);
		}

		public Vector3d GetRelVelocityAtTrueAnomaly(double trueAnomaly) {
			return orbitData.GetVelocityAtTrueAnomaly(trueAnomaly);
		}

		public void UpdateObjectOrbitDynamicParameters(double deltatime) {
			if (attractor == null) {
				return;
			}
			orbitData.UpdateOrbitAnomaliesByTime(deltatime);
			orbitData.SetPositionByCurrentAnomaly();
			orbitData.SetVelocityByCurrentAnomaly();
		}

		public void RefreshCurrentPositionAndVelocityFromOrbitData() {
			if (attractor == null) {
				return;
			}
			position = attractor._position + orbitData.position;
			velocity = attractor.velocity + orbitData.velocity;
		}

		public bool GetAscendingNode(out Vector3 asc) {
			if (attractor != null) {
				if (orbitData.GetAscendingNode(out asc)) {
					return true;
				}
			}
			asc = new Vector3();
			return false;
		}

		public bool GetDescendingNode(out Vector3 desc) {
			if (attractor != null) {
				if (orbitData.GetDescendingNode(out desc)) {
					return true;
				}
			}
			desc = new Vector3();
			return false;
		}

	}//celestial body class

}//namespace