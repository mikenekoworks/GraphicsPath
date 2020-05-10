using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UIElements;
using static GraphicsPath.BezierCurve;

namespace GraphicsPath
{
    [CustomEditor( typeof( BezierCurveMaker ) )]
    public class BezierCurveMakerEditor : Editor
    {
        private BezierCurveMaker bezierCurveMaker;
        private GraphicsPath.BezierCurve bezierCurve;

        private ReorderableList anchors;

        private SerializedProperty propertyBezierCurve;
        private SerializedProperty propertyBezierCurveAnchorPoints;
        private SerializedProperty propertyBezierCurveIsClosed;
        private SerializedProperty propertyBezierCurveSegment;

        private float elementHeight;

        private int selectedIndex = -1;
        private int nearestControl = -1;

        public class ControlIds
        {
            public int AnchorId;
            public int InHandleId;
            public int OutHandleId;
        }

        private Dictionary< string, ControlIds > ids = new Dictionary<string, ControlIds>();

        /// <summary>
        /// アンカーを登録
        /// </summary>
        /// <param name="new_anchor"></param>
        public void RegisterAnchor( AnchorPoint new_anchor )
        {
            if ( ids.ContainsKey( new_anchor.Guid ) == true )
            {
                return;
            }

            var new_id = new ControlIds()
            {
                AnchorId = GUIUtility.GetControlID( FocusType.Passive ),
                InHandleId = GUIUtility.GetControlID( FocusType.Passive ),
                OutHandleId = GUIUtility.GetControlID( FocusType.Passive )
            };

            ids.Add( new_anchor.Guid, new_id );
        }

        /// <summary>
        /// アンカーの削除
        /// </summary>
        /// <param name="target_anchor"></param>
        public void UnregisterAnchor( AnchorPoint target_anchor )
        {
            if ( ids.ContainsKey( target_anchor.Guid ) == true )
            {
                return;
            }

            ids.Remove( target_anchor.Guid );

        }

        /// <summary>
        /// 線分化したデータを出力
        /// </summary>
        /// <param name="filename"></param>
        private void SaveToAssetFile( string filename )
        {
            var points = bezierCurve.ToLineSegments();

            var seg = ScriptableObject.CreateInstance<SegmentData>();

            seg.Points = points;
            seg.IsClosed = bezierCurve.IsClosed;

            int next_index = 0;

            for ( int i = 0; i < bezierCurve.PointCount; i++ )
            {
                next_index = GetNextIndex( i, bezierCurve.PointCount, bezierCurve.IsClosed );

                if ( next_index == -1 )
                {
                    break;
                }

                seg.Distances.AddRange( Segmentalized( bezierCurve[ i ], bezierCurve[ next_index ], bezierCurve.Segment ) );

            }

            AssetDatabase.CreateAsset( seg, filename );

            // 未保存のアセットをアセットデータベースに保存
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        ///
        /// </summary>
        public void OnEnable()
        {
            bezierCurveMaker = ( BezierCurveMaker )target;

            //var prop = serializedObject.GetIterator();

            //while ( prop.Next( true ) )
            //{
            //    Debug.Log( prop.propertyPath );
            //}

            propertyBezierCurveAnchorPoints = serializedObject.FindProperty( "Bezier.anchorPoints" );
            propertyBezierCurveIsClosed = serializedObject.FindProperty( "Bezier.isClosed" );
            propertyBezierCurveSegment = serializedObject.FindProperty( "Bezier.segment" );

            bezierCurve = ( GraphicsPath.BezierCurve )bezierCurveMaker.Bezier;

            if ( bezierCurve.PointCount == 0 )
            {
                RegisterAnchor( bezierCurve.Add( Vector3.zero ) );
                RegisterAnchor( bezierCurve.Add( Vector3.forward ) );
            }

            elementHeight = ( EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing ) * 3 + EditorGUIUtility.standardVerticalSpacing * 2;

            anchors = new ReorderableList( serializedObject, propertyBezierCurveAnchorPoints, true, true, true, true )
            {
                //headerHeight = ( EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing ) * 2 + EditorGUIUtility.standardVerticalSpacing * 2,
                index = selectedIndex,
                //showDefaultBackground = false,

                drawElementCallback = DrawElementCallback,
                drawElementBackgroundCallback = DrawElementBackground,
                drawHeaderCallback = ( rect ) =>
                {
                    EditorGUI.LabelField( rect, "アンカー情報:" );

                    rect.xMin += 90.0f;
                    propertyBezierCurveIsClosed.boolValue = EditorGUI.ToggleLeft( rect, new GUIContent( "クローズパス" ), propertyBezierCurveIsClosed.boolValue );

                },
                elementHeightCallback = ( int index ) =>
                {
                    return elementHeight;
                },
                onSelectCallback = ( ReorderableList list ) =>
                {
                    selectedIndex = list.index;
                },
                onAddCallback = ( ReorderableList list ) =>
                {
                    var index0 = bezierCurve.PointCount - 1;
                    var index1 = index0 - 1;

                    var new_pos = bezierCurve.Last().position + ( bezierCurve[ index0 ].position - bezierCurve[ index1 ].position );

                    RegisterAnchor( bezierCurve.Add( new_pos ) );

                    SceneView.RepaintAll();
                },
                onRemoveCallback = ( ReorderableList list ) =>
                {
                    var item = bezierCurve[ list.index ];

                    UnregisterAnchor( item );
                    bezierCurve.Remove( item );

                    selectedIndex = Mathf.Clamp( list.index - 1, 0, list.count );
                    list.index = selectedIndex;

                    SceneView.RepaintAll();
                }
            };

        }

        /// <summary>
        ///
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if ( GUILayout.Button( "線分化データの出力" ) == true )
            {
                var path = EditorUtility.SaveFilePanelInProject(
                               "線分化したデータの保存",
                               "*",
                               "asset",
                               "");

                if ( path.Length != 0 )
                {
                    SaveToAssetFile( path );
                }
            }

            EditorGUILayout.PropertyField( propertyBezierCurveSegment, new GUIContent( "セグメント分割数" ) );

            anchors.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawElementCallback( Rect rect, int index, bool isActive, bool isFocused )
        {
            var element = bezierCurve[ index ];

            rect.y += EditorGUIUtility.standardVerticalSpacing;

            string position_label = index + " : Position";

            EditorGUI.BeginChangeCheck();

            Vector3 old_position = element.position;
            Vector3 new_position = EditorGUIVector3( rect, position_label, EditorGUIUtility.labelWidth * 0.7f, old_position );
            element.position = new_position;

            if ( old_position != new_position )
            {
                // 描画更新
                GUI.changed = true;
            }

            //rect.x += EditorGUIUtility.singleLineHeight;
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            //rect.width -= EditorGUIUtility.singleLineHeight;

            var enum_rect = rect;
            enum_rect.height = EditorGUIUtility.singleLineHeight;
            enum_rect.width = EditorGUIUtility.labelWidth * 0.48f;

            AnchorPoint.TypeHandleStyle new_handle_style = ( AnchorPoint.TypeHandleStyle )EditorGUI.EnumPopup( enum_rect, "", element.HandleStyle );

            if ( element.HandleStyle != new_handle_style )
            {
                element.HandleStyle = new_handle_style;

                // 描画更新
                GUI.changed = true;
            }

            //rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            rect.xMin += EditorGUIUtility.labelWidth * 0.5f;

            if ( element.HandleStyle == AnchorPoint.TypeHandleStyle.None )
            {
                EditorGUI.BeginDisabledGroup( true );
            }

            element.InHandle = EditorGUIVector3( rect, "In", EditorGUIUtility.labelWidth * 0.2f, element.InHandle );
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            element.OutHandle = EditorGUIVector3( rect, "Out", EditorGUIUtility.labelWidth * 0.2f, element.OutHandle );
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if ( element.HandleStyle == AnchorPoint.TypeHandleStyle.None )
            {
                EditorGUI.EndDisabledGroup();
            }

            if ( EditorGUI.EndChangeCheck() == true )
            {
                bezierCurve.CalculateLength();

                Repaint();

                SceneView.RepaintAll();
            }

            rect.y += EditorGUIUtility.standardVerticalSpacing;
        }

        private Vector3 EditorGUIVector3( Rect rect, string title, float title_width, Vector3 v )
        {

            TextAnchor label_alignment = GUI.skin.label.alignment;
            TextAnchor text_field_alignment = GUI.skin.textField.alignment;
            float fixedHeight = GUI.skin.textField.fixedHeight;

            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
            GUI.skin.textField.fixedHeight = EditorGUIUtility.singleLineHeight;

            rect.height = EditorGUIUtility.singleLineHeight;

            // 要素のタイトル部分だけのサイズ
            Rect title_rect = rect;
            title_rect.width = title_width;


            // 要素の数字的なサイズ
            Rect value_rect = rect;
            value_rect.x += title_rect.width;
            value_rect.width = rect.width - title_rect.width;

            EditorGUI.LabelField( title_rect, title );

            GUIContent [] labels = new GUIContent[] { new GUIContent( "X" ), new GUIContent( "Y" ), new GUIContent( "Z" ) };
            float[] p = new float[] { v.x, v.y, v.z };
            EditorGUI.MultiFloatField( value_rect, labels, p );

            GUI.skin.label.alignment = label_alignment;
            GUI.skin.textField.alignment = text_field_alignment;
            GUI.skin.textField.fixedHeight = fixedHeight;

            v.x = p[ 0 ];
            v.y = p[ 1 ];
            v.z = p[ 2 ];

            return v;
        }

        /// <summary>
        /// リストの背景を描画する
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="isActive"></param>
        /// <param name="isFocused"></param>
        public void DrawElementBackground( Rect rect, int index, bool isActive, bool isFocused )
        {

            if ( isActive == true )
            {
                Texture2D tex = new Texture2D( 1, 1 );
                tex.SetPixel( 0, 0, new Color32( 49, 77, 121, 255 ) );
                tex.Apply();
                rect.height = elementHeight;
                GUI.DrawTexture( rect, tex as Texture );
            }

        }

        public void OnSceneGUI()
        {

            var root_pos = bezierCurveMaker.gameObject.transform.position;

            int len = bezierCurve.PointCount;

            for ( int i = 0; i < len; ++i )
            {

                var p0 = bezierCurve[ i ];

                var next_index = i + 1;

                if ( bezierCurve.IsClosed == false )
                {
                    if ( next_index >= len )
                    {
                        break;
                    }

                }

                next_index %= len;

                var p1 = bezierCurve[ next_index ];

                Handles.color = Color.green;
                Handles.DrawAAPolyLine( 2.0f, BezierCurve.GetInterpolationPoints( p0, p1, 20 ).ToArray() );

            }

            for ( int i = 0; i < len; ++i )
            {
                var p0 = bezierCurve[ i ];
                var pos0 = root_pos + p0.position;

                if ( ids.ContainsKey( p0.Guid ) == false )
                {
                    // 念のために登録処理をしておく
                    RegisterAnchor( p0 );
                }

                var control_ids = ids[ p0.Guid ];
                var anchor_id = control_ids.AnchorId;
                var in_handle_id = control_ids.InHandleId;
                var out_handle_id = control_ids.OutHandleId;

                // 頂点処理
                Handles.color = Color.white;
                Handles.FreeMoveHandle( anchor_id, pos0, Quaternion.identity, HandleUtility.GetHandleSize( p0.position ) * 0.1f, Vector3.one * 0.5f, Handles.SphereHandleCap );

                if ( nearestControl == anchor_id )
                {
                    var new_pos = Handles.PositionHandle( pos0, Quaternion.identity );
                    p0.position = new_pos - root_pos;

                    bezierCurve.CalculateLength();
                }

                // ハンドル処理
                if ( p0.HandleStyle != AnchorPoint.TypeHandleStyle.None )
                {
                    Handles.DrawLine( pos0, pos0 + p0.InHandle );
                    Handles.FreeMoveHandle( in_handle_id, pos0 + p0.InHandle, Quaternion.identity, HandleUtility.GetHandleSize( p0.position ) * 0.05f, Vector3.one * 0.5f, Handles.RectangleHandleCap );

                    if ( nearestControl == in_handle_id )
                    {
                        var in_handle = Handles.PositionHandle( pos0 + p0.InHandle, Quaternion.identity );
                        p0.InHandle = in_handle - pos0;

                        bezierCurve.CalculateLength();
                    }

                    Handles.DrawLine( pos0, pos0 + p0.OutHandle );
                    Handles.FreeMoveHandle( out_handle_id, pos0 + p0.OutHandle, Quaternion.identity, HandleUtility.GetHandleSize( p0.position ) * 0.05f, Vector3.one * 0.5f, Handles.RectangleHandleCap );

                    if ( nearestControl == out_handle_id )
                    {
                        var out_handle = Handles.PositionHandle( pos0 + p0.OutHandle, Quaternion.identity );
                        p0.OutHandle = out_handle - pos0;

                        bezierCurve.CalculateLength();
                    }
                }
            }

            // イベントタイプ別処理
            if ( Event.current.type == EventType.MouseMove )
            {
                foreach ( var kv in ids )
                {
                    if (
                        ( kv.Value.AnchorId == HandleUtility.nearestControl ) ||
                        ( kv.Value.InHandleId == HandleUtility.nearestControl ) ||
                        ( kv.Value.OutHandleId == HandleUtility.nearestControl ) )
                    {
                        int index = bezierCurve.FindIndexByGuid( kv.Key );

                        if ( index != -1 )
                        {
                            selectedIndex = index;
                            anchors.index = index;

                            nearestControl = HandleUtility.nearestControl;

                            // インスペクターを再描画したい。
                            Repaint();

                            bezierCurve.CalculateLength();

                        }

                    }
                }

            }

        }

    }

}