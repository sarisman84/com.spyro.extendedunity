using System.Collections.Generic;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;
using Spyro.EditorExtensions;
using Debug = UnityEngine.Debug;

namespace Spyro.Editor.RecursiveScriptableObject
{
	public class RecursiveEditorService
	{
		public static RecursiveEditorService Instance { get; private set; } = new();


		private HashSet<int> renderedEditorsCache;

		private Dictionary<int, bool> _internalData;
		private Dictionary<int, bool> cachedEditorStates
		{
			get
			{
				if (_internalData == null)
				{
					VerifyFile();
					using (StreamReader sr = File.OpenText(DataFilePath))
					{
						_internalData = JsonConvert.DeserializeObject<Dictionary<int, bool>>(sr.ReadToEnd()) ?? new();

					}
				}

				return _internalData;
			}
		}

		private static string DataFilePath => Path.Combine(ExtraUtilities.EditorTempDirectory, "temp_editor_data.json");

		RecursiveEditorService()
		{
			renderedEditorsCache = new HashSet<int>();

			AssemblyReloadEvents.beforeAssemblyReload -= OnEditorReloadStart;
			AssemblyReloadEvents.beforeAssemblyReload += OnEditorReloadStart;
		}

		private void VerifyFile()
		{
			if (!File.Exists(DataFilePath))
			{
				File.Create(DataFilePath).Close();
			}
		}

		private void OnEditorReloadStart()
		{
			var data = JsonConvert.SerializeObject(cachedEditorStates);
			File.WriteAllText(DataFilePath, data);

		}

		public void CacheIdentifier(int id)
		{
			if (!renderedEditorsCache.Contains(id))
			{
				renderedEditorsCache.Add(id);
			}

		}

		public void UpdateCachedIdentifierState(int id, bool newState)
		{
			if (!cachedEditorStates.TryAdd(id, newState))
			{
				cachedEditorStates[id] = newState;
			}

		}

		public bool GetCachedIdentifierState(int id)
		{
			cachedEditorStates.TryAdd(id, false);
			return cachedEditorStates[id];
		}

		public void RemoveCachedIdentifier(int id)
		{
			renderedEditorsCache.Remove(id);
		}

		public bool IsAlreadyRendered(int id)
		{
			return renderedEditorsCache.Contains(id);
		}


	}
}