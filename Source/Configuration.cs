using System;
using System.IO;
using Nett;
using LogViewer2.Utilities;
using System.Drawing;

namespace LogViewer2
{
    public class TomlConfiguration
    {
        public string HighlightColour { get; set; }
        public string ContextColour { get; set; }
        public int MultiSelectLimit { get; set; }
        public int NumContextLines { get; set; }
    }

    //TODO: The configuration needs to be rewritten so that highlightColour is a Color and not a string
    public class Configuration
    {
        #region Public Properties

        public Color HighlightColour;
        public Color ContextColour;
        public int MultiSelectLimit;
        public int NumContextLines;

        #endregion
    }

    /// <summary>
    /// Allows us to save/load the configuration file to/from TOML
    /// </summary>
    public class ConfigurationUtilities
    {
        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurationFilename">The name of the file to load</param>
        /// <param name="errorMessage">The error message if the method returns FALSE.</param>
        /// <returns>TRUE if the configuration was loaded successfull</returns>
        public static Configuration LoadConfigurationFromFile(string configurationFilename, out string errorMessage)
        {
            try
            {
                string configurationFilenamePath = FileUtilities.GetPathForFilename(configurationFilename);

                if (File.Exists(configurationFilenamePath) == false)
                {
                    errorMessage = $"Configuration file '{configurationFilenamePath}' not found.";
                    
                    return null;
                }

                TomlConfiguration tomlConfiguration = Toml.ReadFile<TomlConfiguration>(configurationFilenamePath);
                Configuration configuration = ParseTomlConfiguration(tomlConfiguration);

                errorMessage = String.Empty;

                return configuration;
            }
            catch (FileNotFoundException exception)
            {
                errorMessage = exception.Message;
                
                return null;
            }
            catch (UnauthorizedAccessException exception)
            {
                errorMessage =  exception.Message;
                
                return null;
            }
            catch (IOException exception)
            {
                errorMessage =  exception.Message;

                return null;
            }
            catch (Exception exception)
            {
                errorMessage =  exception.Message;

                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string Save(Configuration configuration, string configurationFilename)
        {
            try
            {
                string configurationFilenamePath = FileUtilities.GetPathForFilename(configurationFilename);

                Toml.WriteFile(configuration, configurationFilenamePath);

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

        private static Configuration ParseTomlConfiguration(TomlConfiguration tomlConfiguration)
        {
            Configuration configuration = new Configuration
            {
                MultiSelectLimit = tomlConfiguration.MultiSelectLimit,
                NumContextLines = tomlConfiguration.NumContextLines
            };

            if (configuration.MultiSelectLimit > 10000)
            {
                configuration.MultiSelectLimit = 10000;

                Console.WriteLine("[WARNING] ParseTomlConfiguration: The multiselect limit is 10000");
            }

            if (configuration.NumContextLines > 10)
            {
                configuration.NumContextLines = 10;

                Console.WriteLine("[WARNING] ParseTomlConfiguration: The maximum number of context lines is 10");
            }

            if(ColorUtilities.TryParseColour(tomlConfiguration.HighlightColour, out configuration.HighlightColour) == false)
            {
                configuration.HighlightColour = Color.Lime;
            }

            if(ColorUtilities.TryParseColour(tomlConfiguration.ContextColour, out configuration.ContextColour) == false)
            {
                configuration.ContextColour = Color.LightGray;
            }

            return configuration;
        }

        #endregion
    }
}