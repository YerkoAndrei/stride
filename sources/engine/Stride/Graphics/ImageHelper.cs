// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Stride.Core;
using Stride.Core.IO;
using Stride.Core.Serialization;

namespace Stride.Graphics
{
    public class ImageHelper
    {
        internal static DataSerializer<ImageDescription> ImageDescriptionSerializer = SerializerSelector.Default.GetSerializer<ImageDescription>();
        internal static readonly FourCC MagicCode = "TKTX";

        public static unsafe Image LoadFromMemory(IntPtr pSource, int size, bool makeACopy, GCHandle? handle)
        {
            Debug.Assert(size >= 0);
            var ums = new UnmanagedMemoryStream((byte*)pSource, size, capacity: size, access: FileAccess.Read);
            var stream = new BinarySerializationReader(ums);

            // Read and check magic code
            var magicCode = stream.ReadUInt32();
            if (magicCode != MagicCode)
                return null;

            // Read header
            var imageDescription = new ImageDescription();
            ImageDescriptionSerializer.Serialize(ref imageDescription, ArchiveMode.Deserialize, stream);

            if (makeACopy)
            {
                var buffer = Utilities.AllocateMemory(size);
                Unsafe.CopyBlockUnaligned((void*)buffer, source: (void*)pSource, (uint)size);
                pSource = buffer;
                makeACopy = false;
            }

            var image = new Image(imageDescription, pSource, 0, handle, !makeACopy);

            var totalSizeInBytes = stream.ReadInt32();
            if (totalSizeInBytes != image.TotalSizeInBytes)
                throw new InvalidOperationException("Image size is different than expected.");

            // Read image data
            stream.Serialize(new Span<byte>((void*)image.DataPointer, image.TotalSizeInBytes));

            return image;
        }

        public static unsafe void SaveFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, System.IO.Stream imageStream)
        {
            var stream = new BinarySerializationWriter(imageStream);

            // Write magic code
            stream.Write(MagicCode);

            // Write image header
            ImageDescriptionSerializer.Serialize(ref description, ArchiveMode.Serialize, stream);

            // Write total size
            int totalSize = 0;
            foreach (var pixelBuffer in pixelBuffers)
                totalSize += pixelBuffer.BufferStride;

            stream.Write(totalSize);

            // Write buffers contiguously
            foreach (var pixelBuffer in pixelBuffers)
            {
                stream.Serialize(new Span<byte>((void*)pixelBuffer.DataPointer, pixelBuffer.BufferStride));
            }
        }
    }
}
