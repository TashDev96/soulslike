using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TowerDefense
{
	[CreateAssetMenu(fileName = "UpgradeLevelsData", menuName = "Configs/Upgrade Levels Data")]
	public class UpgradeLevelsData : ScriptableObject
	{
		[SerializeField]
		private List<UpgradeLevelConfig> _upgradeLevels;

		public List<UpgradeLevelConfig> UpgradeLevels => _upgradeLevels;

		[Button("Paste From Google Sheets")]
		private void PasteFromGoogleSheets()
		{
			string clipboardText = GUIUtility.systemCopyBuffer;
			if (string.IsNullOrEmpty(clipboardText))
			{
				Debug.LogWarning("Clipboard is empty");
				return;
			}

			string[] lines = clipboardText.Split('\n');
			int validLines = 0;
			
			EnsureUpgradeLevelsExist(lines.Length);

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i].Trim();
				if (string.IsNullOrEmpty(line))
					continue;

				string[] columns = line.Split('\t');
				if (columns.Length < 2)
					continue;

				int price = 0;
				float damage = 0f;
				float damageMultiplier = 0f;
				int towersToAdd = 0;

				if (columns.Length > 0 && !string.IsNullOrWhiteSpace(columns[0]))
					int.TryParse(columns[0].Trim(), out price);

				if (columns.Length > 1 && !string.IsNullOrWhiteSpace(columns[1]))
					float.TryParse(columns[1].Trim(), out damage);

				if (columns.Length > 2 && !string.IsNullOrWhiteSpace(columns[2]))
					float.TryParse(columns[2].Trim(), out damageMultiplier);

				if (columns.Length > 3 && !string.IsNullOrWhiteSpace(columns[3]))
					int.TryParse(columns[3].Trim(), out towersToAdd);

				_upgradeLevels[validLines].LevelUpPrice = price;
				_upgradeLevels[validLines].BaseDamage = damage;
				_upgradeLevels[validLines].DamageMultiplier = damageMultiplier;
				_upgradeLevels[validLines].TowersToAdd = towersToAdd;
				
				validLines++;
			}

			Debug.Log($"Pasted {validLines} upgrade levels from Google Sheets");
		}

		private void EnsureUpgradeLevelsExist(int requiredCount = 25)
		{
			if (_upgradeLevels == null)
			{
				_upgradeLevels = new List<UpgradeLevelConfig>();
			}

			while (_upgradeLevels.Count < requiredCount)
			{
				_upgradeLevels.Add(new UpgradeLevelConfig());
			}
		}
	}
}

