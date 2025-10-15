using System;
using System.Globalization;
using System.Numerics;

namespace FireBlade.WinInteropUtils
{
    /// <summary>
    /// Defines Win32 macros.
    /// </summary>
    public static class Macros
    {
        /// <summary>
        /// Maps a system error code to an <see cref="HRESULT"/> value.
        /// </summary>
        /// <param name="error">The system error code.</param>
        /// <returns>The created <see cref="HRESULT"/>.</returns>
        public static HRESULT HResultFromWin32(this Win32ErrorCode error)
        {
            return ((HRESULT)((int)error) <= 0 ? ((HRESULT)((int)error)) : ((HRESULT)((((int)error) & 0x0000FFFF) | (Win32Constants.FACILITY_WIN32 << 16) | 0x80000000)));
        }

        /// <summary>
        /// Converts an integer value to a resource type compatible with the resource-management functions.
        /// This macro is used in place of a string containing the name of the resource.
        /// </summary>
        /// <remarks>
        /// The return value should be passed only to functions which explicitly indicate that
        /// they accept <see cref="MakeIntResource(int)"/> as a parameter. For example, the resource management functions
        /// allow the return value of <see cref="MakeIntResource(int)"/> to be passed as the <c>lpType</c> or <c>lpName</c> parameters.
        /// </remarks>
        /// <param name="i">The integer value to be converted.</param>
        /// <returns>The created resource.</returns>
        public static string MakeIntResource(int i) => ((ulong)((ushort)(i))).ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Provides a generic test for success on any <see cref="HRESULT"/> status value.
        /// </summary>
        /// <param name="hr">The status code. This value can be a <see cref="HRESULT"/>. A non-negative number indicates success.</param>
        /// <returns><see langword="true"/> if the status code is successful; otherwise, <see langword="false"/>.</returns>
        public static bool Succeeded(HRESULT hr) => hr >= 0;

        /// <summary>
        /// Provides a generic test for success on any <see cref="Win32ErrorCode"/> status value.
        /// </summary>
        /// <param name="hr">The status code. This value can be a <see cref="Win32ErrorCode"/>. A non-negative number indicates success.</param>
        /// <returns><see langword="true"/> if the status code is successful; otherwise, <see langword="false"/>.</returns>
        public static bool Succeeded(Win32ErrorCode hr) => ((HRESULT)hr) >= 0;

        /// <summary>
        /// Provides a generic test for failure on any <see cref="HRESULT"/> status value.
        /// </summary>
        /// <param name="hr">The status code. This value can be a <see cref="HRESULT"/>. A non-negative number indicates success.</param>
        /// <returns><see langword="true"/> if the status code is successful; otherwise, <see langword="false"/>.</returns>
        public static bool Failed(HRESULT hr) => hr < 0;

        /// <summary>
        /// Provides a generic test for failure on any <see cref="Win32ErrorCode"/> status value.
        /// </summary>
        /// <param name="hr">The status code. This value can be a <see cref="Win32ErrorCode"/>. A negative number indicates failure.</param>
        /// <returns><see langword="true"/> if the status code indicates failure; otherwise, <see langword="false"/>.</returns>
        public static bool Failed(Win32ErrorCode hr) => ((HRESULT)hr) < 0;

        /// <summary>
        /// Retrieves the low-order word from the specified value.
        /// </summary>
        /// <typeparam name="TNum">The number type.</typeparam>
        /// <param name="l">The value to be converted.</param>
        /// <returns>The low-order word of the value specified by <paramref name="l"/>.</returns>
        public static ushort LowWord<TNum>(TNum l) where TNum : INumber<TNum> => ((ushort)((nuint.CreateTruncating(l)) & 0xffff));

        /// <summary>
        /// Retrieves the high-order word from the specified 32-bit value.
        /// </summary>
        /// <typeparam name="TNum">The number type.</typeparam>
        /// <param name="l">The value to be converted.</param>
        /// <returns>The high-order word of the value specified by <paramref name="l"/>.</returns>
        public static ushort HighWord<TNum>(TNum l) where TNum: INumber<TNum> => ((ushort)(((nuint.CreateTruncating(l)) >> 16) & 0xffff));

        /// <summary>
        /// Retrieves the low-order word from the specified value.
        /// </summary>
        /// <param name="l">The value to be converted.</param>
        /// <returns>The low-order word of the value specified by <paramref name="l"/>.</returns>
        public static ushort LowWord(int l) => LowWord<int>(l);

        /// <summary>
        /// Retrieves the low-order word from the specified value.
        /// </summary>
        /// <param name="l">The value to be converted.</param>
        /// <returns>The low-order word of the value specified by <paramref name="l"/>.</returns>
        public static ushort LowWord(float l) => LowWord<float>(l);

        /// <summary>
        /// Retrieves the low-order word from the specified value.
        /// </summary>
        /// <param name="l">The value to be converted.</param>
        /// <returns>The low-order word of the value specified by <paramref name="l"/>.</returns>
        public static ushort LowWord(double l) => LowWord<double>(l);

        /// <summary>
        /// Retrieves the low-order word from the specified value.
        /// </summary>
        /// <param name="l">The value to be converted.</param>
        /// <returns>The low-order word of the value specified by <paramref name="l"/>.</returns>
        public static ushort LowWord(decimal l) => LowWord<decimal>(l);

        /// <summary>
        /// Retrieves the high-order word from the specified 32-bit value.
        /// </summary>
        /// <param name="l">The value to be converted.</param>
        /// <returns>The high-order word of the value specified by <paramref name="l"/>.</returns>
        public static ushort HighWord(int l) => HighWord<int>(l);

        /// <summary>
        /// Retrieves the high-order word from the specified 32-bit value.
        /// </summary>
        /// <param name="l">The value to be converted.</param>
        /// <returns>The high-order word of the value specified by <paramref name="l"/>.</returns>
        public static ushort HighWord(float l) => HighWord<float>(l);

        /// <summary>
        /// Retrieves the high-order word from the specified 32-bit value.
        /// </summary>
        /// <param name="l">The value to be converted.</param>
        /// <returns>The high-order word of the value specified by <paramref name="l"/>.</returns>
        public static ushort HighWord(double l) => HighWord<double>(l);

        /// <summary>
        /// Retrieves the high-order word from the specified 32-bit value.
        /// </summary>
        /// <param name="l">The value to be converted.</param>
        /// <returns>The high-order word of the value specified by <paramref name="l"/>.</returns>
        public static ushort HighWord(decimal l) => HighWord<decimal>(l);

        /// <summary>
        /// Retrieves the low-order byte from the specified value.
        /// </summary>
        /// <typeparam name="TNum">The number type.</typeparam>
        /// <param name="w">The value to be converted.</param>
        /// <returns>The low-order byte of the value specified by <paramref name="w"/>.</returns>
        public static byte LowByte<TNum>(TNum w) where TNum : INumber<TNum> => ((byte)((UIntPtr.CreateTruncating(w)) & 0xff));

        /// <summary>
        /// Retrieves the high-order byte from the given value.
        /// </summary>
        /// <typeparam name="TNum">The number type.</typeparam>
        /// <param name="w">The value to be converted.</param>
        /// <returns>The high-order byte of the specified value.</returns>
        public static byte HighByte<TNum>(TNum w) where TNum : INumber<TNum> => ((byte)(((UIntPtr.CreateTruncating(w)) >> 8) & 0xff));

        /// <summary>
        /// Retrieves the low-order byte from the specified value.
        /// </summary>
        /// <param name="w">The value to be converted.</param>
        /// <returns>The low-order byte of the value specified by <paramref name="w"/>.</returns>
        public static byte LowByte(int w) => LowByte<int>(w);

        /// <summary>
        /// Retrieves the low-order byte from the specified value.
        /// </summary>
        /// <param name="w">The value to be converted.</param>
        /// <returns>The low-order byte of the value specified by <paramref name="w"/>.</returns>
        public static byte LowByte(float w) => LowByte<float>(w);

        /// <summary>
        /// Retrieves the low-order byte from the specified value.
        /// </summary>
        /// <param name="w">The value to be converted.</param>
        /// <returns>The low-order byte of the value specified by <paramref name="w"/>.</returns>
        public static byte LowByte(double w) => LowByte<double>(w);

        /// <summary>
        /// Retrieves the low-order byte from the specified value.
        /// </summary>
        /// <param name="w">The value to be converted.</param>
        /// <returns>The low-order byte of the value specified by <paramref name="w"/>.</returns>
        public static byte LowByte(decimal w) => LowByte<decimal>(w);

        /// <summary>
        /// Retrieves the high-order byte from the given value.
        /// </summary>
        /// <param name="w">The value to be converted.</param>
        /// <returns>The high-order byte of the specified value.</returns>
        public static byte HighByte(int w) => HighByte<int>(w);

        /// <summary>
        /// Retrieves the high-order byte from the given value.
        /// </summary>
        /// <param name="w">The value to be converted.</param>
        /// <returns>The high-order byte of the specified value.</returns>
        public static byte HighByte(float w) => HighByte<float>(w);

        /// <summary>
        /// Retrieves the high-order byte from the given value.
        /// </summary>
        /// <param name="w">The value to be converted.</param>
        /// <returns>The high-order byte of the specified value.</returns>
        public static byte HighByte(double w) => HighByte<double>(w);

        /// <summary>
        /// Retrieves the high-order byte from the given value.
        /// </summary>
        /// <param name="w">The value to be converted.</param>
        /// <returns>The high-order byte of the specified value.</returns>
        public static byte HighByte(decimal w) => HighByte<decimal>(w);
    }
}
