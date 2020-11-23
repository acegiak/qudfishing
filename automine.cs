using System;
using System.Text;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_automine: IPart
	{
		public int iTimer = -1;

		public bool bPlayerMine = true;

		public GameObject Explosive;

		public GameObject Owner;

		public string Message = "AfterThrown";

		public bool Armed = true;

		public string ArmedDetailColor = "R";

		public string DisarmedDetailColor = "y";

		public int Mark = 1;

		public void setType(){

			string populationName = "Explosives " + Mark;
			int num = 0;
			while (true)
			{
				string MineType = PopulationManager.RollOneFrom(populationName).Blueprint;
				GameObject gameObject = GameObjectFactory.Factory.CreateSampleObject(MineType);
				bool flag = gameObject.HasPart("Tinkering_Layable");
				if (flag)
				{
					this.Explosive = gameObject;
					break;
				}				gameObject.Destroy();

			}

			this.Arm();


		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "BeforeDestroyObject");
			Object.RegisterPartEvent(this, "CanBeDisassembled");
			Object.RegisterPartEvent(this, "CanBeTaken");
			Object.RegisterPartEvent(this, "CollectBroadcastCharge");
			Object.RegisterPartEvent(this, "EffectApplied");
			Object.RegisterPartEvent(this, "EndTurn");
			Object.RegisterPartEvent(this, "GetDisplayName");
			Object.RegisterPartEvent(this, "GetInventoryActions");
			Object.RegisterPartEvent(this, "GetShortDescription");
			Object.RegisterPartEvent(this, "GetShortDisplayName");
			Object.RegisterPartEvent(this, "InvCommandArm");
			Object.RegisterPartEvent(this, "InvCommandDisarm");
			Object.RegisterPartEvent(this, "InvCommandLay");
			Object.RegisterPartEvent(this, "ObjectCreated");
			Object.RegisterPartEvent(this, "ObjectEnteredCell");
			Object.RegisterPartEvent(this, "QueryBroadcastDraw");
			Object.RegisterPartEvent(this, "TakeDamage");
			Object.RegisterPartEvent(this, "EnteredCell");
			base.Register(Object);
		}

		public void Boom(GameObject who = null)
		{
			if(who==null){
				who=ParentObject;
			}
			GameObject explosive = Explosive;
			Explosive = null;
			ForceDropSelf();
			Cell currentCell = ParentObject.CurrentCell;
			if (currentCell != null)
			{

				currentCell.RemoveObject(ParentObject);
				currentCell.AddObject(explosive);
				if (explosive.pRender != null && ParentObject.pRender != null)
				{
					explosive.pRender.DisplayName = ParentObject.pRender.DisplayName;
				}
				Event @event = Event.New(Message);
				if (Owner != null && Owner.IsValid())
				{
					@event.AddParameter("Owner", who);
				}
				explosive.FireEvent(@event);
			}
			ParentObject.Destroy();
		}

		public bool AttemptArm(GameObject who)
		{
			if (Armed)
			{
				return false;
			}
			if (who.IsPlayer() && !ParentObject.Understood())
			{
				return false;
			}
			if (!who.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			Arm(who);
			string verb = "arm";
			GameObject parentObject = ParentObject;
			IPart.XDidYToZ(who, verb, parentObject);
			who.UseEnergy(1000, "Tinkering " + ((iTimer <= 0) ? "Mine" : "Bomb") + " Arm");
			return true;
		}

		public bool AttemptLay(GameObject who)
		{
			if (ParentObject.InInventory != who)
			{
				return false;
			}
			if (Armed)
			{
				return false;
			}
			if (who.IsPlayer() && !ParentObject.Understood())
			{
				return false;
			}
			if (who.OnWorldMap())
			{
				if (who.IsPlayer())
				{
					Popup.Show("You cannot do that on the world map.");
				}
				return false;
			}
			if (!who.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			Cell cell;
			if (who.IsPlayer())
			{
				string text = XRL.UI.PickDirection.ShowPicker();
				if (text == null)
				{
					return false;
				}
				cell = who.CurrentCell.GetCellFromDirection(text);
			}
			else
			{
				cell = who.CurrentCell.GetEmptyAdjacentCells().GetRandomElement();
			}
			if (cell == null || !cell.IsEmpty())
			{
				if (who.IsPlayer())
				{
					Popup.Show("You can't deploy there!");
				}
				return false;
			}
			GameObject gameObject = ParentObject.RemoveOne();
			gameObject.RemoveFromContext();
			gameObject.GetPart<acegiak_automine>().Arm(who);
			if (who.IsPlayer())
			{
				IPart.XDidY(who, (iTimer <= 0) ? "lay" : "set", gameObject.the + gameObject.DisplayName);
			}
			else if (who.IsVisible())
			{
				IPart.XDidY(who, (iTimer <= 0) ? "lay" : "set", gameObject.a + gameObject.DisplayName);
			}
			cell.AddObject(gameObject);
			who.UseEnergy(1000, "Tinkering " + ((iTimer <= 0) ? "Bomb Set" : "Mine Lay"));
			return true;
		}

		public bool AttemptDisarm(GameObject who)
		{
			if (!Armed)
			{
				return false;
			}
			if (who.IsPlayer() && !ParentObject.Understood())
			{
				return false;
			}
			if (!who.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			int num = 9 + Explosive.GetMark() * 3;
			if (who.HasSkill("Tinkering_GadgetInspector"))
			{
				num -= 4;
			}
			if (who.HasSkill("Tinkering_LayMine"))
			{
				num -= 4;
			}
			string text = (iTimer > 0) ? "Tinkering Bomb Disarm" : "Tinkering Mine Disarm";
			string stat = "Intelligence";
			int difficulty = num;
			string vs = text;
			int num2 = who.SaveChance(stat, difficulty, null, null, vs);
			int num3 = num2;
			int num4 = who.Stat("Intelligence");
			Mutations part = who.GetPart<Mutations>();
			if (part != null)
			{
				if (part.HasMutation("Intuition"))
				{
					num4 += 10;
				}
				if (part.HasMutation("Precognition"))
				{
					num4 += 5;
				}
				if (part.HasMutation("Skittish"))
				{
					num3 -= 10;
				}
			}
			if (who.HasSkill("Discipline_IronMind"))
			{
				num4 += 2;
			}
			if (who.HasSkill("Discipline_Lionheart"))
			{
				num3 += 3;
			}
			if (num4 <= 10)
			{
				num3 += 20;
			}
			else if (num4 <= 15)
			{
				num3 += 10;
			}
			else if (num4 <= 20)
			{
				num3 -= 10;
			}
			else if (num4 <= 25)
			{
				num3 -= 5;
			}
			int num5 = (num4 <= 10) ? 20 : ((num4 <= 20) ? 10 : ((num4 > 30) ? 1 : 5));
			if (num5 > 1)
			{
				num3 -= num3 % num5;
			}
			if (num3 > 100 - num5)
			{
				num3 = 100 - num5;
			}
			if (num3 < 0)
			{
				num3 = 0;
			}
			if (who.IsPlayer())
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				string chanceColor = Stat.GetChanceColor(num3);
				stringBuilder.Append("&yFailing to disarm ").Append(ParentObject.the).Append(ParentObject.ShortDisplayName)
					.Append("&y will detonate ")
					.Append(ParentObject.it)
					.Append(". You estimate you have");
				if (num3 < num5)
				{
					stringBuilder.Append(" less than a ").Append(chanceColor).Append(num5.ToString())
						.Append("%");
				}
				else
				{
					stringBuilder.Append(" about a ").Append(chanceColor).Append(num3.ToString())
						.Append("%");
				}
				stringBuilder.Append(chanceColor).Append("&y chance of success. Do you want to make the attempt?");
				if (Popup.ShowYesNo(stringBuilder.ToString()) != 0)
				{
					return false;
				}
			}
			else if (Stat.Random(1, 100) > num3 && Stat.Random(1, 100) > num3)
			{
				return false;
			}
			vs = "Intelligence";
			difficulty = num;
			stat = text;
			if (who.MakeSave(vs, difficulty, null, null, stat))
			{
				stat = "disarm";
				GameObject parentObject = ParentObject;
				IPart.XDidYToZ(who, stat, parentObject);
				Disarm();
			}
			else
			{
				Boom();
			}
			who.UseEnergy(1000, "Tinkering Mine Disarm");
			return true;
		}

		public void Disarm()
		{
			Armed = false;
			if (!string.IsNullOrEmpty(DisarmedDetailColor) && ParentObject.pRender != null)
			{
				ParentObject.pRender.DetailColor = DisarmedDetailColor;
			}
		}

		public void Arm(GameObject who = null)
		{
			Armed = true;
			Owner = who;
			bPlayerMine = (Owner != null && Owner.IsPlayer());
			if (!string.IsNullOrEmpty(ArmedDetailColor) && ParentObject.pRender != null)
			{
				ParentObject.pRender.DetailColor = ArmedDetailColor;
			}
		}

		public override bool FireEvent(Event E)
		{

			if (E.ID == "EnteredCell" && this.Explosive == null)
			{
				this.setType();
			}

			if (E.ID == "EndTurn")
			{
				if (Armed && iTimer > 0)
				{
					iTimer--;
					if (iTimer <= 0)
					{
						Boom();
					}
				}
			}
			else if (E.ID == "CollectBroadcastCharge" || E.ID == "QueryBroadcastDraw")
			{
				if (Explosive != null && !Explosive.FireEvent(E))
				{
					return false;
				}
			}
			else if (E.ID == "ObjectEnteredCell")
			{
				if (Armed && iTimer <= 0)
				{
					GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
					if (Explosive != null && gameObjectParameter != null && gameObjectParameter.HasPart("Combat") && gameObjectParameter.HasPart("Brain") && gameObjectParameter.PhaseAndFlightMatches(ParentObject))
					{
					
							Boom(gameObjectParameter);
						
					}
				}
			}
			else if (E.ID == "GetShortDescription")
			{
				if (Explosive != null)
				{
					foreach (IModification item in Explosive.GetPartsDescendedFrom<IModification>())
					{
						item.FireEvent(E);
					}
				}
				if (ParentObject.Understood())
				{
					if (Armed)
					{
						if (iTimer > 0)
						{
							E.SetParameter("Infix", E.GetParameter("Infix") + "\n" + ParentObject.Ithas + " " + Grammar.Cardinal(iTimer) + ((iTimer != 1) ? "seconds" : "second") + " left on " + ParentObject.its + " timer.");
						}
					}
					else
					{
						E.SetParameter("Infix", E.GetParameter("Infix") + "\n" + ParentObject.Ithas + " been disarmed.");
					}
				}
			}
			else if (E.ID == "GetDisplayName" || E.ID == "GetShortDisplayName")
			{
				if (Explosive != null)
				{
					foreach (IModification item2 in Explosive.GetPartsDescendedFrom<IModification>())
					{
						item2.FireEvent(E);
					}
				}
				if (ParentObject.Understood())
				{
					if (Armed)
					{
						if (iTimer > 0)
						{
							E.GetParameter<StringBuilder>("Postfix").Append(" &y[&R").Append(iTimer)
								.Append(" sec&y]");
						}
					}
					else
					{
						E.GetParameter<StringBuilder>("Prefix").Append("&ydisarmed ");
					}
				}
			}
			else if (E.ID == "CanBeTaken" || E.ID == "CanBeDisassembled")
			{
				if (Armed)
				{
					return false;
				}
			}
			else if (E.ID == "EffectApplied")
			{
				if (Armed)
				{
					if (IsEMPed())
					{
						Disarm();
					}
					else if (IsBroken() || IsRusted())
					{
						Boom();
					}
				}
			}
			else if (E.ID == "TakeDamage")
			{
				if (Armed)
				{
					Boom();
				}
			}
			else if (E.ID == "GetInventoryActions")
			{
				if (ParentObject.Understood())
				{
					if (ParentObject.CurrentCell != null && !ParentObject.OnWorldMap())
					{
						if (Armed)
						{
							E.AddInventoryAction("Disarm", 'd', false, "&Wd&yisarm", "InvCommandDisarm", 100, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
						}
						else
						{
							E.AddInventoryAction("Arm", 'a', false, "&Wa&yrm", "InvCommandArm", 100, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
						}
					}
					else if (ParentObject.InInventory != null && ParentObject.InInventory.IsPlayer() && !ParentObject.InInventory.OnWorldMap())
					{
						if (iTimer > 0)
						{
							E.AddInventoryAction("Set", 's', false, "&Ws&yet", "InvCommandLay", 100, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
						}
						else
						{
							E.AddInventoryAction("Lay", 'L', false, "&WL&yay", "InvCommandLay", 100, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
						}
					}
				}
			}
			else if (E.ID == "BeforeDestroyObject")
			{
				if (Explosive != null && Explosive.IsValid())
				{
					Explosive.Destroy();
				}
			}
			else if (E.ID == "InvCommandArm")
			{
				if (AttemptArm(E.GetGameObjectParameter("Owner")))
				{
					E.RequestInterfaceExit();
				}
			}
			else if (E.ID == "InvCommandLay")
			{
				if (AttemptLay(E.GetGameObjectParameter("Owner")))
				{
					E.RequestInterfaceExit();
				}
			}
			else if (E.ID == "InvCommandDisarm")
			{
				if (AttemptDisarm(E.GetGameObjectParameter("Owner")))
				{
					E.RequestInterfaceExit();
				}
			}
			else if (E.ID == "ObjectCreated")
			{
				Disarm();
			}
			return base.FireEvent(E);
		}
	}

    
}