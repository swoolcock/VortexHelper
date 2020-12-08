﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.VortexHelper.Entities
{
    [CustomEntity("VortexHelper/AttachedJumpThru")]
    [Tracked(false)]
    class AttachedJumpThru : JumpThru
    {
        private int columns;
        private int overrideSoundIndex = -1;
        private string overrideTexture;
        private StaticMover staticMover;
        private Vector2 imageOffset;

        public Color EnabledColor = Color.White;
        public Color DisabledColor = Color.White;
        public bool VisibleWhenDisabled;

        public AttachedJumpThru(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Attr("texture", "default"), data.Int("surfaceIndex", -1))
        { }

        public AttachedJumpThru(Vector2 position, int width, string overrideTexture, int overrideSoundIndex = -1)
            : base(position, width, safe: false)
        {
            columns = width / 8;
            base.Depth = -60;
            this.overrideTexture = overrideTexture;
            this.overrideSoundIndex = overrideSoundIndex;
            staticMover = new StaticMover();
            staticMover.OnAttach = delegate (Platform p)
            {
                base.Depth = p.Depth + 1;
            };
            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                OnEnable = OnEnable,
                OnDisable = OnDisable
            });
        }
        public override void Awake(Scene scene)
        {
            AreaData areaData = AreaData.Get(scene);
            string jumpthru = areaData.Jumpthru;
            if (!string.IsNullOrEmpty(overrideTexture) && !overrideTexture.Equals("default"))
            {
                jumpthru = overrideTexture;
            }
            if (overrideSoundIndex > 0)
            {
                SurfaceSoundIndex = overrideSoundIndex;
            }
            else
            {
                switch (jumpthru.ToLower())
                {
                    case "dream":
                        SurfaceSoundIndex = 32;
                        break;
                    case "temple":
                    case "templeb":
                        SurfaceSoundIndex = 8;
                        break;
                    case "core":
                        SurfaceSoundIndex = 3;
                        break;
                    default:
                        SurfaceSoundIndex = 5;
                        break;
                }
            }
            MTexture mTexture = GFX.Game["objects/jumpthru/" + jumpthru];
            int num = mTexture.Width / 8;
            for (int i = 0; i < columns; i++)
            {
                int num2;
                int num3;
                if (i == 0)
                {
                    num2 = 0;
                    num3 = ((!CollideCheck<Solid>(Position + new Vector2(-1f, 0f))) ? 1 : 0);
                }
                else if (i == columns - 1)
                {
                    num2 = num - 1;
                    num3 = ((!CollideCheck<Solid>(Position + new Vector2(1f, 0f))) ? 1 : 0);
                }
                else
                {
                    num2 = 1 + Calc.Random.Next(num - 2);
                    num3 = Calc.Random.Choose(0, 1);
                }
                Image image = new Image(mTexture.GetSubtexture(num2 * 8, num3 * 8, 8, 8));
                image.X = i * 8;
                Add(image);
            }
            base.Awake(scene);
            areaData.Jumpthru = jumpthru;
        }
        private bool IsRiding(Solid solid)
        {
            if (CollideCheck(solid, Position + Vector2.UnitX))
                return true;
            if (CollideCheck(solid, Position - Vector2.UnitX))
                return true;
            else return false;
        }
        public override void Render()
        {
            Vector2 position = Position;
            Position += imageOffset;
            base.Render();
            Position = position;
        }
        public override void OnShake(Vector2 amount)
        {
            imageOffset += amount;
            ShakeStaticMovers(amount);
        }
        private void OnDisable()
        {
            Active = (Collidable = false);
            DisableStaticMovers();
            if (VisibleWhenDisabled)
            {
                foreach (Component component in base.Components)
                {
                    Image image = component as Image;
                    if (image != null)
                    {
                        image.Color = DisabledColor;
                    }
                }
            }
            else
            {
                Visible = false;
            }
        }
        private void OnEnable()
        {
            EnableStaticMovers();
            Active = (Visible = (Collidable = true));
        }
        public override void OnStaticMoverTrigger(StaticMover sm)
        {
            staticMover.TriggerPlatform();
        }
        public override void Update()
        {
            base.Update();
            Player playerRider = GetPlayerRider();
            if (playerRider != null && playerRider.Speed.Y >= 0f)
            {
                staticMover.TriggerPlatform();
            }
        }
    }
}