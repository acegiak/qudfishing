using System;
using XRL.Core;
using XRL.Messages;
using XRL.UI;
using Qud.API;


namespace XRL.World.Parts.Effects
{
	[Serializable]
	public class acegiak_InstantSecret : Effect
	{
		public acegiak_InstantSecret()
		{
			base.DisplayName = "&Kinsightful";
		}

		public acegiak_InstantSecret(int _Duration)
			: this()
		{
			Duration = 0;
		}

		public override string GetDetails()
		{
			return "A moment of insight.";
		}

		public override bool Apply(GameObject Object)
		{

            if (Object.IsPlayer())
            {
                Popup.Show("You gain a moment of insight...");
                JournalAPI.RevealRandomSecret();
                return true;
            }
            
        
			return false;
		}



	}
}
