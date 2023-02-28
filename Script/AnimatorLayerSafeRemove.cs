#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Galaktikos.AnimationTools
{
	public class AnimatorLayerSafeRemove : EditorWindow
	{
		public AnimatorController Animator;
		public List<AnimatorControllerLayer> Layers = new List<AnimatorControllerLayer>();

		private AnimatorController LastAnimator;
		private UnityEditorInternal.ReorderableList LayerList;


		[MenuItem("Window/Galaktikos/Animation Tools/Animator Layer Safe Remove")]
		private static void Init()
		{
			AnimatorLayerSafeRemove window = (AnimatorLayerSafeRemove)GetWindow(typeof(AnimatorLayerSafeRemove));
			window.titleContent = new GUIContent("Animator Layer Safe Remove");
			window.Show();
		}

		private void OnEnable()
		{
			LayerList = new UnityEditorInternal.ReorderableList(Layers, Layers.GetType(), false, false, false, false)
			{
				drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					AnimatorControllerLayer layer = Layers[index];
					rect.y += 2;

					EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - 10, EditorGUIUtility.singleLineHeight), layer.name);
					if (GUI.Button(new Rect(rect.x + rect.width - 100, rect.y, 100, EditorGUIUtility.singleLineHeight), "Remove"))
					{
						Layers.Remove(layer);
						Animator.layers = Layers.ToArray();
					}
				}
			};
		}

		private void OnGUI()
		{
			GUILayout.Space(10);
			Animator = (AnimatorController)EditorGUILayout.ObjectField("Animator", Animator, typeof(AnimatorController), false);

			if (Animator == null)
				return;

			if (LastAnimator != Animator)
			{
				Layers.Clear();
				Layers.AddRange(Animator.layers);

				LastAnimator = Animator;
			}
			
			GUILayout.Space(10);
			GUILayout.Label("Layers");
			LayerList.DoLayoutList();
		}
	}
}
#endif