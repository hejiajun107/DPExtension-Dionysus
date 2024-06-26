﻿using DynamicPatcher;
using Extension.CWUtilities;
using Extension.INI;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{
    public partial class TechnoGlobalExtension
    {
        public bool HasAnimTrailer { get; set; } = false;

        private bool _trailerInited = false;

        private List<TrailerAnim> trailerAnims = null;

        [AwakeAction]
        public void Anim_Trailer_Awake()
        {
            HasAnimTrailer = !string.IsNullOrEmpty(Art.AnimTrailer0);
        }

        //[PutAction]
        //public void Anim_Trailer_Put(CoordStruct coord, Direction faceDir)
        //{

        //}

        [UpdateAction]
        public void Anim_Trailer_Update()
        {

            if (!HasAnimTrailer)
            {
                return;
            }


            if (!_trailerInited)
            {
                if (!Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap)
                {
                    trailerAnims = new List<TrailerAnim>();
                    if (!string.IsNullOrEmpty(Art.AnimTrailer0))
                    {
                        var tr = CreateTrailerAnim(Art.AnimTrailer0, Art.AnimTrailer0FLH);
                        if (tr != null)
                            trailerAnims.Add(tr);
                    }
                    if (!string.IsNullOrEmpty(Art.AnimTrailer1))
                    {
                        var tr = CreateTrailerAnim(Art.AnimTrailer1, Art.AnimTrailer1FLH);
                        if (tr != null)
                            trailerAnims.Add(tr);
                    }
                    if (!string.IsNullOrEmpty(Art.AnimTrailer2))
                    {
                        var tr = CreateTrailerAnim(Art.AnimTrailer2, Art.AnimTrailer2FLH);
                        if (tr != null)
                            trailerAnims.Add(tr);
                    }
                    if (!string.IsNullOrEmpty(Art.AnimTrailer3))
                    {
                        var tr = CreateTrailerAnim(Art.AnimTrailer3, Art.AnimTrailer3FLH);
                        if (tr != null)
                            trailerAnims.Add(tr);
                    }
                    if (!string.IsNullOrEmpty(Art.AnimTrailer4))
                    {
                        var tr = CreateTrailerAnim(Art.AnimTrailer4, Art.AnimTrailer4FLH);
                        if (tr != null)
                            trailerAnims.Add(tr);
                    }
                    _trailerInited = true;
                }
            }

            if (trailerAnims != null)
            {
                if (!Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap)
                {
                    var visible = true;

                    if (Owner.OwnerObject.CastToFoot(out var pfoot))
                    {
                        if (pfoot.Ref.GetCurrentSpeed() <= 0)
                            visible = false;
                    }

                    foreach (var trailer in trailerAnims)
                    {
                        var facing = Owner.OwnerObject.Ref.Facing.current();
                        trailer.UpdateLocation(ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, trailer.FLH, false), facing, visible);
                    }
                }

            }
        }

        [RemoveAction]
        public void Anim_Trailer_Remove()
        {
            if (_trailerInited && trailerAnims != null)
            {
                foreach (var trailer in trailerAnims)
                {
                    trailer.Kill();
                }
            }
        }

        private TrailerAnim CreateTrailerAnim(string anim, int[] flh)
        {
            if (string.IsNullOrEmpty(anim))
            {
                return null;
            }

            Pointer<AnimTypeClass> animType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(anim);

            if (animType.IsNull)
            {
                return null;
            }

            CoordStruct coord = new CoordStruct(0, 0, 0);

            if (flh.Count() >= 3)
            {
                coord = new CoordStruct(flh[0], flh[1], flh[2]);
            }

            var location = ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, coord, false);

            var pAnim = YRMemory.Create<AnimClass>(animType, location);
            var spAnim = new SwizzleablePointer<AnimClass>(pAnim);

            var trailerAnim = new TrailerAnim(spAnim, coord, anim);

            return trailerAnim;
        }

        //[LateUpdateAction]
        //public void Anim_Trailer_Late_Update()
        //{
        //    if (trailerAnims == null)
        //        return;

        //    if (Owner.OwnerObject.Ref.Base.InLimbo || !Owner.OwnerObject.Ref.Base.IsOnMap)
        //        return;

        //    foreach (var trailer in trailerAnims)
        //    {
        //        if (trailer.Anim.IsNull)
        //            continue;
        //        DisplayClass.Instance.Ref.Submit(trailer.Anim.Pointer.Convert<ObjectClass>());
        //    }
        //}

    }

    [Serializable]
    public class TrailerAnim
    {
        public TrailerAnim(SwizzleablePointer<AnimClass> anim, CoordStruct fLH, string animType)
        {
            Anim = anim;
            FLH = fLH;
            AnimType = animType;
        }

        public SwizzleablePointer<AnimClass> Anim { get;private set; }

        public CoordStruct FLH { get; private set; }

        public string AnimType { get; private set; }



        public bool Killed { get {
                return Anim.IsNull;
        } }

        public void UpdateLocation(CoordStruct coordStruct, DirStruct dir, bool visible = true)
        {
            if (Killed)
            {
                var pAnim = YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(AnimType), coordStruct);
                Anim.Pointer = pAnim;
                Anim.Ref.Invisible = !visible;
            }
            else
            {
                Anim.Ref.Base.SetLocation(coordStruct);
                Anim.Ref.Invisible = !visible;
            }

            //if (AnimTrailerDirection > 0)
            //{
            //    var max = short.MaxValue - short.MinValue;
            //    var current =Math.Abs(dir.value()) - short.MinValue;
            //    var currentDirIndex = (int)Math.Round(AnimTrailerDirection * current / (double)max);

            //    var offssetAnim = Anim.Ref.Animation.Value % AnimTrailerDirection;

            //    var framePerDir = AnimTrailerDuration / AnimTrailerDirection;

            //    var shouldFrame = currentDirIndex * framePerDir + offssetAnim;

            //    if(shouldFrame!= Anim.Ref.Animation.Value)
            //    {
            //        Anim.Ref.Animation.Value = currentDirIndex * framePerDir + offssetAnim;
            //    }
            //}
        }

        public void Kill()
        {
            if (!Killed)
            {
                Anim.Ref.TimeToDie = true;
                Anim.Ref.Base.UnInit();
                Anim.Pointer = IntPtr.Zero;
            }
        }
    }


    public partial class TechnoGlobalArtExt
    {
        [INIField(Key ="AnimTrailer0.Animation")]
        public string AnimTrailer0;
        [INIField(Key = "AnimTrailer0.FLH")]
        public int[] AnimTrailer0FLH;
        [INIField(Key = "AnimTrailer1.Animation")]
        public string AnimTrailer1;
        [INIField(Key = "AnimTrailer1.FLH")]
        public int[] AnimTrailer1FLH;
        [INIField(Key = "AnimTrailer2.Animation")]
        public string AnimTrailer2;
        [INIField(Key = "AnimTrailer2.FLH")]
        public int[] AnimTrailer2FLH;
        [INIField(Key = "AnimTrailer3.Animation")]
        public string AnimTrailer3;
        [INIField(Key = "AnimTrailer3.FLH")]
        public int[] AnimTrailer3FLH;
        [INIField(Key = "AnimTrailer4.Animation")]
        public string AnimTrailer4;
        [INIField(Key = "AnimTrailer4.FLH")]
        public int[] AnimTrailer4FLH;
    }
}
