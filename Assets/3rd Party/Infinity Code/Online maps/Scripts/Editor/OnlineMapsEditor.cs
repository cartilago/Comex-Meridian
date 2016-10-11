/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

#if !UNITY_4_3 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
#define UNITY_5_0P
#endif

#if UNITY_5_0P && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
#define UNITY_5_3P
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#if UNITY_5_3P
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

[CustomEditor(typeof (OnlineMaps))]
public class OnlineMapsEditor : Editor
{
    private enum TextureType
    {
        Texture,
        Sprite
    }

    private static GUIStyle _warningStyle;

    private OnlineMaps api;
    public static readonly int[] availableSizes = {256, 512, 1024, 2048, 4096};
    private string[] availableSizesStr;
    private bool showAdvanced;
    private bool showCreateTexture;
    private bool showCustomProviderTokens;
    private bool showMarkers;
    private bool showSave;
    private bool showResourcesTokens;
    private bool showTroubleshooting;
    private int textureHeight = 512;
    private int textureWidth = 512;
    private TextureType textureType;
    private GUIContent updateAvailableContent;
    private string textureFilename = "OnlineMaps";

    private bool saveSettings = true;
    private bool saveTexture = true;
    private bool saveControl = true;
    private bool saveMarkers = true;
    private bool saveMarkers3D = true;
    private bool saveLocationService = true;
    private bool allowSaveMarkers3D;
    private bool allowSaveLocationService;
    private bool allowSaveTexture;

    private SerializedProperty pSource;
    private SerializedProperty pMapType;

    private SerializedProperty pProvider;
    private SerializedProperty pType;
    private SerializedProperty pLabels;
    private SerializedProperty pCustomProviderURL;
    private SerializedProperty pResourcesPath;
    private SerializedProperty pTexture;
    private SerializedProperty pTarget;
    private SerializedProperty pRedrawOnPlay;
    private SerializedProperty pUseSmartTexture;
    private SerializedProperty pUseCurrentZoomTiles;
    private SerializedProperty pTraffic;
    private SerializedProperty pEmptyColor;
    private SerializedProperty pDefaultTileTexture;
    private SerializedProperty pTooltipTexture;
    private SerializedProperty pDefaultMarkerTexture;
    private SerializedProperty pDefaultMarkerAlign;
    private SerializedProperty pShowMarkerTooltip;
    private SerializedProperty pLanguage;
    private SerializedProperty pUseSoftwareJPEGDecoder;
    private SerializedProperty pNotInteractUnderGUI;
    private SerializedProperty pStopPlayingWhenScriptsCompile;
    private SerializedProperty pTilesetWidth;
    private SerializedProperty pTilesetHeight;
    private SerializedProperty pTilesetSize;
    private SerializedProperty pMarkers;

    private GUIContent cUseSmartTexture;
    private GUIContent cDefaultMarkerAlign;
    private GUIContent cTilesetWidth;
    private GUIContent cTilesetHeight;
    private GUIContent cTilesetSize;
    private GUIContent cUseSoftwareJPEGDecoder;

    private OnlineMapsProvider[] providers;
    private string[] providersTitle;
    private OnlineMapsProvider.MapType mapType;
    private int providerIndex;
    private GUIContent cTooltipTexture;

#if UNITY_WEBGL || UNITY_WEBPLAYER
    private SerializedProperty pUseProxy;
    private SerializedProperty pWebplayerProxyURL;
#endif

#if !UNITY_WEBGL
    private SerializedProperty pRenderInThread;
#endif

    public static GUIStyle warningStyle
    {
        get
        {
            if (_warningStyle == null)
            {
                _warningStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = {textColor = Color.red},
                    fontStyle = FontStyle.Bold
                };
            }
            
            return _warningStyle;
        }
    }

    private static bool isPlay
    {
        get { return Application.isPlaying; }
    }

    public static void AddCompilerDirective(string directive)
    {
        string currentDefinitions = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> directives = currentDefinitions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
        if (!directives.Contains(directive)) directives.Add(directive);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, String.Join(";", directives.ToArray()));
    }

    private void CacheSerializedProperties()
    {
        pSource = serializedObject.FindProperty("source");
        pMapType = serializedObject.FindProperty("mapType");

        pProvider = serializedObject.FindProperty("provider");
        pType = serializedObject.FindProperty("type");

        pCustomProviderURL = serializedObject.FindProperty("customProviderURL");
        pResourcesPath = serializedObject.FindProperty("resourcesPath");

        pLabels = serializedObject.FindProperty("labels");
        pLanguage = serializedObject.FindProperty("language");
        pTarget = serializedObject.FindProperty("target");
        pTexture = serializedObject.FindProperty("texture");

        pTilesetWidth = serializedObject.FindProperty("tilesetWidth");
        pTilesetHeight = serializedObject.FindProperty("tilesetHeight");
        pTilesetSize = serializedObject.FindProperty("tilesetSize");

        pMarkers = serializedObject.FindProperty("markers");

        pRedrawOnPlay = serializedObject.FindProperty("redrawOnPlay");
        pUseSmartTexture = serializedObject.FindProperty("useSmartTexture");
        pUseCurrentZoomTiles = serializedObject.FindProperty("useCurrentZoomTiles");
        pTraffic = serializedObject.FindProperty("traffic");
        pEmptyColor = serializedObject.FindProperty("emptyColor");
        pDefaultTileTexture = serializedObject.FindProperty("defaultTileTexture");
        pTooltipTexture = serializedObject.FindProperty("tooltipBackgroundTexture");
        pDefaultMarkerTexture = serializedObject.FindProperty("defaultMarkerTexture");
        pDefaultMarkerAlign = serializedObject.FindProperty("defaultMarkerAlign");
        pShowMarkerTooltip = serializedObject.FindProperty("showMarkerTooltip");
        pUseSoftwareJPEGDecoder = serializedObject.FindProperty("useSoftwareJPEGDecoder");

#if !UNITY_WEBGL
        pRenderInThread = serializedObject.FindProperty("renderInThread");
#endif
        pNotInteractUnderGUI = serializedObject.FindProperty("notInteractUnderGUI");
        pStopPlayingWhenScriptsCompile = serializedObject.FindProperty("stopPlayingWhenScriptsCompile");

#if UNITY_WEBGL || UNITY_WEBPLAYER
        pUseProxy = serializedObject.FindProperty("useWebplayerProxy");
        pWebplayerProxyURL = serializedObject.FindProperty("webplayerProxyURL");
#endif

        cUseSmartTexture = new GUIContent("Smart Texture");
        cDefaultMarkerAlign = new GUIContent("Marker Align");
        cTilesetWidth = new GUIContent("Width (pixels)");
        cTilesetHeight = new GUIContent("Height (pixels)");
        cTilesetSize = new GUIContent("Size (in scene)");
        cUseSoftwareJPEGDecoder = new GUIContent("Software JPEG Decoder");
        cTooltipTexture = new GUIContent("Tooltip Background");
    }

    private void CheckAPITextureImporter(SerializedProperty property)
    {
        Texture2D texture = property.objectReferenceValue as Texture2D;
        CheckAPITextureImporter(texture);
    }

    private static void CheckAPITextureImporter(Texture2D texture)
    {
        if (texture == null) return;

        string textureFilename = AssetDatabase.GetAssetPath(texture.GetInstanceID());
        TextureImporter textureImporter = AssetImporter.GetAtPath(textureFilename) as TextureImporter;
        if (textureImporter == null) return;

        bool needReimport = false;
        if (textureImporter.mipmapEnabled)
        {
            textureImporter.mipmapEnabled = false;
            needReimport = true;
        }
        if (!textureImporter.isReadable)
        {
            textureImporter.isReadable = true;
            needReimport = true;
        }
        if (textureImporter.textureFormat != TextureImporterFormat.RGB24)
        {
            textureImporter.textureFormat = TextureImporterFormat.RGB24;
            needReimport = true;
        }
        if (textureImporter.maxTextureSize < 256)
        {
            textureImporter.maxTextureSize = 256;
            needReimport = true;
        }

        if (needReimport) AssetDatabase.ImportAsset(textureFilename, ImportAssetOptions.ForceUpdate);
    }

#if UNITY_WEBPLAYER
    private void CheckJSLoader()
    {
        OnlineMapsJSLoader loader = api.GetComponent<OnlineMapsJSLoader>();
        if (loader != null) return;

        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.HelpBox("For Webplayer strongly recommended to use JS Loader.", MessageType.Warning);
        if (GUILayout.Button("Add JS Loader")) api.gameObject.AddComponent<OnlineMapsJSLoader>();

        EditorGUILayout.EndVertical();
    }
#endif

    public static void CheckMarkerTextureImporter(SerializedProperty property)
    {
        Texture2D texture = property.objectReferenceValue as Texture2D;
        CheckMarkerTextureImporter(texture);
    }

    public static void CheckMarkerTextureImporter(Texture2D texture)
    {
        if (texture == null) return;

        string textureFilename = AssetDatabase.GetAssetPath(texture.GetInstanceID());
        TextureImporter textureImporter = AssetImporter.GetAtPath(textureFilename) as TextureImporter;
        if (textureImporter == null) return;

        bool needReimport = false;
        if (textureImporter.mipmapEnabled)
        {
            textureImporter.mipmapEnabled = false;
            needReimport = true;
        }
        if (!textureImporter.isReadable)
        {
            textureImporter.isReadable = true;
            needReimport = true;
        }
        if (textureImporter.textureFormat != TextureImporterFormat.ARGB32)
        {
            textureImporter.textureFormat = TextureImporterFormat.ARGB32;
            needReimport = true;
        }

        if (needReimport) AssetDatabase.ImportAsset(textureFilename, ImportAssetOptions.ForceUpdate);
    }

    private void CheckNullControl()
    {
        OnlineMapsControlBase[] controls = api.GetComponents<OnlineMapsControlBase>();
        if (controls != null && controls.Length != 0) return;

        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.HelpBox("Problem detected:\nCan not find OnlineMaps Control component.", MessageType.Error);
        if (GUILayout.Button("Add Control"))
        {
            GenericMenu menu = new GenericMenu();

            Type[] types = api.GetType().Assembly.GetTypes();
            foreach (Type t in types)
            {
                if (t.IsSubclassOf(typeof (OnlineMapsControlBase)))
                {
                    if (t == typeof (OnlineMapsControlBase2D) || t == typeof (OnlineMapsControlBase3D)) continue;

                    string fullName = t.FullName.Substring(10);

                    int controlIndex = fullName.IndexOf("Control");
                    fullName = fullName.Insert(controlIndex, " ");

                    int textureIndex = fullName.IndexOf("Texture");
                    if (textureIndex > 0) fullName = fullName.Insert(textureIndex, " ");

                    menu.AddItem(new GUIContent(fullName), false, data =>
                    {
                        Type ct = data as Type;
                        api.gameObject.AddComponent(ct);
                        pTarget.enumValueIndex = ct == typeof (OnlineMapsTileSetControl) ? (int)OnlineMapsTarget.tileset : (int)OnlineMapsTarget.texture;
                        Repaint();
                    }, t);
                }
            }

            menu.ShowAsContext();
        }

        EditorGUILayout.EndVertical();
    }

    private void CreateTexture()
    {
        string texturePath = string.Format("Assets/{0}.png", textureFilename);
        
        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        File.WriteAllBytes(texturePath, texture.EncodeToPNG());
        AssetDatabase.Refresh();
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (textureImporter != null)
        {
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = true;
            textureImporter.textureFormat = TextureImporterFormat.RGB24;
            textureImporter.maxTextureSize = Mathf.Max(textureWidth, textureHeight);

            if (textureType == TextureType.Sprite)
            {
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.npotScale = TextureImporterNPOTScale.None;
            }

            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            Texture2D newTexture = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
            pTexture.objectReferenceValue = newTexture;

#if NGUI
            UITexture uiTexture = api.GetComponent<UITexture>();
            if (uiTexture != null) uiTexture.mainTexture = newTexture;
#endif
        }

        OnlineMapsUtils.DestroyImmediate(texture);

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
        EditorUtility.UnloadUnusedAssets();
#else
        EditorUtility.UnloadUnusedAssetsImmediate();
#endif
    }

    private void DrawAdvancedGUI()
    {
        float oldWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 160;

        if (pTarget.enumValueIndex == (int)OnlineMapsTarget.texture)
        {
            EditorGUILayout.PropertyField(pRedrawOnPlay);
            EditorGUILayout.PropertyField(pUseSmartTexture, cUseSmartTexture);
        }

        EditorGUILayout.PropertyField(pUseCurrentZoomTiles);
        EditorGUILayout.PropertyField(pTraffic);
        EditorGUILayout.PropertyField(pEmptyColor);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(pDefaultTileTexture);
        if (EditorGUI.EndChangeCheck()) CheckAPITextureImporter(pDefaultTileTexture);

        EditorGUILayout.PropertyField(pTooltipTexture, cTooltipTexture);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(pDefaultMarkerTexture);
        if (EditorGUI.EndChangeCheck()) CheckMarkerTextureImporter(pDefaultMarkerTexture);

        EditorGUILayout.PropertyField(pDefaultMarkerAlign, cDefaultMarkerAlign);
        EditorGUILayout.PropertyField(pShowMarkerTooltip);

        EditorGUIUtility.labelWidth = oldWidth;
    }

    private void DrawCacheGUI(ref bool dirty)
    {
        if (pSource.enumValueIndex == (int)OnlineMapsSource.Resources || !GUILayout.Button("Cache tiles to Resources")) return;

        lock (OnlineMapsTile.tiles)
        {
            const string resPath = "Assets/Resources";

            foreach (OnlineMapsTile tile in OnlineMapsTile.tiles)
            {
                if (tile.status != OnlineMapsTileStatus.loaded || tile.texture == null) continue;

                string tilePath = Path.Combine(resPath, tile.resourcesPath + ".png");
                FileInfo info = new FileInfo(tilePath);
                if (!info.Directory.Exists) info.Directory.Create();

                if (pTarget.enumValueIndex == (int)OnlineMapsTarget.tileset) File.WriteAllBytes(tilePath, tile.texture.EncodeToPNG());
                else
                {
                    Texture2D texture = new Texture2D(OnlineMapsUtils.tileSize, OnlineMapsUtils.tileSize, TextureFormat.ARGB32, false);
                    texture.SetPixels32(tile.colors);
                    texture.Apply();
                    File.WriteAllBytes(tilePath, texture.EncodeToPNG());
                }
            }
        }

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
        AssetDatabase.Refresh();
#else
        EditorPrefs.SetBool("OnlineMapsRefreshAssets", true);
#endif

        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Cache complete", "Stop playback and select 'Source - Resources And Online'.", "OK");

        dirty = true;
    }

    private void DrawCreateTextureGUI(ref bool dirty)
    {
        if (availableSizesStr == null) availableSizesStr = availableSizes.Select(s => s.ToString()).ToArray();

        textureFilename = EditorGUILayout.TextField("Filename", textureFilename);
        textureType = (TextureType)EditorGUILayout.EnumPopup("Type", textureType);

        textureWidth = EditorGUILayout.IntPopup("Width", textureWidth, availableSizesStr, availableSizes);
        textureHeight = EditorGUILayout.IntPopup("Height", textureHeight, availableSizesStr, availableSizes);

        if (GUILayout.Button("Create"))
        {
            CreateTexture();
            dirty = true;
        }

        EditorGUILayout.Space();
    }

    private bool DrawGeneralGUI()
    {
        bool dirty = false;

        DrawSourceGUI();
        DrawLocationGUI(ref dirty);
        DrawTargetGUI();

        if (isPlay)
        {
            DrawCacheGUI(ref dirty);

            if (!showSave) 
            {
                if (GUILayout.Button("Save state"))
                {
                    allowSaveMarkers3D = api.GetComponent<OnlineMapsControlBase3D>() != null;
                    allowSaveLocationService = api.GetComponent<OnlineMapsLocationService>() != null;
                    allowSaveTexture = pTarget.enumValueIndex == (int)OnlineMapsTarget.texture;

                    showSave = true;
                    dirty = true;
                }
            }
            else
            {
                DrawSaveGUI(ref dirty);
            }
        }

        return dirty;
    }

    private void DrawLabelsGUI()
    {
        bool showLanguage;
        if (mapType.hasLabels)
        {
            EditorGUILayout.PropertyField(pLabels);
            showLanguage = pLabels.boolValue;
        }
        else
        {
            showLanguage = mapType.labelsEnabled;
            GUILayout.Label("Labels " + (showLanguage ? "enabled" : "disabled"));
        }
        if (showLanguage && mapType.hasLanguage)
        {
            EditorGUILayout.PropertyField(pLanguage);
            EditorGUILayout.HelpBox(mapType.provider.twoLetterLanguage ? "Use two-letter code such as: en": "Use three-letter code such as: eng", MessageType.Info);
        }
    }

    private void DrawLocationGUI(ref bool dirty)
    {
        double px, py;
        api.GetPosition(out px, out py);

        EditorGUI.BeginChangeCheck();
#if UNITY_5_0P
        py = EditorGUILayout.DoubleField("Latitude", py);
        px = EditorGUILayout.DoubleField("Longitude", px);
#else
        py = EditorGUILayout.FloatField("Latitude", (float)py);
        px = EditorGUILayout.FloatField("Longitude", (float) px);
#endif

        if (EditorGUI.EndChangeCheck())
        {
            dirty = true;
            api.SetPosition(px, py);
        }

        EditorGUI.BeginChangeCheck();
        api.zoom = EditorGUILayout.IntSlider("Zoom", api.zoom, 3, 20);
        if (EditorGUI.EndChangeCheck()) dirty = true;
    }

    private void DrawMarkersGUI(ref bool dirty)
    {
        int removedIndex = -1;

        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < pMarkers.arraySize; i++)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            OnlineMapsMarkerPropertyDrawer.isRemoved = false;
            EditorGUILayout.PropertyField(pMarkers.GetArrayElementAtIndex(i), new GUIContent("Marker " + (i + 1)));
            if (OnlineMapsMarkerPropertyDrawer.isRemoved) removedIndex = i;
            EditorGUILayout.EndHorizontal();
        }
        if (EditorGUI.EndChangeCheck()) dirty = true;

        if (removedIndex != -1)
        {
            ArrayUtility.RemoveAt(ref api.markers, removedIndex);
            dirty = true;
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Marker"))
        {
            if (!Application.isPlaying)
            {
                OnlineMapsMarker marker = new OnlineMapsMarker
                {
                    position = api.position,
                    align = (OnlineMapsAlign)pDefaultMarkerAlign.enumValueIndex
                };
                ArrayUtility.Add(ref api.markers, marker);
            }
            else
            {
                OnlineMapsMarker marker = api.AddMarker(api.position);
                marker.align = (OnlineMapsAlign)pDefaultMarkerAlign.enumValueIndex;
            }

            dirty = true;
        }
    }

    private void DrawProviderGUI()
    {
        EditorGUI.BeginChangeCheck();
        providerIndex = EditorGUILayout.Popup("Provider", providerIndex, providersTitle);
        if (EditorGUI.EndChangeCheck())
        {
            mapType = providers[providerIndex].types[0];
            pMapType.stringValue = mapType.ToString();
        }

        if (mapType.useHTTP)
        {
            EditorGUILayout.HelpBox(mapType.provider.title + " - " + mapType.title + " uses HTTP, which can cause problems in iOS9+.", MessageType.Warning);
        }
        else if (mapType.isCustom)
        {
            EditorGUILayout.PropertyField(pCustomProviderURL);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            showCustomProviderTokens = Foldout(showCustomProviderTokens, "Available tokens");
            if (showCustomProviderTokens)
            {
                GUILayout.Label("{zoom}");
                GUILayout.Label("{x}");
                GUILayout.Label("{y}");
                GUILayout.Label("{quad}");
                GUILayout.Space(10);
            }
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawSaveGUI(ref bool dirty)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.LabelField("Save state:");

        saveSettings = EditorGUILayout.Toggle("Settings", saveSettings);
        
        if (allowSaveTexture) saveTexture = EditorGUILayout.Toggle("Texture", saveTexture);

        saveControl = EditorGUILayout.Toggle("Control", saveControl);
        saveMarkers = EditorGUILayout.Toggle("Markers", saveMarkers);

        if (allowSaveMarkers3D) saveMarkers3D = EditorGUILayout.Toggle("Markers 3D", saveMarkers3D);
        if (allowSaveLocationService) saveLocationService = EditorGUILayout.Toggle("Location Service", saveLocationService);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save state"))
        {
            if (allowSaveTexture && saveTexture)
            {
                api.Save();

                string path = AssetDatabase.GetAssetPath(api.texture);
                File.WriteAllBytes(path, api.texture.EncodeToPNG());
                AssetDatabase.Refresh();
            }

            OnlineMapsXML prefs = new OnlineMapsXML("OnlineMaps");

            if (saveSettings) api.SaveSettings(prefs);
            if (saveControl) api.GetComponent<OnlineMapsControlBase>().SaveSettings(prefs);
            if (saveMarkers) api.SaveMarkers(prefs);
            if (allowSaveMarkers3D && saveMarkers3D) api.GetComponent<OnlineMapsControlBase3D>().SaveMarkers3D(prefs);
            if (allowSaveLocationService && saveLocationService) api.GetComponent<OnlineMapsLocationService>().Save(prefs);

            OnlineMapsPrefs.Save(prefs.outerXml);

            ResetSaveSettings();
            dirty = true;
        }

        if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
        {
            ResetSaveSettings();
            dirty = true;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawSourceGUI()
    {
        EditorGUI.BeginDisabledGroup(isPlay);

        EditorGUILayout.PropertyField(pSource);

#if UNITY_WEBPLAYER || UNITY_WEBGL
        if (pSource.enumValueIndex != (int)OnlineMapsSource.Resources)
        {
            EditorGUILayout.PropertyField(pUseProxy, new GUIContent("Use Proxy"));
            EditorGUI.BeginDisabledGroup(!pUseProxy.boolValue);
            
            EditorGUILayout.PropertyField(pWebplayerProxyURL, new GUIContent("Proxy"));
            EditorGUI.EndDisabledGroup();
        }
#endif

        if (pSource.enumValueIndex != (int)OnlineMapsSource.Online)
        {
            if (GUILayout.Button("Fix Import Settings for Tiles")) FixImportSettings();
            if (GUILayout.Button("Import from GMapCatcher")) ImportFromGMapCatcher();
            
            EditorGUILayout.PropertyField(pResourcesPath);
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showResourcesTokens = Foldout(showResourcesTokens, "Available Tokens");
            if (showResourcesTokens)
            {
                GUILayout.Label("{zoom}");
                GUILayout.Label("{x}");
                GUILayout.Label("{y}");
                GUILayout.Label("{quad}");
                GUILayout.Space(10);
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUI.EndDisabledGroup();

        if (pSource.enumValueIndex != (int)OnlineMapsSource.Resources)
        {
            DrawProviderGUI();

            if (mapType.provider.types.Length > 1)
            {
                GUIContent[] availableTypes = mapType.provider.types.Select(t => new GUIContent(t.title)).ToArray();
                if (availableTypes != null)
                {
                    int index = mapType.index;
                    EditorGUI.BeginChangeCheck();
                    index = EditorGUILayout.Popup(new GUIContent("Type", "Type of map texture"), index, availableTypes);
                    if (EditorGUI.EndChangeCheck())
                    {
                        mapType = mapType.provider.types[index];
                        pMapType.stringValue = mapType.ToString();
                    }
                }
            }

            DrawLabelsGUI();
        }
    }

    private void DrawTargetGUI()
    {
        EditorGUI.BeginDisabledGroup(isPlay);
        EditorGUILayout.PropertyField(pTarget, new GUIContent("Target", "Where will be drawn map"));

        if (pTarget.enumValueIndex == (int)OnlineMapsTarget.texture)
        {
            EditorGUI.BeginChangeCheck();
            Object oldValue = pTexture.objectReferenceValue;
            EditorGUILayout.PropertyField(pTexture);
            if (EditorGUI.EndChangeCheck())
            {
                Texture2D texture = pTexture.objectReferenceValue as Texture2D;
                if (texture != null && (!Mathf.IsPowerOfTwo(texture.width) || !Mathf.IsPowerOfTwo(texture.height)))
                {
                    EditorUtility.DisplayDialog("Error", "Texture width and height must be power of two!!!", "OK");
                    pTexture.objectReferenceValue = oldValue;
                }
                else CheckAPITextureImporter(texture);
            }
        }
        else DrawTilesetPropsGUI();

        EditorGUI.EndDisabledGroup();
    }

    private void DrawTilesetPropsGUI()
    {
        EditorGUILayout.PropertyField(pTilesetWidth, cTilesetWidth);
        EditorGUILayout.PropertyField(pTilesetHeight, cTilesetHeight);
        EditorGUILayout.PropertyField(pTilesetSize, cTilesetSize);

        int dts = OnlineMapsUtils.tileSize * 2;
        if (pTilesetWidth.intValue % dts != 0) pTilesetWidth.intValue = Mathf.FloorToInt(pTilesetWidth.intValue / (float) dts + 0.5f) * dts;
        if (pTilesetHeight.intValue % dts != 0) pTilesetHeight.intValue = Mathf.FloorToInt(pTilesetHeight.intValue / (float) dts + 0.5f) * dts;

        if (pTilesetWidth.intValue <= 0) pTilesetWidth.intValue = dts;
        if (pTilesetHeight.intValue <= 0) pTilesetHeight.intValue = dts;
    }

    private void DrawToolbarGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (OnlineMapsUpdater.hasNewVersion && updateAvailableContent != null)
        {
            Color defBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1, 0.5f, 0.5f);
            if (GUILayout.Button(updateAvailableContent, EditorStyles.toolbarButton))
            {
                OnlineMapsUpdater.OpenWindow();
            }
            GUI.backgroundColor = defBackgroundColor;
        }
        else GUILayout.Label("");

        if (GUILayout.Button("Help", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Documentation"), false, OnViewDocs);
            menu.AddItem(new GUIContent("API Reference"), false, OnViewAPI);
            menu.AddItem(new GUIContent("Atlas of Examples"), false, OnViewAtlas);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Product Page"), false, OnProductPage);
            menu.AddItem(new GUIContent("Forum"), false, OnViewForum);
            menu.AddItem(new GUIContent("Check Updates"), false, OnCheckUpdates);
            menu.AddItem(new GUIContent("Support"), false, OnSendMail);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("About"), false, OnAbout);
            menu.ShowAsContext();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawTroubleshootingGUI(ref bool dirty)
    {
        EditorGUILayout.BeginHorizontal(GUI.skin.box);
        float oldWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 220;
        GUILayout.Label("Use this props only if you have a problem!!!", warningStyle);
        EditorGUILayout.EndHorizontal();
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(pUseSoftwareJPEGDecoder, cUseSoftwareJPEGDecoder);

#if !UNITY_WEBGL
        EditorGUILayout.PropertyField(pRenderInThread);
#endif

        EditorGUILayout.PropertyField(pNotInteractUnderGUI);
        EditorGUILayout.PropertyField(pStopPlayingWhenScriptsCompile);

        EditorGUIUtility.labelWidth = oldWidth;

        if (EditorGUI.EndChangeCheck()) dirty = true;
    }

    private static void FixImportSettings()
    {
        string resourcesFolder = Path.Combine(Application.dataPath, "Resources/OnlineMapsTiles");
        if (!Directory.Exists(resourcesFolder)) return;

        string[] tiles = Directory.GetFiles(resourcesFolder, "*.png", SearchOption.AllDirectories);
        float count = tiles.Length;
        int index = 0;
        foreach (string tile in tiles)
        {
            string shortPath = "Assets/" + tile.Substring(Application.dataPath.Length + 1);
            FixTileImporter(shortPath, index / count);
            index++;
        }

        EditorUtility.ClearProgressBar();
    }

    private static void FixTileImporter(string shortPath, float progress)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(shortPath) as TextureImporter;
        EditorUtility.DisplayProgressBar("Update import settings for tiles", "Please wait, this may take several minutes.", progress);
        if (textureImporter != null)
        {
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = true;
            textureImporter.textureFormat = TextureImporterFormat.RGB24;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.maxTextureSize = 256;
            AssetDatabase.ImportAsset(shortPath, ImportAssetOptions.ForceSynchronousImport);
        }
    }

    public static bool Foldout(bool value, string text)
    {
        return GUILayout.Toggle(value, text, EditorStyles.foldout);
    }

    public static Texture2D GetIcon(string iconName)
    {
        string[] path = Directory.GetFiles(Application.dataPath, iconName, SearchOption.AllDirectories);
        if (path.Length == 0) return null;
        string iconFile = "Assets" + path[0].Substring(Application.dataPath.Length).Replace('\\', '/');
        return AssetDatabase.LoadAssetAtPath(iconFile, typeof (Texture2D)) as Texture2D;
    }

    private void ImportFromGMapCatcher()
    {
        string folder = EditorUtility.OpenFolderPanel("Select GMapCatcher tiles folder", string.Empty, "");
        if (string.IsNullOrEmpty(folder)) return;

        string[] files = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories);
        if (files.Length == 0) return;

        const string resPath = "Assets/Resources/OnlineMapsTiles";

        bool needAsk = true;
        bool overwrite = false;
        foreach (string file in files)
        {
            if (!ImportTileFromGMapCatcher(file, folder, resPath, ref overwrite, ref needAsk)) break;
        }

        AssetDatabase.Refresh();
    }

    private static bool ImportTileFromGMapCatcher(string file, string folder, string resPath, ref bool overwrite, ref bool needAsk)
    {
        string shortPath = file.Substring(folder.Length + 1);
        shortPath = shortPath.Replace('\\', '/');
        string[] shortArr = shortPath.Split('/');
        int zoom = 17 - int.Parse(shortArr[0]);
        int x = int.Parse(shortArr[1]) * 1024 + int.Parse(shortArr[2]);
        int y = int.Parse(shortArr[3]) * 1024 + int.Parse(shortArr[4].Substring(0, shortArr[4].Length - 4));
        string dir = Path.Combine(resPath, string.Format("{0}/{1}", zoom, x));
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string destFileName = Path.Combine(dir, y + ".png");
        if (File.Exists(destFileName))
        {
            if (needAsk)
            {
                needAsk = false;
                int result = EditorUtility.DisplayDialogComplex("File already exists", "File already exists. Overwrite?", "Overwrite", "Skip", "Cancel");
                if (result == 0) overwrite = true;
                else if (result == 1)
                {
                    overwrite = false;
                    return true;
                }
                else return false;
            }

            if (!overwrite) return true;
        }
        File.Copy(file, destFileName, true);
        return true;
    }

    private void OnAbout()
    {
        OnlineMapsAboutWindow.OpenWindow();
    }

    private void OnCheckUpdates()
    {
        OnlineMapsUpdater.OpenWindow();
    }

    private void OnEnable()
    {
        CacheSerializedProperties();
        api = (OnlineMaps) target;

        providers = OnlineMapsProvider.GetProviders();
        providersTitle = providers.Select(p => p.title).ToArray();

        if (string.IsNullOrEmpty(pMapType.stringValue)) pMapType.stringValue = OnlineMapsProvider.Upgrade(pProvider.enumValueIndex, pType.intValue);

        if (pDefaultMarkerTexture.objectReferenceValue == null) pDefaultMarkerTexture.objectReferenceValue = GetIcon("DefaultMarker.png");
        if (pTooltipTexture.objectReferenceValue == null) pTooltipTexture.objectReferenceValue = GetIcon("Tooltip.psd");

        string[] files = Directory.GetFiles("Assets", "update_available.png", SearchOption.AllDirectories);
        if (files.Length > 0)
        {
            Texture updateAvailableIcon = AssetDatabase.LoadAssetAtPath(files[0], typeof (Texture)) as Texture;
            updateAvailableContent = new GUIContent("Update Available", updateAvailableIcon, "Update Available");
        }

        OnlineMapsUpdater.CheckNewVersionAvailable();

        mapType = OnlineMapsProvider.FindMapType(pMapType.stringValue);
        providerIndex = mapType.provider.index;

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        DrawToolbarGUI();

        serializedObject.Update();

        bool dirty = DrawGeneralGUI();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        showMarkers = Foldout(showMarkers, string.Format("2D Markers (Count: {0})", pMarkers.arraySize));
        if (showMarkers) DrawMarkersGUI(ref dirty);
        EditorGUILayout.EndVertical();

        if (pTarget.enumValueIndex == (int)OnlineMapsTarget.texture)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showCreateTexture = Foldout(showCreateTexture, "Create texture");
            if (showCreateTexture) DrawCreateTextureGUI(ref dirty);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical(GUI.skin.box);
        showAdvanced = Foldout(showAdvanced, "Advanced");
        if (showAdvanced) DrawAdvancedGUI();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        showTroubleshooting = Foldout(showTroubleshooting, "Troubleshooting");
        if (showTroubleshooting) DrawTroubleshootingGUI(ref dirty);
        EditorGUILayout.EndVertical();

        CheckNullControl();
#if UNITY_WEBPLAYER
        CheckJSLoader();
#endif

        serializedObject.ApplyModifiedProperties();

        if (dirty)
        {
            EditorUtility.SetDirty(api);
            if (!Application.isPlaying)
            {
#if UNITY_5_3P
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif
            }
            else api.Redraw();
        }
    }

    private void OnProductPage()
    {
        Process.Start("http://infinity-code.com/en/products/online-maps");
    }

    private void OnSendMail()
    {
        Process.Start("mailto:support@infinity-code.com?subject=Online maps");
    }

    private void OnViewAPI()
    {
        Process.Start("http://infinity-code.com/en/docs/api/online-maps");
    }

    private void OnViewAtlas()
    {
        Process.Start("http://infinity-code.com/atlas/online-maps");
    }

    private void OnViewDocs()
    {
        Process.Start("http://infinity-code.com/en/docs/online-maps");
    }

    private void OnViewForum()
    {
        Process.Start("http://forum.infinity-code.com");
    }

    private void ResetSaveSettings()
    {
        showSave = false;

        saveControl = true;
        saveLocationService = true;
        saveMarkers = true;
        saveMarkers3D = true;
        saveSettings = true;
        saveTexture = true;
    }
}