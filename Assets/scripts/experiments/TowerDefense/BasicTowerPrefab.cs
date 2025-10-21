using UnityEngine;

namespace TowerDefense
{
	public class BasicTowerPrefab : MonoBehaviour
	{
		[Header("Visual Components")]
		[SerializeField]
		private GameObject towerBase;
		[SerializeField]
		private GameObject towerTurret;
		[SerializeField]
		private LineRenderer attackLine;

		private void Awake()
		{
			SetupTowerComponents();
		}

		[ContextMenu("Setup Tower")]
		public void SetupTower()
		{
			SetupTowerComponents();
		}

		private void SetupTowerComponents()
		{
			if(towerBase == null)
			{
				towerBase = CreateTowerBase();
			}

			if(towerTurret == null)
			{
				towerTurret = CreateTowerTurret();
			}

			if(attackLine == null)
			{
				attackLine = CreateAttackLine();
			}

			var towerUnit = GetComponent<TowerUnit>();
			if(towerUnit == null)
			{
				towerUnit = gameObject.AddComponent<TowerUnit>();
			}
		}

		private GameObject CreateTowerBase()
		{
			var baseGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			baseGO.name = "TowerBase";
			baseGO.transform.SetParent(transform);
			baseGO.transform.localPosition = Vector3.zero;
			baseGO.transform.localScale = new Vector3(1f, 0.5f, 1f);

			var renderer = baseGO.GetComponent<Renderer>();
			if(renderer != null)
			{
				renderer.material.color = Color.gray;
			}

			return baseGO;
		}

		private GameObject CreateTowerTurret()
		{
			var turretGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
			turretGO.name = "TowerTurret";
			turretGO.transform.SetParent(transform);
			turretGO.transform.localPosition = new Vector3(0, 1f, 0);
			turretGO.transform.localScale = new Vector3(0.8f, 0.8f, 1.2f);

			var renderer = turretGO.GetComponent<Renderer>();
			if(renderer != null)
			{
				renderer.material.color = Color.blue;
			}

			return turretGO;
		}

		private LineRenderer CreateAttackLine()
		{
			var lineGO = new GameObject("AttackLine");
			lineGO.transform.SetParent(transform);
			lineGO.transform.localPosition = Vector3.zero;

			var line = lineGO.AddComponent<LineRenderer>();
			line.material = new Material(Shader.Find("Sprites/Default"));
			line.startColor = Color.red;
			line.endColor = Color.red;
			line.startWidth = 0.1f;
			line.endWidth = 0.05f;
			line.positionCount = 2;
			line.enabled = false;

			return line;
		}
	}
}
