using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Xml.Linq;

using ModApi;
using ModApi.Craft;
using ModApi.Craft.Parts;
using ModApi.Craft.Parts.Modifiers;
using ModApi.Flight.GameView;
using ModApi.Flight.Sim;
using ModApi.Mods;
using ModApi.Scenes;
using ModApi.Settings.Core;
using ModApi.State;
using ModApi.Ui;

using Assets.Packages.DevConsole;
using Assets.Scripts.Craft;
using Assets.Scripts.Craft.Parts;
using Assets.Scripts.Design;
using Assets.Scripts.Flight.GameView;
using Assets.Scripts.Input;
using Assets.Scripts.Menu;
using Assets.Scripts.OperatingSystem;
using Assets.Scripts.Flight;
using Assets.Scripts.Flight.Sim;

using Jundroo.ModTools;
using TMPro;
using UI.Xml;

using Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
using UnityEngine.U2D;

using HarmonyLib;

namespace Assets.Scripts
{
	public partial class CraftUtils
	{
		public static void DestroyCraft(string name)
		{
			try
			{
				FlightSceneScript.Instance.FlightState.GetCraftNode(node => node.Name == name && !node.IsDestroyed && node != null).DestroyCraft();
			}
			catch (Exception) { }
		}

		public static CraftNode GetCraftByName(string name)
		{
			CraftNode craftNode = null;
			try
			{
				craftNode = FlightSceneScript.Instance.FlightState.GetCraftNode(node => node.Name == name && !node.IsDestroyed && node != null);
			}
			catch (Exception) { }
			return craftNode;
		}

		public static IReadOnlyList<PartData> GetCraftParts(ICraftNode craft)
		{
			return craft.CraftScript.Data.Assembly.Parts;
		}

		public static IReadOnlyList<CraftNode> GetAllCraftsInScene()
		{
			return FlightSceneScript.Instance.FlightState.CraftNodes;
		}

		public static bool DisablePhysics(CraftNode craft)
		{
			try
			{
				IReferenceFrame referenceFrame = craft.ReferenceFrame;
				craft.SetPhysicsEnabled(false, PhysicsChangeReason.UnloadPhysics);
				craft.RecalculateFrameState(referenceFrame);
				return true;
			}
			catch (Exception e)
			{
				FlightSceneScript.Instance.FlightSceneUI.ShowMessage(e.ToString(), false, 10f);
				return false;
			}
		}


		private static void interpolatedTransform(CraftNode craft, Vector3d initialSurfacePosition, Vector3d targetSurfacePosition, Vector3d initialSurfaceVelocity, Vector3d targetSurfaceVelocity, Quaternion initialGroundedTransform, Quaternion targetGroundedTransform, float percentage)
		{

			Quaterniond surfaceRotation = new Quaterniond(Quaternion.Slerp(initialGroundedTransform, targetGroundedTransform, percentage));

			IPlanetNode planet = craft.Parent;
			Vector3d interpolatedPos = Vector3d.Lerp(initialSurfacePosition, targetSurfacePosition, percentage);
			Vector3d interpolatedVel = Vector3d.Lerp(initialSurfaceVelocity, targetSurfaceVelocity, percentage);

			SetCraftTransform(craft, interpolatedPos, interpolatedVel, surfaceRotation, planet);
		}


	}
}

