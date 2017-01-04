using UnityEngine;
using System.Collections;


namespace SpaceGravity2D {
	/// <summary>
	/// Basic static Sphere of Influence component script, alternative to Dynamic Attractor Changing. If attached to gameobject whith no colliders, new collider will be created.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu( "SpaceGravity2D/SphereOfInfluence" )]
	public class SphereOfInfluence : MonoBehaviour {

		public SphereCollider detector;
		public CelestialBody body;
		[Header( "Range of ingluence:" )]
		public float triggerRadius;
		[Header( "Calculate radius value based on orbit data:" )]
		/// <summary>
		/// if true and attractor is not null, range of influence will be calculated automaticaly. Useful for making first approach
		/// </summary>
		public bool useAutoROI = false;
		[Space( 15 )]
		/// <summary>
		/// Dynamic attractor changing of celestial body is full alternative to this component.
		/// </summary>
		public bool ignoreBodiesWithDynamicAttrChanging = true;
		public bool ignoreTransformsScale = true;
		public bool ignoreOtherSpheresOfInfluences = true;
		public bool drawGizmo;

		void Awake() {
			GetTriggerCollider();
			body = GetComponentInParent<CelestialBody>();
			if ( !detector || !body ) {
				enabled = false;
			}
			triggerRadius = Mathf.Abs( triggerRadius );
		}

		void GetTriggerCollider() {
			var colliders = GetComponentsInChildren<SphereCollider>();
			for ( int i = 0; i < colliders.Length; i++ ) {
				if ( colliders[i].isTrigger ) {
					detector = colliders[i];
				}
			}
			if ( !detector ) {
				detector = gameObject.AddComponent<SphereCollider>();
				Debug.Log("SpaceGravity2D: Sphere Of Influence autocreate trigger for " + name);
			}
		}

#if UNITY_EDITOR
		void Update() {
			if ( useAutoROI ) {
				if ( body && body.attractor && !double.IsNaN( body.orbitData.semiMajorAxis ) ) {
					body.attractor.FindReferences();
					body.FindReferences();
					triggerRadius = (float)body.orbitData.semiMajorAxis * Mathf.Pow((float)(body.mass / body.attractor.mass), 2f / 5f);
				}
			}
			float parentScale = 1f;
			float scale = 1f;
			if ( ignoreTransformsScale ) {
				parentScale = transform.parent == null ? 1 : ( transform.parent.localScale.x + transform.parent.localScale.y ) / 2f;
				scale = ( transform.localScale.x + transform.localScale.y ) / 2f;
			}
			detector.radius = triggerRadius / scale / parentScale;
		}
#endif


		void OnTriggerEnter(Collider col) {
			if ( col.transform != transform.parent ) {
				if ( ignoreOtherSpheresOfInfluences && col.GetComponentInChildren<SphereOfInfluence>() != null ) {
					return;
				}
				var cBody = col.GetComponentInParent<CelestialBody>();
				if ( cBody && cBody.attractor != body && cBody.mass < body.mass && ( !ignoreBodiesWithDynamicAttrChanging || !cBody.isAttractorSearchActive ) ) {
					if ( cBody.attractor != null ) {
						//Check if body is already attracted by child of current _body.
						if ( cBody.attractor.attractor == body ) {
							return; 
						}
					}
					cBody.SetAttractor( body );
				}
			}
		}

		void OnTriggerExit( Collider col ) {
			if ( col.transform != transform.parent ) {
				var colBody = col.GetComponentInParent<CelestialBody>();
				if ( colBody && colBody.attractor == body && ( !ignoreBodiesWithDynamicAttrChanging || !colBody.isAttractorSearchActive ) ) {
					colBody.SetAttractor( body.attractor, checkIsInRange: true );
				}
			}
		}

		void OnDrawGizmos() {
			if ( enabled && drawGizmo ) {
				Gizmos.color = new Color( 1f, 1f, 1f, 0.2f );
				Gizmos.DrawSphere( transform.position, triggerRadius );
			}
		}

	}
}