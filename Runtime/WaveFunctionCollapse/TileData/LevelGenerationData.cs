using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spyro.ProcedualGeneration
{
    [CreateAssetMenu(fileName = "Level Generation Data", menuName = "Procedual Generation/Level", order = 0)]
    public class LevelGenerationData : ScriptableObject
    {
        public enum Direction
        {
            North,
            South,
            East,
            West,
            Up,
            Down
        }
        [System.Serializable]
        public struct Tile
        {
            public Vector3 position;
            public int tileType;
            public List<List<Tile>> validTiles;


        }

        public List<Tile> tileData;
        public List<GameObject> gameObjectData;
        public Vector3 tileSize;


        public static Tile CreateTile(LevelGenerationData data, GameObject obj, Vector3 position)
        {
            Tile tile = new Tile();
            tile.position = position;
            tile.tileType = data.gameObjectData.IndexOf(obj);
            if (tile.tileType == -1)
            {
                data.gameObjectData = data.gameObjectData ?? new List<GameObject> { obj };
                if (data.gameObjectData.Count == 0)
                    data.gameObjectData.Add(obj);
                tile.tileType = data.gameObjectData.IndexOf(obj);
            }
            data.tileData.Add(tile);
            return tile;
        }

        public static void AddValidTilesToTile(LevelGenerationData data, Tile tileToEdit, Direction direction, List<Tile> validTiles)
        {
            var indx = data.tileData.IndexOf(tileToEdit);
            if (indx == -1)
            {
                return;
            }

            if (tileToEdit.validTiles == null)
            {
                tileToEdit.validTiles = new List<List<Tile>>();
            }

            tileToEdit.validTiles[(int)direction].AddRange(validTiles);
        }

        public static void RemoveTile(LevelGenerationData data, Vector3 position)
        {
            var indx = data.tileData.FindIndex((t) => t.position == position);
            if (indx == -1)
            {
                return;
            }

            var tile = data.tileData[indx];
            data.tileData.RemoveAt(indx);
        }
    }
}

