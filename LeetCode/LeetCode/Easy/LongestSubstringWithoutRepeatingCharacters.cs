using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

namespace LeetCode.Easy
{

    [MemoryDiagnoser]
    public class LongestSubstringWithoutRepeatingCharacters
    {
        private const string str = "As already explained in the previous article, managed pointers have their well-justified limitations – especially in that they are not allowed to appear on the Managed Heap (as a field of reference type or just by boxing). However, for some scenarios, it would be really nice to have a type that contains a managed pointer. The main motivation behind such type is Span<T> – which should be able to represent references “inside” objects (interior pointers), stack address or even unmanaged memory.";
        /*
         * a 0
         * b 1
         * c 2
         * a 3
         * b 4
         * c 5
         * b 6
         * b 7
         */

        // One of the best RUNTIME submissions in LeetCode
        // 0.3% of solutions used 50 ms of RUNTIME
        [Benchmark]
        [Arguments(str)]
        public int InvokeRuntime(string str)
        {
            int count = 0;
            string result = "";
            foreach (var c in str)
            {
                if (!result.Contains(c))
                    result += c;
                else
                {
                    if (result.Length > count)
                        count = result.Length;
                    result = result.Remove(0, result.IndexOf(c) + 1) + c;
                }
            }
            if (result.Length > count)
                count = result.Length;
            return count;
        }

        // One of the best MEMORY submissions in LeetCode
        // 0.04% of solutions used 35.42 MB
        [Benchmark]
        [Arguments(str)]
        public int InvokeMemory(string str)
        {
            if (str.Length == 0)
            {
                return 0;
            }

            var alphabet = new int[char.MaxValue + 1];
            int maxLength = 0;
            int currentLength = 1;
            int substringStartPosition = 1;
            alphabet[str[0]] = 1;
            for (int i = 1; i < str.Length; i++)
            {
                if (alphabet[str[i]] < substringStartPosition)
                {
                    alphabet[str[i]] = i + 1;
                    currentLength++;
                }
                else
                {
                    substringStartPosition = alphabet[str[i]];
                    maxLength = Math.Max(maxLength, currentLength);
                    currentLength = i + 1 - substringStartPosition;
                    alphabet[str[i]] = i + 1;
                }
            }

            return Math.Max(maxLength, currentLength);
        }

        [Benchmark]
        [Arguments(str)]
        public int InvokeInStack(string str)
        {
            int count;
            var readOnly = str.AsSpan();
            ref var refToFirstChar = ref Unsafe.AsRef(in readOnly[0]);
            unsafe
            {
                Span<char> span = new(Unsafe.AsPointer(ref refToFirstChar), str.Length);
                count = span.Length;
                for (int i = 0; i < span.Length; i++)
                {
                    var c = span[i];
                    if (Contains(span, i, c))
                    {
                        if (i > count)
                            count = i;

                        span = span[(span.IndexOf(c) + 1)..];
                        i = 0;
                    }
                    if (i > count)
                        count = i + 1;
                }

                if (span.Length > count)
                {
                    count = span.Length;
                }
            }
            return count;

            bool Contains(Span<char> span, int endIndex, char ch)
            {
                var results = span.Slice(0, endIndex);
                var result = results.IndexOf(ch);
                return result != -1;
            }
        }
    }
}
