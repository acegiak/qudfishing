using System;
using System.Text;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts


  
{
	
	[Serializable]
	[WantLoadBlueprint]
	[HasGameBasedStaticCache]
	public class acegiak_fishvendorhotloader: IPart
	{
		static bool popped = false;
        public acegiak_fishvendorhotloader(){}

        [GameBasedCacheInit]
        public static void HotLoad(){
			if(!popped){
            AddToPopTable("Ingredients_MidTiers", new PopulationTable { Name = "FishIngredients", Weight = 10, Number="1" });
            AddToPopTable("Tier1Wares", new PopulationObject { Blueprint = "FishingRod", Number="1", Chance="30" });

                popped = true;
			}
			
        }

        public static bool AddToPopTable(string table, params PopulationItem[] items) {
            PopulationInfo info;
            if (!PopulationManager.Populations.TryGetValue(table, out info))
                return false;
                
            // If this is a single group population, add to that group.
            if (info.Items.Count == 1 && info.Items[0] is PopulationGroup) { 
                var group = info.Items[0] as PopulationGroup;
                group.Items.AddRange(items);
                return true;
            }

            info.Items.AddRange(items);
            return true;
        }

    }
}