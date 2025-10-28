namespace LolApi.Riot
{
    /// <summary>
    /// Provides a static, read‑only view of the Riot API key.
    /// </summary>
    public static class RiotApiKey
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
                Console.WriteLine("Attempting to read Riot API key from RiotSecret.txt...");
                // Open the text file using a stream reader.
                using StreamReader reader = new("RiotSecret.txt");
                Console.WriteLine("Riot API key file opened successfully.");
                // Read the stream as a string.
                _value = reader.ReadToEnd();
            }
            catch (IOException e)
            {
                Console.WriteLine("The RiotSecret.txt could not be read:");
                Console.WriteLine(e.Message);
            }
        }
    }
}