using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private LevelData levelData;

    private Item selectedPalettePrefab;
    private Vector2 paletteScroll;
    private Vector2 gridScroll;
    private bool showPalette = true;
    private bool showRawList = true;

    private readonly List<Item> palettePrefabs = new List<Item>();

    private const float CellSize = 82f;

    private void OnEnable()
    {
        levelData = (LevelData)target;
        RebuildPaletteFromCurrentItems();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawHeaderFields();
        EditorGUILayout.Space(8);
        DrawPaletteSection();
        EditorGUILayout.Space(8);
        DrawGridSection();
        EditorGUILayout.Space(8);
        DrawRawListSection();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(levelData);
        }
    }

    private void DrawHeaderFields()
    {
        EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);

        levelData.levelName = EditorGUILayout.TextField("Level Name", levelData.levelName);
        levelData.width = Mathf.Max(1, EditorGUILayout.IntField("Width", levelData.width));
        levelData.height = Mathf.Max(1, EditorGUILayout.IntField("Height", levelData.height));
        levelData.minTime = EditorGUILayout.IntField("Min Time", levelData.minTime);
        levelData.minSteps = EditorGUILayout.IntField("Min Steps", levelData.minSteps);
    }

    private void DrawPaletteSection()
    {
        showPalette = EditorGUILayout.Foldout(showPalette, "Palette", true);
        if (!showPalette)
            return;

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.HelpBox(
            "Choose a prefab, then left-click a cell to place it. Right-click a cell to remove items or clear the tile.",
            MessageType.Info
        );

        selectedPalettePrefab = (Item)EditorGUILayout.ObjectField(
            "Selected Prefab",
            selectedPalettePrefab,
            typeof(Item),
            false
        );

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add Selected To Palette"))
        {
            if (selectedPalettePrefab != null && !palettePrefabs.Contains(selectedPalettePrefab))
            {
                palettePrefabs.Add(selectedPalettePrefab);
            }
        }

        if (GUILayout.Button("Refresh Palette From Level"))
        {
            RebuildPaletteFromCurrentItems();
        }

        EditorGUILayout.EndHorizontal();

        paletteScroll = EditorGUILayout.BeginScrollView(paletteScroll, GUILayout.Height(140));

        const int columns = 3;
        int index = 0;

        while (index < palettePrefabs.Count)
        {
            EditorGUILayout.BeginHorizontal();

            for (int c = 0; c < columns && index < palettePrefabs.Count; c++, index++)
            {
                Item prefab = palettePrefabs[index];
                bool isSelected = prefab == selectedPalettePrefab;

                GUIStyle style = new GUIStyle(GUI.skin.button);
                if (isSelected)
                {
                    style.fontStyle = FontStyle.Bold;
                }

                if (GUILayout.Button(GetPaletteButtonText(prefab), style, GUILayout.Height(42)))
                {
                    selectedPalettePrefab = prefab;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private void DrawGridSection()
    {
        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);

        float gridPixelWidth = levelData.width * CellSize;
        float gridPixelHeight = levelData.height * CellSize;

        gridScroll = EditorGUILayout.BeginScrollView(
            gridScroll,
            GUILayout.Height(Mathf.Min(520f, gridPixelHeight + 20f))
        );

        Rect fullRect = GUILayoutUtility.GetRect(gridPixelWidth, gridPixelHeight);

        for (int drawRow = 0; drawRow < levelData.height; drawRow++)
        {
            int y = levelData.height - 1 - drawRow;

            for (int x = 0; x < levelData.width; x++)
            {
                Rect cellRect = new Rect(
                    fullRect.x + x * CellSize,
                    fullRect.y + drawRow * CellSize,
                    CellSize,
                    CellSize
                );

                DrawCell(cellRect, x, y);
                HandleCellInput(cellRect, x, y);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawCell(Rect rect, int x, int y)
    {
        List<ItemSetup> items = GetItemsAt(x, y);

        bool hasEmpty = items.Any(i => i.itemPrefab is Empty);
        bool hasPlayer = items.Any(i => i.itemPrefab is PlayerController);

        Color oldBg = GUI.backgroundColor;

        if (hasEmpty)
            GUI.backgroundColor = new Color(0.35f, 0.22f, 0.22f);
        else if (hasPlayer)
            GUI.backgroundColor = new Color(0.22f, 0.35f, 0.22f);
        else
            GUI.backgroundColor = Color.white;

        GUI.Box(rect, GUIContent.none);
        GUI.backgroundColor = oldBg;

        GUI.Label(
            new Rect(rect.x + 4, rect.y + 3, rect.width - 8, 14),
            $"({x},{y})",
            EditorStyles.miniLabel
        );

        float lineY = rect.y + 18f;
        int maxLines = 4;

        for (int i = 0; i < Mathf.Min(items.Count, maxLines); i++)
        {
            string label = GetShortLabel(items[i].itemPrefab);

            GUI.Label(
                new Rect(rect.x + 4, lineY, rect.width - 8, 14),
                label,
                EditorStyles.miniBoldLabel
            );

            lineY += 14f;
        }

        if (items.Count > maxLines)
        {
            GUI.Label(
                new Rect(rect.x + 4, lineY, rect.width - 8, 14),
                $"+{items.Count - maxLines} more",
                EditorStyles.miniLabel
            );
        }
    }

    private void HandleCellInput(Rect rect, int x, int y)
    {
        Event e = Event.current;

        if (!rect.Contains(e.mousePosition))
            return;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (selectedPalettePrefab != null)
            {
                Undo.RecordObject(levelData, "Place Level Item");

                levelData.setUpItems.Add(new ItemSetup
                {
                    itemPrefab = selectedPalettePrefab,
                    x = x,
                    y = y
                });

                EditorUtility.SetDirty(levelData);
            }

            e.Use();
        }

        if (e.type == EventType.ContextClick)
        {
            ShowContextMenuForCell(x, y);
            e.Use();
        }
    }

    private void ShowContextMenuForCell(int x, int y)
    {
        GenericMenu menu = new GenericMenu();
        List<ItemSetup> items = GetItemsAt(x, y);

        if (selectedPalettePrefab != null)
        {
            menu.AddItem(new GUIContent("Add Selected Prefab Here"), false, () =>
            {
                Undo.RecordObject(levelData, "Add Level Item");

                levelData.setUpItems.Add(new ItemSetup
                {
                    itemPrefab = selectedPalettePrefab,
                    x = x,
                    y = y
                });

                EditorUtility.SetDirty(levelData);
            });
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Add Selected Prefab Here"));
        }

        if (items.Count > 0)
        {
            menu.AddSeparator("");

            foreach (ItemSetup item in items)
            {
                ItemSetup captured = item;
                string label = GetShortLabel(captured.itemPrefab);

                menu.AddItem(new GUIContent("Remove/" + label), false, () =>
                {
                    Undo.RecordObject(levelData, "Remove Level Item");
                    levelData.setUpItems.Remove(captured);
                    EditorUtility.SetDirty(levelData);
                });

                menu.AddItem(new GUIContent("Ping Prefab/" + label), false, () =>
                {
                    if (captured.itemPrefab != null)
                    {
                        EditorGUIUtility.PingObject(captured.itemPrefab);
                        Selection.activeObject = captured.itemPrefab;
                    }
                });
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Clear Cell"), false, () =>
            {
                Undo.RecordObject(levelData, "Clear Cell");
                levelData.setUpItems.RemoveAll(i => i.x == x && i.y == y);
                EditorUtility.SetDirty(levelData);
            });
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Clear Cell"));
        }

        menu.ShowAsContext();
    }

    private void DrawRawListSection()
    {
        showRawList = EditorGUILayout.Foldout(showRawList, "Raw Setup Items", true);
        if (!showRawList)
            return;

        EditorGUILayout.BeginVertical("box");

        for (int i = 0; i < levelData.setUpItems.Count; i++)
        {
            ItemSetup item = levelData.setUpItems[i];

            EditorGUILayout.BeginHorizontal();

            item.itemPrefab = (Item)EditorGUILayout.ObjectField(item.itemPrefab, typeof(Item), false);
            item.x = EditorGUILayout.IntField(item.x, GUILayout.Width(45));
            item.y = EditorGUILayout.IntField(item.y, GUILayout.Width(45));

            GUILayout.Label(GetShortLabel(item.itemPrefab), GUILayout.Width(90));

            if (GUILayout.Button("Ping", GUILayout.Width(45)))
            {
                if (item.itemPrefab != null)
                {
                    EditorGUIUtility.PingObject(item.itemPrefab);
                    Selection.activeObject = item.itemPrefab;
                }
            }

            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                Undo.RecordObject(levelData, "Delete Level Item");
                levelData.setUpItems.RemoveAt(i);
                EditorUtility.SetDirty(levelData);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private List<ItemSetup> GetItemsAt(int x, int y)
    {
        return levelData.setUpItems
            .Where(i => i != null && i.itemPrefab != null && i.x == x && i.y == y)
            .ToList();
    }

    private void RebuildPaletteFromCurrentItems()
    {
        palettePrefabs.Clear();

        foreach (Item prefab in levelData.setUpItems
                     .Where(i => i != null && i.itemPrefab != null)
                     .Select(i => i.itemPrefab)
                     .Distinct())
        {
            palettePrefabs.Add(prefab);
        }

        if (selectedPalettePrefab == null && palettePrefabs.Count > 0)
        {
            selectedPalettePrefab = palettePrefabs[0];
        }
    }

    private string GetPaletteButtonText(Item prefab)
    {
        if (prefab == null)
            return "NULL";

        return $"{prefab.name}\n[{GetShortLabel(prefab)}]";
    }

    private string GetShortLabel(Item prefab)
    {
        if (prefab == null)
            return "NULL";

        if (prefab is PlayerController player && !player.notMain)
            return "Player";

        if (prefab is PlayerController mirror && mirror.notMain)
            return "Mirror" + (mirror.isMirrorHorizontal? "-Horizontal" : "") + (mirror.isMirrorVertical ? "-Vertical" : "");

        if (prefab is Empty)
            return "Empty";

        if (prefab is WinningGate gate)
            return $"Win{gate.requiredSize}";

        if (prefab is Teleporter tp)
            return $"Teleporter{tp.id}" + (tp.active ? " on" : " off");

        if (prefab is PushButton btn)
            return $"Button{btn.id}:{btn.size}";

        if (prefab is Launcher launcher)
            return $"Launcher {launcher.upDistance}/{launcher.downDistance}/{launcher.leftDistance}/{launcher.rightDistance}";

        if (prefab is DollPiece doll)
        {
            string prefix = doll.type == DollPieceType.Top ? "Top Piece" : "Bottom Piece";
            return $"{prefix}{doll.size}";
        }

        if (prefab is Block block)
        {
            string dir = DirectionToSymbol(block.blockSide);
            return $"Spikes-{dir} " + ( block.id >= 0 ? block.id : "unpassable");
        }

        return prefab.GetType().Name;
    }

    private string DirectionToSymbol(object directionValue)
    {
        if (directionValue == null)
            return "?";

        string name = directionValue.ToString();

        switch (name)
        {
            case "Up": return "^";
            case "Down": return "v";
            case "Left": return "<";
            case "Right": return ">";
            default: return name;
        }
    }
}