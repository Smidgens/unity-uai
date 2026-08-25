// smidgens @ github

// resharper disable all

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	// indexers for icon atlas
	internal enum EUAIAtlasIcon
	{
		None = -1,
		Action = 0, // row 0
		Bucket,
		Consideration,
		MemoryKey,
		Service,
		Active = 16, // row 3
		Cancelled,
		Uncancellable,
		Deactivating,
		Finished,
		Selectable,
		Muted,
		Stats = 48,
		Timeline,
		SelectRandom = 56, // row 7
		SelectTop,
		SelectTopPercentage
	}
}

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;

	// wraps use of icon texture atlas
	internal sealed class UAIEditorAtlas
	{
		private static UAIEditorAtlas Create()
		{
			var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(_ATLAS_GUID));
			return new UAIEditorAtlas(atlas);
		}

		private static UAIEditorAtlas _instance;

		private static UAIEditorAtlas GetInstance()
		{
			if (_instance == null)
			{
				_instance = Create();
			}
			return _instance;
		}

		private UAIEditorAtlas(Texture2D atlas)
		{
			for (int i = 0; i < _atlasIcons.Length; i++)
			{
				int y = i / _TILE_COUNT, x = i % _TILE_COUNT;
				var offset = new Vector2(x, y) * _TILE_SIZE;
				_atlasIcons[i] = new UAIAtlasIcon(new Rect(offset, Vector2.one * _TILE_SIZE), atlas);
			}
		}

		public static ref readonly UAIAtlasIcon GetIcon(EUAIAtlasIcon icon)
		{
			if (icon == EUAIAtlasIcon.None)
			{
				return ref UAIAtlasIcon.Empty;
			}
			return ref GetInstance()._atlasIcons[(int)icon];
		}

		private const int _TILE_COUNT = 8;
		private const float _TILE_SIZE = 1f / _TILE_COUNT;
		private const string _ATLAS_GUID = "a1446d554144a4944b389210a34ff6b9";
		
		private readonly UAIAtlasIcon[] _atlasIcons = new UAIAtlasIcon[_TILE_COUNT * _TILE_COUNT];
	}
}

#endif