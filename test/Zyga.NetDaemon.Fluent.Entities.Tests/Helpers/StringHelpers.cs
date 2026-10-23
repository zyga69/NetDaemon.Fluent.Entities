namespace NetDaemon.Fluent.Entities.Tests.Helpers
{
    public static class StringHelpers
    {
        private static readonly Random _random = new Random();
        private const string _chars = "abcdefghijklmnopqrstuvwxyz";

        public static string Random()
        {
            int length = _random.Next(1, 21); // Random length between 1 and 20
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = _chars[_random.Next(_chars.Length)];
            }

            return new string(result);
        }
    }
}