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
	public partial class Mod : GameMod
	{
		private Mod()
		{
		}
		public static Mod Instance { get; } = GameModBase.GetModInstance<Mod>();

		protected override void OnModInitialized()
		{
			base.OnModInitialized();
			new Harmony("TLDR.AEROSPACE.SIMPLEMP").PatchAll();

			//SceneManager.sceneLoaded += OnSceneLoaded;
			/*
			new Thread(delegate () {
				while (true)
				{
					NativeMethods.SetWindowText(NativeMethods.FindWindow(null, "Juno: New Origins"), "SimpleRockets 2 ");
					Thread.Sleep(10000);
				}
			})
			{ IsBackground = true }.Start();
			*/
			DevConsoleApi.RegisterCommand("CtrlCV", delegate ()
			{
				if (FlightSceneScript.Instance != null)
				{
					try
					{
						string craftId = FlightSceneScript.Instance.CraftNode.Name;
						//CraftData craftData = Game.Instance.CraftLoader.LoadCraftImmediate(craftId);

						CraftNode originalCraft = FlightSceneScript.Instance.GetType().GetField("_craftNode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(FlightSceneScript.Instance) as CraftNode;

						byte[] xml = CraftUtils.SendCraft(originalCraft);
						//UnityEngine.Debug.Log(xml);
						System.Windows.Forms.Clipboard.SetText(xml.ToString());
						CraftNode craft = CraftUtils.ReceiveCraft(xml);

						//CraftNode craft = FlightSceneScript.Instance.SpawnCraft("t0", craftData, new LaunchLocation("t0", LaunchLocationType.SurfaceLockedGround, "Droo", 0, -130.26, new Vector3d(), 0, 1000));
						//IReferenceFrame referenceFrame = craft.ReferenceFrame;


						CraftUtils.SetCraftTransform(
							craft,
							FlightSceneScript.Instance.CraftNode.Position + new Vector3d(-100f, 0f, 0f),
							FlightSceneScript.Instance.CraftNode.Velocity,
							FlightSceneScript.Instance.CraftNode.Heading
						);




						var type = craft.GetType();
						CraftScript craftScript = type.GetField("_craftScript", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(craft) as CraftScript;
						//craftScript.IsPhysicsEnabled = false;
						//craft.RecalculateFrameState(referenceFrame);
						FlightSceneScript.Instance.FlightSceneUI.ShowMessage("It's not over yet, kid -- " + craft.NodeId.ToString(), false, 5f);
					}
					catch (Exception e)
					{
						FlightSceneScript.Instance.FlightSceneUI.ShowMessage(e.ToString(), false, 10f);
					}
				}
			});

			GameObject CIC = null;
			DevConsoleApi.RegisterCommand("SyncCurrentCraft", delegate ()
			{
				if (FlightSceneScript.Instance != null)
				{
					//GameObject syncController = new GameObject();
					//syncController.AddComponent<CraftController>();
					//UnityEngine.Debug.Log("Sync Initialized");
					//syncController.SetActive(true);

					if (CIC == null)
					{
						CIC = new GameObject();
						CIC.AddComponent<CraftController>();
						UnityEngine.Debug.Log("Sync Initialized");
						CIC.SetActive(true);
					}
					else
					{
						UnityEngine.Debug.Log("Sync Disabled");
						CIC.SetActive(false);
						GameObject.Destroy(CIC);
						CIC = null;
					}

				}
			});


			DevConsoleApi.RegisterCommand("tttttttt", delegate ()
			{
				if (Game.Instance.Designer != null)
				{
					try
					{

					}
					catch (Exception e)
					{
						Game.Instance.Designer.ShowMessage(e.ToString(), 10f);
					}
				}
			});


			/*
			// called second
			void OnSceneLoaded(Scene scene, LoadSceneMode mode)
			{
				if (scene.name == "Menu")
				{
					UnityEngine.UI.Image[] objs = GameObject.FindObjectsOfType<UnityEngine.UI.Image>();
					foreach (var s in objs)
					{
						if (s.mainTexture.name == "PrimaryGameLogo")
						{
							s.overrideSprite = Sprite.Create(
								Instance.Mod.AssetBundle.LoadAsset<Texture2D>("Assets/Content/XML UI/SemiHarmonizedSR2.png"),
								s.overrideSprite.rect,
								s.overrideSprite.pivot,
								s.overrideSprite.pixelsPerUnit
							);
						}
					}
				}
			}
			*/
		}

		public class CraftController : MonoBehaviour
		{

			CraftNode newCraft;
			Dictionary<BodyData, BodyData> BodyPairs = new Dictionary<BodyData, BodyData>();
			bool physicsEnabled = true;
			Vector3d offset = new Vector3d(new System.Random().Next(20, 100), new System.Random().Next(20, 100), new System.Random().Next(20, 100));
			void Awake()
			{
				try
				{
					string craftId = FlightSceneScript.Instance.CraftNode.Name;
					//CraftData craftData = Game.Instance.CraftLoader.LoadCraftImmediate(craftId);
					//newCraft = FlightSceneScript.Instance.SpawnCraft("t0", craftData, new LaunchLocation("t0", LaunchLocationType.SurfaceLockedGround, "Droo", 0, -130.26, new Vector3d(), 0, 1000));

					byte[] xml = CraftUtils.SendCraft(FlightSceneScript.Instance.CraftNode);
					

					//new CraftData(craftXml, instance.CraftThemes, instance.PartTypes)

					newCraft = CraftUtils.ReceiveCraft(xml);

					CraftUtils.SetCraftTransform(
						//newCraft,
						//FlightSceneScript.Instance.CraftNode.Position + new Vector3d(-100f, 0f, 0f),
						//FlightSceneScript.Instance.CraftNode.Velocity,
						//FlightSceneScript.Instance.CraftNode.CraftScript.Transform.rotation//FlightSceneScript.Instance.CraftNode.Heading
						newCraft,
						newCraft.Parent.PlanetVectorToSurfaceVector(FlightSceneScript.Instance.CraftNode.Position) + offset,
						newCraft.Parent.PlanetVectorToSurfaceVector(FlightSceneScript.Instance.CraftNode.Velocity),
						newCraft.Parent.Rotation * FlightSceneScript.Instance.CraftNode.Heading,
						newCraft.Parent
					);
					//Mod.DisableCraftPhysicCalculation(newCraft);
					FlightSceneScript.Instance.FlightSceneUI.ShowMessage("Sync start", false, 5f);
					//Mod.DisableCraftPhysicCalculation(newCraft);
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
					DevConsoleApi.RegisterCommand("dpc", delegate ()
					{
						if (FlightSceneScript.Instance != null)
						{
							CraftUtils.DisableCraftPhysicCalculation(ref newCraft);
							UnityEngine.Debug.Log("Disabling Physic Calculations");
						}
					});
					//Mod.DisableCraftPhysicCalculation(newCraft);
				}
				catch (Exception e)
				{
					UnityEngine.Debug.Log("Physic Decimation Failed: " + e.ToString());
				}

			}

			void Update()
			{
				try
				{

					newCraft.AllowPlayerControl = false;

					/*
					CraftUtils.SetCraftTransform(
						newCraft,
						newCraft.Parent.PlanetVectorToSurfaceVector(FlightSceneScript.Instance.CraftNode.Position) + offset,
						newCraft.Parent.PlanetVectorToSurfaceVector(FlightSceneScript.Instance.CraftNode.Velocity),
						newCraft.Parent.Rotation * FlightSceneScript.Instance.CraftNode.Heading,
						newCraft.Parent
					);
					*/

					//int colcount = 0;

					for (int i = 0; i < BodyPairs.Count; i++)
					{
						BodyData b;
						if (BodyPairs.TryGetValue(BodyPairs.ElementAt(i).Key, out b)) 
						{ 
							CraftUtils.SetBodyTransform(
								b,
								newCraft.Parent.PlanetVectorToSurfaceVector(BodyPairs.ElementAt(i).Key.BodyScript.Transform.position) + offset,
								newCraft.Parent.Rotation * new Quaterniond(BodyPairs.ElementAt(i).Key.BodyScript.Transform.rotation),
								newCraft.Parent
							);
						}
					}
					

					//FlightSceneScript.Instance.FlightSceneUI.ShowMessage(colcount.ToString(), false, 10f);

					//FlightSceneScript.Instance.FlightSceneUI.ShowMessage("Syncing " + FlightSceneScript.Instance.CraftNode.Position, false, 5f);
				}
				catch (Exception e)
				{
					UnityEngine.Debug.Log("Sync Failed: " + e.ToString());
				}
			}

			void FixedUpdate()
			{
				try
				{
					if (physicsEnabled && newCraft.CraftScript.Data.Assembly != null)
					{
						physicsEnabled = !CraftUtils.DisableCraftPhysicCalculation(ref newCraft);

						List<BodyData> bodiesNew = new List<BodyData>(newCraft.CraftScript.Data.Assembly.Bodies);
						List<BodyData> bodiesOriginal = new List<BodyData>(FlightSceneScript.Instance.CraftNode.CraftScript.Data.Assembly.Bodies);

						for (int i = 0; i < bodiesOriginal.Count; i++)
						{
							BodyPairs.Add(bodiesOriginal[i], bodiesNew[i]);
						}
					}
				}
				catch (Exception e)
				{
					UnityEngine.Debug.Log("Physic Update Failed: " + e.ToString());
				}
			}
		}


	}
}

