using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UnityVFXReactiveAudio
{
    // Extension methods for NativeArray/NativeSlice <-> ReadOnlySpan conversion
    static class SpanNativeArraySliceExtensions
    {
        public unsafe static NativeSlice<T>
          GetNativeSlice<T>(this ReadOnlySpan<T> span, int offset, int stride)
          where T : unmanaged
        {
            fixed (void* ptr = &span.GetPinnableReference())
            {
                var headPtr = (T*)ptr + offset;
                var strideInByte = sizeof(T) * stride;
                var elementCount = span.Length / stride - offset / stride;

                var slice =
                  NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<T>
                  (headPtr, strideInByte, elementCount);

              #if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeSliceUnsafeUtility.SetAtomicSafetyHandle
                  (ref slice, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
              #endif

                return slice;
            }
        }
        
        public unsafe static NativeSlice<T>
            GetNativeSlice<T>(this NativeSlice<T> sourceSlice, int offset, int stride)
            where T : unmanaged
        {
            var ptr = sourceSlice.GetUnsafeReadOnlyPtr();
            
            var headPtr = (T*)ptr + offset;
            var strideInByte = sizeof(T) * stride;
            var elementCount = sourceSlice.Length / stride - offset / stride;

            var slice =
                NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<T>
                    (headPtr, strideInByte, elementCount);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeSliceUnsafeUtility.SetAtomicSafetyHandle
                (ref slice, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif

            return slice;
            
        }
        

        public unsafe static NativeSlice<T>
          GetNativeSlice<T>(this ReadOnlySpan<T> span)
          where T : unmanaged
          => GetNativeSlice(span, 0, 1);

        public unsafe static ReadOnlySpan<T>
          GetReadOnlySpan<T>(this NativeArray<T> array)
          where T : unmanaged
        {
            var ptr = NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(array);
            return new Span<T>(ptr, array.Length);
        }

        public unsafe static ReadOnlySpan<T>
          GetReadOnlySpan<T>(this NativeSlice<T> slice)
          where T : unmanaged
        {
            var ptr = NativeSliceUnsafeUtility.GetUnsafeReadOnlyPtr(slice);
            return new Span<T>(ptr, slice.Length);
        }
    }

    // NativeArray allocation utilities
    static class TempJobMemory
    {
        public static NativeArray<T> New<T>(int size) where T : unmanaged
          => new NativeArray<T>(size, Allocator.TempJob,
                                NativeArrayOptions.UninitializedMemory);
    }

    static class PersistentMemory
    {
        public static NativeArray<T> New<T>(int size) where T : unmanaged
          => new NativeArray<T>(size, Allocator.Persistent,
                                NativeArrayOptions.UninitializedMemory);
    }

    // Extension methods for List<T>
    static class ListExtensions
    {
        // Find and retrieve an entry with removing it
        public static T FindAndRemove<T>(this List<T> list, Predicate<T> match)
        {
            var index = list.FindIndex(match);
            if (index < 0) return default(T);
            var res = list[index];
            list.RemoveAt(index);
            return res;
        }
    }

    // Math utility functions
    static class MathUtils
    {
        // Decibel (full scale) calculation
        // Reference level (full scale sin wave) = 1/sqrt(2)
        public static float dBFS(float p)
          => 20 * math.log10(p / 0.7071f + 1.5849e-13f);

        public static float2 dBFS(float2 p)
          => 20 * math.log10(p / 0.7071f + 1.5849e-13f);
    }
}
