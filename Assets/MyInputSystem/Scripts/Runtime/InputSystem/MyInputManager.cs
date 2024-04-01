//#define JoyStick

using GoldSprite.GUtils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GoldSprite.UnityPlugins.MyInputSystem {
    public abstract partial class MyInputManager {
        //引用
        public InputActions InputActions { get; private set; }
        private Dictionary<InputActionMap, bool> InputEnables;

        //配置
        [ShowInputManager]
        [SerializeField]
        private string draw;
        public bool debugLog = true;
        //实时
        public Dictionary<InputAction, Delegate> actions = new();
        protected Dictionary<InputAction, object> actionValues = new();


        public void Awake()
        {
            InitManager();

            InitDefaultActions();

            InitActions();
        }

        private void InitDefaultActions()
        {
            foreach(var map in InputEnables.Keys) {
                foreach(var actionKey in map) {
                    var type = actionKey.type;
                    Debug.Log($"ActionKey-{actionKey.name}: type-{type}");
                }
            }
        }

        private void InitManager()
        {
            InputActions = new InputActions();
            InputActions.Enable();

            InputEnables = SetInputActionMaps();
        }

        protected abstract Dictionary<InputActionMap, bool> SetInputActionMaps();

        protected virtual void InitActions()
        {
        }


        public void OnDisable()
        {
            InputActions.Disable();
        }


        public void AddActionListener<T>(InputAction keyAction, Action<T> act, bool log = false)
        {
            Action<InputAction.CallbackContext> proxy = null;
            //首次时增加基方法, 用于首先更新值
            if (!actions.ContainsKey(keyAction)) {
                Action<T> actParent = (p) => { };
                actParent += act;
                actions.Add(keyAction, actParent);
                actionValues.Add(keyAction, default(T));

                proxy = (c) => {
                    var valObj = keyAction.ReadValueAsObject();
                    if (valObj == null)
                        valObj = default(T);
                    T val = (T)Convert.ChangeType(valObj, typeof(T));

                    var disable = !IsInputEnable(keyAction.actionMap);
                    if (disable) return;  //输入禁用时返回
                    {   //自动存值
                        actionValues[keyAction] = val;
                        //debug log
                        if (log || debugLog) Debug.Log($"[InputSystem]: {keyAction.name}: {val}");
                        //事件调用
                        actions[keyAction]?.DynamicInvoke(val);
                    }
                };
                keyAction.performed += proxy;
                keyAction.canceled += proxy;
            } else {
                actions[keyAction] = ((Action<T>)actions[keyAction]) + act;
            }
        }


        /// <summary>
        /// 是否启用了该输入行动表
        /// </summary>
        /// <param name="actionMap"></param>
        /// <returns></returns>
        private bool IsInputEnable(InputActionMap actionMap)
        {
            return (!InputEnables.ContainsKey(actionMap)) || InputEnables[actionMap];
        }


        /// <summary>
        /// 设置输入启用/禁用
        /// </summary>
        /// <param name="actionMap">对应行为表</param>
        /// <param name="boo">启用或禁用</param>
        public void SetInputEnable(InputActionMap actionMap, bool boo)
        {
            if (!InputEnables.ContainsKey(actionMap)) return;
            InputEnables[actionMap] = boo;
        }


        public T GetValue<T>(InputAction keyAction)
        {
            if (actionValues.TryGetValue(keyAction, out object val)) return (T)val;
            return default(T);
        }


    }


    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class ShowInputManagerAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ShowInputManagerAttribute))]
    public class MyInputManagerDrawer : PropertyDrawer {
        private float height;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            height = 0;
            var target = property.serializedObject.targetObject;
            var input = ReflectionHelper.GetField<MyInputManager>(target);
            if (input == null) {
                return;
            }

            var actionValues = ReflectionHelper.GetValue<MyInputManager, Dictionary<InputAction, object>>(input, "actionValues");
            if (actionValues != null && actionValues.Count > 0) {
                position.height = EditorGUIUtility.singleLineHeight;
                //Debug.Log("Draw");
                float lineMargin = 5f;
                var i = 0;

                height += lineMargin;
                position.y += lineMargin;
                foreach (var (k, v) in actionValues) {
                    string label1 = k.name;
                    {
                        EditorGUI.TextField(position, label1, v == null ? "" : v.ToString());
                        height += EditorGUIUtility.singleLineHeight;
                        position.y += EditorGUIUtility.singleLineHeight + lineMargin;
                    }
                    i++;
                }
                height += EditorGUIUtility.singleLineHeight + lineMargin;

                EditorUtility.SetDirty(property.serializedObject.targetObject); //实时刷新重绘
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return height;
        }
    }
#endif


#if JoyStick
    public partial class MyInputManager : MonoBehaviour {

        /// <summary>
        /// 更新同步所有Action值
        /// </summary>
        private void Update()
        {
            if (joystick != null)
                UpdateJoystickValue();
        }


        /// <summary>
        /// 更改时<para/>
        /// 刷新Joystick轮盘输入
        /// </summary>
        private void UpdateJoystickValue()
        {
            var move = MoveActionValue;
            move.x = joystick.Horizontal;
            move.y = joystick.Vertical;
            if (oldJoystickMoveActionValue != move) {
                MoveActionValue = oldJoystickMoveActionValue = move;
                //Debug.Log("刷新摇杆值");
            }
        }

        private void GetJoystick()
        {
            joystick = GameObject.FindObjectOfType<Joystick>().GetComponent<Joystick>();
        }
    }
#endif


}