using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WM.SelfLaunchingPods
{

	public abstract class Command_Launch : Command_Action
	{
		public override string Label
		{
			get
			{
				return ("WM.LaunchCaravanGizmo".Translate());
			}
		}

		public int MaxLaunchDistanceOneWay
		{
			get
			{
				return (TravelingPodsUtils.MaxLaunchDistance(ParentLeastFueledPodFuelLevel, 1, true));
			}
		}

		public int MaxLaunchDistanceRoundTrip
		{
			get
			{
				return (TravelingPodsUtils.MaxLaunchDistance(ParentLeastFueledPodFuelLevel, 1, false));
			}
		}

		public abstract float ParentLeastFueledPodFuelLevel
		{
			get;
		}

		public abstract int ParentTile
		{
			get;
		}

		public abstract int ParentPodsCount
		{
			get;
		}

		//TODO: rearrange destination validation
		internal Command_Launch()
		{
			this.icon = Resources.LaunchCommandTex;

			action = delegate
			{
				try
				{
					CameraJumper.TryJump(CameraJumper.GetWorldTarget(new GlobalTargetInfo(ParentTile)));
					Find.WorldSelector.ClearSelection();

					Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, Resources.LaunchCommandTex, false,
														delegate
														{
															GenDraw.DrawWorldRadiusRing(ParentTile, MaxLaunchDistanceOneWay);
															GenDraw.DrawWorldRadiusRing(ParentTile, MaxLaunchDistanceRoundTrip);
														},
														  delegate (GlobalTargetInfo target)
														{
															if (!target.IsValid)
															{
																return (null);
															}
															int num = Find.WorldGrid.TraversalDistanceBetween(ParentTile, target.Tile);

															if (num <= MaxLaunchDistanceOneWay)
															{
																return (null);
															}
															return ("TransportPodNotEnoughFuel".Translate());
														});
				}
				catch (Exception ex)
				{
					Log.Error("Error while trying to target destination: " + ex.Message + "\n" + ex.StackTrace);
				}
			};
		}

		internal abstract void Launch(int tile, IntVec3 cell, PawnsArriveMode arriveMode = PawnsArriveMode.Undecided, bool attackOnArrival = false);

		bool ChoseWorldTarget(GlobalTargetInfo target)
		{
			if (!target.IsValid)
			{
				Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageSound.RejectInput);
				return (false);
			}
			int num = Find.WorldGrid.TraversalDistanceBetween(this.ParentTile, target.Tile);

			if (num > this.MaxLaunchDistanceOneWay)
			{
				Messages.Message(
							string.Format(
									"MessageTransportPodsDestinationIsTooFar".Translate(),
									TravelingPodsUtils.FuelNeededToLaunchAtDistance(num, this.ParentPodsCount)),
							MessageSound.RejectInput);

				return (false);
			}
			MapParent mapParent = target.WorldObject as MapParent;

			if (mapParent == null)
			{
				Launch(target.Tile, target.Cell);
			}
			else
			{
				Action<LocalTargetInfo> targeter;

				if (mapParent.HasMap)
				{
					if (!CameraJumper.TryHideWorld())
						throw new Exception("CameraJumper.TryHideWorld() failed.");

					Current.Game.VisibleMap = mapParent.Map;
					targeter = delegate (LocalTargetInfo localTarget)
					{
						Launch(target.Tile, localTarget.Cell);
					};

					Find.Targeter.BeginTargeting(TargetingParameters.ForDropPodsDestination(), targeter, null, null, Resources.LaunchCommandTex);
				}
				else if (mapParent.TransportPodsCanLandAndGenerateMap)
				{
					var list = new List<FloatMenuOption>();
					Settlement settlement = mapParent as Settlement;

					if (settlement != null && settlement.Visitable)
					{
						list.Add(new FloatMenuOption("VisitSettlement".Translate(new object[] { settlement.Label }), delegate
												{
														//if (!this.LoadingInProgressOrReadyToLaunch)
														//	return;
														this.Launch(target.Tile, target.Cell, PawnsArriveMode.Undecided, false);
													CameraJumper.TryHideWorld();
												}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}

					list.Add(new FloatMenuOption("DropAtEdge".Translate(), delegate
					{
							//if (!this.LoadingInProgressOrReadyToLaunch)
							//	return;
							this.Launch(target.Tile, target.Cell, PawnsArriveMode.EdgeDrop, true);
						//CameraJumper.TryHideWorld();
					}, MenuOptionPriority.Default, null, null, 0f, null, null));

					list.Add(new FloatMenuOption("DropInCenter".Translate(), delegate
					{
							//if (!this.LoadingInProgressOrReadyToLaunch)
							//	return;
							this.Launch(target.Tile, target.Cell, PawnsArriveMode.CenterDrop, true);
						//CameraJumper.TryHideWorld();
					}, MenuOptionPriority.Default, null, null, 0f, null, null));

					Find.WindowStack.Add(new FloatMenu(list));
				}
				else
				{
					throw new Exception("Error when choosing pods destination.");
				}
			}

			return (true);
		}
	}
}