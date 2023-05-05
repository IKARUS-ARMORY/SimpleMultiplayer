/*
using System;
using System.Collections.Generic;
using System.Reflection;

using ModApi.Craft;
using ModApi.Craft.Parts;
using ModApi.Craft.Parts.Modifiers;
using ModApi.Flight.GameView;
using ModApi.Mods;
using ModApi.State;

using Assets.Packages.DevConsole;
using Assets.Scripts.Craft;
using Assets.Scripts.Flight;
using Assets.Scripts.Flight.Sim;

using Jundroo.ModTools;
using UnityEngine;

namespace Assets.Scripts
{
	public class Mod : GameMod
	{
		private Mod()
		{
		}
		public static Mod Instance { get; } = GameModBase.GetModInstance<Mod>();

		protected override void OnModInitialized()
		{
			base.OnModInitialized();

			GameObject Controller = null;
			DevConsoleApi.RegisterCommand("SyncCurrentCraft", delegate ()
			{
				if (FlightSceneScript.Instance != null)
				{
					if (Controller == null)
					{
						Controller = new GameObject();
						Controller.AddComponent<CraftController>();
						UnityEngine.Debug.Log("Sync Initialized");
						Controller.SetActive(true);
					}
					else
					{
						UnityEngine.Debug.Log("Sync Disabled");
						Controller.SetActive(false);
						GameObject.Destroy(Controller);
						Controller = null;
					}

				}
			});

		}
		public static bool DisableCraftPhysicCalculation(ref CraftNode craft)
		{
			try
			{

				List<PartData> parts = new List<PartData>(craft.CraftScript.Data.Assembly.Parts);
				foreach (PartData part in parts)
				{
					part.Damage = -2147483647;
					part.PartDrag.ClearDrag();
					part.PartScript.Colliders.Clear();

					ConfigData config = (ConfigData)part.Config;
					config.PreventDebris = true;
					config.IncludeInDrag = false;
					config.HeatShield = 2147483647;
				}


				List<BodyData> bodies = new List<BodyData>(craft.CraftScript.Data.Assembly.Bodies);
				foreach (BodyData body in bodies)
				{

					GameObject obj = ((BodyScript)body.BodyScript).GameObject;
					foreach (Joint j in obj.GetComponents<Joint>())
					{
						GameObject.DestroyImmediate(j);
					}
					foreach (Rigidbody r in obj.GetComponents<Rigidbody>())
					{
						r.isKinematic = true;
					}
					foreach (Collider c in obj.GetComponents<Collider>())
					{
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

		public static bool SetCraftTransform(CraftNode craft, Vector3d position, Vector3d velocity, Quaterniond orientation)
		{
			try
			{
				IReferenceFrame referenceFrame = craft.ReferenceFrame;

				var type = craft.GetType();
				type.GetProperty("Heading", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(craft, orientation);
				craft.SetStateVectorsAtDefaultTime(position, velocity);

				craft.CraftScript.Transform.rotation = new Quaternion((float)orientation.x, (float)orientation.y, (float)orientation.z, (float)orientation.w);

				craft.RecalculateFrameState(referenceFrame);
				return true;
			}
			catch (Exception e)
			{
				FlightSceneScript.Instance.FlightSceneUI.ShowMessage(e.ToString(), false, 10f);
				return false;
			}
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


	}

	public class CraftController : MonoBehaviour
	{
		CraftNode newCraft;
		bool physicsEnabled = true;
		Vector3d offset = new Vector3d(new System.Random().Next(20, 100), new System.Random().Next(20, 100), new System.Random().Next(20, 100));
		void Awake()
		{
			try
			{
				string craftId = FlightSceneScript.Instance.CraftNode.Name;
				CraftData craftData = Game.Instance.CraftLoader.LoadCraftImmediate(craftId);
				newCraft = FlightSceneScript.Instance.SpawnCraft("new", craftData, new LaunchLocation("spawn", LaunchLocationType.SurfaceLockedGround, "Droo", 0, -130.26, new Vector3d(), 0, 1000));

				FlightSceneScript.Instance.FlightSceneUI.ShowMessage("Sync start", false, 5f);
				UnityEngine.Debug.Log("Sync Started");
			}
			catch (Exception e)
			{
				UnityEngine.Debug.Log("Sync Activation Failed: " + e.ToString());
			}
		}
		void Start()
		{
			try
			{
				DevConsoleApi.RegisterCommand("DisablePhysics", delegate ()
				{
					if (FlightSceneScript.Instance != null)
					{
						Mod.DisableCraftPhysicCalculation(ref newCraft);
						UnityEngine.Debug.Log("Disabling Physic Calculations");
					}
				});
			}
			catch (Exception e)
			{
				UnityEngine.Debug.Log("Physic Decimation Failed: " + e.ToString());
			}

		}
		void FixedUpdate()
		{
			try
			{
				if (physicsEnabled && newCraft.CraftScript.Data.Assembly != null)
				{
					physicsEnabled = !Mod.DisableCraftPhysicCalculation(ref newCraft);
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.Log("Physic Update Failed: " + e.ToString());
			}
		}
	}
}
*/