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
		public static CraftNode ReceiveCraft(byte[] compressedCraftXML, Vector3d position = new Vector3d(), Vector3d velocity = new Vector3d(), Quaterniond heading = new Quaterniond(), string playerID = "1234567891011", string planetName = "Droo")
		{
			XElement craftXML = CraftLoaderScript.LoadCraftXmlFromBytes(compressedCraftXML);
			string craftId = FlightSceneScript.Instance.CraftNode.Name;
			CraftData craftData;
			if (craftXML != null)
			{

				craftData = Game.Instance.CraftLoader.LoadCraftImmediate(craftXML);
			}
			else craftData = Game.Instance.CraftLoader.LoadCraftImmediate(craftId);

			IPlanetNode planet = FlightSceneScript.Instance.FlightState.RootNode.FindPlanet(planetName);
			LaunchLocation location;
			if (position == new Vector3d())
				location = new LaunchLocation(playerID + " Spawn", LaunchLocationType.SurfaceLockedGround, planetName, 0, -130.26, velocity, heading, 1000);
			else
				location = LaunchLocation.CreateLaunchLocation(playerID + " Spawn", planet, position, velocity, heading, FlightSceneScript.Instance.ViewManager.GameView.ReferenceFrame, LaunchLocationType.Orbital);
			CraftNode newCraft = FlightSceneScript.Instance.SpawnCraft(playerID, craftData, location);
			newCraft.AllowPlayerControl = false;


			return newCraft;
		}

		public static bool DisableCraftPhysicCalculation(ref CraftNode craft)
		{
			try
			{
				//craft.GetType().GetField("_craftScript", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(craft, null);

				//craft.CraftScript.Data

				List<PartData> parts = new List<PartData>(craft.CraftScript.Data.Assembly.Parts);
				foreach (PartData part in parts)
				{
					part.Damage = -2147483647;
					part.PartDrag.ClearDrag();
					part.PartScript.Colliders.Clear();

					/*
					foreach (PartColliderScript col in part.PartScript.Colliders)
					{

					}
					*/
					ConfigData config = (ConfigData)part.Config;
					config.PreventDebris = true;
					config.IncludeInDrag = false;
					config.HeatShield = 2147483647;
				}


				List<BodyData> bodies = new List<BodyData>(craft.CraftScript.Data.Assembly.Bodies);
				foreach (BodyData body in bodies)
				{

					//var type = ((BodyScript)body.BodyScript).GetType();
					//type.GetField("_bodyCollisionHandler", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(body.BodyScript, null);
					GameObject obj = ((BodyScript)body.BodyScript).GameObject;
					foreach (Joint j in obj.GetComponentsInChildren<Joint>(true))
					{
						//UnityEngine.Debug.Log("Joint");
						GameObject.DestroyImmediate(j);
					}
					foreach (Rigidbody r in obj.GetComponentsInChildren<Rigidbody>(true))
					{
						//UnityEngine.Debug.Log("Rigid");
						r.isKinematic = true;
					}
					foreach (Collider c in obj.GetComponentsInChildren<Collider>(true))
					{
						//UnityEngine.Debug.Log("Collider");
						c.enabled = false;
					}

				}
				UnityEngine.Debug.Log("Calculations Disabled");
				return true;
			}
			catch (Exception e)
			{
				FlightSceneScript.Instance.FlightSceneUI.ShowMessage(e.ToString(), false, 10f);
				return false;
			}
		}

		public static byte[] SendCraft(ICraftNode craftNode)
		{
			var type = craftNode.GetType();
			CraftData data = type.GetField("_craftData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(craftNode) as CraftData;
			XElement craftXML = data.GenerateXml(null, true, false);
			/*
			XElement craftXML = null;
			if (Instance.dscraft != null)
			{
				craftXML = Instance.dscraft.Data.GenerateXml(null, true, false);
				System.Windows.Forms.Clipboard.SetText(craftXML.ToString());
			}
			


			foreach (XElement element in craftXML.Descendants("Craft")
				.Descendants("Assembly")
				.Descendants("Parts")
				.Descendants("Part")
				.Descendants("Config"))
			{
				if (element.Name == "initialCraftNodeId")
				{

					element.Remove();
				}
			}
			*/
			byte[] compressedCraftXML = CraftLoaderScript.CompressCraftXml(craftXML);
			return compressedCraftXML;
		}
	}
}

