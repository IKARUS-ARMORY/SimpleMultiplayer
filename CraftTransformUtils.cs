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

		public static bool SetCraftTransform(CraftNode craft, Vector3d position, Vector3d velocity, Quaterniond orientation)
		{
			try
			{
				IReferenceFrame referenceFrame = craft.ReferenceFrame;

				var type = craft.GetType();
				type.GetProperty("Heading", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(craft, orientation);
				craft.SetStateVectorsAtDefaultTime(position, velocity);

				craft.CraftScript.Transform.rotation = new Quaternion((float)orientation.x, (float)orientation.y, (float)orientation.z, (float)orientation.w);

				//craft.CraftScript.CenterOfMass.rotation = new Quaternion((float)orientation.x, (float)orientation.y, (float)orientation.z, (float)orientation.w); 

				//((ReferenceFrame)craft.GameView.ReferenceFrame).FrameToPlanetRotation(((CraftScript)craft.CraftScript).FrameHeading);

				craft.RecalculateFrameState(referenceFrame);
				return true;
			}
			catch (Exception e)
			{
				FlightSceneScript.Instance.FlightSceneUI.ShowMessage(e.ToString(), false, 10f);
				return false;
			}
		}

		public static bool SetCraftTransform(CraftNode craft, Vector3d position, Vector3d velocity, Quaternion orientation)
		{
			try
			{
				IReferenceFrame referenceFrame = craft.ReferenceFrame;

				var type = craft.GetType();
				type.GetProperty("Heading", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(craft, new Quaterniond(orientation));
				craft.SetStateVectorsAtDefaultTime(position, velocity);

				craft.CraftScript.Transform.rotation = orientation;

				//craft.CraftScript.CenterOfMass.rotation = orientation;

				//((ReferenceFrame)craft.GameView.ReferenceFrame).FrameToPlanetRotation(((CraftScript)craft.CraftScript).FrameHeading);

				craft.RecalculateFrameState(referenceFrame);
				return true;
			}
			catch (Exception e)
			{
				FlightSceneScript.Instance.FlightSceneUI.ShowMessage(e.ToString(), false, 10f);
				return false;
			}
		}

		public static bool SetCraftTransform(CraftNode craft, Vector3d surfacePosition, Vector3d surfaceVelocity, Quaterniond groundedTransform, IPlanetNode planet)
		{
			try
			{
				var rotation = planet.RotationInverse * groundedTransform;
				var position = planet.SurfaceVectorToPlanetVector(surfacePosition);
				var velocity = planet.SurfaceVectorToPlanetVector(surfaceVelocity);
				SetCraftTransform(craft, position, velocity, rotation);
				return true;
			}
			catch (Exception e)
			{
				FlightSceneScript.Instance.FlightSceneUI.ShowMessage(e.ToString(), false, 10f);
				return false;
			}
		}

		public static bool SetCraftTransform(CraftNode craft, double latitude, double longtitude, double asl, Vector3d surfaceVelocity, Quaterniond groundedTransform, PlanetNode planet)
		{
			try
			{
				var rotation = planet.RotationInverse * groundedTransform;
				var position = planet.GetSurfacePosition(latitude, longtitude, AltitudeType.AboveSeaLevel, asl);
				var velocity = planet.SurfaceVectorToPlanetVector(surfaceVelocity);
				SetCraftTransform(craft, position, velocity, rotation);
				return true;
			}
			catch (Exception e)
			{
				FlightSceneScript.Instance.FlightSceneUI.ShowMessage(e.ToString(), false, 10f);
				return false;
			}
		}


		public static bool SetBodyTransform(BodyData body, Vector3 position, Quaternion orientation)
		{
			try
			{
				body.BodyScript.Transform.position = position;
				body.BodyScript.Transform.rotation = orientation;
				return true;
			}
			catch (Exception e)
			{
				FlightSceneScript.Instance.FlightSceneUI.ShowMessage(e.ToString(), false, 10f);
				return false;
			}
		}

		public static bool SetBodyTransform(BodyData body, Vector3d position, Quaterniond orientation)
		{
			try
			{
				body.BodyScript.Transform.position = new Vector3((float)position.x, (float)position.y, (float)position.z);
				body.BodyScript.Transform.rotation = new Quaternion((float)orientation.x, (float)orientation.y, (float)orientation.z, (float)orientation.w);
				return true;
			}
			catch (Exception e)
			{
				FlightSceneScript.Instance.FlightSceneUI.ShowMessage(e.ToString(), false, 10f);
				return false;
			}
		}

		public static bool SetBodyTransform(BodyData body, Vector3d surfacePosition, Quaterniond groundedTransform, IPlanetNode planet)
		{
			try
			{
				var rotation = planet.RotationInverse * groundedTransform;
				var position = planet.SurfaceVectorToPlanetVector(surfacePosition);
				SetBodyTransform(body, position, rotation);
				return true;
			}
			catch (Exception e)
			{
				FlightSceneScript.Instance.FlightSceneUI.ShowMessage(e.ToString(), false, 10f);
				return false;
			}
		}
	}
}

