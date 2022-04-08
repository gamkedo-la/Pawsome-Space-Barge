// Allows you to change the up vector in the scene view to the Z axis.
// Y-up mode uses Unity's default implementation.
//
// Original code from Akulist on the Unity forums, modified by Freya Holmér:
// • Fixed orbit+RMB zoom not working
// • Toggling will now align the camera immediately to the new up vector
// • When the camera is upside down in Z axis mode, left/right orbit will no longer be mirrored (pls fix this too Unity~)
// • Moved toggle buttons to under the gizmo
// • Fixed broken rotation state when focusing a different window in Unity and coming back to the scene view
// 
// Akulist's original: https://forum.unity.com/threads/change-scene-view-camera-behaviour.649624/
// Unity's original: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/SceneView/SceneViewMotion.cs

using UnityEditor;
using UnityEngine;

[InitializeOnLoad] public static class SceneViewZAlign {

	static bool useZAxis = true;

	// interaction states
	static float s_StartZoom, s_ZoomSpeed, s_TotalMotion; // matches Unity's variable names
	static float yawSign = 1;

	static SceneViewZAlign() => SceneView.duringSceneGui += OnSceneGUI;

	static void OnSceneGUI( SceneView view ) {
		DrawToggle( view );

		if( !useZAxis || view.isRotationLocked )
			return;

		Event e = Event.current;
		bool alt = ( Event.current.modifiers & EventModifiers.Alt ) != 0;
		bool lmb = e.button == 0;
		bool rmb = e.button == 1;

		switch( e.type ) {
			case EventType.MouseDown:
				if( e.button == 1 ) {
					s_StartZoom = view.size;
					s_ZoomSpeed = Mathf.Max( Mathf.Abs( s_StartZoom ), .3f );
					s_TotalMotion = 0;
				}

				UpdateYawSign( view );

				break;
			case EventType.MouseDrag:
				if( alt == false && rmb ) {
					FPSCameraBehaviour( view, e );
				} else if( alt ) {
					if( lmb )
						OrbitCameraBehavior( view, e );
					else if( rmb )
						OrbitZoomBehavior( view, e );
				}

				break;
		}
	}

	static void DrawToggle( SceneView view ) {
		const float w = 30; // button width
		Vector2 p = new Vector2( view.position.width - 50, 113 );

		Handles.BeginGUI();
		using( var chchk = new EditorGUI.ChangeCheckScope() ) {
			GUI.color = useZAxis ? Color.white : Handles.yAxisColor;
			useZAxis = !GUI.Toggle( new Rect( p.x - w, p.y, w, 14 ), !useZAxis, "Y", EditorStyles.miniButtonLeft );
			GUI.color = useZAxis ? Handles.zAxisColor : Color.white;
			useZAxis = GUI.Toggle( new Rect( p.x, p.y, w, 14 ), useZAxis, "Z", EditorStyles.miniButtonRight );
			if( chchk.changed )
				AlignCameraRoll( view );
		}

		Handles.EndGUI();
	}

	static void UpdateYawSign( SceneView view ) => yawSign = -Mathf.Sign( ( view.rotation * Vector3.up ).z );

	static void AlignCameraRoll( SceneView view ) {
		UpdateYawSign( view );
		// Keep forward vector, but align roll so that cam X is perpendicular to the current up vector
		Vector3 up = useZAxis ? Vector3.back : Vector3.up;
		view.rotation = Quaternion.LookRotation( view.rotation * Vector3.forward, yawSign * up );
	}

	static void FPSCameraBehaviour( SceneView view, Event e ) {
		Vector3 camPos = view.pivot - view.rotation * Vector3.forward * view.cameraDistance;
		view.rotation = GetMouseRotation( view, e );
		view.pivot = camPos + view.rotation * Vector3.forward * view.cameraDistance;
		e.Use();
	}

	static void OrbitCameraBehavior( SceneView view, Event e ) {
		view.FixNegativeSize();
		Quaternion rotation = GetMouseRotation( view, e );
		if( view.size < 0 ) {
			view.pivot = view.camera.transform.position;
			view.size = 0;
		}

		view.rotation = rotation;
		e.Use();
	}

	static Quaternion GetMouseRotation( SceneView view, Event e ) {
		Quaternion rotation = view.rotation;
		rotation = Quaternion.AngleAxis( e.delta.y * .003f * Mathf.Rad2Deg, rotation * Vector3.right ) * rotation;
		rotation = Quaternion.AngleAxis( yawSign * e.delta.x * .003f * Mathf.Rad2Deg, Vector3.back ) * rotation;
		return rotation;
	}

	static void OrbitZoomBehavior( SceneView view, Event e ) {
		float zoomDelta = HandleUtility.niceMouseDeltaZoom * ( ( Event.current.modifiers & EventModifiers.Shift ) != 0 ? 9 : 3 );

		if( view.orthographic ) {
			view.size = Mathf.Max( .0001f, view.size * ( 1 + zoomDelta * .001f ) );
		} else {
			s_TotalMotion += zoomDelta;

			if( s_TotalMotion < 0 )
				view.size = s_StartZoom * ( 1 + s_TotalMotion * .001f );
			else
				view.size += zoomDelta * s_ZoomSpeed * .003f;
		}

		e.Use();
	}


}