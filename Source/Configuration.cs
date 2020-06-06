using System;
using System.IO;
using System.Drawing;
using Nett;
using LogViewer2.Utilities;

namespace LogViewer2
{
    //TODO: The configuration needs to be rewritten so that highlightColour is a Color and not a string
    public class Configuration
    {
        #region Public Properties

        public string HighlightColour { get; set; } = "Lime";
        public string ContextColour { get; set; } = "LightGray";
        public int MultiSelectLimit { get; set; } = 1000;
        public int NumContextLines { get; set; } = 0;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Color GetHighlightColour()
        {
            Color temp = Color.FromName(this.HighlightColour);
            if (temp.IsKnownColor == false)
            {
                return Color.Lime;
            }

            return temp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Color GetContextColour()
        {
            Color temp = Color.FromName(this.ContextColour);
            if (temp.IsKnownColor == false)
            {
                return Color.LightGray;
            }

            return temp;
        }

        public void Validate()
        {
            if (MultiSelectLimit > 10000)
            {
                MultiSelectLimit = 10000;

                Console.WriteLine("[WARNING] ValidateConfiguration: The multiselect limit is 10000");
            }

            if (NumContextLines > 10)
            {
                NumContextLines = 10;

                Console.WriteLine("[WARNING] ValidateConfiguration: The maximum number of context lines is 10");
            }
        }
    }

    /// <summary>
    /// Allows us to save/load the configuration file to/from TOML
    /// </summary>
    public class TomlUtilities
    {
       

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurationFilename">The name of the file to load</param>
        /// <param name="errorMessage">The error message if the method returns FALSE.</param>
        /// <returns>TRUE if the configuration was loaded successfull</returns>
        public static Configuration LoadConfiguration(string configurationFilename, out string errorMessage)
        {
            try
            {
                string configurationFilenamePath = FileUtilities.GetPathForFilename(configurationFilename);

                if (File.Exists(configurationFilenamePath) == false)
                {
                    errorMessage = $"Configuration file '{configurationFilenamePath}' not found.";
                    
                    return null;
                }

                Configuration configuration = Toml.ReadFile<Configuration>(configurationFilenamePath);
                configuration.Validate();

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

        
        #endregion
    }
}