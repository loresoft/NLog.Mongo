using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace NLog.Mongo
{
    public static class MongoConvert
    {
        public static bool TryBoolean(string value, out BsonValue bsonValue)
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

        public static bool TryDateTime(string value, out BsonValue bsonValue)
        {
            bsonValue = null;
            if (value == null)
                return false;

            DateTime result;
            var r = DateTime.TryParse(value, out result);
            if (r) bsonValue = new BsonDateTime(result);

            return r;
        }

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
