﻿using System;
using System.Collections.Generic;
using System.Linq;
using SardineFish.Utils;
using SardineFish.Utils.Editor;
using UnityEditor;
using UnityEngine;
using WFC.Tilemap3D;

namespace WFC.Editor
{
    public class Tilemap3DEditor : EditorWindow
    {
        [SerializeField] private Texture2D _iconCursor;
        [SerializeField] private Texture2D _iconBrush;
        [SerializeField] private Texture2D _iconColorPicker;
        [SerializeField] private Texture2D _iconEraser;

        enum EditMode
        {
            None,
            Paint,
            Pick,
            Erase,
        }

        private static EditMode[] ToolsMode = new[]
        {
            EditMode.None,
            EditMode.Paint,
            EditMode.Pick,
            EditMode.Erase,
        };

        private EditMode _editMode = EditMode.None;
        
        private List<Tilemap3D.Tilemap3D> _tilemaps = new List<Tilemap3D.Tilemap3D>();
        private Tilemap3D.Tilemap3D _palette;
        private Tilemap3D.Tilemap3D _editingTilemap;
        private int _controlID;
        private bool _shouldReload = true;
        private GameObjectTile _selectedTile;
        
        [MenuItem("Window/Tilemap 3D")]
        private static void ShowWindow()
        {
            var window = GetWindow<Tilemap3DEditor>();
            window.titleContent = new GUIContent("Tilemap 3D Editor");
            window.Show();
        }

        private void OnGUI()
        {
            EditorUtils.Horizontal(() =>
            {
                var toolIdx = ToolsMode.IndexOf(_editMode);
                toolIdx = GUILayout.SelectionGrid(toolIdx, new GUIContent[]
                {
                    EditorGUIUtility.IconContent("Grid.Default"),
                    EditorGUIUtility.IconContent("Grid.PaintTool"),
                    EditorGUIUtility.IconContent("Grid.PickingTool"),
                    EditorGUIUtility.IconContent("Grid.EraserTool"),
                }, 4);
                _editMode = ToolsMode[toolIdx];

            });

            var selected = _palette ? _tilemaps.IndexOf(_palette) : -1;
            // var idx = EditorGUILayout.DropdownButton(new GUIContent(selected), FocusType.Keyboard,
            //     _tilemaps.Select(t => new GUIContent(t.name)).ToArray());
            selected = EditorGUILayout.Popup("Palette", selected, _tilemaps.Select(t => t.name).ToArray());
            if (selected >= 0)
                _palette = _tilemaps[selected];
            else
                _palette = null;
            
            selected = _editingTilemap ? _tilemaps.IndexOf(_editingTilemap) : -1;
            // var idx = EditorGUILayout.DropdownButton(new GUIContent(selected), FocusType.Keyboard,
            //     _tilemaps.Select(t => new GUIContent(t.name)).ToArray());
            selected = EditorGUILayout.Popup("Palette", selected, _tilemaps.Select(t => t.name).ToArray());
            if (selected >= 0)
                _editingTilemap = _tilemaps[selected];
            else
                _editingTilemap = null;

            if (GUILayout.Button("Refresh"))
            {
                _tilemaps = GameObject.FindObjectsOfType<Tilemap3D.Tilemap3D>().ToList();
                
                foreach (var tilemap in _tilemaps)
                {
                    tilemap.ReloadTileFromChildren();
                }
            }
            
            EditorGUILayout.LabelField("Selected Tile", _selectedTile ? _selectedTile.name : "<None>");
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnScene;
            AssemblyReloadEvents.afterAssemblyReload += AssemblyReload;
            _tilemaps = GameObject.FindObjectsOfType<Tilemap3D.Tilemap3D>().ToList();
            _controlID = GUIUtility.GetControlID(FocusType.Passive);
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnScene;
            AssemblyReloadEvents.afterAssemblyReload -= AssemblyReload;
        }

        private void AssemblyReload()
        {
            // _shouldReload = true;
            foreach (var tilemap in _tilemaps)
            {
                tilemap.ReloadTileFromChildren();
            }
        }

        private void OnScene(SceneView obj)
        {
            if (_selectedTile)
            {
                Handles.color = Color.cyan;
                Handles.DrawWireCube(_selectedTile.Position + (Vector3.one / 2), Vector3.one);
            }
            if (_editMode == EditMode.None)
                return;
            
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var ev = Event.current;
            if (!(ev.type == EventType.MouseDown && ev.button == 0))
                return;

            if (_editMode == EditMode.Pick && _palette)
            {
                var ray = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
                _selectedTile = _palette.RayMarch(ray, 100);
                _editMode = EditMode.Paint;
                
                ev.Use();
            }
            else if (_editMode == EditMode.Paint && _editingTilemap && _selectedTile)
            {
                var ray = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
                var tile = _editingTilemap.RayMarch(ray, 100, out _, out var normal);
                Vector3Int pos;
                if (tile)
                    pos = tile.Position + normal;
                else
                {
                    pos = (ray.origin + ray.direction * (-ray.origin.y / ray.direction.y)).FloorToVector3Int();
                }
                
                _editingTilemap.SetTile(pos, _selectedTile);
            }
            else if (_editMode == EditMode.Erase && _editingTilemap)
            {
                var ray = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
                var tile = _editingTilemap.RayMarch(ray, 100, out var hitPos, out _);
                if (tile)
                {
                    _editingTilemap.RemoveTile(hitPos);
                }
            }
        }
    }
}