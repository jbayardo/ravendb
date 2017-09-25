using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Sparrow;
using Sparrow.Threading;

namespace Sparrow.Json
{
    public class UnmanagedBlittableStreamParser
    {
        private const int BufferSize = 4096;

        private readonly Stream _stream;

        private readonly byte[] _buffer = new byte[BufferSize];
        // This avoids pinning the _buffer every time we read from the stream
        private GCHandle _bufferGcHandle;

        public UnmanagedBlittableStreamParser(Stream inputStream)
        {
            Debug.Assert(_buffer.Length >= 4);
            _stream = inputStream;

            _bufferGcHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        }

        public async Task<BlittableJsonReaderObject> MoveNext(JsonOperationContext context)
        {
            // Read the size of the blittable
            if (await ReadFromStreamIntoBuffer(4) == false)
                return null;

            int size = 0;
            size |= _buffer[0];
            size |= _buffer[1] << 8;
            size |= _buffer[2] << 16;
            size |= _buffer[3] << 24;
            if (size <= 0)
                return null;

            var jsonObjectMemory = context.GetMemory(size);
            int amountReadFromStream = await ReadFromStreamIntoUnmanaged(jsonObjectMemory, size);
            if (amountReadFromStream < size)
            {
                context.ReturnMemory(jsonObjectMemory);
                return null;
            }

            unsafe
            {
                return new BlittableJsonReaderObject(jsonObjectMemory.Address, size, context);
            }
        }

        private async Task<bool> ReadFromStreamIntoBuffer(int amount)
        {
            Debug.Assert(amount > 0);
            int i;
            for (i = 0; i < amount;)
            {
                var effective = await _stream.ReadAsync(_buffer, i, amount - i);
                if (effective <= 0)
                    // The end of stream has been reached.
                    break;

                i += effective;
            }

            return i == amount;
        }

        public async Task<int> ReadFromStreamIntoUnmanaged(AllocatedMemoryData targetMemory, int size)
        {
            for (int position = 0; position < size;)
            {
                int expected = Math.Min(size - position, BufferSize);

                var effective = await _stream.ReadAsync(_buffer, 0, expected);
                if (effective <= 0)
                    return position;

                unsafe
                {
                    byte* bufferPtr = (byte*)_bufferGcHandle.AddrOfPinnedObject().ToPointer();
                    Memory.Copy(targetMemory.Address + position, bufferPtr, effective);
                }

                position += effective;
            }


            return size;
        }
    }

    internal class StreamCorruptionException : Exception
    {
        public StreamCorruptionException(string message)
            : base(message)
        {
        }

        public StreamCorruptionException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
