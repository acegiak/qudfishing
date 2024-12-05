using System;
using XRL.Core;
using XRL.UI;
using System.Linq;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;
using UnityEngine;
using XRL.World.ZoneBuilders;


namespace XRL.World.Parts.Skill
{
	[Serializable]
	internal class acegiak_CookingAndGathering_Fishing : BaseSkill
	{
		//public Guid ButcherActivatedAbilityID = Guid.Empty;

		[NonSerialized]
		public XRL.World.GameObject fishinHole = null;
		public int turnCount = 0;

		public acegiak_CookingAndGathering_Fishing()
		{
			//DisplayName = "acegiak_CookingAndGathering_Fishing";
		}

		public override void Register(XRL.World.GameObject Object)
		{
			Object.RegisterPartEvent(this, "StartFishing");
			Object.RegisterPartEvent(this, "StopFishing");
			Object.RegisterPartEvent(this, "AddedToInventory");
			Object.RegisterPartEvent(this, "UsingEnergy");
			base.Register(Object);
		}

		public override bool FireEvent(Event E)
		{
			//Debug.Log("Event: "+E.ID+".");

			if (E.ID == "StartFishing"){
				//Debug.Log("startfishevent.");
				//Popup.Show("You cast a line.");
                fishinHole = E.GetGameObjectParameter("Pool");
            }
            if (E.ID == "StopFishing"){
				//Debug.Log("stopfishevent.");

                if(fishinHole != null){
					IPart.AddPlayerMessage("You stop fishing.");
                }
                fishinHole = null;
            }
			if (E.ID == "UsingEnergy"){
				//IPart.AddPlayerMessage("use energy.");
				//IPart.AddPlayerMessage(E.GetStringParameter("Type", string.Empty));

                 if(E.GetStringParameter("Type", string.Empty) == "Pass" || E.GetStringParameter("Type", string.Empty) == string.Empty || E.GetStringParameter("Type", string.Empty) == "Fishing"|| E.GetStringParameter("Type", string.Empty) == "None"){

					 //Debug.Log("passturn.");
                     if(fishinHole != null){
						if(fishinHole.GetPart<acegiak_Fishable>().Epic != null){
							IPart.AddPlayerMessage("The line goes slack.");
						}
						fishinHole.GetPart<acegiak_Fishable>().fromCell = null;
						fishinHole.GetPart<acegiak_Fishable>().Epic = null;
                        fishinHole.FireEvent(Event.New("InvCommandContinueFish", "Owner", ParentObject,"Count",++turnCount));
                     }
                 }else{
					//Debug.Log("notfish.");

                     if(fishinHole != null){
 						//Debug.Log("stopfish.");

						if(fishinHole.GetPart<acegiak_Fishable>() == null ||  fishinHole.GetPart<acegiak_Fishable>().Epic == null){
                        	IPart.AddPlayerMessage("You stop fishing.");
                     		fishinHole = null;

						}else{
							if(fishinHole.GetPart<acegiak_Fishable>().Epic.HasStat("Strength") && fishinHole.GetPart<acegiak_Fishable>().Epic.MakeSave("Strength",1,ParentObject,"Strength")){
								if(ParentObject.CurrentCell != fishinHole.GetPart<acegiak_Fishable>().fromCell){
									CombatJuice.punch(fishinHole.GetPart<acegiak_Fishable>().fromCell.Location,ParentObject.CurrentCell.location);
									fishinHole.GetPart<acegiak_Fishable>().fromCell.AddObject(ParentObject);
									//ParentObject.CurrentCell = fishinHole.GetPart<acegiak_Fishable>().fromCell;
									IPart.AddPlayerMessage("You strain at the line!");
								}
							}else{
								Popup.Show("You reel in "+fishinHole.GetPart<acegiak_Fishable>().Epic.the+fishinHole.GetPart<acegiak_Fishable>().Epic.DisplayNameOnly+".");
								fishinHole.GetPart<acegiak_Fishable>().Epic.AwardXPTo(ParentObject,false,"Catch");
								fishinHole.GetPart<acegiak_Fishable>().fromCell.AddObject(fishinHole.GetPart<acegiak_Fishable>().Epic);
								fishinHole.GetPart<acegiak_Fishable>().fromCell = null;
								fishinHole.GetPart<acegiak_Fishable>().Epic = null;
								fishinHole = null;
							}
						}
                     }
                 }
            }
			//Debug.Log("End event: "+E.ID+".");

			return base.FireEvent(E);
		}

		public override bool AddSkill(XRL.World.GameObject GO)
		{
			return true;
		}

		public override bool RemoveSkill(XRL.World.GameObject GO)
		{
			return true;
		}
	}
}
