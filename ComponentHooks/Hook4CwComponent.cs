using DynamicPatcher;
using Extension.Ext;
using PatcherYRpp;
using System;
using Extension.CW;
using Extension.Encryption;
using System.Runtime.Remoting.Messaging;

namespace ComponentHooks
{
    public class Hook4CwComponent
    {
        [Hook(HookType.AresHook, Address = 0x702E9D, Size = 6)]
        public static unsafe UInt32 TechnoClass_RegisterDestruction(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                Pointer<TechnoClass> pKiller = (IntPtr)R->EDI;

                double giveExpMultiple = 1.0;
                double gainExpMultiple = 1.0;

                if (pTechno != null)
                {
                    var technoExt = TechnoExt.ExtMap.Find(pTechno);
                    var globalExt = technoExt.GameObject.GetComponent<TechnoGlobalExtension>();
                    giveExpMultiple = globalExt.Data.GiveExperienceMultiple;
                }

                if (pKiller != null)
                {
                    var killerExt = TechnoExt.ExtMap.Find(pKiller);
                    var globalExt = killerExt.GameObject.GetComponent<TechnoGlobalExtension>();
                    gainExpMultiple = globalExt.Data.GainExperienceMultiple;
                }

                int cost = (int)R->EBP;
                R->EBP = (uint)(cost * gainExpMultiple * giveExpMultiple);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4DB7F7, Size = 6)]
        public static unsafe UInt32 FootClass_In_Which_Layer(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
            TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
            var gscript = ext.GameObject.GetComponent<TechnoGlobalExtension>();
            if (gscript != null)
            {
                var layer = gscript.Data.RenderLayer;

                if (!string.IsNullOrEmpty(layer))
                {
                    if (layer == "air")
                    {
                        R->EAX = (uint)Layer.Air;
                    }
                    else if (layer == "top")
                    {
                        R->EAX = (uint)Layer.Top;
                    }
                    else if (layer == "ground")
                    {
                        R->EAX = (uint)Layer.Ground;
                    }
                    return 0x4DB803;
                }
            }
            return 0;
        }


        //[Hook(HookType.AresHook, Address = 0x5B3C28, Size = 6)]
        //[Hook(HookType.AresHook, Address = 0x5B3D38, Size = 7)]
        [Hook(HookType.AresHook, Address = 0x5B3E30, Size = 7)]
        //[Hook(HookType.AresHook, Address = 0x5B3D93, Size = 5)]
        public static unsafe UInt32 MixFileClass_Load_Completed(REGISTERS* R)
        {
            Pointer<MixFileClass> mix = (IntPtr)R->ESI;
     
            if (MagicMixProvider.IsEncrypted(mix.Ref.FileName))
            {
                var realHeaders = MagicMixProvider.GetHeaders(mix.Ref.FileName);
                Pointer<MixHeaderData> headers = mix.Ref.Headers;
                if (!headers.IsNull)
                {
                    for (var i = 0; i < mix.Ref.CountFiles; i++)
                    {
                        headers[i] = realHeaders[i].CastToHeader();
                        //Logger.Log($"Offset:{header.Offset},ID:{header.ID},Size:{header.Size}");
                    }
                }

                for (var i = 0; i < mix.Ref.CountFiles; i++)
                {
                    var header = headers[i];
                    //Logger.Log($"Offset:{header.Offset},ID:{header.ID},Size:{header.Size}");
                }

                //Logger.Log($"Count:{mix.Ref.CountFiles},FileStartOffset:{mix.Ref.FileStartOffset},Size:{mix.Ref.FileSize}");

                //mix.Ref.CountFiles = realHeaders.Count;
                //mix.Ref.FileStartOffset = 4 + 6 + realHeaders.Count * 12;
            }

            return 0;
        }
    }
}
