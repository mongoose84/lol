namespace RiotProxy.Infrastructure
{
    /// <summary>
    /// Provides a static, read‑only view of the Riot API key.
    /// </summary>
    public static class ApiKey
    {
        private static string _value = string.Empty;
        public static string Value {
            get
            {
                if (_value == string.Empty)
                {
                    throw new InvalidOperationException("Riot API key has not been initialized. Call Read() to load the key from file.");
                }
                
                return _value;
            }
        }

        public static void Read()
        {
            try
            {
                // Open the text file using a stream reader.
                using StreamReader reader = new("RiotSecret.txt");
                // Read the stream as a string.
                _value = reader.ReadToEnd();
            }
            catch (IOException e)
            {
                Console.WriteLine("The RiotSecret could not be read:");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                
            }
        }
    }
}