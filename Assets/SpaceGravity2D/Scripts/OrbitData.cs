using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SpaceGravity2D {
	[System.Serializable]
	public class OrbitData {

		public double epsilon = 1e-027;

		public double gravConst = 1;
		public Vector3d eclipticNormal = new Vector3d(0, 0, 1);
		public Vector3d eclipticUp = new Vector3d(0, 1, 0);//up direction on ecliptic plane

		public Vector3d position;
		public double attractorDistance;
		public double attractorMass;
		public Vector3d velocity;

		public double semiMinorAxis;
		public double semiMajorAxis;
		public double focalParameter;
		public double eccentricity;
		public double energyTotal;
		public double period;
		public double trueAnomaly;
		public double meanAnomaly;
		public double eccentricAnomaly;
		public double squaresConstant;
		public Vector3d periapsis;
		public double periapsisDistance;
		public Vector3d apoapsis;
		public double apoapsisDistance;
		public Vector3d centerPoint;
		public double orbitCompressionRatio;
		public Vector3d orbitNormal;
		public Vector3d semiMinorAxisBasis;
		public Vector3d semiMajorAxisBasis;
		public double inclination;
		/// <summary>
		/// if > 0, then orbit motion is clockwise
		/// </summary>
		public double orbitNormalDotEclipticNormal;

		public bool isValidOrbit {
			get {
				return eccentricity >= 0 && period > epsilon && attractorDistance > epsilon && attractorMass > epsilon;
			}
		}
		public bool isDirty = false;

		public void CalculateNewOrbitData() {
			isDirty = false;
			var MG = attractorMass * gravConst;
			attractorDistance = position.magnitude;
			var angularMomentumVector =  CelestialBodyUtils.CrossProduct(position, velocity);
			orbitNormal = angularMomentumVector.normalized;
			Vector3d eccVector;
			if (orbitNormal.sqrMagnitude < 0.9 || orbitNormal.sqrMagnitude > 1.1) {//check if zero lenght
				orbitNormal = CelestialBodyUtils.CrossProduct(position, eclipticUp).normalized;
				eccVector = new Vector3d();
			} else {
				eccVector = CelestialBodyUtils.CrossProduct(velocity, angularMomentumVector) / MG - position / attractorDistance;
			}
			orbitNormalDotEclipticNormal = CelestialBodyUtils.DotProduct(orbitNormal, eclipticNormal);
			focalParameter = angularMomentumVector.sqrMagnitude / MG;
			eccentricity = eccVector.magnitude;
			//if (debug) {
			//	string format = "0.0000000000";
			//	Debug.Log(
			//		"ECC: " + eccVector.ToString(format) + " LEN: " + eccVector.magnitude.ToString(format) + "\n" +
			//		"POS: " + position.ToString(format) + " LEN: " + position.magnitude.ToString(format) + "\n" +
			//		"POSNORM: " + ( position / attractorDistance ).ToString(format) + " LEN: " + ( position / attractorDistance ).magnitude.ToString(format) + "\n" +
			//		"VEL: " + velocity.ToString(format) + " LEN: " + velocity.magnitude.ToString(format) + "\n" +
			//		"POScrossVEL: " + angularMomentumVector.ToString(format) + " LEN: " + angularMomentumVector.magnitude.ToString(format) + "\n"
			//		);
			//}
			energyTotal = velocity.sqrMagnitude - 2 * MG / attractorDistance;
			semiMinorAxisBasis = CelestialBodyUtils.CrossProduct(angularMomentumVector, eccVector).normalized;
			if (semiMinorAxisBasis.sqrMagnitude < 0.5) {
				semiMinorAxisBasis = CelestialBodyUtils.CrossProduct(orbitNormal, position).normalized;
			}
			semiMajorAxisBasis = CelestialBodyUtils.CrossProduct(orbitNormal, semiMinorAxisBasis).normalized;
			inclination = Vector3d.Angle(orbitNormal, eclipticNormal) * Mathd.Deg2Rad;
			if (eccentricity < 1) {
				orbitCompressionRatio = 1 - eccentricity * eccentricity;
				semiMajorAxis = focalParameter / orbitCompressionRatio;
				semiMinorAxis = semiMajorAxis * System.Math.Sqrt(orbitCompressionRatio);
				centerPoint = -semiMajorAxis * eccVector;
				period = Mathd.PI_2 * Mathd.Sqrt(Mathd.Pow(semiMajorAxis, 3) / MG);
				apoapsis = centerPoint + semiMajorAxisBasis * semiMajorAxis;
				periapsis = centerPoint - semiMajorAxisBasis * semiMajorAxis;
				periapsisDistance = periapsis.magnitude;
				apoapsisDistance = apoapsis.magnitude;
				trueAnomaly = Vector3d.Angle(position, -semiMajorAxisBasis) * Mathd.Deg2Rad;
				if (CelestialBodyUtils.DotProduct(CelestialBodyUtils.CrossProduct(position, semiMajorAxisBasis), orbitNormal) < 0) {
					trueAnomaly = Mathd.PI_2 - trueAnomaly;
				}
				eccentricAnomaly = CelestialBodyUtils.ConvertTrueToEccentricAnomaly(trueAnomaly, eccentricity);
				meanAnomaly = eccentricAnomaly - eccentricity * System.Math.Sin(eccentricAnomaly);
			} else {
				orbitCompressionRatio = eccentricity * eccentricity - 1;
				semiMajorAxis = focalParameter / orbitCompressionRatio;
				semiMinorAxis = semiMajorAxis * System.Math.Sqrt(orbitCompressionRatio);
				centerPoint = semiMajorAxis * eccVector;
				period = double.PositiveInfinity;
				apoapsis = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
				periapsis = centerPoint + semiMajorAxisBasis * ( semiMajorAxis );
				periapsisDistance = periapsis.magnitude;
				apoapsisDistance = double.PositiveInfinity;
				trueAnomaly = Vector3d.Angle(position, eccVector) * Mathd.Deg2Rad;
				if (CelestialBodyUtils.DotProduct(CelestialBodyUtils.CrossProduct(position, semiMajorAxisBasis), orbitNormal) < 0) {
					trueAnomaly = -trueAnomaly;
				}
				eccentricAnomaly = CelestialBodyUtils.ConvertTrueToEccentricAnomaly(trueAnomaly, eccentricity);
				meanAnomaly = System.Math.Sinh(eccentricAnomaly) * eccentricity - eccentricAnomaly;
			}
		}

		public Vector3d GetVelocityAtEccentricAnomaly(double eccentricAnomaly) {
			return GetVelocityAtTrueAnomaly(CelestialBodyUtils.ConvertEccentricToTrueAnomaly(eccentricAnomaly, eccentricity));
		}

		public Vector3d GetVelocityAtTrueAnomaly(double trueAnomaly) {
			if (focalParameter < 1e-5) {
				return new Vector3d();
			}
			var sqrtMGdivP = System.Math.Sqrt(attractorMass * gravConst / focalParameter);
			double vX = sqrtMGdivP * ( eccentricity + System.Math.Cos(trueAnomaly) );
			double vY = sqrtMGdivP * System.Math.Sin(trueAnomaly);
			return semiMinorAxisBasis * vX + semiMajorAxisBasis * vY;
		}

		public Vector3d GetCentralPositionAtTrueAnomaly(double trueAnomaly) {
			var ecc = CelestialBodyUtils.ConvertTrueToEccentricAnomaly(trueAnomaly, eccentricity);
			return GetCentralPositionAtEccentricAnomaly(ecc);
		}

		public Vector3d GetCentralPositionAtEccentricAnomaly(double eccentricAnomaly) {
			Vector3d result = eccentricity < 1 ?
				new Vector3d(System.Math.Sin(eccentricAnomaly) * semiMinorAxis, -System.Math.Cos(eccentricAnomaly) * semiMajorAxis) :
				new Vector3d(System.Math.Sinh(eccentricAnomaly) * semiMinorAxis, System.Math.Cosh(eccentricAnomaly) * semiMajorAxis);
			return semiMinorAxisBasis * result.x + semiMajorAxisBasis * result.y;
		}

		public Vector3d GetFocalPositionAtEccentricAnomaly(double eccentricAnomaly) {
			return GetCentralPositionAtEccentricAnomaly(eccentricAnomaly) + centerPoint;
		}

		public Vector3d GetFocalPositionAtTrueAnomaly(double trueAnomaly) {
			return GetCentralPositionAtTrueAnomaly(trueAnomaly) + centerPoint;
		}

		public Vector3d GetCentralPosition() {
			return position - centerPoint;
		}

		public Vector3d[] GetOrbitPoints(int pointsCount = 50, double maxDistance = 1000d) {
			return GetOrbitPoints(pointsCount, new Vector3d(), maxDistance);
		}

		public Vector3d[] GetOrbitPoints(int pointsCount, Vector3d origin, double maxDistance = 1000d) {
			if (pointsCount < 2) {
				return new Vector3d[0];
			}
			var result = new Vector3d[pointsCount];
			if (eccentricity < 1) {
				if (apoapsisDistance < maxDistance) {
					for (var i = 0; i < pointsCount; i++) {
						result[i] = GetFocalPositionAtEccentricAnomaly(i * Mathd.PI_2 / ( pointsCount - 1d )) + origin;
					}
				} else {
					var maxAngle =CelestialBodyUtils.CalcTrueAnomalyForDistance(maxDistance, eccentricity, semiMajorAxis);
					for (int i = 0; i < pointsCount; i++) {
						result[i] = GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / ( pointsCount - 1 )) + origin;
					}
				}
			} else {
				if (maxDistance < periapsisDistance) {
					return new Vector3d[0];
				}
				var maxAngle = CelestialBodyUtils.CalcTrueAnomalyForDistance(maxDistance, eccentricity, semiMajorAxis);

				for (int i = 0; i < pointsCount; i++) {
					result[i] = GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / ( pointsCount - 1 )) + origin;
				}
			}
			return result;
		}

		public Vector3[] GetOrbitPoints(int pointsCount = 50, float maxDistance = 1000f) {
			return GetOrbitPoints(pointsCount, new Vector3(), maxDistance);
		}

		public Vector3[] GetOrbitPoints(int pointsCount, Vector3 origin, float maxDistance = 1000f) {
			if (pointsCount < 2) {
				return new Vector3[0];
			}
			var result = new Vector3[pointsCount];
			if (eccentricity < 1) {
				if (apoapsisDistance < maxDistance) {
					for (var i = 0; i < pointsCount; i++) {
						result[i] = (Vector3)GetFocalPositionAtEccentricAnomaly(i * Mathd.PI_2 / ( pointsCount - 1d )) + origin;
					}
				} else {
					var maxAngle =CelestialBodyUtils.CalcTrueAnomalyForDistance(maxDistance, eccentricity, semiMajorAxis);
					for (int i = 0; i < pointsCount; i++) {
						result[i] = (Vector3)GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / ( pointsCount - 1 )) + origin;
					}
				}
			} else {
				if (maxDistance < periapsisDistance) {
					return new Vector3[0];
				}
				var maxAngle = CelestialBodyUtils.CalcTrueAnomalyForDistance(maxDistance, eccentricity, semiMajorAxis);

				for (int i = 0; i < pointsCount; i++) {
					result[i] = (Vector3)GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / ( pointsCount - 1 )) + origin;
				}
			}
			return result;
		}

		public bool GetAscendingNode(out Vector3 asc) {
			Vector3d v;
			if (GetAscendingNode(out v)) {
				asc = (Vector3)v;
				return true;
			}
			asc = new Vector3();
			return false;
		}

		public bool GetAscendingNode(out Vector3d asc) {
			var norm = CelestialBodyUtils.CrossProduct(orbitNormal, eclipticNormal);
			var s = CelestialBodyUtils.DotProduct(CelestialBodyUtils.CrossProduct(norm, semiMajorAxisBasis), orbitNormal) < 0;
			var ecc = 0d;
			var trueAnom = Vector3d.Angle(norm, centerPoint) * Mathd.Deg2Rad;
			if (eccentricity < 1) {
				var cosT = System.Math.Cos(trueAnom);
				ecc = System.Math.Acos(( eccentricity + cosT ) / ( 1d + eccentricity * cosT ));
				if (!s) {
					ecc = Mathd.PI_2 - ecc;
				}
			} else {
				trueAnom = Vector3d.Angle(-norm, centerPoint) * Mathd.Deg2Rad;
				if (trueAnom >= Mathd.Acos(-1d / eccentricity)) {
					asc = new Vector3d();
					return false;
				}
				var cosT= System.Math.Cos(trueAnom);
				ecc = CelestialBodyUtils.Acosh(( eccentricity + cosT ) / ( 1 + eccentricity * cosT )) * ( !s ? -1 : 1 );
			}
			asc = GetFocalPositionAtEccentricAnomaly(ecc);
			return true;
		}

		public bool GetDescendingNode(out Vector3 desc) {
			Vector3d v;
			if (GetDescendingNode(out v)) {
				desc = (Vector3)v;
				return true;
			}
			desc = new Vector3();
			return false;
		}

		public bool GetDescendingNode(out Vector3d desc) {
			var norm = CelestialBodyUtils.CrossProduct(orbitNormal, eclipticNormal);
			var s = CelestialBodyUtils.DotProduct(CelestialBodyUtils.CrossProduct(norm, semiMajorAxisBasis), orbitNormal) < 0;
			var ecc = 0d;
			var trueAnom = Vector3d.Angle(norm, -centerPoint) * Mathd.Deg2Rad;
			if (eccentricity < 1) {
				var cosT = System.Math.Cos(trueAnom);
				ecc = System.Math.Acos(( eccentricity + cosT ) / ( 1d + eccentricity * cosT ));
				if (s) {
					ecc = Mathd.PI_2 - ecc;
				}
			} else {
				trueAnom = Vector3d.Angle(norm, centerPoint) * Mathd.Deg2Rad;
				if (trueAnom >= Mathd.Acos(-1d / eccentricity)) {
					desc = new Vector3d();
					return false;
				}
				var cosT= System.Math.Cos(trueAnom);
				ecc = CelestialBodyUtils.Acosh(( eccentricity + cosT ) / ( 1 + eccentricity * cosT )) * ( s ? -1 : 1 );
			}
			desc = GetFocalPositionAtEccentricAnomaly(ecc);
			return true;
		}

		public void UpdateOrbitDataByTime(double deltaTime) {
			UpdateOrbitAnomaliesByTime(deltaTime);
			SetPositionByCurrentAnomaly();
			SetVelocityByCurrentAnomaly();
		}

		public void UpdateOrbitAnomaliesByTime(double deltaTime) {
			if (eccentricity < 1) {
				if (period > 1e-5) {
					meanAnomaly += Mathd.PI_2 * deltaTime / period;
				}
				meanAnomaly %= Mathd.PI_2;
				if (meanAnomaly < 0) {
					meanAnomaly = Mathd.PI_2 - meanAnomaly;
				}
				eccentricAnomaly = CelestialBodyUtils.KeplerSolver(meanAnomaly, eccentricity);
				var cosE = System.Math.Cos(eccentricAnomaly);
				trueAnomaly = System.Math.Acos(( cosE - eccentricity ) / ( 1 - eccentricity * cosE ));
				if (meanAnomaly > Mathd.PI) {
					trueAnomaly = Mathd.PI_2 - trueAnomaly;
				}
				if (double.IsNaN(meanAnomaly) || double.IsInfinity(meanAnomaly)) {
					Debug.Log("SpaceGravity2D: NaN(INF) MEAN ANOMALY"); //litle paranoya
					Debug.Break();
				}
				if (double.IsNaN(eccentricAnomaly) || double.IsInfinity(eccentricAnomaly)) {
					Debug.Log("SpaceGravity2D: NaN(INF) ECC ANOMALY");
					Debug.Break();
				}
				if (double.IsNaN(trueAnomaly) || double.IsInfinity(trueAnomaly)) {
					Debug.Log("SpaceGravity2D: NaN(INF) TRUE ANOMALY");
					Debug.Break();
				}
			} else {
				double n = System.Math.Sqrt(attractorMass * gravConst / System.Math.Pow(semiMajorAxis, 3)) * Mathd.Sign(orbitNormalDotEclipticNormal);
				meanAnomaly = meanAnomaly - n * deltaTime;
				eccentricAnomaly = CelestialBodyUtils.KeplerSolverHyperbolicCase(meanAnomaly, eccentricity);
				trueAnomaly = System.Math.Atan2(System.Math.Sqrt(eccentricity * eccentricity - 1.0) * System.Math.Sinh(eccentricAnomaly), eccentricity - System.Math.Cosh(eccentricAnomaly));
			}
		}

		public void SetPositionByCurrentAnomaly() {
			position = GetFocalPositionAtEccentricAnomaly(eccentricAnomaly);
		}

		public void SetVelocityByCurrentAnomaly() {
			velocity = GetVelocityAtEccentricAnomaly(eccentricAnomaly);
		}

		public void SetEccentricity(double e) {
			if (!isValidOrbit) {
				return;
			}
			e = Mathd.Abs(e);
			var _periapsis = periapsisDistance; // Periapsis remains constant
			eccentricity = e;
			var compresion = eccentricity < 1 ? ( 1 - eccentricity * eccentricity ) : ( eccentricity * eccentricity - 1 );
			semiMajorAxis = System.Math.Abs(_periapsis / ( 1 - eccentricity ));
			focalParameter = semiMajorAxis * compresion;
			semiMinorAxis = semiMajorAxis * Mathd.Sqrt(compresion);
			centerPoint = semiMajorAxis * System.Math.Abs(eccentricity) * semiMajorAxisBasis;
			if (eccentricity < 1) {
				eccentricAnomaly = CelestialBodyUtils.KeplerSolver(meanAnomaly, eccentricity);
				var cosE = System.Math.Cos(eccentricAnomaly);
				trueAnomaly = System.Math.Acos(( cosE - eccentricity ) / ( 1 - eccentricity * cosE ));
				if (meanAnomaly > Mathd.PI) {
					trueAnomaly = Mathd.PI_2 - trueAnomaly;
				}
			} else {
				eccentricAnomaly = CelestialBodyUtils.KeplerSolverHyperbolicCase(meanAnomaly, eccentricity);
				trueAnomaly = System.Math.Atan2(System.Math.Sqrt(eccentricity * eccentricity - 1) * System.Math.Sinh(eccentricAnomaly), eccentricity - System.Math.Cosh(eccentricAnomaly));
			}
			SetVelocityByCurrentAnomaly();
			SetPositionByCurrentAnomaly();

			CalculateNewOrbitData();
		}

		public void SetMeanAnomaly(double m) {
			if (!isValidOrbit) {
				return;
			}
			meanAnomaly = m % Mathd.PI_2;
			if (eccentricity < 1) {
				if (meanAnomaly < 0) {
					meanAnomaly += Mathd.PI_2;
				}
				eccentricAnomaly = CelestialBodyUtils.KeplerSolver(meanAnomaly, eccentricity);
				trueAnomaly = CelestialBodyUtils.ConvertEccentricToTrueAnomaly(eccentricAnomaly, eccentricity);
			} else {
				eccentricAnomaly = CelestialBodyUtils.KeplerSolverHyperbolicCase(meanAnomaly, eccentricity);
				trueAnomaly = CelestialBodyUtils.ConvertEccentricToTrueAnomaly(eccentricAnomaly, eccentricity);
			}
			SetPositionByCurrentAnomaly();
			SetVelocityByCurrentAnomaly();
		}

		public void SetTrueAnomaly(double t) {
			if (!isValidOrbit) {
				return;
			}
			t %= Mathd.PI_2;

			if (eccentricity < 1) {
				if (t < 0) {
					t += Mathd.PI_2;
				}
				eccentricAnomaly = CelestialBodyUtils.ConvertTrueToEccentricAnomaly(t, eccentricity);
				meanAnomaly = eccentricAnomaly - eccentricity * System.Math.Sin(eccentricAnomaly);
			} else {
				eccentricAnomaly = CelestialBodyUtils.ConvertTrueToEccentricAnomaly(t, eccentricity);
				meanAnomaly = System.Math.Sinh(eccentricAnomaly) * eccentricity - eccentricAnomaly;
			}
			SetPositionByCurrentAnomaly();
			SetVelocityByCurrentAnomaly();
		}

		public void SetEccentricAnomaly(double e) {
			if (!isValidOrbit) {
				return;
			}
			e %= Mathd.PI_2;
			eccentricAnomaly = e;
			if (eccentricity < 1) {
				if (e < 0) {
					e = Mathd.PI_2 + e;
				}
				eccentricAnomaly = e;
				trueAnomaly = CelestialBodyUtils.ConvertEccentricToTrueAnomaly(e, eccentricity);
				meanAnomaly = eccentricAnomaly - eccentricity * System.Math.Sin(eccentricAnomaly);
			} else {
				trueAnomaly = CelestialBodyUtils.ConvertEccentricToTrueAnomaly(e, eccentricity);
				meanAnomaly = System.Math.Sinh(eccentricAnomaly) * eccentricity - eccentricAnomaly;
			}
			SetPositionByCurrentAnomaly();
			SetVelocityByCurrentAnomaly();
		}

		public void RotateOrbit(Quaternion rotation) {
			position = new Vector3d(rotation * ( (Vector3)position ));
			velocity = new Vector3d(rotation * ( (Vector3)velocity ));
			CalculateNewOrbitData();
		}
	}
}
