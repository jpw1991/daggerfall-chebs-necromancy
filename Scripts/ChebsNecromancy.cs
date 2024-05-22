using System;
using System.Collections;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using Newtonsoft.Json;
using UnityEngine;
using Wenzil.Console;
using Random = System.Random;

namespace ChebsNecromancyMod
{
    public class CustomCorpseItem : DaggerfallUnityItem
    {
        public const int TemplateIndex = 4733;
        public const string DisplayName = "Humanoid Corpse";
        public const ItemGroups TemplateItemGroup = ItemGroups.UselessItems1;

        public CustomCorpseItem() : base(TemplateItemGroup, TemplateIndex)
        {
            value = 0;
            weightInKg = 0f;
            RenameItem(DisplayName);
        }

        // 380 & 1 is the value for MiscItems.Dead_Body in the texture archive. To view the archive, use the
        // utility here: https://www.nexusmods.com/daggerfallunity/mods/460
        public override int InventoryTextureArchive => 380;
        public override int InventoryTextureRecord => 1;

        public override bool IsStackable()
        {
            return true;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(CustomCorpseItem).ToString();
            return data;
        }

        public static DaggerfallUnityItem Create()
        {
            return ItemBuilder.CreateItem(TemplateItemGroup, TemplateIndex);
        }
    }

    public enum Logging
    {
        Errors = 0,
        All,
        None
    }
    public class ChebsNecromancy : MonoBehaviour
    {
        public const string NecromancerCareerName = "Necromancer";

        public static bool CorpseItemEnabled = true;
        //public static DaggerfallUnityItem CorpseItem;

        public static EffectBundleSettings AnimateDeadSpell, NoviceRecallSpell;
        public static bool EnableCustomClassNecromancer = true;
        public static DFCareer NecromancerCareer;
        public static Logging Log = Logging.Errors;

        public static List<DaggerfallEnemy> MinionsToRestore = new List<DaggerfallEnemy>();

        public static readonly List<int> UndeadIds = new List<int>()
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

        private static Mod mod;

        private static bool justFastTravelled = false;

        private static Dictionary<int, int> alreadyLootedGraves = new Dictionary<int, int>();

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

            ConsoleCommandsDatabase.RegisterCommand(
                SpawnCorpseItemCommand.name, SpawnCorpseItemCommand.description,
                SpawnCorpseItemCommand.usage, SpawnCorpseItemCommand.Execute);

            mod.LoadSettingsCallback = LoadSettings;

            #region BeforeSettings
            // Custom items
            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(CustomCorpseItem.TemplateIndex, CustomCorpseItem.TemplateItemGroup, typeof(CustomCorpseItem));
            ChebLog("CustomCorpseItem registered.");

            // Events
            SaveLoadManager.OnLoad += RegisterExistingMinions;
            // On pre-transition, make note of all active minions
            PlayerEnterExit.OnPreTransition += args => { RecordActiveMinions(); };
            // On post-transition, restore aforementioned active minions
            PlayerEnterExit.OnTransitionExterior += args => { RestoreActiveMinions(); };
            PlayerEnterExit.OnTransitionInterior += args => { RestoreActiveMinions(); };
            PlayerEnterExit.OnTransitionDungeonExterior += args => { RestoreActiveMinions(); };
            PlayerEnterExit.OnTransitionDungeonInterior += args => { RestoreActiveMinions(); };
            // Before fast-travel, record minions
            DaggerfallTravelPopUp.OnPreFastTravel += up =>
            {
                ChebLog("OnPreFastTravel: recording active minions. Player pos: {up.");
                RecordActiveMinions();
            };
            // After fast-travel, restore minions
            DaggerfallTravelPopUp.OnPostFastTravel += () =>
            {
                ChebLog("OnPostFastTravel: Restoring minions");
                // For some reason, OnPostFastTravel doesn't work for restoring minion positions. I think it just
                // happens too fast and the player position isn't yet properly changed.
                // As a workaround, just flip a flag to true and then rely on PlayerGPS.OnEnterLocationRect to do the
                // work but only if having just fast travelled.
                // todo: find a better solution
                justFastTravelled = true;

            };
            PlayerGPS.OnEnterLocationRect += location =>
            {
                ChebLog("OnEnterLocationRect");
                if (justFastTravelled)
                {
                    justFastTravelled = false;
                    RestoreActiveMinions();
                }
            };
            // Custom item drops
            EnemyDeath.OnEnemyDeath += OnEnemyDeath; // removed later, if disabled in settings.
            #endregion

            mod.LoadSettings();

            #region AfterSettings
            // Create spells after settings are loaded, so that values from the config get used.
            AnimateDeadSpell = CreateAnimateDeadSpell();
            NoviceRecallSpell = CreateNoviceRecallSpell();
            #endregion

            RegisterCustomActivations(mod);

            mod.IsReady = true;
        }

        public static void OnEnemyDeath(object sender, EventArgs eventArgs)
        {
            // drop a corpse if it's a humanoid or humanoid-like monster
            var enemyDeath = sender as EnemyDeath;
            if (enemyDeath == null) return;
            if (!enemyDeath.TryGetComponent(out DaggerfallEntityBehaviour entityBehaviour)) return;
            if (!(entityBehaviour.Entity is EnemyEntity enemyEntity)) return;
            var dropCorpse = false;
            if (enemyEntity.EntityType == EntityTypes.EnemyClass)
            {
                // humanoid
                switch (enemyEntity.CareerIndex)
                {
                    case (int)ClassCareers.Mage:
                    case (int)ClassCareers.Spellsword:
                    case (int)ClassCareers.Battlemage:
                    case (int)ClassCareers.Sorcerer:
                    case (int)ClassCareers.Healer:
                    case (int)ClassCareers.Nightblade:
                    case (int)ClassCareers.Bard:
                    case (int)ClassCareers.Acrobat:
                    case (int)ClassCareers.Assassin:
                    case (int)ClassCareers.Burglar:
                    case (int)ClassCareers.Rogue:
                    case (int)ClassCareers.Thief:
                    case (int)ClassCareers.Monk:
                    case (int)ClassCareers.Archer:
                    case (int)ClassCareers.Ranger:
                    case (int)ClassCareers.Barbarian:
                    case (int)ClassCareers.Warrior:
                    case (int)ClassCareers.Knight:
                        dropCorpse = true;
                        break;
                }
            }
            else
            {
                // humanoid monster
                switch (enemyEntity.CareerIndex)
                {
                    case (int)MonsterCareers.Orc:
                    case (int)MonsterCareers.OrcSergeant:
                    case (int)MonsterCareers.OrcShaman:
                    case (int)MonsterCareers.OrcWarlord:
                        dropCorpse = true;
                        break;
                }
            }

            if (dropCorpse)
            {
                ChebLog($"Dropping corpse for {enemyEntity.Name}");
                //entityBehaviour.CorpseLootContainer.Items.AddItem(CorpseItem.Clone());
                entityBehaviour.CorpseLootContainer.Items.AddItem(CustomCorpseItem.Create());
            }
        }

        public static void RecordActiveMinions()
        {
            // Record all the active minions so that they can be restored post-transition.
            ChebLog("Recording active minions...");
            MinionsToRestore = UndeadMinion.GetActiveMinions();

        }

        public static void RestoreActiveMinions()
        {
            // Restore the minions that have been recorded pre-transition.
            var player = GameManager.Instance.PlayerEntity;
            var mysticismLevel = player.Skills.GetLiveSkillValue(DFCareer.Skills.Mysticism);
            var intelligence = player.Stats.LiveIntelligence;
            var willpower = player.Stats.LiveWillpower;
            // First, hide (and later destroy) active minions (otherwise ones that already exist will remain)
            var alreadyPresentMinions = UndeadMinion.GetActiveMinions();
            alreadyPresentMinions.ForEach(m => m.gameObject.SetActive(false));
            // Restore the minions
            foreach (var daggerfallEnemy in MinionsToRestore)
            {
                var magnitude = daggerfallEnemy.GetComponent<UndeadMinion>().createdWithMagnitude;
                switch (daggerfallEnemy.MobileUnit.Enemy.ID)
                {
                    case (int)MobileTypes.AncientLich:
                        SummonAncientLichEffect.Spawn(magnitude, mysticismLevel, intelligence, willpower, false);
                        break;
                    case (int)MobileTypes.VampireAncient:
                        SummonAncientVampireEffect.Spawn(magnitude, mysticismLevel, intelligence, willpower, false);
                        break;
                    case (int)MobileTypes.Ghost:
                        SummonGhostEffect.Spawn(magnitude, mysticismLevel, intelligence, willpower, false);
                        break;
                    case (int)MobileTypes.Lich:
                        SummonLichEffect.Spawn(magnitude, mysticismLevel, intelligence, willpower, false);
                        break;
                    case (int)MobileTypes.Mummy:
                        SummonMummyEffect.Spawn(magnitude, mysticismLevel, intelligence, willpower, false);
                        break;
                    case (int)MobileTypes.SkeletalWarrior:
                        SummonSkeletonEffect.Spawn(magnitude, mysticismLevel, intelligence, willpower, false);
                        break;
                    case (int)MobileTypes.Vampire:
                        SummonVampireEffect.Spawn(magnitude, mysticismLevel, intelligence, willpower, false);
                        break;
                    case (int)MobileTypes.Zombie:
                        SummonZombieEffect.Spawn(magnitude, mysticismLevel, intelligence, willpower, false);
                        break;
                    default:
                        ChebError($"RestoreActiveMinions: failed to restore minion of " +
                                  $"unknown type {daggerfallEnemy.MobileUnit.Enemy.ID}");
                        break;
                }
                // Destroy the hidden minions
                alreadyPresentMinions.ForEach(m => Destroy(m.gameObject));
                // Clear the list
                MinionsToRestore = new List<DaggerfallEnemy>();
            }
        }

        private void Start()
        {
            ChebLog("Starting Cheb");

            if (!EnableCustomClassNecromancer)
            {
                ChebLog("Custom Class Necromancer disabled.");
                return;
            }

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
                ChebError("Failed to get DaggerfallUI");
                return;
            }

            daggerfallUi.UserInterfaceManager.OnWindowChange += HijackWizard;
        }

        public static DFCareer GenerateNecromancerCareer(ModSettings modSettings)
        {
            var skillsMap = new Dictionary<int, DFCareer.Skills>
            {
                { 0, DFCareer.Skills.Medical  },
                { 1, DFCareer.Skills.Etiquette  },
                { 2, DFCareer.Skills.Streetwise  },
                { 3, DFCareer.Skills.Jumping  },
                { 4, DFCareer.Skills.Orcish  },
                { 5, DFCareer.Skills.Harpy  },
                { 6, DFCareer.Skills.Giantish  },
                { 7, DFCareer.Skills.Dragonish  },
                { 8, DFCareer.Skills.Nymph  },
                { 9, DFCareer.Skills.Daedric  },
                { 10, DFCareer.Skills.Spriggan  },
                { 11, DFCareer.Skills.Centaurian  },
                { 12, DFCareer.Skills.Impish  },
                { 13, DFCareer.Skills.Lockpicking  },
                { 14, DFCareer.Skills.Mercantile  },
                { 15, DFCareer.Skills.Pickpocket  },
                { 16, DFCareer.Skills.Stealth  },
                { 17, DFCareer.Skills.Swimming  },
                { 18, DFCareer.Skills.Climbing  },
                { 19, DFCareer.Skills.Backstabbing  },
                { 20, DFCareer.Skills.Dodging  },
                { 21, DFCareer.Skills.Running  },
                { 22, DFCareer.Skills.Destruction  },
                { 23, DFCareer.Skills.Restoration  },
                { 24, DFCareer.Skills.Illusion  },
                { 25, DFCareer.Skills.Alteration  },
                { 26, DFCareer.Skills.Thaumaturgy  },
                { 27, DFCareer.Skills.Mysticism  },
                { 28, DFCareer.Skills.ShortBlade  },
                { 29, DFCareer.Skills.LongBlade  },
                { 30, DFCareer.Skills.HandToHand  },
                { 31, DFCareer.Skills.Axe  },
                { 32, DFCareer.Skills.BluntWeapon  },
                { 33, DFCareer.Skills.Archery  },
                { 34, DFCareer.Skills.CriticalStrike  },

            };
            var toleranceMap = new Dictionary<int, DFCareer.Tolerance>()
            {
                {0, DFCareer.Tolerance.Normal},
                {1, DFCareer.Tolerance.Immune},
                {2, DFCareer.Tolerance.Resistant},
                {3, DFCareer.Tolerance.LowTolerance},
                {4, DFCareer.Tolerance.CriticalWeakness},
            };
            var proficiencyMap = (DFCareer.Proficiency[])Enum.GetValues(typeof(DFCareer.Proficiency));

            const string section = "Necromancer Class";

            var result = new DFCareer()
                {
                    Name = NecromancerCareerName,
                    AdvancementMultiplier = modSettings.GetFloat(section, "Advancement Multiplier"),
                    HitPointsPerLevel = modSettings.GetInt(section, "HP per Level"),
                    Strength = modSettings.GetInt(section, "Strength"),
                    Intelligence = modSettings.GetInt(section, "Intelligence"),
                    Willpower = modSettings.GetInt(section, "Willpower"),
                    Agility = modSettings.GetInt(section, "Agility"),
                    Endurance = modSettings.GetInt(section, "Endurance"),
                    Personality = modSettings.GetInt(section, "Personality"),
                    Speed = modSettings.GetInt(section, "Speed"),
                    Luck = modSettings.GetInt(section, "Luck"),
                    PrimarySkill1 = skillsMap[modSettings.GetInt(section, "Primary Skill 1")],
                    PrimarySkill2 = skillsMap[modSettings.GetInt(section, "Primary Skill 2")],
                    PrimarySkill3 = skillsMap[modSettings.GetInt(section, "Primary Skill 3")],
                    MajorSkill1 = skillsMap[modSettings.GetInt(section, "Major Skill 1")],
                    MajorSkill2 = skillsMap[modSettings.GetInt(section, "Major Skill 2")],
                    MajorSkill3 = skillsMap[modSettings.GetInt(section, "Major Skill 3")],
                    MinorSkill1 = skillsMap[modSettings.GetInt(section, "Minor Skill 1")],
                    MinorSkill2 = skillsMap[modSettings.GetInt(section, "Minor Skill 2")],
                    MinorSkill3 = skillsMap[modSettings.GetInt(section, "Minor Skill 3")],
                    MinorSkill4 = skillsMap[modSettings.GetInt(section, "Minor Skill 4")],
                    MinorSkill5 = skillsMap[modSettings.GetInt(section, "Minor Skill 5")],
                    MinorSkill6 = skillsMap[modSettings.GetInt(section, "Minor Skill 6")],
                    Paralysis = toleranceMap[modSettings.GetInt(section, "Paralysis")],
                    Magic = toleranceMap[modSettings.GetInt(section, "Magic")],
                    Poison = toleranceMap[modSettings.GetInt(section, "Poison")],
                    Fire = toleranceMap[modSettings.GetInt(section, "Fire")],
                    Frost = toleranceMap[modSettings.GetInt(section, "Frost")],
                    Shock = toleranceMap[modSettings.GetInt(section, "Shock")],
                    Disease = toleranceMap[modSettings.GetInt(section, "Disease")],
                    ShortBlades = proficiencyMap[modSettings.GetInt(section, "Short Blades")],
                    LongBlades = proficiencyMap[modSettings.GetInt(section, "Long Blades")],
                    HandToHand = proficiencyMap[modSettings.GetInt(section, "Hand to Hand")],
                    Axes = proficiencyMap[modSettings.GetInt(section, "Axes")],
                    BluntWeapons = proficiencyMap[modSettings.GetInt(section, "Blunt Weapons")],
                    MissileWeapons = proficiencyMap[modSettings.GetInt(section, "Missile Weapons")],
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
                    NoRegenSpellPoints = modSettings.GetBool(section, "No Regen Spell Points"),
                    AcuteHearing = modSettings.GetBool(section, "Acute Hearing"),
                    Athleticism = modSettings.GetBool(section, "Athleticism"),
                    AdrenalineRush = modSettings.GetBool(section, "Adrenaline Rush"),
                    Regeneration = DFCareer.RegenerationFlags.InDarkness,
                    RapidHealing = DFCareer.RapidHealingFlags.None,
                    DamageFromSunlight = modSettings.GetBool(section, "Damage From Sunlight"),
                    DamageFromHolyPlaces = modSettings.GetBool(section, "Damage From Holy Places")
                };

            // log the result in case of errors
            ChebLog($"Class result: {JsonConvert.SerializeObject(result)}");

            return result;
        }

        public static Dictionary<DFCareer, List<TextFile.Token>> GetCustomClasses()
        {
            return new Dictionary<DFCareer, List<TextFile.Token>>()
            {
                {
                    NecromancerCareer, new List<TextFile.Token>()
                    {
                        new TextFile.Token()
                            { text = "Necromancers are mystics that specialize in raising the dead to do their" },
                        new TextFile.Token()
                            { text = "bidding. Fueled by darkness, a necromancer is most powerful at night." },
                        new TextFile.Token()
                            { text = "Unfortunately, the sun makes them uncomfortable and they cannot tolerate" },
                        new TextFile.Token() { text = "holy places." },
                        new TextFile.Token() { text = "" },
                        new TextFile.Token() { text = $"The skills most important to a Necromancer are: {NecromancerCareer.MajorSkill1}," },
                        new TextFile.Token() { text = $"{NecromancerCareer.MajorSkill2}, and {NecromancerCareer.MajorSkill3}." },
                        new TextFile.Token() { text = "" },
                        new TextFile.Token() { text = "Do you wish to be a Necromancer?" },
                    }
                }
            };
        }

        void HijackWizard(object sender, EventArgs e)
        {
            // Check if window is a StartNewGameWizard window and if it is, close it and open our custom one.
            var uiManager = (UserInterfaceManager)sender;
            var window = uiManager.TopWindow.Value;
            if (window is StartNewGameWizard)
            {
                ChebLog("StartNewGameWizard opening, close it.");
                window.CloseWindow();
                ChebLog("StartNewGameWizard closed, opening CustomStartNewGameWizard.");
                uiManager.PushWindow(new CustomStartNewGameWizard(uiManager));
            }
        }

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            var loggingMap = (Logging[])Enum.GetValues(typeof(Logging));
            Log = loggingMap[modSettings.GetInt("General", "Logging")];

            const string classSection = "Necromancer Class";
            EnableCustomClassNecromancer = modSettings.GetBool(classSection, "Enabled");

            NecromancerCareer = GenerateNecromancerCareer(modSettings);

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
                //ChebLog($"Processing {baseEntityEffect.DisplayName}");
                if (baseEntityEffect.Properties.SupportChance)
                {
                    //ChebLog("Has chance");
                    baseEntityEffect.ChanceCostA = modSettings.GetValue<int>(baseEntityEffect.Key, "Chance Cost A");
                    baseEntityEffect.ChanceCostB = modSettings.GetValue<int>(baseEntityEffect.Key, "Chance Cost B");
                    baseEntityEffect.ChanceCostOffset = modSettings.GetValue<int>(baseEntityEffect.Key, "Chance Cost Offset");
                }

                if (baseEntityEffect.Properties.SupportMagnitude)
                {
                    //ChebLog("Has magnitude");
                    baseEntityEffect.MagnitudeCostA = modSettings.GetValue<int>(baseEntityEffect.Key, "Magnitude Cost A");
                    baseEntityEffect.MagnitudeCostB = modSettings.GetValue<int>(baseEntityEffect.Key, "Magnitude Cost B");
                    baseEntityEffect.MagnitudeCostOffset = modSettings.GetValue<int>(baseEntityEffect.Key, "Magnitude Cost Offset");
                }

                if (baseEntityEffect.Properties.SupportDuration)
                {
                    // todo
                }

                baseEntityEffect.SetProperties();

                var existing = effectBroker.HasEffectTemplate(baseEntityEffect.Key);
                if (existing)
                {
                    var template = effectBroker.GetEffectTemplate(baseEntityEffect.Key);
                    template.Settings = baseEntityEffect.Settings;
                    // ChebLog($"Updating {baseEntityEffect.Key} with costs " +
                    //           $"A={baseEntityEffect.Properties.ChanceCosts.CostA}, " +
                    //           $"B={baseEntityEffect.Properties.ChanceCosts.CostB}, " +
                    //           $"O={baseEntityEffect.Properties.ChanceCosts.OffsetGold}");
                }
                else
                {
                    // ChebLog($"Registering {baseEntityEffect.Key} with costs " +
                    //           $"A={baseEntityEffect.Properties.ChanceCosts.CostA}, " +
                    //           $"B={baseEntityEffect.Properties.ChanceCosts.CostB}, " +
                    //           $"O={baseEntityEffect.Properties.ChanceCosts.OffsetGold}");
                    effectBroker.RegisterEffectTemplate(baseEntityEffect);
                }
            }

            const string corpseSection = "Corpse Item";
            CorpseItemEnabled = modSettings.GetBool(corpseSection, "Enabled");
            if (!CorpseItemEnabled) EnemyDeath.OnEnemyDeath -= OnEnemyDeath;
            // to do: get that to work
            //CorpseItem.weightInKg = modSettings.GetInt(corpseSection, "Weight in KG");
        }

        private static void RegisterExistingMinions(SaveData_v1 saveDataV1)
        {
            // When the scene loads, existing skeletons from the last session won't have the UndeadMinion script
            // attached to them. Find them and attach it here, so that they follow the player etc.
            var daggerfallEnemies = FindObjectsOfType<DaggerfallEnemy>();
            foreach (var daggerfallEnemy in daggerfallEnemies)
            {
                if (daggerfallEnemy.MobileUnit.Enemy.Team == MobileTeams.PlayerAlly
                    && UndeadIds.Contains(daggerfallEnemy.MobileUnit.Enemy.ID))
                {
                    daggerfallEnemy.gameObject.AddComponent<UndeadMinion>();
                }
            }
        }

        private void OnDestroy()
        {
            SaveLoadManager.OnLoad -= RegisterExistingMinions;
        }

        private static EffectBundleSettings CreateAnimateDeadSpell()
        {
            // In DFU a spell is called an effect bundle - basically a bundle of spell effects (makes sense). Here we
            // create a beginner spell for people so they have some minions to start off with if they wish.
            var effectBroker = GameManager.Instance.EntityEffectBroker;
            if (!effectBroker.HasEffectTemplate(SummonSkeletonEffect.EffectKey))
            {
                ChebError("CreateBeginnerSpell: Failed to get template from effect broker");
                return new EffectBundleSettings();
            }

            var template = effectBroker.GetEffectTemplate(SummonSkeletonEffect.EffectKey);
            var templateSettings = new EffectSettings()
            {
                ChanceBase = 25,
                ChancePerLevel = 5,
                ChancePlus = 1,
                MagnitudeBaseMin = 1,
                MagnitudeBaseMax = 1,
                MagnitudePerLevel = 1,
                MagnitudePlusMax = 1,
                MagnitudePlusMin = 1
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

            return offer.BundleSetttings;
        }

        private static EffectBundleSettings CreateNoviceRecallSpell()
        {
            // Create a basic recall spell so that they can recall their stuck minions
            var effectBroker = GameManager.Instance.EntityEffectBroker;
            if (!effectBroker.HasEffectTemplate(RecallMinionsEffect.EffectKey))
            {
                ChebError("CreateBeginnerSpell: Failed to get template from effect broker");
                return new EffectBundleSettings();
            }

            var template = effectBroker.GetEffectTemplate(RecallMinionsEffect.EffectKey);
            var templateSettings = new EffectSettings()
            {
                ChanceBase = 25,
                ChancePerLevel = 5,
                ChancePlus = 1,
            };
            var effectEntry = new EffectEntry()
            {
                Key = template.Properties.Key,
                Settings = templateSettings,
            };
            var noviceRecall = new EffectBundleSettings()
            {
                Version = 1,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Novice Recall",
                IconIndex = 12,
                Effects = new EffectEntry[] { effectEntry },
            };
            // add it to stores so it can be purchased
            var offer = new EntityEffectBroker.CustomSpellBundleOffer()
            {
                Key = "NoviceRecall-CustomOffer",
                Usage = EntityEffectBroker.CustomSpellBundleOfferUsage.SpellsForSale,
                BundleSetttings = noviceRecall,
            };
            effectBroker.RegisterCustomSpellBundleOffer(offer);

            return offer.BundleSetttings;
        }

        #region Logging
        // Wrappers for logging so I don't keep being too lazy to write "Cheb's Necromancy" on front of messages and
        // then later be unable to find relevant messages.
        public static void ChebLog(string msg)
        {
            if (Log == Logging.All)
                Debug.Log($"Cheb's Necromancy: {msg}");
        }

        public static void ChebError(string msg)
        {
            if (Log != Logging.None)
                Debug.LogError($"Cheb's Necromancy: {msg}");
        }

        #endregion

        private static void GravediggingActivation(RaycastHit hit)
        {
            var test = new Dictionary<int, int>();
            test.Add(1,1);

            var test2 = new List<KeyValuePair<int, int>>();
            test2.Add(new KeyValuePair<int, int>(1,1));

            var instanceId = hit.collider.gameObject.GetInstanceID();
            if (alreadyLootedGraves.ContainsKey(instanceId))
            {
                DaggerfallUI.AddHUDText("You've already searched this grave.");
                return;
            }

            var random = new Random();

            var successMessages = new List<KeyValuePair<string, DaggerfallUnityItem>>
            {
                new KeyValuePair<string, DaggerfallUnityItem>("You find a skeleton!", null ),
                new KeyValuePair<string, DaggerfallUnityItem>("You find a partially decomposed corpse!", null),
                new KeyValuePair<string, DaggerfallUnityItem>( "This one's fresh!", null),
                new KeyValuePair<string, DaggerfallUnityItem>("Someone has left flowers on this grave! How thoughtful!",
                    ItemBuilder.CreateItem(ItemGroups.PlantIngredients2, (int)PlantIngredients2.White_rose)),
                new KeyValuePair<string, DaggerfallUnityItem>("Someone has left flowers on this grave! How thoughtful!",
                    ItemBuilder.CreateItem(ItemGroups.PlantIngredients2, (int)PlantIngredients2.Black_rose)),
                new KeyValuePair<string, DaggerfallUnityItem>("The skeleton is pristine, and the flesh has crumbled away into a useful dust.",
                    ItemBuilder.CreateItem(ItemGroups.CreatureIngredients1, (int)CreatureIngredients1.Lich_dust)),
                new KeyValuePair<string, DaggerfallUnityItem>("Necrotic juices glow upon this corpse.",
                    ItemBuilder.CreateItem(ItemGroups.CreatureIngredients1, (int)CreatureIngredients1.Ectoplasm)),
                new KeyValuePair<string, DaggerfallUnityItem>("This corpse is well preserved.",
                    ItemBuilder.CreateItem(ItemGroups.UselessItems2, (int)UselessItems2.Oil)),
                new KeyValuePair<string, DaggerfallUnityItem>("The wrappings on this stiff may as well be brand new.",
                    ItemBuilder.CreateItem(ItemGroups.UselessItems2, (int)UselessItems2.Bandage)),
                new KeyValuePair<string, DaggerfallUnityItem>("The grave is recently visited! They left behind fresh roses!",
                    ItemBuilder.CreateItem(ItemGroups.PlantIngredients1, (int)PlantIngredients1.Yellow_rose)),
                new KeyValuePair<string, DaggerfallUnityItem>("The grave is recently visited! They left behind fresh roses!",
                    ItemBuilder.CreateItem(ItemGroups.PlantIngredients1, (int)PlantIngredients1.Red_rose)),
            };
            var failureMessages = new List<KeyValuePair<string, DaggerfallUnityItem>>
            {
                new KeyValuePair<string, DaggerfallUnityItem>("Fribble, just fribble...", null),
                new KeyValuePair<string, DaggerfallUnityItem>("The epitaph reads: Gal bursten it.", null),
                new KeyValuePair<string, DaggerfallUnityItem>("Someone has left flowers on this grave! How thoughtful!",
                    ItemBuilder.CreateItem(ItemGroups.PlantIngredients1, (int)PlantIngredients1.Yellow_rose)),
                new KeyValuePair<string, DaggerfallUnityItem>("Someone has left flowers on this grave! How thoughtful!",
                    ItemBuilder.CreateItem(ItemGroups.PlantIngredients1, (int)PlantIngredients1.Red_rose)),
                new KeyValuePair<string, DaggerfallUnityItem>("Nothing but dust... ", null),
                new KeyValuePair<string, DaggerfallUnityItem>("This one's too brittle...", null),
                new KeyValuePair<string, DaggerfallUnityItem>("Oil in a grave? Why?",
                    ItemBuilder.CreateItem(ItemGroups.UselessItems2, (int)UselessItems2.Oil)),
                new KeyValuePair<string, DaggerfallUnityItem>("Someone tried to bandage this corpse, but failed their skillcheck.",
                    ItemBuilder.CreateItem(ItemGroups.UselessItems2, (int)UselessItems2.Bandage)),
                new KeyValuePair<string, DaggerfallUnityItem>("Picked clean already...", null),
            };
            var player = GameManager.Instance.PlayerEntity;

            var mysticism = Math.Max(1, Math.Min(100, (int)player.Skills.GetLiveSkillValue(DFCareer.Skills.Mysticism)));
            var intelligence = Math.Max(1, Math.Min(100, player.Stats.LiveIntelligence));
            var willpower = Math.Max(1, Math.Min(100, player.Stats.LiveWillpower));

            var totalScore = mysticism + intelligence + willpower;
            var successProbability = totalScore / 3;
            var roll = random.Next(1, 100);
            var success = roll <= successProbability;

            var pick = success
                ? successMessages[random.Next(0, successMessages.Count)]
                : failureMessages[random.Next(0, failureMessages.Count)];
            DaggerfallUI.AddHUDText(pick.Key);
            if (success) player.Items.AddItem(CustomCorpseItem.Create());
            if (pick.Value != null) player.Items.AddItem(pick.Value);

            alreadyLootedGraves.Add(instanceId, 0);
        }

        private static void RegisterCustomActivations(Mod mod)
        {
            // Register custom activations for grave robbing
            //
            // These IDs taken from WorldDataEditorObjectData.cs
            var graveyardModelIDs = new List<uint>()
            {
                // Graveyard Monuments
                43079, 43080, 43081, 43082, 43109, 43110, 43111, 43112, 43202, 43204, 62314,
                // Caskets
                43075, 43076, 43077, 43078, 43304, 43305, 43306,
                // Wooden Coffins
                41315, 41317, 41316, 41318, 41322, 41323, 41325, 41326,
                // Stone Coffins
                41319, 41320, 41321, 41324, 41327
            };
            foreach (var graveyardModelID in graveyardModelIDs)
            {
                PlayerActivate.RegisterCustomActivation(mod, graveyardModelID, GravediggingActivation);
            }
        }
    }
}