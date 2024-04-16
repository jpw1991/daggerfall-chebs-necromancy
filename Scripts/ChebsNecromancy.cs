using System;
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
        public const string NecromancerCareerName = "Necromancer";

        private static Mod mod;
        public static EffectBundleSettings AnimateDeadSpell;
        public static bool EnableCustomClassNecromancer = true;

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

            AnimateDeadSpell = CreateAnimateDeadSpell();

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Starting Cheb");

            if (!EnableCustomClassNecromancer)
            {
                Debug.Log("Custom Class Necromancer disabled.");
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
                Debug.LogError("Failed to get DaggerfallUI");
                return;
            }

            daggerfallUi.UserInterfaceManager.OnWindowChange += HijackWizard;
        }

        public static DFCareer GenerateNecromancerCareer()
        {
            return new DFCareer()
                {
                    Name = NecromancerCareerName,
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
        }

        void HijackWizard(object sender, EventArgs e)
        {
            // Check if window is a StartNewGameWizard window and if it is, close it and open our custom one.
            var uiManager = (UserInterfaceManager)sender;
            var window = uiManager.TopWindow.Value;
            if (window is StartNewGameWizard)
            {
                Debug.Log("StartNewGameWizard opening, close it.");
                window.CloseWindow();
                Debug.Log("StartNewGameWizard closed, opening CustomStartNewGameWizard.");
                uiManager.PushWindow(new CustomStartNewGameWizard(uiManager));
            }
        }

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            EnableCustomClassNecromancer = modSettings.GetBool("Custom Class", "Necromancer");

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

        private static EffectBundleSettings CreateAnimateDeadSpell()
        {
            // In DFU a spell is called an effect bundle - basically a bundle of spell effects (makes sense). Here we
            // create a beginner spell for people so they have some minions to start off with if they wish.
            var effectBroker = GameManager.Instance.EntityEffectBroker;
            if (!effectBroker.HasEffectTemplate(SummonSkeletonEffect.EffectKey))
            {
                Debug.LogError("Cheb's Necromancy: CreateBeginnerSpell: Failed to get template from effect broker");
                return new EffectBundleSettings();
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

            return offer.BundleSetttings;
        }
    }
}