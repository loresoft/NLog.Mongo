using System;
using MongoDB.Bson;

namespace NLog.Mongo
{
    /// <summary>
    /// Convert string values to Mongo <see cref="BsonValue"/>.
    /// </summary>
    public static class MongoConvert
    {
        /// <summary>Try to convert the string to a <see cref="BsonBoolean"/>.</summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="bsonValue">The BsonValue result.</param>
        /// <returns><c>true</c> if the value was converted; otherwise <c>false</c>.</returns>
        public static bool TryBoolean(this string value, out BsonValue bsonValue)
        {
            bsonValue = new BsonBoolean(false);

            if (value == null)
                return false;

            bool result;
            if (bool.TryParse(value, out result))
            {
                bsonValue = new BsonBoolean(true);
                return true;
            }

            string v = value.Trim();

            if (string.Equals(v, "t", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "x", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "on", StringComparison.OrdinalIgnoreCase))
                bsonValue = new BsonBoolean(true);

            return true;
        }

        /// <summary>Try to convert the string to a <see cref="BsonDateTime"/>.</summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="bsonValue">The BsonValue result.</param>
        /// <returns><c>true</c> if the value was converted; otherwise <c>false</c>.</returns>
        public static bool TryDateTime(this string value, out BsonValue bsonValue)
        {
            bsonValue = null;
            if (value == null)
                return false;

            DateTime result;
            var r = DateTime.TryParse(value, out result);
            if (r) bsonValue = new BsonDateTime(result);

            return r;
        }

        /// <summary>Try to convert the string to a <see cref="BsonDouble"/>.</summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="bsonValue">The BsonValue result.</param>
        /// <returns><c>true</c> if the value was converted; otherwise <c>false</c>.</returns>
        public static bool TryDouble(this string value, out BsonValue bsonValue)
        {
            bsonValue = null;
            if (value == null)
                return false;

            double result;
            var r = double.TryParse(value, out result);
            if (r) bsonValue = new BsonDouble(result);

            return r;
        }

        /// <summary>Try to convert the string to a <see cref="BsonInt32"/>.</summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="bsonValue">The BsonValue result.</param>
        /// <returns><c>true</c> if the value was converted; otherwise <c>false</c>.</returns>
        public static bool TryInt32(this string value, out BsonValue bsonValue)
        {
            bsonValue = null;
            if (value == null)
                return false;

            int result;
            var r = int.TryParse(value, out result);
            if (r) bsonValue = new BsonInt32(result);

            return r;
        }

        /// <summary>Try to convert the string to a <see cref="BsonInt64"/>.</summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="bsonValue">The BsonValue result.</param>
        /// <returns><c>true</c> if the value was converted; otherwise <c>false</c>.</returns>
        public static bool TryInt64(this string value, out BsonValue bsonValue)
        {
            bsonValue = null;
            if (value == null)
                return false;

            long result;
            var r = long.TryParse(value, out result);
            if (r) bsonValue = new BsonInt64(result);

            return r;
        }

    }
}
