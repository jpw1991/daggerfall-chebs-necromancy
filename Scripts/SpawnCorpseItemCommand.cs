using DaggerfallWorkshop.Game;

namespace ChebsNecromancyMod
{
    public static class SpawnCorpseItemCommand
    {
        public static readonly string name = "sci";
        public static readonly string description = "Spawn some corpse items and add them to your inventory.";
        public static readonly string usage = "sci <amount=1> eg. sci for 1 corpse, sci 5 for 5 corpses";

        public static string Execute(params string[] args)
        {
            var amount = 1;
            if (args.Length > 1)
            {
                if (!int.TryParse(args[0], out amount)) return "Invalid argument.";
            }

            for (var i=0; i<amount; i++)
                GameManager.Instance.PlayerEntity.Items.AddItem(CustomCorpseItem.Create());//, ItemCollection.AddPosition.Back, true);

            return "";
        }
    }
}


