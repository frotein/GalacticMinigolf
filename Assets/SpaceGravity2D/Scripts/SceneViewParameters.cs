#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

namespace SpaceGravity2D {
	public enum VelocityHandlerType {
		Circle,
		Dot,
		Sphere,
		None
	}
	[System.Serializable]
	public class SceneViewSettings {
		const float maxRangeMlt = 3f;
		[Header("Velocity Vectors Parameters:")]
		[Tooltip("Show editable velocity vectors of bodies")]
		public bool drawVelocityVectors = true;
		[Tooltip("Change velocity vectors display style")]
		public bool drawArrowsHead = false;
		[Tooltip("Select velocity handler")]
		public VelocityHandlerType velocityHandlerType = VelocityHandlerType.Circle;
		[Tooltip("Change selection on edit")]
		public bool selectBodyWhenDraggingVelocity = true;
		[Tooltip("Change global velocity instead of relative")]
		public bool editGlobalVelocity = false;
		[Tooltip("Prevent changing orientation of orbit plane, when dragging velocity vector")]
		public bool keepOrbitPlaneWhileChangeVelocity = false;
		[Tooltip("Scale of velocity lines")]
		[Range(0.01f, 10f * maxRangeMlt)]
		public float velocitiesArrowsScale = 1f;
		[Tooltip("Scale of velocity handles")]
		[Range(0.01f, 1f * maxRangeMlt)]
		public float handleScale = 0.1f;
		[Space(12)]
		[Header("Orbit display in editor parameters")]
		[Tooltip("Show orbits in editor view")]
		public bool drawOrbits = true;
		[Range(10, 100 * maxRangeMlt)]
		[Tooltip("Calculated points count of orbits")]
		public int orbitPointsCount = 50;
		[Tooltip("Maximal distance of orbit points of displayed orbits")]
		public double maxOrbitDistance = 1000d;
		[Tooltip("Show projection of current orbits on ecliptic plane")]
		public bool drawOrbitsEclipticProjection = true;
		[Tooltip("Show projection of bodies on ecliptic plane")]
		public bool drawBodiesEclipticProjection = true;
		[Range(0.01f, 10f * maxRangeMlt)]
		public float circlesScale = 1f;
		[Tooltip("Show selectable marks over bodies")]
		public bool drawCirclesOverBodies = false;
		[Tooltip("Show ecliptic orientation helper mark in world center")]
		public bool drawEclipticMark = false;
		[Range(0.01f, 10f * maxRangeMlt)]
		public float eclipticMarkScale = 1f;
		[Space(12)]
		public bool drawPeriapsisPoint = false;
		public bool drawPeriapsisLabel = false;
		public bool drawApoapsisPoint = false;
		public bool drawApoapsisLabel = false;
		[Space(12)]
		public bool drawAscendingNodePoint = false;
		public bool drawAscendingNodeLine = false;
		public bool drawAscendingNodeLabel = false;
		public bool drawDescendingNodePoint = false;
		public bool drawDescendingNodeLine = false;
		public bool drawDescendingNodeLabel = false;
		[Space(12)]
		[Tooltip("Show or hide all labels")]
		public bool drawLabels = true;
		[Tooltip("Show current orbits inclination mark relative to ecliptic plane")]
		public bool drawInclinationLabel = false;
		[Range(0.01f, 10f * maxRangeMlt)]
		public float labelsScale = 1f;
		[Tooltip("Show lines from bodies to their attractors")]
		public bool drawRadiusVector = false;
		[Tooltip("Show orbit normal vector pointing from the center of orbit")]
		public bool drawOrbitsNormal = false;
		[Tooltip("Show orbit semi-major and semi-minor axis vectors with origin in orbit center")]
		public bool drawSemiAxis = false;
		[Tooltip("Show mark of current center of mass position for each orbit")]
		public bool drawCenterOfMassPoint = false;
		[Range(0.01f, 10f * maxRangeMlt)]
		[Tooltip("Scale of displayed unit vectors of normals and axis")]
		public float normalAxisScale = 1f;
		[Space(12)]
		[Range(0.01f, 10f * maxRangeMlt)]
		public float sceneViewButtonsScale = 1f;
		[Tooltip("Screen position of scene view toggle buttons with origin in upper right corner")]
		public Vector2 sceneViewButtonsPosition;
	}
}
#endif
