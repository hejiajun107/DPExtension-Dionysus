using DynamicPatcher;
using Extension.Ext;
using PatcherYRpp;
using System;
using Extension.CW;
using Extension.Encryption;
using System.Runtime.Remoting.Messaging;

namespace ComponentHooks
{
    public class Hook4Encryption
    {
     

        [Hook(HookType.AresHook, Address = 0x5B3E30, Size = 7)]
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
                    }
                }

                for (var i = 0; i < mix.Ref.CountFiles; i++)
                {
                    var header = headers[i];
                }

            }
            return 0;
        }
    }
}
