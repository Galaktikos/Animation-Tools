#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Galaktikos.AnimationTools
{
	public class AnimatorLayerCopy : EditorWindow
	{
		public AnimatorController CopyAnimator;
		public List<LayerData> Layers = new List<LayerData>();
		public List<ParameterData> Parameters = new List<ParameterData>();
		public AnimatorController DestinationAnimator;

		private AnimatorController LastCopyAnimator;
		private AnimatorController LastDestinationAnimator;

		private UnityEditorInternal.ReorderableList LayerList;
		private UnityEditorInternal.ReorderableList ParameterList;

		public struct LayerData
		{
			public AnimatorControllerLayer Layer;
			public bool Copy;
		}

		public struct ParameterData
		{
			public AnimatorControllerParameter Parameter;
			public bool Copy;
		}

		[MenuItem("Window/Galaktikos/Animation Tools/Animator Layer Copy")]
		private static void Init()
		{
			AnimatorLayerCopy window = (AnimatorLayerCopy)GetWindow(typeof(AnimatorLayerCopy));
			window.titleContent = new GUIContent("Animator Layer Copy");
			window.Show();
		}

		private void OnEnable()
		{
			LayerList = new UnityEditorInternal.ReorderableList(Layers, Layers.GetType(), false, true, false, false)
			{
				drawHeaderCallback = (Rect rect) =>
				{
					EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Copy");
					EditorGUI.LabelField(new Rect(rect.x + 50, rect.y, 100, EditorGUIUtility.singleLineHeight), "Name");
				},
				drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					LayerData layerData = Layers[index];
					rect.y += 2;

					layerData.Copy = EditorGUI.Toggle(new Rect(rect.x + 10, rect.y, 40, EditorGUIUtility.singleLineHeight), layerData.Copy);
					EditorGUI.LabelField(new Rect(rect.x + 50, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight), layerData.Layer.name);

					Layers[index] = layerData;
				}
			};

			ParameterList = new UnityEditorInternal.ReorderableList(Parameters, Parameters.GetType(), false, true, false, false)
			{
				drawHeaderCallback = (Rect rect) =>
				{
					EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Copy");
					EditorGUI.LabelField(new Rect(rect.x + 50, rect.y, 100, EditorGUIUtility.singleLineHeight), "Name");
				},
				drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					ParameterData parameterData = Parameters[index];
					rect.y += 2;

					parameterData.Copy = EditorGUI.Toggle(new Rect(rect.x + 10, rect.y, 40, EditorGUIUtility.singleLineHeight), parameterData.Copy);
					EditorGUI.LabelField(new Rect(rect.x + 50, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight), parameterData.Parameter.name);

					Parameters[index] = parameterData;
				}
			};
		}

		private void OnGUI()
		{
			GUILayout.Space(10);
			CopyAnimator = (AnimatorController)EditorGUILayout.ObjectField("Copy Animator", CopyAnimator, typeof(AnimatorController), true);

			if (CopyAnimator == null)
				return;

			if (LastCopyAnimator != CopyAnimator || LastDestinationAnimator != DestinationAnimator)
			{
				Layers.Clear();
				Parameters.Clear();

				foreach (AnimatorControllerLayer layer in CopyAnimator.layers)
					Layers.Add(new LayerData() { Layer = layer, Copy = true });

				foreach (AnimatorControllerParameter parameter in CopyAnimator.parameters)
				{
					bool parameterExists = false;
					foreach (AnimatorControllerParameter existingParameter in DestinationAnimator.parameters)
						if (parameter.name == existingParameter.name && parameter.type == existingParameter.type)
						{
							parameterExists = true;
							break;
						}

					if (!parameterExists)
						Parameters.Add(new ParameterData() { Parameter = parameter, Copy = true });
				}

				LastCopyAnimator = CopyAnimator;
				LastDestinationAnimator = DestinationAnimator;
			}
			
			GUILayout.Space(10);
			GUILayout.Label("Layers");
			LayerList.DoLayoutList();

			GUILayout.Label("Parameters");
			ParameterList.DoLayoutList();

			DestinationAnimator = (AnimatorController)EditorGUILayout.ObjectField("Destination Animator", DestinationAnimator, typeof(AnimatorController), false);

			if (DestinationAnimator == null)
				return;

			GUILayout.Space(10);
			if (GUILayout.Button("Copy"))
			{
				List<AnimatorControllerLayer> layers = new List<AnimatorControllerLayer>();
				layers.AddRange(DestinationAnimator.layers);

				foreach (LayerData layerData in Layers)
					if (layerData.Copy)
						layers.Add(layerData.Layer);

				DestinationAnimator.layers = layers.ToArray();

				List<AnimatorControllerParameter> parameters = new List<AnimatorControllerParameter>();
				parameters.AddRange(DestinationAnimator.parameters);

				foreach (ParameterData parameterData in Parameters)
					if (parameterData.Copy)
						parameters.Add(parameterData.Parameter);

				DestinationAnimator.parameters = parameters.ToArray();
			}
		}
	}
}
#endif