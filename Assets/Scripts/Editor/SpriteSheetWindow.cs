using LitJson;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using Object = UnityEngine.Object;

public class SpriteSheetWindow : EditorWindow
{
    [MenuItem("Tools / SpriteSheetWindow")]
    static void OnMenuItemClick() => Open();

    const float kWINDOW_H = 400f;
    const float kWINDOW_W = 600f;

    public static void Open()
    {
        SpriteSheetWindow window = EditorWindow.GetWindow<SpriteSheetWindow>();
        window.position = new Rect(Screen.width / 2,Screen.height / 2, kWINDOW_W, kWINDOW_H);
        window.Show();
    }

    [SerializeField]
    List<Texture> textureList = new List<Texture>();

    SerializedObject serializedObject;

    ReorderableList reorderableList;

    void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("textureList"), false, true, true, true);

        reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "");
        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;

            GUIContent gUIContent = new GUIContent("Texture " + index);
            EditorGUI.PropertyField(rect, reorderableList.serializedProperty.GetArrayElementAtIndex(index), gUIContent);
        };
    }

    string groupPattern = "[a-zA-Z0-9]*_(?<name>[a-zA-Z0-9]+_[0-9])(?<index>[0-9]+)";

    bool packTogether = false;

    string searchInFolder = "Assets";

    Vector2 scrollPosition = Vector2.zero;

    void OnGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        {
            searchInFolder = EditorGUILayout.TextField("Search In Folder", searchInFolder);
            AcceptDrag(GUILayoutUtility.GetLastRect(), (paths, objs) => {
                searchInFolder = paths[0];
                if (Path.HasExtension(searchInFolder))
                    searchInFolder = Path.GetDirectoryName(searchInFolder);
                return true;
            });

            if (GUILayout.Button("Scan"))
            {
                textureList.Clear();
                string[] guids = AssetDatabase.FindAssets("t:Texture", new string[] { searchInFolder });
                foreach (var guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                    if (texture)
                        textureList.Add(texture);
                }
            }

            if (GUILayout.Button("Clean"))
                textureList.Clear();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);
            reorderableList.DoLayoutList();
            EditorGUILayout.EndScrollView();
            AcceptDrag(GUILayoutUtility.GetLastRect(), (paths, objs) => {
                foreach (var obj in objs)
                {
                    var texture = obj as Texture;
                    if (texture != null)
                        textureList.Add(texture);
                }
                return true;
            });
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            packTogether = EditorGUILayout.ToggleLeft("Pack Together", packTogether,GUILayout.MaxWidth(110));
            EditorGUILayout.LabelField("Group Pattern", GUILayout.MaxWidth(100));
            groupPattern = EditorGUILayout.TextField(groupPattern);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        {
            

            if (GUILayout.Button("Export"))
            {
                if (packTogether)
                {
                    string filePath = null;
                    List<string> assetPaths = new List<string>();
                    for (int i = 0; i < textureList.Count; i++)
                    {
                        var texture = textureList[i];
                        var assetPath = AssetDatabase.GetAssetPath(texture);
                        if (filePath == null)
                            filePath = assetPath.Substring(0, assetPath.LastIndexOf("/") + 1) + Path.GetFileNameWithoutExtension(assetPath) + ".asset";
                        assetPaths.Add(assetPath);
                    }
                    ExportAnimation(filePath, assetPaths.ToArray(),groupPattern);
                }
                else
                {
                    for (int i = 0; i < textureList.Count; i++)
                    {
                        var texture = textureList[i];
                        var assetPath = AssetDatabase.GetAssetPath(texture);
                        var filePath = assetPath.Substring(0, assetPath.LastIndexOf("/") + 1) + Path.GetFileNameWithoutExtension(assetPath) + ".asset";
                        ExportAnimation(filePath, new string[] { assetPath }, groupPattern);
                    }
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    void ExportAnimation(string filePath,string[] assetPaths,string pattern)
    {
        List<string> paths = new List<string>();
        foreach (var assetPath in assetPaths)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
            if (texture == null)
                continue;

            var json = assetPath.Substring(0, assetPath.LastIndexOf("/") + 1) + Path.GetFileNameWithoutExtension(assetPath) + ".json";
            if (SliceSprite(assetPath, json))
                paths.Add(assetPath);
        }

        SetAnimationSprite(filePath, paths.ToArray(),pattern);
    }

    void SetAnimationSprite(string filePath,string[] assetPaths,string pattern)
    {
        SpriteSheetAnimation sheetAnimation = AssetDatabase.LoadAssetAtPath<SpriteSheetAnimation>(filePath);
        if(sheetAnimation == null)
        {
            sheetAnimation = CreateInstance<SpriteSheetAnimation>();
            AssetDatabase.CreateAsset(sheetAnimation, filePath);
        }

        List<Object> sprites = new List<Object>();
        foreach (var assetPath in assetPaths)
        {
            var objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            sprites.AddRange(objects);
        }
        sheetAnimation.SetAllSprites(sprites.ToArray(),pattern);

        EditorUtility.ClearDirty(sheetAnimation);
        EditorUtility.SetDirty(sheetAnimation);
    }

    bool SliceSprite(string assetPath,string jsonPath)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
        var json = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);

        if (texture != null && json != null)
        {
            SpriteMetaData[] spriteMetaDatas = CreateSpriteMeta(json.text);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritesheet = spriteMetaDatas;
            importer.SaveAndReimport();

            return true;
        }

        return false;
    }

    SpriteMetaData[] CreateSpriteMeta(string jsonStr)
    {
        List<SpriteMetaData> spriteMetaDatas = new List<SpriteMetaData>();
        var json = JsonMapper.ToObject(jsonStr);
        var frames = json["frames"];
        var meta = json["meta"];
        var width = (int)meta["size"]["w"];
        var height = (int)meta["size"]["h"];

        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];

            int x = (int)frame["frame"]["x"];
            int y = (int)frame["frame"]["y"];
            int w = (int)frame["frame"]["w"];
            int h = (int)frame["frame"]["h"];

            float ox = (int)frame["spriteSourceSize"]["x"];
            float oy = (int)frame["spriteSourceSize"]["y"];
            float ow = (int)frame["sourceSize"]["w"];
            float oh = (int)frame["sourceSize"]["h"];

            var filename = Path.GetFileNameWithoutExtension((string)frame["filename"]);

            SpriteMetaData spriteMetaData = new SpriteMetaData();
            spriteMetaData.name = filename;
            spriteMetaData.rect = new Rect(x, height - y - h, w, h);
            spriteMetaData.alignment = (int)SpriteAlignment.Custom;
            spriteMetaData.pivot = new Vector2((ow / 2f - ox) / w,(-oh / 2f + h + oy) / h);

            spriteMetaDatas.Add(spriteMetaData);
        }

        return spriteMetaDatas.ToArray();
    }
    
    void AcceptDrag(Rect rect,Func<string[],Object[],bool> func)
    {
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!rect.Contains(evt.mousePosition))
                    break;
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                if (evt.type == EventType.DragPerform)
                {
                    if (func(DragAndDrop.paths,DragAndDrop.objectReferences))
                        DragAndDrop.AcceptDrag();
                }
                break;
        }
    }
}
