using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace SpaceGravity2D.Demo {

	public class PooledBody {
		public GameObject go;
		public CelestialBody body;
		public bool isReady;
		public OrbitDisplay orbitDisplay;
		public PredictionSystemTarget predictionDisplay;
		public Collider coll;
		public MeshRenderer meshRend;
		public Light light;
		//public GameObject velocityHandle;//will add in future version
		public LineRenderer velocityLine;
	}

	public class BodiesPool : MonoBehaviour {

		public PooledBody this[int i] {
			get{
				return pool[i];
			}
		}
		public int Count {
			get {
				return pool.Count;
			}
		}

		public int ActiveCount {
			get {
				return pool.Count(b => !b.isReady);
			}
		}

		List<PooledBody> pool = new List<PooledBody>();

		public PooledBody GetReadyOne() {
			int readyIndex = -1;
			for (int i = 0; i < pool.Count; i++) {
				if (pool[i].isReady) {
					pool[i].isReady = false;
					pool[i].body.attractor = null;
					readyIndex = i;
					break;
				}
			}
			if (readyIndex < 0) {
				pool.Add(CreatePooledBody());
				readyIndex = pool.Count - 1;
			}
			return pool[readyIndex];
		}

		public void ReleaseOne(CelestialBody body) {
			bool destroy = true;
			for (int i = 0; i < pool.Count; i++) {
				if (pool[i].body == body) {
					pool[i].isReady = true;
					destroy = false;
					break;
				}
			}
			body.position = new Vector3d();
			body.velocity = new Vector3d();
			if (destroy) {
				Destroy(body.gameObject);
			} else {
				body.gameObject.SetActive(false);
			}
		}

		PooledBody CreatePooledBody() {
			var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			var cb = go.AddComponent<CelestialBody>();
		
			cb.isAttractorSearchActive = true;
			go.GetComponent<Collider>().isTrigger = true;
			var l = go.AddComponent<Light>();
			l.enabled = false;
			l.range = 1000f;
			l.intensity = 0.5f;
			var predSys = go.AddComponent<PredictionSystemTarget>();
			predSys.enabled = false;
			var orbDraw =  go.AddComponent<OrbitDisplay>();
			orbDraw.enabled = false;
			//var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
			//handle.GetComponent<Collider>().isTrigger = true;
			//handle.transform.SetParent(go.transform);
			//handle.transform.localScale = new Vector3(1, 1, 1) * 0.2f;
			var velocityLine = go.AddComponent<LineRenderer>();
			velocityLine.SetVertexCount(2);
			velocityLine.SetWidth(0.8f, 0.8f);

			return new PooledBody() {
				go = go,
				body = cb,
				coll = cb.GetComponent<Collider>(),
				isReady = false,
				light = l,
				meshRend = go.GetComponent<MeshRenderer>(),
				orbitDisplay = orbDraw,
				predictionDisplay = predSys,
				//velocityHandle = handle,
				velocityLine = velocityLine,
			};
		}

		public bool IsAnyLightActive() {
			return pool.Any(p => !p.isReady && p.light.enabled);
		}
	}
}