using System;
using XRL.Core;
using XRL.UI;
using System.Linq;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.World;
using XRL.World.Encounters;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;
using XRL.World.Parts.Skill;
using XRL.World.ZoneBuilders;


namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_Fishable : IPart
	{
		[NonSerialized]
		public GameObject Epic;

		[NonSerialized]
		public Cell fromCell;



		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "CommandSmartUse");
			Object.RegisterPartEvent(this, "CanHaveSmartUseConversation");
			Object.RegisterPartEvent(this, "CanSmartUse");
			Object.RegisterPartEvent(this, "GetInventoryActions");
			Object.RegisterPartEvent(this, "InvCommandFish");
			Object.RegisterPartEvent(this, "InvCommandContinueFish");
			base.Register(Object);
		}

		public bool Fish(GameObject who,int count = 0)
		{
            ParentObject.Splash(ConsoleLib.Console.ColorUtility.StripBackgroundFormatting(ParentObject.pRender.ColorString + "."));
            int sittingMod = who.HasEffect("Sitting")?10:0;
			
			int skillMod = !who.HasSkill("acegiak_CookingAndGathering_Fishing")?0:who.Stat("Wisdom")/2;


			GameObject caught = null;
			
			if(who.HasSkill("acegiak_CookingAndGathering_Fishing")){
				if(ParentObject.pPhysics != null && ParentObject.pPhysics.CurrentCell != null && ParentObject.pPhysics.CurrentCell.ParentZone != null && !ParentObject.pPhysics.CurrentCell.ParentZone.IsWorldMap()){

					//IPart.AddPlayerMessage("Fishing_"+ParentObject.pPhysics.CurrentCell.ParentZone.NameContext);
					if(ParentObject.pPhysics.CurrentCell.ParentZone.NameContext != null && ParentObject.pPhysics.CurrentCell.ParentZone.NameContext != String.Empty){
						caught = EncounterFactory.Factory.RollOneFromTable("Fishing_"+ParentObject.pPhysics.CurrentCell.ParentZone.NameContext);
					}
					if(caught == null){
						//IPart.AddPlayerMessage("Fishing_"+ParentObject.pPhysics.CurrentCell.ParentZone.GetRegion());
						caught = EncounterFactory.Factory.RollOneFromTable("Fishing_"+ParentObject.pPhysics.CurrentCell.ParentZone.GetRegion());
					}

				}
				if(caught == null){
					caught = EncounterFactory.Factory.RollOneFromTable("Fishing");
					//IPart.AddPlayerMessage("backup:"+caught.DisplayName);
				}else{

					//IPart.AddPlayerMessage("got:"+caught.DisplayName);
				}
			}
            if(Stat.Roll("1d100")+who.StatMod("Agility")+sittingMod+skillMod > 95){

				if(caught.HasTag("EpicFish") || true){
					if(caught.HasTag("LegendaryReel") && Stat.Rnd2.Next(1,100) < Int32.Parse(caught.GetTag("LegendaryReel"))){
						GameObject g =  heroify(caught);
						if(g == null){
							IPart.AddPlayerMessage("Couldn't heroify");
						}else{
							caught = g;
						}
					}
					this.Epic = caught;
					this.fromCell = who.CurrentCell;
					who.ParticleText("&R!");
					IPart.AddPlayerMessage("There is a tug on your line.");
				}else{
					if(caught.GetPart<Brain>() != null){
						var rndGen = new Random();
						ParentObject.pPhysics.CurrentCell.GetAdjacentCells(1).ElementAt(rndGen.Next(ParentObject.pPhysics.CurrentCell.GetAdjacentCells(1).Count)).AddObject(caught);
						XRLCore.Core.Game.ActionManager.AddActiveObject(caught);

					}else{
						who.GetPart<Inventory>().AddObject(caught);
					}
					Popup.Show("You reel in "+caught.a+caught.DisplayName+"!");
					caught.AwardXPTo(who,false,"Catch");
					who.FireEvent(Event.New("StopFishing"));
					return true;
				}
            }
			if(!who.HasSkill("acegiak_CookingAndGathering_Fishing")){
				IPart.AddPlayerMessage("But nothing bites.");
			}
			return false;
		}

		public GameObject heroify(GameObject gameObject){
			AnimateObject.Animate(gameObject);
			gameObject.pRender.DisplayName = gameObject.pRender.DisplayName.Replace("animated ","");
			gameObject.pBrain.FactionMembership.Add("Fish", 100);
			gameObject.pBrain.FactionMembership["Newly Sentient Beings"] = 10;
			gameObject = HeroMaker.MakeHero(gameObject);
			return gameObject;
		}

		public bool CheckRod(GameObject who)
		{
            if(who == null){
                // Popup.Show("You dont exist.");
                return false;
            }
			Body part = who.GetPart<Body>();
            if(part == null){
                // Popup.Show("You have no body.");
                return false;
            }
			List<BodyPart> equippedParts = part.GetEquippedParts();
			foreach (BodyPart item in equippedParts)
			{
				if(item.Equipped.HasTag("FishingRod")){

                    // Popup.Show("You have a rod.");
                    return true;
                }
			}
			if(who.GetPart<Inventory>() != null){
				foreach (GameObject obj in who.GetPart<Inventory>().GetObjects()){
					if(obj.HasTag("FishingRod")){

                    // Popup.Show("You have a rod.");
                    return true;
                }
				}

			}
            // Popup.Show("You don't have a rod.");
			return false;
		}

		public override bool FireEvent(Event E)
		{


            LiquidVolume liquidVolume = ParentObject.GetPart<LiquidVolume>();
            if(liquidVolume.Volume < 1000 && !liquidVolume.ContainsSignificantLiquid("water")){
                return false;
            }

			if (E.ID == "CanSmartUse")
			{
                if(!CheckRod(E.GetGameObjectParameter("User"))){
                    return false;
                }
				if ((!E.GetGameObjectParameter("User").IsPlayer() || !ParentObject.IsPlayerLed()) && !liquidVolume.IsFreshWater())
				{
					return false;
				}
			}
			else if (E.ID == "CanHaveSmartUseConversation")
			{
                if(!CheckRod(E.GetGameObjectParameter("User"))){
                    return false;
                }
				if (!liquidVolume.IsFreshWater())
				{
					return false;
				}
			}
			else if (E.ID == "CommandSmartUse")
			{
                if(!CheckRod(E.GetGameObjectParameter("User"))){
                    return false;
                }
				if (!E.GetGameObjectParameter("User").IsPlayer() || !ParentObject.IsPlayerLed())
				{
                    ParentObject.FireEvent(new Event("InvCommandFish", "Owner", E.GetGameObjectParameter("User")));
				}
			}
			else if (E.ID == "GetInventoryActions")
			{
                if(!CheckRod(XRLCore.Core.Game.Player.Body)){
                    return false;
                }
				E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("Fish", 'f',  false, "&Wf&yish", "InvCommandFish", Default: 10);
			}
			if (E.ID == "InvCommandFish")
			{
				if(E.GetGameObjectParameter("Owner").IsPlayer()){
					IPart.AddPlayerMessage("You cast a line.");
					// IPart.AddPlayerMessage("Water in "+ParentObject.pPhysics.CurrentCell.ParentZone.DisplayName);
					// IPart.AddPlayerMessage("Water in "+ParentObject.pPhysics.CurrentCell.ParentZone.BaseDisplayName);
					// IPart.AddPlayerMessage("Water in "+ParentObject.pPhysics.CurrentCell.ParentZone.ReferenceDisplayName);
					// IPart.AddPlayerMessage("Water in "+ParentObject.pPhysics.CurrentCell.ParentZone.NameContext);

				}
                E.GetGameObjectParameter("Owner").FireEvent(Event.New("StartFishing", "Pool", ParentObject, "Fisher", E.GetGameObjectParameter("User")));
                Fish(E.GetGameObjectParameter("Owner"));
				E.RequestInterfaceExit();
			}
			if (E.ID == "InvCommandContinueFish")
			{
                Fish(E.GetGameObjectParameter("Owner"),E.GetIntParameter("Count"));
            }
			return base.FireEvent(E);
		}
	}
}
