using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using MFPSEditor;
using MFPSEditor.Addons;
using UnityEditor.IMGUI.Controls;


public class bl_MFPSManagerWindow : EditorWindow
{
    public static Color primaryColor = new Color(0.098f, 0.098f, 0.098f, 1.00f);
    public static Color altColor = new Color(0.07f, 0.07f, 0.07f, 1.00f);
    public static Color hightlightColor = new Color(0.1176471f, 0.7523355f, 1f, 1.00f);
    public static Color whiteColor = new Color(0.754717f, 0.754717f, 0.754717f, 1.00f);

    private ManagerPanel[] managerPanels = new ManagerPanel[]
    {
    new ManagerPanel(){Name = "Game Data", Identifier = PanelManagerType.GameData},
    new ManagerPanel(){Name = "ULogin Pro", Identifier = PanelManagerType.ULoginPro},
    new ManagerPanel(){Name = "Player Selector", Identifier = PanelManagerType.PlayerSelector},
    new ManagerPanel(){Name = "Shop", Identifier = PanelManagerType.Shop},
    new ManagerPanel(){Name = "Class Customization", Identifier = PanelManagerType.ClassCustomization},
    new ManagerPanel(){Name = "Customizer", Identifier = PanelManagerType.Customizer},
     new ManagerPanel(){Name = "Vehicles", Identifier = PanelManagerType.Vehicle},
    new ManagerPanel(){Name = "Localization", Identifier = PanelManagerType.Localization},
    new ManagerPanel(){Name = "Level Manager", Identifier = PanelManagerType.LevelManager},
    new ManagerPanel(){Name = "Input Manager", Identifier = PanelManagerType.InputManager},
    new ManagerPanel(){Name = "Kill Streaks", Identifier = PanelManagerType.KillStreaks},
    new ManagerPanel(){Name = "Third Person", Identifier = PanelManagerType.ThirdPerson},
    new ManagerPanel(){Name = "Minimap", Identifier = PanelManagerType.MiniMap},
    new ManagerPanel(){Name = "Clan System", Identifier = PanelManagerType.Clan},
    new ManagerPanel(){Name = "Floating Text", Identifier = PanelManagerType.FloatingText},
    new ManagerPanel(){Name = "Emblems and Cards", Identifier = PanelManagerType.Emblems},
    new ManagerPanel(){Name = "Layout Customizer", Identifier = PanelManagerType.LayoutCustomizer},
    };
    private WindowType currentWindow = WindowType.Home;
    private PanelManagerType currentPanelType = PanelManagerType.None;
    public Dictionary<string, GUIStyle> styles = new Dictionary<string, GUIStyle>() { { "panelButton", null }, { "titleH2", null }, { "borders", null }, { "textC", null } };
    public bool m_initGUI = false;
    private bl_GameData gameData;
    private Vector2 bodyScroll = Vector2.zero;
    private Texture2D mfpsLogo, soldierIcon;
    SearchField weaponSearchfield;
    private string weaponSearchKey = "";
    readonly string[] slotsNames = new string[] { "Assault", "Recon", "Support", "Engineer" };
    public int currentSlot, currentSlot2 = 0;
    private bl_PlayerClassLoadout selectedLoadout = null;
    private int selectedSlot = -1;
    private Vector2 weaponScroll, managersScroll = Vector2.zero;
    private PlayerClass currentPlayerClass = PlayerClass.Assault;
    private Dictionary<ScriptableObject, Editor> cachedEditors = new Dictionary<ScriptableObject, Editor>();

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        titleContent = new GUIContent(" MFPS", GetUnityIcon("Audio Mixer"));
        mfpsLogo = Resources.Load("content/Images/mfps-name",typeof(Texture2D)) as Texture2D;
        soldierIcon = AssetDatabase.LoadAssetAtPath("Assets/MFPS/Content/Art/UI/Icons/mfps-soldier.png", typeof(Texture2D)) as Texture2D;
        minSize = new Vector2(720, 570);
        weaponSearchfield = new SearchField();
        LovattoStats.SetStat("mfps-manager-window", 1);
        currentPlayerClass = currentPlayerClass.GetSavePlayerClass();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        var cColor = GUI.contentColor;
        GUI.contentColor = whiteColor;
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), altColor);
        if (!m_initGUI || styles["panelButton"] == null) InitGUI();

        GUILayout.BeginHorizontal();
        LeftPanel();
        EditorGUILayout.BeginVertical();
        Header();

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginVertical();
        DrawBody();
        EditorGUILayout.EndVertical();
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUI.contentColor = cColor;

    }

    /// <summary>
    /// 
    /// </summary>
    void Header()
    {
        Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Height(60));
        EditorGUI.DrawRect(r, primaryColor);
        GUILayout.FlexibleSpace();
        DrawWindowButtons();
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawWindowButtons()
    {
        WindowButton("MANAGERS", WindowType.Managers);
        WindowButton("WEAPONS", WindowType.Weapons);
        WindowButton("LOADOUTS", WindowType.Loadouts);

#if LM
        WindowButton("LEVELS", WindowType.Levels);
#endif

        GUILayout.Space(25);
    }

    void WindowButton(string title, WindowType windowType)
    {
        Rect r = GUILayoutUtility.GetRect(new GUIContent(title), styles["textC"], GUILayout.Height(40), GUILayout.Width(120));
        r.y += 20;
        EditorGUI.DrawRect(r, altColor);
        if (GUI.Button(r, title, styles["textC"]))
        {
            currentWindow = windowType;
        }
        if(currentWindow == windowType)
        {
            r.y += r.height;
            r.height = 1;
            EditorGUI.DrawRect(r, hightlightColor);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void LeftPanel()
    {
        Rect r = EditorGUILayout.BeginVertical(GUILayout.Width(150));
        EditorGUI.DrawRect(r, primaryColor);
        GUILayout.Label(mfpsLogo, GUILayout.Height(58), GUILayout.Width(150));
        DrawBottomLine(0.2f);
        if(currentWindow == WindowType.Managers || currentWindow == WindowType.Home)
        DrawManagerButtons();
        if (currentWindow == WindowType.Weapons) DrawWeaponPanel();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawBody()
    {
        GUILayout.Space(10);
        bodyScroll = GUILayout.BeginScrollView(bodyScroll, false, false);
        if (currentWindow == WindowType.Managers) DrawManagersBody();
        else if (currentWindow == WindowType.Weapons) DrawWeaponsBody();
        else if (currentWindow == WindowType.Loadouts) DrawLoadoutsBody();
        else if (currentWindow == WindowType.Home) DrawHomeBody();
#if LM
        else if (currentWindow == WindowType.Levels) LevelManagerWindowEditor.DrawLevels();
#endif
        GUILayout.FlexibleSpace();
        GUILayout.Space(100);
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawHomeBody()
    {
        GUILayout.Label("SELECT THE GAME SETTING WINDOW", styles["textC"]);
        GUILayout.Space(20);
        Rect r;
        for (int i = 0; i < managerPanels.Length; i++)
        {
            r = GUILayoutUtility.GetRect(new GUIContent(managerPanels[i].Name), styles["panelButton"], GUILayout.Height(30));
            if (currentPanelType == managerPanels[i].Identifier)
            {
               // EditorGUI.DrawRect(r, altColor);
                float w = r.width;
                r.width = 1;
                EditorGUI.DrawRect(r, hightlightColor);
                r.width = w;
            }
            else
            {
                EditorGUI.DrawRect(r, primaryColor);
            }
            if (GUI.Button(r, managerPanels[i].Name, styles["panelButton"]))
            {
                bodyScroll = Vector2.zero;
                currentPanelType = managerPanels[i].Identifier;
                currentWindow = WindowType.Managers;
            }
            GUILayout.Space(4);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawManagersBody()
    {
        switch (currentPanelType)
        {
            case PanelManagerType.GameData:
                DrawGameDataSettings();
                break;
            case PanelManagerType.PlayerSelector:
                DrawPlayerSelector();
                break;
            case PanelManagerType.Customizer:
                DrawCustomizer();
                break;
            case PanelManagerType.ClassCustomization:
                DrawClassCustomizer();
                break;
            case PanelManagerType.ULoginPro:
                DrawULogin();
                break;
            case PanelManagerType.Localization:
                DrawLocalization();
                break;
            case PanelManagerType.LevelManager:
                DrawlevelManager();
                break;
            case PanelManagerType.InputManager:
                DrawInputManager();
                break;
            case PanelManagerType.KillStreaks:
                DrawKillStreaks();
                break;
            case PanelManagerType.Shop:
                DrawShop();
                break;
            case PanelManagerType.ThirdPerson:
                DrawThirdPerson();
                break;
            case PanelManagerType.MiniMap:
                DrawMinimap();
                break;
            case PanelManagerType.Clan:
                DrawClan();
                break;
            case PanelManagerType.FloatingText:
                DrawFloatingText();
                break;
            case PanelManagerType.Emblems:
                DrawEmblems();
                break;
            case PanelManagerType.LayoutCustomizer:
                DrawLayoutCustomizer();
                break;
            case PanelManagerType.Vehicle:
                DrawVehicles();
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawWeaponsBody()
    {
        GUILayout.Space(10);
        float lw = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 75;
        var sls = GUI.skin.horizontalSlider;
        GUI.skin.horizontalSlider = MFPSEditorStyles.EditorSkin.customStyles[9];
        var slt = GUI.skin.horizontalSliderThumb;
        GUI.skin.horizontalSliderThumb = MFPSEditorStyles.EditorSkin.customStyles[10];

        var all = bl_GameData.Instance.AllWeapons;
        int rowID = 0;
        int SpaceBetweenRow = 20;
        int rowCapacity = Mathf.FloorToInt((position.width - 150) / (370 + SpaceBetweenRow));
        for (int i = 0; i < all.Count; i++)
        {
            if (!string.IsNullOrEmpty(weaponSearchKey))
            {
                if (all[i].Name.ToLower().Contains(weaponSearchKey))
                {
                    WeaponCard(all[i]);
                    GUILayout.Space(SpaceBetweenRow);
                    continue;
                }
                else continue;
            }

            if (rowID == 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }
            WeaponCard(all[i]);
            GUILayout.Space(10);
            if (rowID == (rowCapacity - 1) || i == all.Count - 1)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(SpaceBetweenRow);
            }
            rowID = (rowID + 1) % rowCapacity;
        }

        EditorGUIUtility.labelWidth = lw;
        GUI.skin.horizontalSlider = sls;
        GUI.skin.horizontalSliderThumb = slt;
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawLoadoutsBody()
    {
        GUILayout.Space(10);
#if !CLASS_CUSTOMIZER
        var player = bl_GameData.Instance.Player1.PlayerReferences;
        DrawPlayerLoadout(player,ref currentSlot);
        GUILayout.Space(15);
        player = bl_GameData.Instance.Player2.PlayerReferences;
        DrawPlayerLoadout(player,ref currentSlot2);
#else
        GUILayout.Label("<b>Class Customizer is enabled</b>\n<b><size=8><i>The player loadout is managed for a global loadouts that can be change in game instead of a fixed loadout per player prefab (MFPS default)</i></size></b>\n", styles["textC"]);
        GUILayout.Space(10);
        Rect r = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        {
            EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
            //Slot buttons
            using (new GUILayout.HorizontalScope(GUILayout.Height(30)))
            {
                GUILayout.Space(5);
                for (int i = 0; i < slotsNames.Length; i++)
                {
                    Rect br = GUILayoutUtility.GetRect(new GUIContent(slotsNames[i]), styles["textC"], GUILayout.Height(30));
                    if (i != currentSlot)
                        EditorGUI.DrawRect(br, primaryColor);
                    else
                    {
                        EditorGUI.DrawRect(br, altColor);
                        GUI.color = new Color(1, 1, 1, 0.2f);
                        GUI.Box(br, GUIContent.none, MFPSEditorStyles.OutlineButtonStyle);
                        GUI.color = Color.white;
                    }
                    if (GUI.Button(br, slotsNames[i], styles["textC"]))
                    {
                        if (currentSlot != i)
                        {
                            selectedLoadout = null;
                            selectedSlot = -1;
                        }
                        currentSlot = i;
                    }
                }
                GUILayout.Space(5);
            }
            GUILayout.Space(20);

            var loadout = bl_ClassManager.Instance.DefaultAssaultClass;
            if (currentSlot == 1) loadout = bl_ClassManager.Instance.DefaultReconClass;
            if (currentSlot == 2) loadout = bl_ClassManager.Instance.DefaultSupportClass;
            if (currentSlot == 3) loadout = bl_ClassManager.Instance.DefaultEngineerClass;
            //loadout
            DrawSlots(loadout);

        }
        EditorGUILayout.EndVertical();
#endif
        GUILayout.Space(10);
        var latpc = currentPlayerClass;
        currentPlayerClass = (PlayerClass)EditorGUILayout.EnumPopup("Current Player Class", currentPlayerClass);
        if(currentPlayerClass != latpc)
        {
            currentPlayerClass.SavePlayerClass();
        }
    }

    void DrawPlayerLoadout(bl_PlayerReferences player, ref int slotID)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(256));
        {
            GUILayout.Space(10);
            //soldier icon
            Rect r = EditorGUILayout.BeginVertical(styles["borders"], GUILayout.Width(200));
            {
                EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(soldierIcon, GUILayout.Height(200), GUILayout.Width(100));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Label(player.name,styles["textC"]);
            }
            EditorGUILayout.EndVertical();

            r = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
                //Slot buttons
                using (new GUILayout.HorizontalScope(GUILayout.Height(30)))
                {
                    GUILayout.Space(5);
                    for (int i = 0; i < slotsNames.Length; i++)
                    {
                        Rect br = GUILayoutUtility.GetRect(new GUIContent(slotsNames[i]), styles["textC"], GUILayout.Height(30));
                        if (i != slotID)
                            EditorGUI.DrawRect(br, primaryColor);
                        else
                        {
                            EditorGUI.DrawRect(br, altColor);
                            GUI.color = new Color(1, 1, 1, 0.2f);
                            GUI.Box(br, GUIContent.none, MFPSEditorStyles.OutlineButtonStyle);
                            GUI.color = Color.white;
                        }
                        if (GUI.Button(br, slotsNames[i], styles["textC"]))
                        {
                            if (slotID != i)
                            {
                                selectedLoadout = null;
                                selectedSlot = -1;
                            }
                            slotID = i;
                        }
                    }
                    GUILayout.Space(5);
                }
                GUILayout.Space(20);

                var loadout = player.gunManager.m_AssaultClass;
                if (slotID == 1) loadout = player.gunManager.m_ReconClass;
                if (slotID == 2) loadout = player.gunManager.m_SupportClass;
                if (slotID == 3) loadout = player.gunManager.m_EngineerClass;
                //loadout
                DrawSlots(loadout);

            }
            EditorGUILayout.EndVertical();

        }
        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
    }

    void DrawSlots(bl_PlayerClassLoadout loadout)
    {
        if (loadout == null) return;

        if(selectedLoadout != null && selectedLoadout == loadout)
        {
            DrawWeaponSelectionList();
            return;
        }

        var gun = loadout.GetPrimaryGunInfo();
        DrawSlot(gun, "PRIMARY", loadout, 0);
        GUILayout.Space(10);
        gun = loadout.GetSecondaryGunInfo();
        DrawSlot(gun, "SECONDARY", loadout, 1);
        GUILayout.Space(10);
        gun = loadout.GetPerksGunInfo();
        DrawSlot(gun, "PERK", loadout, 2);
        GUILayout.Space(10);
        gun = loadout.GetLetalGunInfo();
        DrawSlot(gun, "LETAL", loadout, 3);
        GUILayout.Space(10);
    }

    void DrawSlot(bl_GunInfo gun, string Title, bl_PlayerClassLoadout loadout, int slotID)
    {
        if (gun == null) return;

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(25);
            GUILayout.Label($"<b>{Title}:</b>", styles["textC"], GUILayout.Width(100));
            GUILayout.Label(gun.Name, styles["textC"], GUILayout.Width(150));
            if (gun.GunIcon != null)
                GUILayout.Label(gun.GunIcon.texture, GUILayout.Height(22),GUILayout.Width(100));

            Rect br = GUILayoutUtility.GetRect(new GUIContent("CHANGE"), styles["textC"], GUILayout.Width(100));
            EditorGUI.DrawRect(br, altColor);
            if(GUI.Button(br,"CHANGE", styles["textC"]))
            {
                selectedSlot = slotID;
                selectedLoadout = loadout;
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawWeaponSelectionList()
    {
        var all = bl_MFPS.AllWeapons;
        weaponScroll = GUILayout.BeginScrollView(weaponScroll);
        GUILayout.Space(10);
        Color c = new Color(0, 0, 0, 0.1f);
        for (int i = 0; i < all.Count; i++)
        {
            Rect r = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(r, c);
            GUILayout.Space(20);
            GUILayout.Label(all[i].Name, styles["textC"], GUILayout.Width(120), GUILayout.Height(20));
            GUILayout.Space(25);
            if (all[i].GunIcon != null)
                GUILayout.Label(all[i].GunIcon.texture, GUILayout.Height(22), GUILayout.Width(100));

            GUILayout.Space(25);
            Rect br = GUILayoutUtility.GetRect(new GUIContent("SELECT"), styles["textC"], GUILayout.Width(100));
            EditorGUI.DrawRect(br, altColor);
            if (GUI.Button(br, "SELECT", styles["textC"]))
            {
                if (selectedSlot == 0) selectedLoadout.Primary = i;
                else if (selectedSlot == 1) selectedLoadout.Secondary = i;
                else if (selectedSlot == 2) selectedLoadout.Perks = i;
                else if (selectedSlot == 3) selectedLoadout.Letal = i;

                selectedSlot = -1;
                selectedLoadout = null;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
        }
        GUILayout.EndScrollView();
    }

    void WeaponCard(bl_GunInfo gun)
    {
        Rect rect = EditorGUILayout.BeginVertical(styles["borders"], GUILayout.Width(370), GUILayout.Height(90));
        {
            EditorGUI.DrawRect(rect, new Color(1, 1, 1, 0.02f));

            EditorGUILayout.BeginHorizontal();
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(5);
                    GUILayout.Label($"<size=14>{gun.Name}</size>", styles["titleH2"], GUILayout.Height(18));
                    GUILayout.Label($"<size=9><color=#727272>{gun.Type.ToString()}</color></size>", styles["titleH2"], GUILayout.Height(18));
                    GUILayout.FlexibleSpace();
                    Rect iconR = GUILayoutUtility.GetRect(120, 70);
                    GUI.DrawTexture(iconR, gun.GunIcon.texture, ScaleMode.ScaleToFit);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            {
                gun.Damage = EditorGUILayout.IntSlider("DAMAGE", gun.Damage, 1, 100);
                gun.FireRate = EditorGUILayout.Slider("FIRE RATE", gun.FireRate, 0.01f, 2f);
                gun.Accuracy = EditorGUILayout.IntSlider("ACCURACY", gun.Accuracy, 1, 5);
                gun.ReloadTime = EditorGUILayout.Slider("RELOAD", gun.ReloadTime, 0.5f, 10);
                gun.Range = EditorGUILayout.IntSlider("RANGE", gun.Range, 1, 700);
                gun.Weight = EditorGUILayout.Slider("WEIGHT", gun.Weight, 0, 4);
                gun.Unlockability.Price = EditorGUILayout.IntField("PRICE", gun.Unlockability.Price);
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

#region Manager Windows
    void DrawGameDataSettings()
    {
        if(gameData == null) gameData = bl_GameData.Instance;

        DrawTitleText("GAME DATA");
        if (gameData == null) return;
        DrawEditorOf(gameData);
    }

    void DrawPlayerSelector()
    {
        DrawTitleText("PLAYER SELECTOR");
#if PSELECTOR
        DrawEditorOf(bl_PlayerSelector.Data);
#else
        DrawDisableAddon("PLAYER SELECTOR", "PSELECTOR");
#endif
    }

    void DrawCustomizer()
    {
        DrawTitleText("Customizer");
#if CUSTOMIZER
        DrawEditorOf(bl_CustomizerData.Instance);
#else
        DrawDisableAddon("Class Customizer", "CUSTOMIZER");
#endif
    }

    void DrawClassCustomizer()
    {
        DrawTitleText("Class Customizer");
#if CLASS_CUSTOMIZER
        DrawEditorOf(bl_ClassManager.Instance);
#else
        DrawDisableAddon("Class Customizer", "CLASS_CUSTOMIZER");
#endif
    }

    void DrawULogin()
    {
        DrawTitleText("ULogin Pro");
#if ULSP
        DrawEditorOf(bl_LoginProDataBase.Instance);
#else
        DrawDisableAddon("ULogin Pro", "ULSP");
#endif
    }

    void DrawlevelManager()
    {
        DrawTitleText("Level Manager");
#if LM
        DrawEditorOf(bl_LevelManager.Instance);
#else
        DrawDisableAddon("Level Manager", "LM");
#endif
    }

    void DrawMinimap()
    {
        DrawTitleText("MiniMap");
#if UMM
        DrawEditorOf(bl_MiniMapData.Instance);
#else
        DrawDisableAddon("MiniMap", "UMM");
#endif
    }

    void DrawClan()
    {
        DrawTitleText("Clan System");
#if CLANS
        DrawEditorOf(bl_ClanSettings.Instance);
#else
        DrawDisableAddon("Clan System", "CLANS");
#endif
    }

    void DrawFloatingText()
    {
        var fm = Resources.Load("FloatingTextManagerSettings", typeof(ScriptableObject)) as ScriptableObject;
        if (fm == null)
        {
            DrawDisableAddon("Floating Text", "FT");
            return;
        }

        DrawTitleText("Floating Text");
        DrawEditorOf(fm);
    }

    void DrawEmblems()
    {
        var fm = Resources.Load("EmblemsDataBase", typeof(ScriptableObject)) as ScriptableObject;
        if (fm == null)
        {
            DrawDisableAddon("Emblems And Calling Cards", "EACC");
            return;
        }

        DrawTitleText("Emblems And Calling Cards");
        DrawEditorOf(fm);
    }

    void DrawLayoutCustomizer()
    {
        var fm = Resources.Load("LayoutCustomizerSettings", typeof(ScriptableObject)) as ScriptableObject;
        if (fm == null)
        {
            DrawDisableAddon("Layout Customizer", "LCZ");
            return;
        }

        DrawTitleText("Layout Customizer");
        DrawEditorOf(fm);
    }

    void DrawVehicles()
    {
        var fm = Resources.Load("VehicleSettings", typeof(ScriptableObject)) as ScriptableObject;
        if (fm == null)
        {
            DrawDisableAddon("Vehicle", "");
            return;
        }

        DrawTitleText("General Vehicle Settings");
        DrawEditorOf(fm);
    }

    void DrawThirdPerson()
    {
        DrawTitleText("Third Person");
#if MFPSTPV
        DrawEditorOf(bl_CameraViewSettings.Instance);
#else
        DrawDisableAddon("Third Person", "MFPSTPV");
#endif
    }

    void DrawInputManager()
    {
        DrawTitleText("Input Manager");

        DrawEditorOf(bl_Input.InputData);
        GUILayout.Space(10);   
        if (bl_Input.InputData.DefaultMapped != null)
        {
            DrawTitleText($"Mapped ({bl_Input.InputData.DefaultMapped.name})");
            DrawEditorOf(bl_Input.InputData.DefaultMapped);
        }
    }

    void DrawShop()
    {
        DrawTitleText("SHOP");
#if SHOP
        DrawEditorOf(bl_ShopData.Instance);
#else
        DrawDisableAddon("Shop", "SHOP");
#endif

        GUILayout.Space(10);
        DrawTitleText("Unity IAP");
#if SHOP_UIAP
        DrawEditorOf(bl_UnityIAP.Instance);
#else
        DrawDisableAddon("Unity IAP", "SHOP_UIAP");
#endif

        GUILayout.Space(10);
        DrawTitleText("Paypal");
#if SHOP_PAYPAL
        DrawEditorOf(bl_Paypal.Settings);
#else
        DrawDisableAddon("Paypal", "SHOP_PAYPAL");
#endif
    }

    void DrawKillStreaks()
    {
        DrawTitleText("Kill Streaks");
#if KSA
         DrawEditorOf(bl_KillStreakData.Instance);
#else
        DrawDisableAddon("Kill Streaks", "KSA");
#endif

        GUILayout.Space(10);
        DrawTitleText("Kill Streak Notifier");
#if KILL_STREAK
        DrawEditorOf(MFPS.Addon.KillStreak.bl_KillNotifierData.Instance);
#else
        DrawDisableAddon("Kill Streak Notifier", "KILL_STREAK");
#endif
    }

    void DrawLocalization()
    {
        DrawTitleText("Localization");
#if LOCALIZATION
        var editor = bl_LocalizationEditor.CreateEditor(bl_Localization.Instance);
        if (editor != null)
            editor.OnInspectorGUI();
#else
        DrawDisableAddon("Localization", "LOCALIZATION");
#endif
    }
#endregion

    void DrawManagerButtons()
    {
        Rect r;
        managersScroll = GUILayout.BeginScrollView(managersScroll);
        for (int i = 0; i < managerPanels.Length; i++)
        {
            r = GUILayoutUtility.GetRect(new GUIContent(managerPanels[i].Name), styles["panelButton"], GUILayout.Height(26));
            if (currentPanelType == managerPanels[i].Identifier)
            {
                EditorGUI.DrawRect(r, altColor);
                float w = r.width;
                r.width = 1;
                EditorGUI.DrawRect(r, hightlightColor);
                r.width = w;
            }
            if (GUI.Button(r, managerPanels[i].Name, styles["panelButton"]))
            {
                bodyScroll = Vector2.zero;
                currentPanelType = managerPanels[i].Identifier;
                currentWindow = WindowType.Managers;
            }
        }
        DrawBottomLine(0.2f);
        GUILayout.Space(30);
        EditorGUILayout.EndScrollView();
    }

    void DrawWeaponPanel()
    {
        GUILayout.Space(20);
        weaponSearchKey = weaponSearchfield.OnToolbarGUI(weaponSearchKey);
    }

#region Drawers
    void DrawTitleText(string title)
    {
        GUILayout.Label(title.ToUpper(), styles["titleH2"]);
        DrawBottomLine();
        GUILayout.Space(20);
    }

    void DrawBottomLine(float alpha = 0.7f) => DrawBottomLine(GUILayoutUtility.GetLastRect(), alpha);

    void DrawBottomLine(Rect fullRect, float alpha = 0.7f)
    {
        var color = whiteColor;
        color.a = alpha;
        fullRect.y += fullRect.height;
        fullRect.height = 1;
        EditorGUI.DrawRect(fullRect, color);
    }

    void DrawEditorOf(ScriptableObject scriptable)
    {
        if (scriptable == null) return;

        Editor editor;
        if (cachedEditors.ContainsKey(scriptable))
        {
            editor = cachedEditors[scriptable];
        }
        else
        {
            editor = Editor.CreateEditor(scriptable);
            cachedEditors.Add(scriptable, editor);
        }

        if (editor == null) return;

        Rect r = EditorGUILayout.BeginVertical(styles["borders"]);
        EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
        editor.OnInspectorGUI();
        EditorGUILayout.EndVertical();
    }

    void DrawDisableAddon(string addonName, string addonKey)
    {     
        Rect r = EditorGUILayout.BeginVertical(styles["borders"]);
        EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical();
        GUILayout.Label(GetUnityIcon("console.warnicon"), styles["panelButton"]);
        GUILayout.Label($"<color=#fff><size=18><b>{addonName.ToUpper()}</b></size></color>", styles["textC"]);
        var addonInfo = GetAddonsInfo(addonKey);
        if (addonInfo.IsAddonInProject())
        {
            GUILayout.Label("Is disabled.", styles["textC"]);
        }
        else
        {
            GUILayout.Label("Is not available in your project.", styles["textC"]);
        }
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (MFPSEditorStyles.ButtonOutline("Addon Manager", Color.yellow, GUILayout.Width(110)))
        {
            EditorWindow.GetWindow<MFPSAddonsWindow>().OpenAddonPage(addonInfo.NiceName);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
#endregion

    /// <summary>
    /// 
    /// </summary>
    void InitGUI()
    {
        m_initGUI = true;

        styles["panelButton"] = GetStyle("label", (ref GUIStyle style) =>
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = new Color(0.399f, 0.399f, 0.399f, 1.00f);
            style.fontStyle = FontStyle.Bold;
        });
        styles["textC"] = GetStyle("textmwc", (ref GUIStyle style) =>
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = new Color(0.399f, 0.399f, 0.399f, 1.00f);
            style.richText = true;
        },"label");
        styles["titleH2"] = GetStyle("titleH2", (ref GUIStyle style) =>
        {
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 18;
        }, "label");
        styles["borders"] = GetStyle("grey_border", (ref GUIStyle style) =>
        {
            style.normal.textColor = new Color(0.399f, 0.399f, 0.399f, 1.00f);
            style.overflow.left = style.overflow.right = style.overflow.top = style.overflow.bottom = 5;
            style.padding.left = -5; style.padding.right = 5;
            style.margin.left = style.margin.right = 10;
        });
    }

#region Utils
    private GUIStyle GetStyle(string name, TutorialWizard.Style.OnCreateStyleOp onCreate, string overlap = "") => TutorialWizard.Style.GetUnityStyle(name, onCreate, overlap);

    private static Dictionary<string, Texture2D> cachedUnityIcons;
    public static Texture2D GetUnityIcon(string iconName)
    {
        if (cachedUnityIcons == null) cachedUnityIcons = new Dictionary<string, Texture2D>();
        if (!cachedUnityIcons.ContainsKey(iconName))
        {
            var icon = (Texture2D)EditorGUIUtility.IconContent(iconName).image;
            cachedUnityIcons.Add(iconName, icon);
        }
        return cachedUnityIcons[iconName];
    }

    private Dictionary<string, MFPSAddonsInfo> cachedAddonsInfo;
    public MFPSAddonsInfo GetAddonsInfo(string addonKey)
    {
        if (cachedAddonsInfo == null) cachedAddonsInfo = new Dictionary<string, MFPSAddonsInfo>();
        if (!cachedAddonsInfo.ContainsKey(addonKey))
        {
            cachedAddonsInfo.Add(addonKey, MFPSAddonsData.Instance.GetAddonInfoByKey(addonKey));
        }
        return cachedAddonsInfo[addonKey];
    }

    [MenuItem("MFPS/Manager %m")]
    static void Open()
    {
        GetWindow<bl_MFPSManagerWindow>();
    }

    [Serializable]
    public class ManagerPanel
    {
        public string Name;
        public PanelManagerType Identifier;
    }

    [Serializable]
    public enum PanelManagerType
    {
        GameData = 0,
        ULoginPro = 1,
        PlayerSelector = 2,
        ClassCustomization = 3,
        Customizer = 4,
        Localization = 5,
        LevelManager,
        InputManager,
        KillStreaks,
        Shop,
        ThirdPerson,
        MiniMap,
        Clan,
        BattleRoyal,
        FloatingText,
        Emblems,
        LayoutCustomizer,
        Vehicle,
        None,
    }

    [Serializable]
    public enum WindowType
    {
        Home,
        Managers,
        Weapons,
        Players,
        Loadouts,
        Levels,
    }
#endregion
}