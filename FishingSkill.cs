using System;
using XRL.Core;
using XRL.UI;
using System.Linq;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Parts.Effects;


namespace XRL.World.Parts.Skill
{
	[Serializable]
	internal class acegiak_CookingAndGathering_Fishing : BaseSkill
	{
		//public Guid ButcherActivatedAbilityID = Guid.Empty;

		public GameObject fishinHole = null;
		public int turnCount = 0;

		public acegiak_CookingAndGathering_Fishing()
		{
			//DisplayName = "acegiak_CookingAndGathering_Fishing";
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "StartFishing");
			Object.RegisterPartEvent(this, "StopFishing");
			Object.RegisterPartEvent(this, "AddedToInventory");
			Object.RegisterPartEvent(this, "UseEnergy");
			base.Register(Object);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "StartFishing"){
				//Popup.Show("You cast a line.");
                fishinHole = E.GetGameObjectParameter("Pool");
            }
            if (E.ID == "StopFishing"){
                if(fishinHole != null){
					IPart.AddPlayerMessage("You stop fishing.");
                }
                fishinHole = null;
            }
			if (E.ID == "UseEnergy"){
                 if(E.GetStringParameter("Type", string.Empty) == "Pass" || E.GetStringParameter("Type", string.Empty) == string.Empty || E.GetStringParameter("Type", string.Empty) == "Fishing"|| E.GetStringParameter("Type", string.Empty) == "None"){
                     if(fishinHole != null){
                         fishinHole.FireEvent(Event.New("InvCommandContinueFish", "Owner", ParentObject,"Count",++turnCount));
                     }
                 }else{
                     if(fishinHole != null){
                        Popup.Show("You stop fishing.");
                     }
                     fishinHole = null;
                 }
            }
			return base.FireEvent(E);
		}

		public override bool AddSkill(GameObject GO)
		{
			return true;
		}

		public override bool RemoveSkill(GameObject GO)
		{
			return true;
		}
	}
}
