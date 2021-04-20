using System;
using System.Globalization;

namespace Orcus.Plugins
{
    /// <summary>
    ///     Serializable version of the System.Version class.
    /// </summary>
    [Serializable]
    public class PluginVersion : ICloneable, IComparable
    {
        private int _major;
        private int _minor;

        /// <summary>
        ///     Creates a new <see cref="PluginVersion" /> instance.
        /// </summary>
        public PluginVersion()
        {
            _major = 0;
            _minor = 0;
        }

        /// <summary>
        ///     Creates a new <see cref="PluginVersion" /> instance.
        /// </summary>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        public PluginVersion(int major, int minor)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException(nameof(major), "ArgumentOutOfRange_Version");

            if (minor < 0)
                throw new ArgumentOutOfRangeException(nameof(minor), "ArgumentOutOfRange_Version");

            _major = major;
            _minor = minor;
        }

        /// <summary>
        ///     Gets the major.
        /// </summary>
        /// <value></value>
        public int Major
        {
            get { return _major; }
            set { _major = value; }
        }

        /// <summary>
        ///     Gets the minor.
        /// </summary>
        /// <value></value>
        public int Minor
        {
            get { return _minor; }
            set { _minor = value; }
        }

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new PluginVersion
            {
                _major = _major,
                _minor = _minor
            };
        }

        /// <summary>
        ///     Compares to.
        /// </summary>
        /// <param name="version">Obj.</param>
        /// <returns></returns>
        public int CompareTo(object version)
        {
            if (version == null)
                return 1;

            var version1 = version as PluginVersion;
            if (version1 == null)
                throw new ArgumentException("Arg_MustBeVersion");

            if (_major != version1.Major)
            {
                if (_major > version1.Major)
                    return 1;
                return -1;
            }

            if (_minor != version1.Minor)
            {
                if (_minor > version1.Minor)
                    return 1;
                return -1;
            }

            return 0;
        }

        /// <summary>
        ///     Parse the <see cref="version" /> to a <see cref="PluginVersion" />
        /// </summary>
        /// <param name="version">The raw string</param>
        public static PluginVersion Parse(string version)
        {
            var result = new PluginVersion();

            if (string.IsNullOrEmpty(version))
                throw new ArgumentNullException(nameof(version));

            char[] chArray1 = {'.'};
            var textArray1 = version.Split(chArray1);

            if (textArray1.Length != 2)
                throw new ArgumentException("Couldn't parse string");

            result._major = int.Parse(textArray1[0], CultureInfo.InvariantCulture);
            if (result._major < 0)
                throw new ArgumentOutOfRangeException(nameof(version), "ArgumentOutOfRange_Version");

            result._minor = int.Parse(textArray1[1], CultureInfo.InvariantCulture);
            if (result._minor < 0)
                throw new ArgumentOutOfRangeException(nameof(version), "ArgumentOutOfRange_Version");

            return result;
        }

        /// <summary>
        ///     Tries to parse the <see cref="version" /> to a <see cref="PluginVersion" />
        /// </summary>
        /// <param name="version">The raw string</param>
        /// <param name="result">The output</param>
        /// <returns>True if successful, false if not</returns>
        public static bool TryParse(string version, out PluginVersion result)
        {
            result = null;

            var temp = new PluginVersion();

            if (string.IsNullOrEmpty(version))
                return false;

            char[] chArray1 = {'.'};
            var textArray1 = version.Split(chArray1);

            if (textArray1.Length != 2)
                return false;

            if (!int.TryParse(textArray1[0], NumberStyles.Any, CultureInfo.InvariantCulture, out temp._major) ||
                temp._major < 0)
                return false;

            if (!int.TryParse(textArray1[1], NumberStyles.Any, CultureInfo.InvariantCulture, out temp._minor) ||
                temp._minor < 0)
                return false;

            result = temp;
            return true;
        }

        /// <summary>
        ///     Equalss the specified obj.
        /// </summary>
        /// <param name="obj">Obj.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var version1 = obj as PluginVersion;
            return _major == version1?.Major && _minor == version1.Minor;
        }

        /// <summary>
        ///     Gets the hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int num1 = 0;
            num1 |= (_major & 15) << 0x1c;
            num1 |= (_minor & 0xff) << 20;
            return num1;
        }

        /// <summary>
        ///     Operator ==s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator ==(PluginVersion v1, PluginVersion v2)
        {
            return v1 != null && v1.Equals(v2);
        }

        /// <summary>
        ///     Operator &gt;s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator >(PluginVersion v1, PluginVersion v2)
        {
            return v2 < v1;
        }

        /// <summary>
        ///     Operator &gt;=s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator >=(PluginVersion v1, PluginVersion v2)
        {
            return v2 <= v1;
        }

        /// <summary>
        ///     Operator !=s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator !=(PluginVersion v1, PluginVersion v2)
        {
            return v1.Equals(v2);
        }

        /// <summary>
        ///     Operator &lt;s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator <(PluginVersion v1, PluginVersion v2)
        {
            if (v1 == null)
            {
                throw new ArgumentNullException(nameof(v1));
            }
            return v1.CompareTo(v2) < 0;
        }

        /// <summary>
        ///     Operator &lt;=s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator <=(PluginVersion v1, PluginVersion v2)
        {
            if (v1 == null)
            {
                throw new ArgumentNullException(nameof(v1));
            }
            return v1.CompareTo(v2) <= 0;
        }

        /// <summary>
        ///     Toes the string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(2);
        }

        /// <summary>
        ///     Toes the string.
        /// </summary>
        /// <param name="fieldCount">Field count.</param>
        /// <returns></returns>
        public string ToString(int fieldCount)
        {
            switch (fieldCount)
            {
                case 0:
                {
                    return string.Empty;
                }
                case 1:
                {
                    return _major.ToString();
                }
                case 2:
                {
                    return _major + "." + _minor;
                }
            }

            throw new ArgumentException("ArgumentOutOfRange_Bounds_Lower_Upper 0,2", nameof(fieldCount));
        }
    }
}