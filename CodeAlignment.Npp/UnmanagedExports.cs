﻿using System;
using System.Runtime.InteropServices;
using NppPluginNET;
using NppPlugin.DllExport;
using System.IO;
using System.Reflection;

namespace CMcG.CodeAlignment
{
    class UnmanagedExports
    {
        static UnmanagedExports()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainAssemblyResolve;
        }

        static string Location => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        static Assembly OnCurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("CodeAlignment.Common"))
            {
                var name = args.Name.Substring(0, args.Name.IndexOf(','));
                if (!name.EndsWith(".resources"))
                    return Assembly.LoadFile(Path.Combine(Location, $@"CodeAlignment\{name}.dll"));
            }

            return null;
        }

        [DllExport(CallingConvention=CallingConvention.Cdecl)]
        static bool isUnicode() => true;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void setInfo(NppData notepadPlusData)
        {
            PluginBase.nppData = notepadPlusData;
            Main.CommandMenuInit();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getFuncsArray(ref int nbF)
        {
            nbF = PluginBase._funcItems.Items.Count;
            return PluginBase._funcItems.NativePointer;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam) => 1;

        static IntPtr _ptrPluginName = IntPtr.Zero;
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getName()
        {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(Main.PluginName);
            return _ptrPluginName;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            SCNotification nc = (SCNotification)Marshal.PtrToStructure(notifyCode, typeof(SCNotification));
            if (nc.nmhdr.code == (uint)NppMsg.NPPN_TBMODIFICATION)
            {
                PluginBase._funcItems.RefreshItems();
                Main.SetToolBarIcon();
            }
            else if (nc.nmhdr.code == (uint)NppMsg.NPPN_SHUTDOWN)
            {
                Main.PluginCleanUp();
                Marshal.FreeHGlobal(_ptrPluginName);
            }
        }
    }
}
