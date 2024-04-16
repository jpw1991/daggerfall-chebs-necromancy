using System;
using System.Collections;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using UnityEngine;
using Wenzil.Console;

namespace ChebsNecromancyMod
{
    public class ChebsNecromancy : MonoBehaviour
    {
        private static Mod mod;
        // private DaggerfallUI daggerfallUI;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<ChebsNecromancy>();

            ConsoleCommandsDatabase.RegisterCommand(
                SpawnCommand.name, SpawnCommand.description,
                SpawnCommand.usage, SpawnCommand.Execute);

            ConsoleCommandsDatabase.RegisterCommand(
                RecallMinionsCommand.name, RecallMinionsCommand.description,
                RecallMinionsCommand.usage, RecallMinionsCommand.Execute);

            mod.LoadSettingsCallback = LoadSettings;

            SaveLoadManager.OnLoad += RegisterExistingMinions;

            mod.LoadSettings();

            CreateBeginnerSpell();

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Starting Cheb");

            var objs = FindObjectsOfType(typeof(Transform));
            DaggerfallUI daggerfallUi = null;
            foreach (var obj in objs)
            {
                var t = (Transform)obj;
                daggerfallUi = t.GetComponentInChildren<DaggerfallUI>();
                if (daggerfallUi != null)
                    break;
            }

            if (daggerfallUi == null)
            {
                Debug.LogError("Failed to get DaggerfallUI");
                return;
            }

            daggerfallUi.UserInterfaceManager.OnWindowChange += AddNecromancerClass;
        }

        void AddNecromancerClass(object sender, EventArgs e)
        {
            // Check if window is a CreateCharClassSelect window and if it is, add our custom Necromancer class to it.
            var uiManager = (UserInterfaceManager)sender;
            var window = uiManager.TopWindow.Value;
            if (window is CreateCharClassSelect)
            {
                Debug.Log("Add Necromancer class!");
                var necromancer = new DFCareer()
                {
                    Name = "Necromancer",
                    AdvancementMultiplier = 1.0f,
                    HitPointsPerLevel = 8,
                    Strength = 40,
                    Intelligence = 65,
                    Willpower = 70,
                    Agility = 40,
                    Endurance = 50,
                    Personality = 40,
                    Speed = 40,
                    Luck = 50,
                    PrimarySkill1 = DFCareer.Skills.Mysticism,
                    PrimarySkill2 = DFCareer.Skills.Restoration,
                    PrimarySkill3 = DFCareer.Skills.Illusion,
                    MajorSkill1 = DFCareer.Skills.Alteration,
                    MajorSkill2 = DFCareer.Skills.Thaumaturgy,
                    MajorSkill3 = DFCareer.Skills.Destruction,
                    MinorSkill1 = DFCareer.Skills.Etiquette,
                    MinorSkill2 = DFCareer.Skills.Mercantile,
                    MinorSkill3 = DFCareer.Skills.Dodging,
                    MinorSkill4 = DFCareer.Skills.Medical,
                    MinorSkill5 = DFCareer.Skills.ShortBlade,
                    MinorSkill6 = DFCareer.Skills.Stealth,
                    Paralysis = DFCareer.Tolerance.Normal,
                    Magic = DFCareer.Tolerance.Normal,
                    Poison = DFCareer.Tolerance.Normal,
                    Fire = DFCareer.Tolerance.Normal,
                    Frost = DFCareer.Tolerance.Normal,
                    Shock = DFCareer.Tolerance.Normal,
                    Disease = DFCareer.Tolerance.Normal,
                    ShortBlades = DFCareer.Proficiency.Expert,
                    LongBlades = DFCareer.Proficiency.Normal,
                    HandToHand = DFCareer.Proficiency.Normal,
                    Axes = DFCareer.Proficiency.Normal,
                    BluntWeapons = DFCareer.Proficiency.Expert,
                    MissileWeapons = DFCareer.Proficiency.Normal,
                    UndeadAttackModifier = DFCareer.AttackModifier.Bonus,
                    DaedraAttackModifier = DFCareer.AttackModifier.Normal,
                    HumanoidAttackModifier = DFCareer.AttackModifier.Normal,
                    AnimalsAttackModifier = DFCareer.AttackModifier.Normal,
                    DarknessPoweredMagery = DFCareer.DarknessMageryFlags.ReducedPowerInLight,
                    LightPoweredMagery = DFCareer.LightMageryFlags.Normal,
                    ForbiddenMaterials = DFCareer.MaterialFlags.Silver,
                    ForbiddenShields = DFCareer.ShieldFlags.KiteShield | DFCareer.ShieldFlags.RoundShield | DFCareer.ShieldFlags.Buckler | DFCareer.ShieldFlags.TowerShield,
                    ForbiddenArmors = DFCareer.ArmorFlags.Chain,
                    ForbiddenProficiencies = DFCareer.ProficiencyFlags.HandToHand,
                    ExpertProficiencies = DFCareer.ProficiencyFlags.ShortBlades,
                    SpellPointMultiplier = DFCareer.SpellPointMultipliers.Times_3_00,
                    SpellPointMultiplierValue = 1.0f,
                    SpellAbsorption = DFCareer.SpellAbsorptionFlags.InDarkness,
                    NoRegenSpellPoints = false,
                    AcuteHearing = false,
                    Athleticism = false,
                    AdrenalineRush = false,
                    Regeneration = DFCareer.RegenerationFlags.InDarkness,
                    RapidHealing = DFCareer.RapidHealingFlags.None,
                    DamageFromSunlight = false,
                    DamageFromHolyPlaces = true
                };

                var charWindow = (CreateCharClassSelect)window;
                charWindow.OnItemPicked += (index, itemString) =>
                {
                    if (itemString == necromancer.Name)
                    {
                        //charWindow.SelectedClass = charWindow.ClassList[index];
                        //charWindow.SelectedClassIndex = index;

                        // TextFile.Token[] textTokens = DaggerfallUnity.Instance.TextProvider.GetRSCTokens(startClassDescriptionID + index);
                        DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, charWindow);
                        //messageBox.SetTextTokens("test");
                        messageBox.SetText("test");
                        messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                        Button noButton = messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                        noButton.ClickSound = DaggerfallUI.Instance.GetAudioClip(SoundClips.ButtonClick);
                        messageBox.OnButtonClick += (box, button) =>
                        {
                            if (button == DaggerfallMessageBox.MessageBoxButtons.Yes)
                            {
                                box.CloseWindow();
                                charWindow.CloseWindow();
                            }
                            else if (button == DaggerfallMessageBox.MessageBoxButtons.No)
                            {
                                //charWindow.selectedClass = null;
                                box.CancelWindow();
                            }
                        };
                        uiManager.PushWindow(messageBox);

                        AudioClip clip = DaggerfallUnity.Instance.SoundReader.GetAudioClip(SoundClips.SelectClassDrums);
                        DaggerfallUI.Instance.AudioSource.PlayOneShot(clip, DaggerfallUnity.Settings.SoundVolume);
                    }
                };

                // we have, unfortunately, no way of knowing when the list has finished loading. There's no events
                // we can make use of or similar. So: start a coroutine and wait for the list to be > 0. If it's
                // > 0 then it's been filled and we add Necromancer to the end
                StartCoroutine(WaitForList(necromancer, charWindow));
            }
        }

        IEnumerator WaitForList(DFCareer career, CreateCharClassSelect window)
        {
            yield return new WaitUntil(() => window == null || window.ListBox.Count > 0);
            if (window != null)
            {
                if (!window.ClassList.Contains(career))
                {
                    window.ClassList.Add(career);
                }
                if (window.ListBox.Count < window.ClassList.Count + 1) // +1 for custom
                {
                    // remove custom
                    window.ListBox.RemoveItem(window.ListBox.Count-1);
                    // add necromancer
                    window.ListBox.AddItem(career.Name);
                    // add custom back again
                    window.ListBox.AddItem(TextManager.Instance.GetLocalizedText("Custom"));
                }
            }
            else
            {
                Debug.LogError("Class window somehow became null");
            }
        }

        static void ReplaceVanillaMethod()
        {
            // Use reflection to replace the CreateCharClassSelect class with my own CharacterCreationReplacement
            // which adds the necromancer class at the end. This is not very nice or compatible with other mods,
            // but gets around using something like injection via Harmony as described here:
            // https://forums.dfworkshop.net/viewtopic.php?t=4213
            // and also gets around creating data files as described here:
            // https://forums.dfworkshop.net/viewtopic.php?t=6226
            // var assembly = Assembly.GetExecutingAssembly();
            //
            // // Get the type we're replacing
            // var originalType = assembly.GetType("DaggerfallWorkshop.Game.UserInterfaceWindows.CreateCharClassSelect");
            //
            // // instantiate the replacement class
            // var replacementInstance = Activator.CreateInstance(typeof(CharacterCreationReplacement));
            //
            // // Replace the original class' method with my own
            // var replacementMethod = replacementInstance.GetType().GetMethod("Setup");
            // var originalMethod = originalType.GetMethod("Setup");
            // originalMethod = replacementMethod;

            // // Get the method to be replaced
            // MethodInfo originalMethod = typeof(CreateCharClassSelect).GetMethod("Setup",
            //     BindingFlags.NonPublic | BindingFlags.Instance);
            // if (originalMethod == null)
            // {
            //     Debug.LogError("Failed to replace method: original method is null");
            //     return;
            // }
            //
            // // Define a dynamic method with the same signature as the original method
            // DynamicMethod replacementMethod = new DynamicMethod(
            //     "Setup",
            //     null,
            //     null,
            //     typeof(CreateCharClassSelect)
            // );
            //
            // // Generate IL to call the replacement method
            // // ILGenerator ilGenerator = replacementMethod.GetILGenerator();
            // MethodInfo replacementMethodInfo = typeof(CharacterCreationReplacement).GetMethod("Setup",
            //     BindingFlags.NonPublic| BindingFlags.Instance);
            // if (replacementMethodInfo == null)
            // {
            //     Debug.LogError("Failed to replace method: replacement method is null");
            //     return;
            // }
            // if (replacementMethodInfo.DeclaringType == null)
            // {
            //     Debug.LogError("Failed to replace method: replacement method declaring type is null");
            //     return;
            // }
            // // ilGenerator.Emit(OpCodes.Newobj, replacementMethodInfo.DeclaringType.GetConstructor(Type.EmptyTypes));
            // // ilGenerator.EmitCall(OpCodes.Callvirt, replacementMethodInfo, null);
            // // ilGenerator.Emit(OpCodes.Ret);
            //
            // // Get the original method's method handle
            // RuntimeMethodHandle handle = originalMethod.MethodHandle;
            //
            // // Get the method attributes
            // MethodAttributes attributes = originalMethod.Attributes;
            //
            // // Replace the original method with the replacement method
            // RuntimeHelpers.PrepareMethod(handle);
            // unsafe
            // {
            //     IntPtr slot = handle.Value + (IntPtr.Size * 2);
            //     IntPtr funcPtr = replacementMethod.MethodHandle.GetFunctionPointer();
            //     *(IntPtr*)slot.ToPointer() = funcPtr;
            // }
            // // Get the method to be replaced
            // MethodInfo originalMethod = typeof(CreateCharClassSelect).GetMethod("Setup",
            //     BindingFlags.NonPublic | BindingFlags.Instance);
            // if (originalMethod == null)
            // {
            //     Debug.LogError("Failed to replace method: original method is null");
            //     return;
            // }
            //
            // // Create a new dynamic type inheriting from CreateCharClassSelect
            // TypeBuilder typeBuilder = CreateDynamicType(typeof(CreateCharClassSelect));
            // if (typeBuilder == null)
            // {
            //     Debug.LogError("Failed to create dynamic type");
            //     return;
            // }
            //
            // // Define a new method with the same signature as the original method
            // MethodBuilder replacementMethod = typeBuilder.DefineMethod("Setup",
            //     MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
            //     typeof(void),
            //     new Type[] { });
            // ILGenerator ilGenerator = replacementMethod.GetILGenerator();
            // MethodInfo replacementMethodInfo = typeof(CharacterCreationReplacement).GetMethod("Setup",
            //     BindingFlags.NonPublic | BindingFlags.Instance);
            // if (replacementMethodInfo == null)
            // {
            //     Debug.LogError("Failed to replace method: replacement method is null");
            //     return;
            // }
            //
            // ilGenerator.Emit(OpCodes.Ldarg_0); // Load 'this' pointer for the method call
            // ilGenerator.EmitCall(OpCodes.Call, replacementMethodInfo, null);
            // ilGenerator.Emit(OpCodes.Ret);
            //
            // // Override the original method with the replacement method
            // typeBuilder.DefineMethodOverride(replacementMethod, originalMethod);
        }

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            var spellEffects = new List<ChebEffect>()
            {
                new RecallMinionsEffect(),
                new SummonSkeletonEffect(),
                new SummonGhostEffect(),
                new SummonLichEffect(),
                new SummonMummyEffect(),
                new SummonVampireEffect(),
                new SummonAncientLichEffect(),
                new SummonAncientVampireEffect(),
                new SummonZombieEffect()
            };

            var effectBroker = GameManager.Instance.EntityEffectBroker;

            foreach (var baseEntityEffect in spellEffects)
            {
                baseEntityEffect.CostA = modSettings.GetValue<int>(baseEntityEffect.Key, "Chance Cost A");
                baseEntityEffect.CostB = modSettings.GetValue<int>(baseEntityEffect.Key, "Chance Cost B");
                baseEntityEffect.CostOffset = modSettings.GetValue<int>(baseEntityEffect.Key, "Chance Cost Offset");

                baseEntityEffect.SetProperties();

                var existing = effectBroker.HasEffectTemplate(baseEntityEffect.Key);
                if (existing)
                {
                    var template = effectBroker.GetEffectTemplate(baseEntityEffect.Key);
                    template.Settings = baseEntityEffect.Settings;
                    // Debug.Log($"Updating {baseEntityEffect.Key} with costs " +
                    //           $"A={baseEntityEffect.Properties.ChanceCosts.CostA}, " +
                    //           $"B={baseEntityEffect.Properties.ChanceCosts.CostB}, " +
                    //           $"O={baseEntityEffect.Properties.ChanceCosts.OffsetGold}");
                }
                else
                {
                    // Debug.Log($"Registering {baseEntityEffect.Key} with costs " +
                    //           $"A={baseEntityEffect.Properties.ChanceCosts.CostA}, " +
                    //           $"B={baseEntityEffect.Properties.ChanceCosts.CostB}, " +
                    //           $"O={baseEntityEffect.Properties.ChanceCosts.OffsetGold}");
                    effectBroker.RegisterEffectTemplate(baseEntityEffect);
                }
            }
        }

        private static void RegisterExistingMinions(SaveData_v1 saveDataV1)
        {
            // When the scene loads, existing skeletons from the last session won't have the UndeadMinion script
            // attached to them. Find them and attach it here, so that they follow the player etc.
            var undeadIds = new List<int>()
            {
                (int)MobileTypes.AncientLich,
                (int)MobileTypes.VampireAncient,
                (int)MobileTypes.Ghost,
                (int)MobileTypes.Lich,
                (int)MobileTypes.Mummy,
                (int)MobileTypes.SkeletalWarrior,
                (int)MobileTypes.Vampire,
                (int)MobileTypes.Zombie
            };
            var daggerfallEnemies = FindObjectsOfType<DaggerfallEnemy>();
            foreach (var daggerfallEnemy in daggerfallEnemies)
            {
                if (daggerfallEnemy.MobileUnit.Enemy.Team == MobileTeams.PlayerAlly
                    && undeadIds.Contains(daggerfallEnemy.MobileUnit.Enemy.ID))
                {
                    daggerfallEnemy.gameObject.AddComponent<UndeadMinion>();
                }
            }
        }

        private void OnDestroy()
        {
            SaveLoadManager.OnLoad -= RegisterExistingMinions;
        }

        private static void CreateBeginnerSpell()
        {
            // In DFU a spell is called an effect bundle - basically a bundle of spell effects (makes sense). Here we
            // create a beginner spell for people so they have some minions to start off with if they wish.
            var effectBroker = GameManager.Instance.EntityEffectBroker;
            if (!effectBroker.HasEffectTemplate(SummonSkeletonEffect.EffectKey))
            {
                Debug.LogError("Cheb's Necromancy: CreateBeginnerSpell: Failed to get template from effect broker");
                return;
            }

            var template = effectBroker.GetEffectTemplate(SummonSkeletonEffect.EffectKey);
            var templateSettings = new EffectSettings()
            {
                ChanceBase = 25,
                ChancePerLevel = 5,
                ChancePlus = 1
            };
            var effectEntry = new EffectEntry()
            {
                Key = template.Properties.Key,
                Settings = templateSettings,
            };
            var animateDead = new EffectBundleSettings()
            {
                Version = 1,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Animate Dead",
                IconIndex = 12,
                Effects = new EffectEntry[] { effectEntry },
            };
            // add it to stores so it can be purchased
            var offer = new EntityEffectBroker.CustomSpellBundleOffer()
            {
                Key = "AnimateDead-CustomOffer",
                Usage = EntityEffectBroker.CustomSpellBundleOfferUsage.SpellsForSale,
                BundleSetttings = animateDead,
            };
            effectBroker.RegisterCustomSpellBundleOffer(offer);
            // todo: add it to necromancer origin
        }
    }

    // public class CustomCreateCharClassSelect : DaggerfallListPickerWindow
    // {
    //     const int startClassDescriptionID = 2100;
    //
    //     List<DFCareer> classList = new List<DFCareer>();
    //     DFCareer selectedClass;
    //     int selectedClassIndex = 0;
    //
    //     public DFCareer SelectedClass
    //     {
    //         get { return selectedClass; }
    //     }
    //
    //     public CustomCreateCharClassSelect(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
    //         : base(uiManager, previous)
    //     {
    //     }
    //
    //     protected override void Setup()
    //     {
    //         base.Setup();
    //
    //         // Read all CLASS*.CFG files and add to listbox
    //         string[] files = Directory.GetFiles(DaggerfallUnity.Instance.Arena2Path, "CLASS*.CFG");
    //         if (files != null && files.Length > 0)
    //         {
    //             for (int i = 0; i < files.Length - 1; i++)
    //             {
    //                 ClassFile classFile = new ClassFile(files[i]);
    //                 classList.Add(classFile.Career);
    //                 listBox.AddItem(TextManager.Instance.GetLocalizedText(classFile.Career.Name));
    //             }
    //         }
    //         // Last option is for creating custom classes
    //         listBox.AddItem(TextManager.Instance.GetLocalizedText("Custom"));
    //
    //         OnItemPicked += DaggerfallClassSelectWindow_OnItemPicked;
    //     }
    //
    //     void DaggerfallClassSelectWindow_OnItemPicked(int index, string className)
    //     {
    //         if (index == classList.Count) // "Custom" option selected
    //         {
    //             selectedClass = null;
    //             selectedClassIndex = -1;
    //             CloseWindow();
    //         }
    //         else
    //         {
    //             selectedClass = classList[index];
    //             selectedClass.Name = className; // Ensures any localized display names are assigned after selection from list
    //             selectedClassIndex = index;
    //
    //             TextFile.Token[] textTokens = DaggerfallUnity.Instance.TextProvider.GetRSCTokens(startClassDescriptionID + index);
    //             DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
    //             messageBox.SetTextTokens(textTokens);
    //             messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
    //             Button noButton = messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
    //             noButton.ClickSound = DaggerfallUI.Instance.GetAudioClip(SoundClips.ButtonClick);
    //             messageBox.OnButtonClick += ConfirmClassPopup_OnButtonClick;
    //             uiManager.PushWindow(messageBox);
    //
    //             AudioClip clip = DaggerfallUnity.Instance.SoundReader.GetAudioClip(SoundClips.SelectClassDrums);
    //             DaggerfallUI.Instance.AudioSource.PlayOneShot(clip, DaggerfallUnity.Settings.SoundVolume);
    //         }
    //     }
    //
    //     void ConfirmClassPopup_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
    //     {
    //         if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
    //         {
    //             sender.CloseWindow();
    //             CloseWindow();
    //         }
    //         else if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.No)
    //         {
    //             selectedClass = null;
    //             sender.CancelWindow();
    //         }
    //     }
    //
    //     public int SelectedClassIndex
    //     {
    //         get { return selectedClassIndex; }
    //     }
    //
    //     public List<DFCareer> ClassList
    //     {
    //         get { return classList; }
    //     }
    // }
}