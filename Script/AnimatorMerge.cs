#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Galaktikos.AnimationHelper
{
	[CreateAssetMenu(menuName = "Galaktikos/Animation Tools/Animator Merge")]
	public class AnimatorMerge : ScriptableObject
	{
		public string Name;
		public List<AnimatorData> Animators = new List<AnimatorData>();
		public AnimatorController CurrentMergedAnimator;

		internal void Merge()
		{
			List<AnimatorControllerLayer> layers = new List<AnimatorControllerLayer>();
			List<AnimatorControllerParameter> parameters = new List<AnimatorControllerParameter>();
			foreach (AnimatorData animator in Animators)
			{
				foreach (AnimatorControllerLayer layer in animator.Animator.layers)
					layers.Add(new AnimatorControllerLayer()
					{
						name = animator.Prefix + layer.name,
						avatarMask = layer.avatarMask,
						blendingMode = layer.blendingMode,
						defaultWeight = layer.defaultWeight,
						iKPass = layer.iKPass,
						stateMachine = layer.stateMachine,
						syncedLayerAffectsTiming = layer.syncedLayerAffectsTiming,
						syncedLayerIndex = layer.syncedLayerIndex
					});

				foreach (AnimatorControllerParameter parameter in animator.Animator.parameters)
				{
					string name = parameter.name[0] == '.' ? parameter.name.Remove(0, 1) : animator.Prefix + parameter.name;

					bool parameterFound = false;
					foreach (AnimatorControllerParameter existingParameter in parameters)
						if (name == existingParameter.name && parameter.type == existingParameter.type)
						{
							parameterFound = true;
							break;
						}

					if (!parameterFound)
						parameters.Add(new AnimatorControllerParameter()
						{
							name = name,
							defaultBool = parameter.defaultBool,
							defaultFloat = parameter.defaultFloat,
							defaultInt = parameter.defaultInt,
							type = parameter.type
						});
				}
			}

			if (CurrentMergedAnimator != null)
			{
				CurrentMergedAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(this));
				CurrentMergedAnimator.layers = layers.ToArray();
				CurrentMergedAnimator.parameters = parameters.ToArray();
			}
			else
			{
				CurrentMergedAnimator = new AnimatorController()
				{
					name = Name,
					hideFlags = HideFlags.None,
					layers = layers.ToArray(),
					parameters = parameters.ToArray(),
				};
				AssetDatabase.AddObjectToAsset(CurrentMergedAnimator, AssetDatabase.GetAssetPath(this));
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(CurrentMergedAnimator));
			AssetDatabase.Refresh();
		}

		[Serializable]
		public struct AnimatorData
		{
			public string Prefix;
			public AnimatorController Animator;
		}
	}

	[CustomEditor(typeof(AnimatorMerge))]
	public class AnimatorMergeEditor : Editor
	{
		private UnityEditorInternal.ReorderableList AnimatorList;

		private void OnEnable()
		{
			AnimatorList = new UnityEditorInternal.ReorderableList(serializedObject,
					serializedObject.FindProperty("Animators"),
					true, true, true, true)
			{
				drawHeaderCallback = (Rect rect) =>
				{
					rect.x += 20;
					EditorGUI.LabelField(new Rect(rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight), "Animator");
					EditorGUI.LabelField(new Rect(rect.x + 180, rect.y, 150, EditorGUIUtility.singleLineHeight), "Prefix");
				}
			};

			AnimatorList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				SerializedProperty element = AnimatorList.serializedProperty.GetArrayElementAtIndex(index);
				rect.y += 2;

				EditorGUI.PropertyField(
					new Rect(rect.x, rect.y, 170, EditorGUIUtility.singleLineHeight),
					element.FindPropertyRelative("Animator"), GUIContent.none);

				EditorGUI.PropertyField(
					new Rect(rect.x + 180, rect.y, rect.width - 180, EditorGUIUtility.singleLineHeight),
					element.FindPropertyRelative("Prefix"), GUIContent.none);
			};
		}

		public override void OnInspectorGUI()
		{
			AnimatorMerge mergeData = (AnimatorMerge)target;

			GUILayout.Space(10);
			mergeData.Name = EditorGUILayout.TextField("Name", mergeData.Name);

			GUILayout.Space(20);
			serializedObject.Update();
			AnimatorList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();

			if (mergeData.Animators.Count == 0)
				return;

			foreach (AnimatorMerge.AnimatorData animator in mergeData.Animators)
				if (animator.Animator == null)
					return;

			GUILayout.Space(20);
			if (GUILayout.Button("Merge", GUILayout.Height(40)))
				mergeData.Merge();
		}
	}
}
#endif