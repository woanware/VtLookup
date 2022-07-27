using Tomlyn;

namespace VtLookup
{
    internal class Config
    {
        #region Member Variables
        public string ApiKey { get; set; } = "";
        public int BatchSize { get; set; } = 100;
        private const string FILENAME = "VtLookup.config";
        #endregion

        #region Public Methods
        /// <summary>
        /// Loads the config file and performs validation
        /// </summary>
        /// <returns></returns>
        public string Load()
        {
            try
            {
                if (File.Exists(this.GetPath()) == false)
                {
                    return string.Empty;
                }
                
                var toml = Toml.ToModel<Config>(File.ReadAllText(this.GetPath()));
                this.ApiKey = toml.ApiKey;
                this.BatchSize = toml.BatchSize;

                return string.Empty;
            }
            catch (FileNotFoundException fileNotFoundEx)
            {
                return fileNotFoundEx.Message;
            }
            catch (UnauthorizedAccessException unauthAccessEx)
            {
                return unauthAccessEx.Message;
            }
            catch (IOException ioEx)
            {
                return ioEx.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string Save()
        {
            try
            {
                var toml = Toml.FromModel(this);

                File.WriteAllText(this.GetPath(), toml);

                return string.Empty;
            }
            catch (FileNotFoundException fileNotFoundEx)
            {
                return fileNotFoundEx.Message;
            }
            catch (UnauthorizedAccessException unauthAccessEx)
            {
                return unauthAccessEx.Message;
            }
            catch (IOException ioEx)
            {
                return ioEx.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

        #region Misc Methods
        /// <summary>
        /// Returns the running processes directory
        /// </summary>
        /// <returns></returns>
        private static string GetApplicationDirectory()
        {
            return Path.GetDirectoryName(Environment.ProcessPath);
        }

        /// <summary>
        /// Returns the config file path 
        /// </summary>
        /// <returns></returns>
        private string GetPath()
        {
            return Path.Combine(GetApplicationDirectory(), FILENAME);
        }
        #endregion
    }
}
