using System;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Soulslike.Application;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Soulslike.Editor
{
	public class ServerTestWindow : OdinEditorWindow
	{
		[BoxGroup("Connection Settings")]
		[SerializeField]
		private string baseUrl = "http://localhost:5189";

		[BoxGroup("Authentication")]
		[SerializeField]
		private string deviceId = "test_device_01";

		[BoxGroup("Authentication")]
		[ReadOnly]
		[SerializeField]
		private string currentToken;

		[BoxGroup("Save Data")]
		[TextArea(10, 20)]
		[SerializeField]
		private string saveDataJson = "{}";

		[BoxGroup("Shared Writings")]
		[SerializeField]
		private WritingData testWriting = new WritingData { locationId = "area_01", position = new float[3], rotation = new float[3], wordIndexes = new int[] { 1, 2, 3 } };

		[BoxGroup("Shared Writings")]
		[SerializeField]
		private string fetchLocationId = "area_01";

		[BoxGroup("Shared Writings")]
		[ReadOnly]
		[ListDrawerSettings(ShowIndexLabels = true)]
		[SerializeField]
		private WritingData[] fetchedWritings;

		private readonly BackendClient client = new();

		[HorizontalGroup("Authentication/Buttons")]
		[Button(ButtonSizes.Large)] [GUIColor(0.4f, 0.8f, 1)]
		[EnableIf("@!client.IsLoggedIn")]
		public void Login()
		{
			client.BaseUrl = baseUrl;
			EditorCoroutineUtility.StartCoroutineOwnerless(client.Login(deviceId,
				token =>
				{
					currentToken = token;
					Debug.Log("Login Successful!");
					Repaint();
				},
				error =>
				{
					Debug.LogError("Login Failed: " + error);
					Repaint();
				}));
		}

		[HorizontalGroup("Authentication/Buttons")]
		[Button(ButtonSizes.Large)] [GUIColor(1, 0.4f, 0.4f)]
		[EnableIf("@client.IsLoggedIn")]
		public void Logout()
		{
			client.Token = null;
			currentToken = null;
			Debug.Log("Logged out.");
			Repaint();
		}

		[BoxGroup("Actions")]
		[Button(ButtonSizes.Medium)]
		[EnableIf("@client.IsLoggedIn")]
		public void FetchSave()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(client.GetSave(
				json =>
				{
					saveDataJson = json;
					Debug.Log("Save data fetched.");
					Repaint();
				},
				error =>
				{
					Debug.LogError("Fetch Save Failed: " + error);
					Repaint();
				}));
		}

		[BoxGroup("Actions")]
		[Button(ButtonSizes.Medium)]
		[EnableIf("@client.IsLoggedIn")]
		public void PushSave()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(client.UpdateSave(saveDataJson,
				() => Debug.Log("Save data updated successfully."),
				error => Debug.LogError("Update Save Failed: " + error)));
		}

		[BoxGroup("Shared Writings/Actions")]
		[Button(ButtonSizes.Medium)]
		[EnableIf("@client.IsLoggedIn")]
		public void SaveWriting()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(client.SaveWriting(testWriting,
				() => Debug.Log("Writing saved successfully."),
				error => Debug.LogError("Save Writing Failed: " + error)));
		}

		[BoxGroup("Shared Writings/Actions")]
		[Button(ButtonSizes.Medium)]
		[EnableIf("@client.IsLoggedIn")]
		public void GetWritings()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(client.GetWritings(fetchLocationId,
				writings =>
				{
					fetchedWritings = writings;
					Debug.Log($"Fetched {writings.Length} writings.");
					Repaint();
				},
				error => Debug.LogError("Get Writings Failed: " + error)));
		}

		[MenuItem("Tools/Server/Test Window")]
		private static void OpenWindow()
		{
			GetWindow<ServerTestWindow>().Show();
		}

		[InfoBox("Status: Logged In", InfoMessageType.Info, "@client.IsLoggedIn")]
		[InfoBox("Status: Not Logged In", InfoMessageType.Warning, "@!client.IsLoggedIn")]
		[PropertySpace]
		[Button]
		private void ClearConsole()
		{
			var logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
			var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
			clearMethod.Invoke(null, null);
		}
	}
}
