using System.Globalization;

namespace CommonGIS
{
    public static class GeoCoordinatesHelper
    {
        // decimalVar.ToString ("0.##"); // returns "0"  when decimalVar == 0
        // geo coordinates precision of 0.0000001 is a practical limit of commercial surveying
        private const double MaxLongitudeDouble = 180;
        private const double MinLongitudeDouble = -180;
        private const decimal MaxLongitudeDecimal = 180;
        private const decimal MinLongitudeDecimal = -180;

        private const double MaxLatitudeDouble = 90;
        private const double MinLatitudeDouble = -90;
        private const decimal MaxLatitudeDecimal = 90;
        private const decimal MinLatitudeDecimal = -90;

        public static string GetLocationIdBasedOnGeoCoordinates(decimal longitude, decimal latitude)
        {
            return $"{longitude.ToString(CultureInfo.InvariantCulture)}-{latitude.ToString(CultureInfo.InvariantCulture)}";
        }

        public static bool IsLongitudeValid(decimal longitude)
        {
            if (longitude > MaxLongitudeDecimal || longitude < MinLongitudeDecimal)
            {
                return false;
            }

            return true;
        }

        public static bool IsLongitudeValid(double longitude)
        {
            if (longitude > MaxLongitudeDouble || longitude < MinLongitudeDouble)
            {
                return false;
            }

            return true;
        }
        //=> IsLongitudeValid((decimal)longitude);

        public static bool IsLatitudeValid(decimal latitude)
        {
            if (latitude > MaxLatitudeDecimal || latitude < MinLatitudeDecimal)
            {
                return false;
            }

            return true;
        }

        public static bool IsLatitudeValid(double latitude)
        {
            if (latitude > MaxLatitudeDouble || latitude < MinLatitudeDouble)
            {
                return false;
            }

            return true;
        }
        //=> IsLatitudeValid((decimal)latitude);
    }
}