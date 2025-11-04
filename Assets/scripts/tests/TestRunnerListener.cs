using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Tests
{
	[InitializeOnLoad]
	public class TestRunnerListener
	{
		private const string FlagFilePath = "all_tests_passed";
		private static readonly TestRunnerApi _api;

		static TestRunnerListener()
		{
			_api = ScriptableObject.CreateInstance<TestRunnerApi>();
			_api.RegisterCallbacks(new TestRunCallback());
		}

		private class TestRunCallback : ICallbacks
		{
			private int _testsCount = 0;

			public void RunStarted(ITestAdaptor testsToRun)
			{
			}

			public void RunFinished(ITestResultAdaptor result)
			{
				_api.RetrieveTestList(TestMode.EditMode, adaptor =>
				{
					var count = CountEditorTests(adaptor);

					Debug.Log($"{count}  {result.PassCount}");
				});

				if(result.FailCount == 0)
				{
					CreateFlagFile();
				}

				int CountEditorTests(ITestAdaptor adaptor)
				{
					if(adaptor == null || adaptor.TestMode == TestMode.PlayMode || adaptor.RunState == RunState.Ignored)
					{
						return 0;
					}

					var count = 0;
					if(!adaptor.IsSuite)
					{
						count++;
					}

					foreach(var child in adaptor.Children)
					{
						count += CountEditorTests(child);
					}

					return count;
				}
			}

			public void TestStarted(ITestAdaptor test)
			{
			}

			public void TestFinished(ITestResultAdaptor result)
			{
			}

			private void CreateFlagFile()
			{
				File.WriteAllText(FlagFilePath, string.Empty);
				Debug.Log("Flag file created at: " + FlagFilePath);
			}
		}
	}
}
