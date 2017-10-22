using System.Linq;
using RimWorld.Planet;

namespace WM.SelfLaunchingPods
{
	public class ModController : HugsLib.ModBase
	{
		private static readonly string modName = "WM_Self_Launching_Pods";
		internal static readonly float LandingSpotMaxRange = 7f;
		internal static ModController SingleInstance { get; private set; }

		public override string ModIdentifier
		{
			get
			{
				return (modName);
			}
		}

		public static float PodFuelUsePerTile
		{
			get
			{
				var t = DefOf.WM_TransportPod.comps.First((obj) => obj.GetType() == typeof(CompProperties_SelfLaunchable)) as CompProperties_SelfLaunchable;

				return (t.fuelUsePerTile);
			}
		}

		public static float PodFuelUsePerLaunch
		{
			get
			{
				var t = DefOf.WM_TransportPod.comps.First((obj) => obj.GetType() == typeof(CompProperties_SelfLaunchable)) as CompProperties_SelfLaunchable;

				return (t.fuelUsePerLaunch);
			}
		}

		public override void Initialize()
		{
			base.Initialize();
			SingleInstance = this;
		}

		public override void DefsLoaded()
		{
			base.DefsLoaded();
			DefOf.Caravan.comps.Add(new CaravanTranferCompProperties());
			DefOf.CommsConsole.comps.Add(new CommsRemoteTradeCompProperties());
		}

		public override void Update()
		{
			base.Update();
			if (!WorldRendererUtility.WorldRenderedNow)
				WorldOverlay.Draw();
		}

		internal static class Log
		{
			internal static void Message(string text)
			{
				SingleInstance.Logger.Message(text);
			}
			internal static void Warning(string text)
			{
				SingleInstance.Logger.Warning(text);
			}
			internal static void Error(string text)
			{
				SingleInstance.Logger.Error(text);
			}
		}
	}

	internal static class Log
	{
		internal static void Message(string text)
		{
			ModController.Log.Message(text);
		}
		internal static void Warning(string text)
		{
			ModController.Log.Warning(text);
		}
		internal static void Error(string text)
		{
			ModController.Log.Error(text);
		}
	}
}
