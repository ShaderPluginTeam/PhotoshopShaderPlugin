using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PS_Structures
{
    public delegate Int16 AllocateBufferProc(int size, IntPtr bufferID);
    public delegate IntPtr LockBufferProc(IntPtr bufferID, Boolean moveHigh);
    public delegate Int16 UnlockBufferProc(IntPtr bufferID);
    public delegate Int16 FreeBufferProc(IntPtr bufferID);
    public delegate Int32 BufferSpaceProc();
    public delegate Int16 ReserveSpaceProc(Int32 size);
    public delegate Int16 AllocateBufferProc64(Int64 size, IntPtr bufferID);
    public delegate Int64 BufferSpaceProc64();

    public struct BufferProcs
    {
        public Int16 bufferProcsVersion;       /**< The version number for the Buffer Procs. */

        public Int16 numBufferProcs;           /**< The number of routines in this structure. */

        public AllocateBufferProc allocateProc;    /**< Function pointer for the allocate routine. */

        public LockBufferProc lockProc;        /**< Function pointer for the lock routine. */

        public UnlockBufferProc unlockProc;    /**< Function pointer for the unlock routine. */

        public FreeBufferProc freeProc;        /**< Function pointer for the free routine. */

        public BufferSpaceProc spaceProc;      /**< Function pointer for the space routine. */

        public ReserveSpaceProc reserveProc;   /**< Function pointer for space reservation */

        public AllocateBufferProc64 allocateProc64;    /**< Function pointer for the allocate routine. */

        public BufferSpaceProc64 spaceProc64;      /**< Function pointer for the space routine. */
    }
}
