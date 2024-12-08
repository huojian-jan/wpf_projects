﻿#pragma warning disable CS1591,CS1573,CS0465,CS0649,CS8019,CS1570,CS1584,CS1658,CS0436
namespace Windows.Win32
{
    using global::System;
    using global::System.Diagnostics;
    using global::System.Runtime.CompilerServices;
    using global::System.Runtime.InteropServices;
    using global::System.Runtime.Versioning;
    using winmdroot = global::Windows.Win32;
    namespace Foundation
    {
        [DebuggerDisplay("{Value}")]
        public readonly struct HRESULT
            : IEquatable<HRESULT>
        {
            public readonly int Value;
            public HRESULT(int value) => this.Value = value;
            public static implicit operator int(HRESULT value) => value.Value;
            public static explicit operator HRESULT(int value) => new HRESULT(value);
            public static bool operator ==(HRESULT left, HRESULT right) => left.Value == right.Value;
            public static bool operator !=(HRESULT left, HRESULT right) => !(left == right);

            public bool Equals(HRESULT other) => this.Value == other.Value;

            public override bool Equals(object obj) => obj is HRESULT other && this.Equals(other);

            public override int GetHashCode() => this.Value.GetHashCode();
            public static implicit operator uint(HRESULT value) => (uint)value.Value;
            public static explicit operator HRESULT(uint value) => new HRESULT((int)value);


            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public bool Succeeded => this.Value >= 0;


            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public bool Failed => this.Value < 0;


            /// <inheritdoc cref="Marshal.ThrowExceptionForHR(int, IntPtr)" />
            /// <param name="errorInfo">
            /// A pointer to the IErrorInfo interface that provides more information about the
            /// error. You can specify <see cref="IntPtr.Zero"/> to use the current IErrorInfo interface, or
            /// <c>new IntPtr(-1)</c> to ignore the current IErrorInfo interface and construct the exception
            /// just from the error code.
            /// </param>
            /// <returns><see langword="this"/> <see cref="HRESULT"/>, if it does not reflect an error.</returns>
            /// <seealso cref="Marshal.ThrowExceptionForHR(int, IntPtr)"/>
            public HRESULT ThrowOnFailure(IntPtr errorInfo = default)

            {
                Marshal.ThrowExceptionForHR(this.Value, errorInfo);
                return this;
            }

            public override string ToString() => this.Value.ToString();

            public string ToString(string format, IFormatProvider formatProvider) => ((uint)this.Value).ToString(format, formatProvider);
        }
    }
}