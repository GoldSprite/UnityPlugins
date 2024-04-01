using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GoldSprite.UnityPlugins.MyInputSystem {

    public class BasicRoleInputComponent : MonoBehaviour {
        public BasicRoleInputs inputs;

        private void Awake()
        {
            inputs = new BasicRoleInputs() {
                role = transform,
                rb = GetComponent<Rigidbody2D>(), 
                anim = GetComponent<Animator>()
            };
            inputs.Awake();

        }

        private void OnDisable()
        {
            inputs.OnDisable();
        }
    }


    [Serializable]
    public class BasicRoleInputs : MyInputManager {
        //引用
        public Transform role;
        public Rigidbody2D rb;
        public Animator anim;

        //配置
        public float moveSpeed = 6.5f;

        [Header("实时")]
        public Vector3 realVel;
        public int MoveBoostSpeed = 1;

        public bool moveEvent { get; private set; }

        protected override Dictionary<InputActionMap, bool> SetInputActionMaps()
        {
            var maps = new Dictionary<InputActionMap, bool>() {
                { InputActions.GamePlay, true },
                { InputActions.UIPlay, true }
            };
            return maps;
        }

        protected override void InitActions()
        {
            AddActionListener(InputActions.TestPlay.Move, (Action<Vector2>)Move);
            AddActionListener(InputActions.TestPlay.MoveBoost, (Action<bool>)MoveBoost);
            AddActionListener(InputActions.TestPlay.Attack, (Action<bool>)Attack);
            AddActionListener(InputActions.TestPlay.SpecialAttack, (Action<bool>)SpecialAttack);
            AddActionListener(InputActions.TestPlay.Hurt, (Action<bool>)Hurt);
        }

        private void MoveBoost(bool down)
        {
            if (down)
                MoveBoostSpeed = MoveBoostSpeed == 2 ? 1 : 2;
        }

        public void Move(Vector2 dir)
        {
            var vel = rb.velocity;

            if (dir == Vector2.zero) {
                //停止
                vel.x = 0;
                anim.Play("Idle");
            } else {   //移动
                vel.x = dir.x * moveSpeed * MoveBoostSpeed;
                anim.Play("Move");

                //转向
                var face = role.localScale;
                face.x = vel.x > 0 ? 1 : -1;
                role.localScale = face;
            }

            rb.velocity = vel;
        }

        public void Attack(bool down)
        {
            if (down)
                anim.Play("Attack");
        }

        public void SpecialAttack(bool down)
        {
            if (down)
                anim.Play("SpecialAttack");
        }

        public void Hurt(bool down)
        {
            if (down)
                anim.Play("Hurt");
        }

        private void FixedUpdate()
        {
            realVel = rb.velocity;
            if (moveEvent) {
                moveEvent = false;
            }
        }
    }
}