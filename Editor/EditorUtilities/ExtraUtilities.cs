using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Spyro.EditorExtensions
{
	public static class ExtraUtilities
	{

		public static string EditorTempDirectory
		{
			get
			{
				var path = Application.dataPath;
				var result = path.Replace("Assets", "Packages/com.spyro.extendedunity/Resources/Temp");

				if (!Directory.Exists(result))
				{
					Directory.CreateDirectory(result);
				}
				return result;
			}
		}

	}
}
