using System;
using System.Text;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_fishvendorhotloader: IPart
	{
        public acegiak_fishvendorhotloader(){

            AddToPopTable("Ingredients_MidTiers", new PopulationTable { Name = "FishIngredients", Weight = 10, Number="1" });
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