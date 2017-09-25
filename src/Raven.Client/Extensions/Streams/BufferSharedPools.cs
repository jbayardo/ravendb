using System.Runtime.CompilerServices;
using Sparrow;

namespace Raven.Client.Extensions.Streams
{
    internal static class BufferSharedPools
    {
        private const int Megabyte = 1024 * 1024;
        private const int Kilobyte = 1024;

        public const int HugeByteBufferSize = 4 * Megabyte;
        public const int BigByteBufferSize = 512 * Kilobyte;
        public const int ByteBufferSize = 64 * Kilobyte;
        public const int SmallByteBufferSize = 4 * Kilobyte;
        public const int MicroByteBufferSize = Kilobyte / 2;

        /// <summary>
        /// Used to reduce the # of temporary byte[]s created to satisfy serialization and
        /// other I/O requests
        /// </summary>
        public static readonly ObjectPool<byte[]> HugeByteArray = new ObjectPool<byte[]>(() => new byte[HugeByteBufferSize], 30);

        /// <summary>
        /// Used to reduce the # of temporary byte[]s created to satisfy serialization and
        /// other I/O requests
        /// </summary>
        public static readonly ObjectPool<byte[]> BigByteArray = new ObjectPool<byte[]>(() => new byte[BigByteBufferSize], 50);

        /// <summary>
        /// Used to reduce the # of temporary byte[]s created to satisfy serialization and
        /// other I/O requests
        /// </summary>
        public static readonly ObjectPool<byte[]> ByteArray = new ObjectPool<byte[]>(() => new byte[ByteBufferSize], 100);

        /// <summary>
        /// Used to reduce the # of temporary byte[]s created to satisfy serialization and
        /// other I/O requests
        /// </summary>
        public static readonly ObjectPool<byte[]> SmallByteArray = new ObjectPool<byte[]>(() => new byte[SmallByteBufferSize], 100);


        /// <summary>
        /// Used to reduce the # of temporary byte[]s created to satisfy serialization and
        /// other I/O requests
        /// </summary>
        public static readonly ObjectPool<byte[]> MicroByteArray = new ObjectPool<byte[]>(() => new byte[MicroByteBufferSize], 100);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] FetchOrAllocate(int lowerBoundOnSize)
        {
            var pool = FetchFromSmallestPool(lowerBoundOnSize);
            return pool != null ? pool.Allocate() : new byte[lowerBoundOnSize];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(byte[] array)
        {
            var pool = FetchFromSmallestPool(array.Length);
            pool?.Free(array);
        }

        private static ObjectPool<byte[]> FetchFromSmallestPool(int lowerBoundOnSize)
        {
            if (lowerBoundOnSize <= MicroByteBufferSize)
                return MicroByteArray;
            if (lowerBoundOnSize <= SmallByteBufferSize)
                return SmallByteArray;
            if (lowerBoundOnSize <= ByteBufferSize)
                return ByteArray;
            if (lowerBoundOnSize <= BigByteBufferSize)
                return BigByteArray;
            if (lowerBoundOnSize <= HugeByteBufferSize)
                return HugeByteArray;

            return null;
        }

    }
}
