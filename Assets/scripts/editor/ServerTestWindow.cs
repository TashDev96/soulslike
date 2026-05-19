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
		public enum ServerEnvironment
		{
			Local,
			Production
		}

		[BoxGroup("Connection Settings")]
		[EnumToggleButtons]
		[OnValueChanged(nameof(UpdateBaseUrl))]
		[SerializeField]
		private ServerEnvironment environment;

		[BoxGroup("Connection Settings")]
		[SerializeField]
		private string baseUrl = BackendClient.LocalUrl;

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
		private WritingData testWriting = new() { locationId = "area_01", position = new float[3], rotation = new float[3], wordIndexes = new[] { 1, 2, 3 } };

		[BoxGroup("Shared Writings")]
		[SerializeField]
		private string fetchLocationId = "area_01";

		[BoxGroup("Shared Writings")]
		[ReadOnly]
		[ListDrawerSettings(ShowIndexLabels = true)]
		[SerializeField]
		private WritingData[] fetchedWritings;

		private readonly BackendClient client = new();

		protected override void OnEnable()
		{
			base.OnEnable();
			environment = (ServerEnvironment)EditorPrefs.GetInt("ServerTestWindow_Environment", (int)ServerEnvironment.Local);
			UpdateBaseUrl();
		}

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

		private void UpdateBaseUrl()
		{
			baseUrl = environment == ServerEnvironment.Local ? BackendClient.LocalUrl : BackendClient.ProductionUrl;
			EditorPrefs.SetInt("ServerTestWindow_Environment", (int)environment);
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
