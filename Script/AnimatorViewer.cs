#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Galaktikos.AnimationTools
{
	public class AnimatorViewer : EditorWindow
	{
		private AnimatorController Animator;
		private List<AnimatorControllerLayer> Layers = new List<AnimatorControllerLayer>();
		private UnityEditorInternal.ReorderableList LayerList;
		private Vector2 LayerListScroll = Vector2.zero;

		[MenuItem("Window/Galaktikos/Animation Tools/Animator Viewer")]
		private static void Init()
		{
			AnimatorViewer window = (AnimatorViewer)GetWindow(typeof(AnimatorViewer));
			window.titleContent = new GUIContent("Animator Viewer");
			window.Show();
		}

		private void OnEnable()
		{
			LayerList = new UnityEditorInternal.ReorderableList(Layers, typeof(AnimatorControllerLayer), true, false, true, true)
			{
				drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					AnimatorControllerLayer layer = Animator.layers[index];
					rect.y += 2;

					EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - 65, EditorGUIUtility.singleLineHeight), layer.name);

					if (GUI.Button(new Rect(rect.x + rect.width - 60, rect.y, 60, EditorGUIUtility.singleLineHeight), "Copy"))
					{
						List<AnimatorControllerLayer> layers = new List<AnimatorControllerLayer>(Animator.layers);

						AnimatorStateMachine stateMachine = new AnimatorStateMachine()
						{
							name = layer.name,
							hideFlags = HideFlags.HideInHierarchy,
							entryPosition = Vector3.zero,
							anyStatePosition = new Vector3(0, 50, 0),
							exitPosition = new Vector3(0, 100, 0)
						};
						AssetDatabase.AddObjectToAsset(stateMachine, AssetDatabase.GetAssetPath(Animator));

						layers.Add(new AnimatorControllerLayer()
						{
							avatarMask = layer.avatarMask,
							blendingMode = layer.blendingMode,
							defaultWeight = layer.defaultWeight,
							iKPass = layer.iKPass,
							name = layer.name,
							stateMachine = stateMachine,
							syncedLayerAffectsTiming = layer.syncedLayerAffectsTiming,
							syncedLayerIndex = layer.syncedLayerIndex
						});

						Animator.layers = layers.ToArray();
					}
				},
				onChangedCallback = (state) => { Animator.layers = Layers.ToArray(); }
			};
		}

		private void OnGUI()
		{
			Animator = (AnimatorController)EditorGUILayout.ObjectField(Animator, typeof(AnimatorController), false);

			if (Animator == null)
				return;

			GUILayout.Space(20);
			LayerListScroll = EditorGUILayout.BeginScrollView(LayerListScroll);
			Layers = new List<AnimatorControllerLayer>(Animator.layers);
			LayerList.list = Layers;
			LayerList.DoLayoutList();
			EditorGUILayout.EndScrollView();
		}
	}
}
#endif