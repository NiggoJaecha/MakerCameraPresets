using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BepInEx;
using KKAPI;
using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;
using BepInEx.Logging;
using BepInEx.Configuration;
using LitJson;
using UniRx;

namespace MakerCameraPresets
{
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class MakerCameraPresets :BaseUnityPlugin
    {
        public const string PluginName = "KKS_MakerCameraPresets";
        public const string GUID = "org.njaecha.plugins.makercamerapresets";
        public const string Version = "1.0.0";

        internal static new ManualLogSource Logger;

        private GameObject cameraObject;
        private CameraControl_Ver2 camCtrl;
        private bool isInit = false;

        private SidebarToggle sidebarToggle;
        private bool UIactive;
        private Vector2 UIcorner;
        private bool removeMode = false;
        private Texture2D buttonTx = new Texture2D(140, 35);
        private Texture2D buttonTxHover = new Texture2D(140, 35);
        private string oldUITheme;

        private PluginData presetData = new PluginData{ cameras = new List<CameraData>()};

        private ConfigEntry<string> UITheme;
        private ConfigEntry<bool> smallerButtons;

        //hotkeys
        private ConfigEntry<KeyboardShortcut> hotkey1;
        private ConfigEntry<KeyboardShortcut> hotkey2;
        private ConfigEntry<KeyboardShortcut> hotkey3;
        private ConfigEntry<KeyboardShortcut> hotkey4;
        private ConfigEntry<KeyboardShortcut> hotkey5;
        private ConfigEntry<KeyboardShortcut> hotkey6;
        private ConfigEntry<KeyboardShortcut> hotkey7;
        private ConfigEntry<KeyboardShortcut> hotkey8;
        private ConfigEntry<KeyboardShortcut> hotkey9;
        private ConfigEntry<KeyboardShortcut> hotkey10;
        private ConfigEntry<KeyboardShortcut> hotkey11;
        private ConfigEntry<KeyboardShortcut> hotkey12;
        private ConfigEntry<KeyboardShortcut> hotkey13;
        private ConfigEntry<KeyboardShortcut> hotkey14;
        private ConfigEntry<KeyboardShortcut> hotkey15;
        private ConfigEntry<KeyboardShortcut> hotkey16;
        private ConfigEntry<KeyboardShortcut> hotkey17;
        private ConfigEntry<KeyboardShortcut> hotkey18;
        private ConfigEntry<KeyboardShortcut> hotkey19;
        private ConfigEntry<KeyboardShortcut> hotkey20;

        private void Awake()
        {
            Logger = base.Logger;
            MakerAPI.MakerBaseLoaded += init;
            MakerAPI.MakerFinishedLoading += getUIcorner;

            AcceptableValueBase themes = new AcceptableValueList<string>("Sunshine", "Koikatsu", "Koikatsu Dark", "Unity");
            UITheme = Config.Bind("UI", "Button Theme", "Sunshine", new ConfigDescription("Theme to use for the UI", themes));
            oldUITheme = UITheme.Value;
            smallerButtons = Config.Bind("UI", "Smaller Buttons", false, "Reduces the size of the buttons, recomended for smaller screens (FullHD and below)");

            //hotkeys
            hotkey1 = Config.Bind("Keybinds", "Hotkey Camera 1", new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftControl), "Press this to switch to camera preset 1 (if existing)");
            hotkey2 = Config.Bind("Keybinds", "Hotkey Camera 2", new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftControl), "Press this to switch to camera preset 2 (if existing)");
            hotkey3 = Config.Bind("Keybinds", "Hotkey Camera 3", new KeyboardShortcut(KeyCode.Alpha3, KeyCode.LeftControl), "Press this to switch to camera preset 3 (if existing)");
            hotkey4 = Config.Bind("Keybinds", "Hotkey Camera 4", new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl), "Press this to switch to camera preset 4 (if existing)");
            hotkey5 = Config.Bind("Keybinds", "Hotkey Camera 5", new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl), "Press this to switch to camera preset 5 (if existing)");
            hotkey6 = Config.Bind("Keybinds", "Hotkey Camera 6", new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl), "Press this to switch to camera preset 6 (if existing)");
            hotkey7 = Config.Bind("Keybinds", "Hotkey Camera 7", new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl), "Press this to switch to camera preset 7 (if existing)");
            hotkey8 = Config.Bind("Keybinds", "Hotkey Camera 8", new KeyboardShortcut(KeyCode.Alpha8, KeyCode.LeftControl), "Press this to switch to camera preset 8 (if existing)");
            hotkey9 = Config.Bind("Keybinds", "Hotkey Camera 9", new KeyboardShortcut(KeyCode.Alpha9, KeyCode.LeftControl), "Press this to switch to camera preset 9 (if existing)");
            hotkey10 = Config.Bind("Keybinds", "Hotkey Camera 10", new KeyboardShortcut(KeyCode.Alpha0, KeyCode.LeftControl), "Press this to switch to camera preset 10 (if existing)");
            hotkey11 = Config.Bind("Keybinds", "Hotkey Camera 11", new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftControl, KeyCode.LeftShift), "Press this to switch to camera preset 11 (if existing)");
            hotkey12 = Config.Bind("Keybinds", "Hotkey Camera 12", new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftControl, KeyCode.LeftShift), "Press this to switch to camera preset 12 (if existing)");
            hotkey13 = Config.Bind("Keybinds", "Hotkey Camera 13", new KeyboardShortcut(KeyCode.Alpha3, KeyCode.LeftControl, KeyCode.LeftShift), "Press this to switch to camera preset 13 (if existing)");
            hotkey14 = Config.Bind("Keybinds", "Hotkey Camera 14", new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl, KeyCode.LeftShift), "Press this to switch to camera preset 14 (if existing)");
            hotkey15 = Config.Bind("Keybinds", "Hotkey Camera 15", new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl, KeyCode.LeftShift), "Press this to switch to camera preset 15 (if existing)");
            hotkey16 = Config.Bind("Keybinds", "Hotkey Camera 16", new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl, KeyCode.LeftShift), "Press this to switch to camera preset 16 (if existing)");
            hotkey17 = Config.Bind("Keybinds", "Hotkey Camera 17", new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl, KeyCode.LeftShift), "Press this to switch to camera preset 17 (if existing)");
            hotkey18 = Config.Bind("Keybinds", "Hotkey Camera 18", new KeyboardShortcut(KeyCode.Alpha8, KeyCode.LeftControl, KeyCode.LeftShift), "Press this to switch to camera preset 18 (if existing)");
            hotkey19 = Config.Bind("Keybinds", "Hotkey Camera 19", new KeyboardShortcut(KeyCode.Alpha9, KeyCode.LeftControl, KeyCode.LeftShift), "Press this to switch to camera preset 19 (if existing)");
            hotkey20 = Config.Bind("Keybinds", "Hotkey Camera 20", new KeyboardShortcut(KeyCode.Alpha0, KeyCode.LeftControl, KeyCode.LeftShift), "Press this to switch to camera preset 20 (if existing)");

        }
        private void getUIcorner(object sender, EventArgs e)
        {
            Vector3[] corners = new Vector3[4];
            RectTransform scrollviewConent = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CvsDraw/Top/Scroll View").GetComponent<RectTransform>();
            scrollviewConent.GetWorldCorners(corners);
            UIcorner = corners[0];
        }

        private void loadTextures()
        {
            int themeID = 0;
            if (UITheme.Value == "Koikatsu") themeID = 1;
            else if (UITheme.Value == "Koikatsu Dark") themeID = 2;

            ImageConversion.LoadImage(buttonTx, File.ReadAllBytes($"{UserData.Path}/MakerCameraPresets/buttons/buttontexture{themeID}.png"));
            ImageConversion.LoadImage(buttonTxHover, File.ReadAllBytes($"{UserData.Path}/MakerCameraPresets/buttons/buttontexture{themeID}hov.png"));
            oldUITheme = UITheme.Value;
        }

        private void init(object sender, RegisterCustomControlsEvent e)
        {
            cameraObject = GameObject.Find("CustomScene/CamBase/Camera");
            if (cameraObject == null) return;
            camCtrl = cameraObject.GetComponent<CameraControl_Ver2>();
            if (camCtrl == null) return;

            sidebarToggle = e.AddSidebarControl(new SidebarToggle("Camera Presets UI", true, this));
            sidebarToggle.ValueChanged.Subscribe(delegate (bool b) { toggleUI(b); });

            loadTextures();

            isInit = true;
            loadPresetData();
        }

        public void toggleUI(bool toggle)
        {
            UIactive = toggle;
        }

        // borrowed from ClothingStateMenu
        private bool CanShow()
        {
            if (MakerAPI.InsideMaker && !MakerAPI.IsInterfaceVisible()) return false;

            if (SceneApi.GetAddSceneName() == "Config") return false;
            if (SceneApi.GetIsOverlap()) return false;
            if (SceneApi.GetIsNowLoadingFade()) return false;

            return true;
        }

        void OnGUI()
        {
            if (!isInit) return;

            if (!UIactive || !CanShow()) return;

            int baseX;
            int baseY;
            if (!smallerButtons.Value)
            {
                baseX = (int)UIcorner.x - 145;
                baseY = Screen.height - (int)UIcorner.y - 80 - 40 * presetData.cameras.Count;
            }
            else
            {
                baseX = (int)UIcorner.x - 85;
                baseY = Screen.height - (int)UIcorner.y - 50 - 25 * presetData.cameras.Count;
            }

            if (oldUITheme != UITheme.Value) loadTextures();

            if(UITheme.Value != "Unity")
            {
                GUIStyle style1 = new GUIStyle();
                style1.fontStyle = FontStyle.Bold;
                style1.fontSize = smallerButtons.Value ? 10 : 20;
                style1.normal.textColor = Color.white;
                style1.normal.background = buttonTx;
                style1.hover.textColor = Color.yellow;
                style1.hover.background = buttonTxHover;
                style1.onHover.textColor = Color.yellow;
                style1.alignment = TextAnchor.MiddleCenter;
                GUIStyle style2 = new GUIStyle(style1);
                style2.normal.textColor = Color.red;
                style2.hover.textColor = Color.yellow;
                GUIStyle style3 = new GUIStyle(style1);
                style3.normal.textColor = removeMode ? Color.blue : Color.white;
                style3.hover.textColor = Color.yellow;
            

                int y = baseY;
                for (int i = 0; i < presetData.cameras.Count; i++)
                {
                    CameraData camData = presetData.cameras[i];
                    if (GUI.Button(new Rect(baseX, y, removeMode ? (smallerButtons.Value ? 60 : 105) : (smallerButtons.Value ? 80 : 140), smallerButtons.Value ? 20 : 35), $"Camera {i + 1}", style1))
                    {
                        loadCamData(presetData.cameras[i]);
                    }
                    if (removeMode)
                    {
                        if (GUI.Button(new Rect(baseX+ (smallerButtons.Value ? 60 : 105), y, smallerButtons.Value ? 20 : 35, smallerButtons.Value ? 20 : 35), "X", style2))
                        {
                            removeCam(i);
                        }
                    }

                    y += smallerButtons.Value ? 25 : 40;
                }
                if (GUI.Button( new Rect(baseX, y, smallerButtons.Value ? 80 : 140, smallerButtons.Value ? 20 : 35), "Save Camera", style1))
                {
                    addCam();
                }
                y += smallerButtons.Value ? 25 : 40;
                if (GUI.Button(new Rect(baseX, y, smallerButtons.Value ? 80 : 140, smallerButtons.Value ? 20 : 35), removeMode ? "☑️ Remove": "☐ Remove", style3))
                {
                    removeMode = !removeMode;
                }
            }
            else
            {
                int y = baseY;
                for (int i = 0; i < presetData.cameras.Count; i++)
                {
                    CameraData camData = presetData.cameras[i];
                    if (GUI.Button(new Rect(baseX, y, removeMode ? (smallerButtons.Value ? 60 : 105) : (smallerButtons.Value ? 80 : 140), smallerButtons.Value ? 20 : 35), $"Camera {i + 1}"))
                    {
                        loadCamData(presetData.cameras[i]);
                    }
                    if (removeMode)
                    {
                        if (GUI.Button(new Rect(baseX + (smallerButtons.Value ? 60 : 105), y, smallerButtons.Value ? 20 : 35, smallerButtons.Value ? 20 : 35), "X"))
                        {
                            removeCam(i);
                        }
                    }

                    y += smallerButtons.Value ? 25 : 40;
                }
                if (GUI.Button(new Rect(baseX, y, smallerButtons.Value ? 80 : 140, smallerButtons.Value ? 20 : 35), "Save Camera"))
                {
                    addCam();
                }
                y += smallerButtons.Value ? 25 : 40;
                if (GUI.Button(new Rect(baseX, y, smallerButtons.Value ? 80 : 140, smallerButtons.Value ? 20 : 35), removeMode ? "☑️ Remove" : "☐ Remove"))
                {
                    removeMode = !removeMode;
                }
            }

        }

        public void loadCam(int index)
        {
            if (presetData.cameras.Count > index)
            {
                loadCamData(presetData.cameras[index]);
            }
        }

        public void loadCamData(CameraData data)
        {
            camCtrl.TargetPos = data.getPosition();
            camCtrl.CameraAngle = data.getAngle();
            camCtrl.CameraDir = data.getDirection();
            camCtrl.CameraFov = data.fov;
        }

        public void removeCam(int index)
        {
            presetData.cameras.RemoveAt(index);
            savePresetData();
        }

        public void addCam()
        {
            presetData.cameras.Add(new CameraData(camCtrl.TargetPos, camCtrl.CameraAngle, camCtrl.CameraDir, camCtrl.CameraFov));
            savePresetData();
        }

        private void logPresetData()
        {
            foreach (CameraData camData in presetData.cameras)
            {
                Logger.LogInfo($"Position: {camData.getPosition()}");
                Logger.LogInfo($"Angle: {camData.getAngle()}");
                Logger.LogInfo($"Direction: {camData.getDirection()}");
                Logger.LogInfo($"FOV: {camData.fov}");
            }
        }

        private void loadPresetData()
        {
            string textAsset = File.ReadAllText($"{UserData.Path}/MakerCameraPresets/CameraPresets.json");
            presetData =  JsonMapper.ToObject<PluginData>(textAsset);
        }

        private void savePresetData()
        {
            StringBuilder builder = new StringBuilder();
            JsonWriter writer = new JsonWriter(builder);
            writer.PrettyPrint = true;
            JsonMapper.ToJson(presetData, writer);
            File.WriteAllText($"{UserData.Path}/MakerCameraPresets/CameraPresets.json", builder.ToString());
        }

        void Update()
        {
            if (hotkey1.Value.IsDown())
            {
                loadCam(0);
            }
            if (hotkey2.Value.IsDown())
            {
                loadCam(1);
            }
            if (hotkey3.Value.IsDown())
            {
                loadCam(2);
            }
            if (hotkey4.Value.IsDown())
            {
                loadCam(3);
            }
            if (hotkey5.Value.IsDown())
            {
                loadCam(4);
            }
            if (hotkey6.Value.IsDown())
            {
                loadCam(5);
            }
            if (hotkey7.Value.IsDown())
            {
                loadCam(6);
            }
            if (hotkey8.Value.IsDown())
            {
                loadCam(7);
            }
            if (hotkey9.Value.IsDown())
            {
                loadCam(8);
            }
            if (hotkey10.Value.IsDown())
            {
                loadCam(9);
            }
            if (hotkey11.Value.IsDown())
            {
                loadCam(10);
            }
            if (hotkey12.Value.IsDown())
            {
                loadCam(11);
            }
            if (hotkey13.Value.IsDown())
            {
                loadCam(12);
            }
            if (hotkey14.Value.IsDown())
            {
                loadCam(13);
            }
            if (hotkey15.Value.IsDown())
            {
                loadCam(14);
            }
            if (hotkey16.Value.IsDown())
            {
                loadCam(15);
            }
            if (hotkey17.Value.IsDown())
            {
                loadCam(16);
            }
            if (hotkey18.Value.IsDown())
            {
                loadCam(17);
            }
            if (hotkey19.Value.IsDown())
            {
                loadCam(18);
            }
            if (hotkey20.Value.IsDown())
            {
                loadCam(19);
            }
        }
    }
}
