using System;
using System.IO;
using LogViewer2.DataTypes;
using Nett;

namespace LogViewer2.Utilities
{
    public class TomlUtilities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurationFilename">The name of the file to load</param>
        /// <param name="errorMessage">The error message if the method returns FALSE.</param>
        /// <returns>TRUE if the configuration was loaded successfull</returns>
        public static TomlConfiguration LoadConfigurationFromFile(string configurationFilename, out string errorMessage)
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

                errorMessage = String.Empty;

                return tomlConfiguration;
            }
            catch (FileNotFoundException exception)
            {
                errorMessage = exception.Message;

                return null;
            }
            catch (UnauthorizedAccessException exception)
            {
                errorMessage = exception.Message;

                return null;
            }
            catch (IOException exception)
            {
                errorMessage = exception.Message;

                return null;
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;

                return null;
            }
        }
    }
}
