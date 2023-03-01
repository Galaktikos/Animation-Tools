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
				{
					layers.Add(new AnimatorControllerLayer()
					{
						name = animator.Prefix + layer.name,
						avatarMask = layer.avatarMask,
						blendingMode = layer.blendingMode,
						defaultWeight = layer.defaultWeight,
						iKPass = layer.iKPass,
						stateMachine = String.IsNullOrEmpty(animator.Prefix) ? layer.stateMachine : FixStateMachineRecursive(layer.stateMachine, animator.Prefix),
						syncedLayerAffectsTiming = layer.syncedLayerAffectsTiming,
						syncedLayerIndex = layer.syncedLayerIndex
					});
				}

				foreach (AnimatorControllerParameter parameter in animator.Animator.parameters)
				{
					string name = PrefixParameter(parameter.name, animator.Prefix);

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

		private static AnimatorStateMachine FixStateMachineRecursive(AnimatorStateMachine stateMachine, string prefix)
		{
			AnimatorStateMachine newStateMachine = Instantiate(stateMachine);

			if (newStateMachine == null)
				return stateMachine;

			newStateMachine.entryTransitions = FixTransitions(newStateMachine.entryTransitions, prefix);
			newStateMachine.anyStateTransitions = FixTransitions(newStateMachine.anyStateTransitions, prefix);

			foreach (ChildAnimatorState childState in newStateMachine.states)
			{
				AnimatorState state = childState.state;

				state.cycleOffsetParameter = PrefixParameter(state.cycleOffsetParameter, prefix);
				state.mirrorParameter = PrefixParameter(state.mirrorParameter, prefix);
				state.speedParameter = PrefixParameter(state.speedParameter, prefix);
				state.timeParameter = PrefixParameter(state.timeParameter, prefix);

				state.transitions = FixTransitions(state.transitions, prefix);
			}

			foreach (ChildAnimatorStateMachine child in newStateMachine.stateMachines)
				FixStateMachineRecursive(child.stateMachine, prefix);

			return newStateMachine;
		}

		private static AnimatorTransition[] FixTransitions(AnimatorTransition[] transitions, string prefix)
		{
			List<AnimatorTransition> newTransitions = new List<AnimatorTransition>();
			foreach (AnimatorTransition transition in transitions)
			{
				AnimatorTransition newTransition = Instantiate(transition);
				List<AnimatorCondition> conditions = new List<AnimatorCondition>();

				foreach (AnimatorCondition condition in newTransition.conditions)
					conditions.Add(new AnimatorCondition()
					{
						mode = condition.mode,
						parameter = PrefixParameter(condition.parameter, prefix),
						threshold = condition.threshold
					});

				newTransition.conditions = conditions.ToArray();
				newTransitions.Add(newTransition);
			}

			return newTransitions.ToArray();
		}

		private static AnimatorStateTransition[] FixTransitions(AnimatorStateTransition[] transitions, string prefix)
		{
			List<AnimatorStateTransition> newTransitions = new List<AnimatorStateTransition>();
			foreach (AnimatorStateTransition transition in transitions)
			{
				AnimatorStateTransition newTransition = Instantiate(transition);
				List<AnimatorCondition> conditions = new List<AnimatorCondition>();

				foreach (AnimatorCondition condition in newTransition.conditions)
					conditions.Add(new AnimatorCondition()
					{
						mode = condition.mode,
						parameter = PrefixParameter(condition.parameter, prefix),
						threshold = condition.threshold
					});

				newTransition.conditions = conditions.ToArray();
				newTransitions.Add(newTransition);
			}

			return newTransitions.ToArray();
		}

		private static string PrefixParameter(string parameter, string prefix)
		{
			if (string.IsNullOrEmpty(parameter))
				return parameter;

			return parameter[0] == '.' ? parameter.Remove(0, 1) : prefix + parameter;
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