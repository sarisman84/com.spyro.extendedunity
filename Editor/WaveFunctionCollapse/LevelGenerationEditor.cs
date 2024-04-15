using System;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Spyro.ProcedualGeneration
{
    [EditorWindowTitle(title = "Level Generation Editor", useTypeNameAsIconName = false)]
    public class LevelGenerationEditor : EditorWindow
    {
        private const string placeModeIcon = "LODGroup Icon";
        private const string removeModeIcon = "RectTransform Icon";

        private enum EditMode
        {
            Place,
            Remove,
            None
        }

        private EditMode editMode;
        private LevelGenerationData data;
        private ToolsPanel toolsPanel;
        private ObjectPicker objectPicker;
        private string targetPrefabPath;


        private Vector3Int pointerPosition;


        private bool PlaceKeybind
            =>
            Event.current != null &&
            Event.current.keyCode == KeyCode.LeftShift &&
            Event.current.keyCode == KeyCode.F;

        private bool RemoveKeybind
            =>
            Event.current != null &&
            Event.current.keyCode == KeyCode.LeftShift &&
            Event.current.keyCode == KeyCode.G;

        public LevelGenerationData Data => data;


        [UnityEditor.Callbacks.OnOpenAsset(1)]
        private static bool Callback(int instanceID, int line)
        {
            var data = EditorUtility.InstanceIDToObject(instanceID) as LevelGenerationData;
            if (data != null)
            {
                OpenEditor(data);
                return true;
            }

            return false;
        }


        private static void OpenEditor(LevelGenerationData data)
        {
            var editor = GetWindow<LevelGenerationEditor>();
            editor.InitData(data);
            editor.InitGUI();
        }

        private void InitData(LevelGenerationData _data)
        {
            data = _data;
            data.tileData = data.tileData ?? new List<LevelGenerationData.Tile>();
            data.gameObjectData = data.gameObjectData ?? new List<GameObject>();
        }

        private void InitGUI()
        {
            rootVisualElement.Clear();
            InitMainControlsPanel();
        }



        private void InitMainControlsPanel()
        {
            toolsPanel = new ToolsPanel(rootVisualElement);
            toolsPanel.AddButton(placeModeIcon, () => { editMode = EditMode.Place; });
            toolsPanel.AddButton(removeModeIcon, () => { editMode = EditMode.Remove; });

            objectPicker = new ObjectPicker(this);
            objectPicker.onObjectSelect += OnPrefabSelect;
        }

        private void OnPrefabSelect(string path)
        {
            targetPrefabPath = path;
        }

        private void OnInspectorUpdate()
        {
            editMode = PlaceKeybind ? EditMode.Place : RemoveKeybind ? EditMode.Remove : editMode;
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void HandleRemoveLogic(SceneView view)
        {
            if (Event.current.type != EventType.MouseDown || Event.current.button != 0)
            {
                return;
            }

            LevelGenerationData.RemoveTile(data, pointerPosition);
        }

        private void HandlePlaceLogic(SceneView view)
        {
            if (Event.current.type != EventType.MouseDown || Event.current.button != 0)
            {
                return;
            }

            LevelGenerationData.CreateTile(data, AssetDatabase.LoadAssetAtPath<GameObject>(targetPrefabPath), pointerPosition);
        }

        private void OnSceneGUI(SceneView view)
        {
            if (!data)
            {
                return;
            }

            DrawPointer(view);
            DrawTiles();

            switch (editMode)
            {
                case EditMode.Place:
                    HandlePlaceLogic(view);
                    break;
                case EditMode.Remove:
                    HandleRemoveLogic(view);
                    break;
                default:
                    return;
            }

            HandleUtility.Repaint();
        }

        private void DrawTiles()
        {
            if (data.tileData != null)
            {
                //using (var scope = new Handles.DrawingScope())
                //{
                //    var oldTest = Handles.zTest;
                //    Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                //    for (var i = 0; i < data.tileData.Count; ++i)
                //    {
                //        Handles.CubeHandleCap(0, data.tileData[i].position, Quaternion.identity, 1, EventType.Repaint);
                //    }
                //    Handles.zTest = oldTest;
                //}
                if (data.gameObjectData.Count == 0)
                    return;

                for (var i = 0; i < data.tileData.Count; ++i)
                {
                    var objType = data.tileData[i].tileType;
                    DrawGameObject(data.gameObjectData[objType], data.tileData[i].position);
                    Handles.DrawWireCube(data.tileData[i].position, Vector3.one * 0.85f);
                }
            }
        }

        private void DrawGameObject(GameObject obj, Vector3 position)
        {
            DrawGameObject(obj, Matrix4x4.identity, position, Quaternion.identity, Vector3.one);
        }

        private void DrawGameObject(GameObject gameObject, Matrix4x4 parentTransform, Vector3 worldPosition, Quaternion worldRotation, Vector3 worldScale)
        {
            // Get the MeshFilter and MeshRenderer components
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();

            // Calculate the GameObject's local transform
            Matrix4x4 localTransform = Matrix4x4.TRS(gameObject.transform.localPosition, gameObject.transform.localRotation, gameObject.transform.localScale);

            // Calculate the GameObject's world transform
            Matrix4x4 worldTransform = parentTransform * localTransform;

            // If the GameObject has a MeshFilter and MeshRenderer
            if (meshFilter != null && meshRenderer != null)
            {
                // Calculate the inverse of the parent's world transform
                Matrix4x4 inverseParentWorldTransform = parentTransform.inverse;

                // Calculate the local position, rotation, and scale that corresponds to the desired world position, rotation, and scale
                Vector3 localPosition = inverseParentWorldTransform.MultiplyPoint3x4(worldPosition);
                Quaternion localRotation = Quaternion.Euler(inverseParentWorldTransform.rotation.eulerAngles - worldRotation.eulerAngles);
                Vector3 localScale = Vector3.Scale(worldScale, inverseParentWorldTransform.lossyScale);

                // Calculate the final world transform using the local position, rotation, and scale
                Matrix4x4 finalWorldTransform = parentTransform * Matrix4x4.TRS(localPosition, localRotation, localScale);

                // Draw the mesh
                Graphics.DrawMesh(meshFilter.sharedMesh, finalWorldTransform, meshRenderer.sharedMaterial, 0);
            }

            // Recursively draw the children
            foreach (Transform child in gameObject.transform)
            {
                DrawGameObject(child.gameObject, worldTransform, worldPosition, worldRotation, worldScale);
            }

        }

        private void DrawPointer(SceneView view)
        {
            pointerPosition = TryUpdatePoint(view, pointerPosition);
            var oldColor = Handles.color;
            switch (editMode)
            {
                case EditMode.Place:
                    Handles.color = Color.green;
                    break;
                case EditMode.Remove:
                    Handles.color = Color.red;
                    break;
                default:
                    Handles.color = oldColor;
                    break;
            }

            Handles.DrawWireCube(pointerPosition, Vector3.one);
            Handles.color = oldColor;
        }

        private Vector3Int TryUpdatePoint(SceneView view, Vector3Int currentPoint)
        {
            if (Event.current.type != EventType.MouseMove)
                return currentPoint;

            var cam = view.camera;
            var mousePos = Event.current.mousePosition;
            mousePos.y = cam.pixelHeight - mousePos.y;
            var ray = cam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y));

            var plane = new Plane(Vector3.up, Vector3.up * pointerPosition.y);

            if (plane.Raycast(ray, out var dist))
            {
                var result = Vector3Int.FloorToInt(ray.GetPoint(dist));
                return new Vector3Int(result.x, pointerPosition.y, result.z);
            }

            return currentPoint;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            if (objectPicker != null)
                objectPicker.onObjectSelect -= OnPrefabSelect;
        }


    }
}

