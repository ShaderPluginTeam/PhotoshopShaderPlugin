using PS_Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ShaderPluginGUI
{
    public static class ImageDump // Used for tests only
    {
        public static void DumpToFile(string FileName, byte[] PixelsBytes, FilterRecord filterRecord)
        {
            byte[] FilterRecordBytes = FilterRecordGetBytes(filterRecord);
            File.WriteAllBytes(FileName + ".img_dump", PixelsBytes);
            File.WriteAllBytes(FileName + ".str_dump", FilterRecordBytes);
        }

        public static void LoadDumpFromFile(string FilePath, out byte[] PixelsBytes, out FilterRecord filterRecord)
        {
            string Dir = Path.GetDirectoryName(FilePath);
            string FileName = Path.GetFileNameWithoutExtension(FilePath);
            PixelsBytes = File.ReadAllBytes(Path.Combine(Dir, FileName + ".img_dump"));
            filterRecord = FilterRecordFromBytes(File.ReadAllBytes(Path.Combine(Dir, FileName + ".str_dump")));
        }

        public static byte[] FilterRecordGetBytes(FilterRecord filterRecord)
        {
            int size = Marshal.SizeOf(filterRecord);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(filterRecord, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public static FilterRecord FilterRecordFromBytes(byte[] Arr)
        {
            FilterRecord filterRecord = new FilterRecord();

            int size = Marshal.SizeOf(filterRecord);
            IntPtr pointer = Marshal.AllocHGlobal(size);

            Marshal.Copy(Arr, 0, pointer, size);

            filterRecord = (FilterRecord)Marshal.PtrToStructure(pointer, filterRecord.GetType());
            Marshal.FreeHGlobal(pointer);

            return filterRecord;
        }
    }
}
