/* 
Date: 08\23\2010 - Ludvik Jerabek - Initial Release
Version: 1.0
Comment: Allow INI manipulation in .NET
License: CPOL

Revisions:

08\23\2010 - Ludvik Jerabek - Initial Release
11\12\2010 - Ludvik Jerabek - Fixed section regex matching on key values with brackets
06\20\2015 - Ludvik Jerabek - Fixed key parsing regex to account for keys with spaces in names


**DISCLAIMER**
THIS MATERIAL IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
EITHER EXPRESS OR IMPLIED, INCLUDING, BUT Not LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE, OR NON-INFRINGEMENT. SOME JURISDICTIONS DO NOT ALLOW THE
EXCLUSION OF IMPLIED WARRANTIES, SO THE ABOVE EXCLUSION MAY NOT
APPLY TO YOU. IN NO EVENT WILL I BE LIABLE TO ANY PARTY FOR ANY
DIRECT, INDIRECT, SPECIAL OR OTHER CONSEQUENTIAL DAMAGES FOR ANY
USE OF THIS MATERIAL INCLUDING, WITHOUT LIMITATION, ANY LOST
PROFITS, BUSINESS INTERRUPTION, LOSS OF PROGRAMS OR OTHER DATA ON
YOUR INFORMATION HANDLING SYSTEM OR OTHERWISE, EVEN If WE ARE
EXPRESSLY ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.
*/

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

// IniFile class used to read and write ini files by loading the file into memory
namespace Orcus.Server.Core.Config
{
    public class IniFile
    {
        // List of IniSection objects keeps track of all the sections in the INI file
        private Hashtable m_sections;

        // Public constructor
        public IniFile()
        {
            m_sections = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        }

        // Loads the Reads the data in the ini file into the IniFile object
        public void Load(string sFileName)
        {
            Load(sFileName, false);
        }

        // Loads the Reads the data in the ini file into the IniFile object
        public void Load(string sFileName, bool bMerge)
        {
            if (!bMerge)
            {
                RemoveAllSections();
            }
            //  Clear the object... 
            IniSection tempsection = null;
            StreamReader oReader = new StreamReader(sFileName);
            Regex regexcomment = new Regex("^([\\s]*#.*)", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
            Regex regexsection = new Regex("^[\\s]*\\[[\\s]*([^\\[\\s].*[^\\s\\]])[\\s]*\\][\\s]*$", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
            Regex regexkey = new Regex("^\\s*([^=]*[^\\s=])\\s*=(.*)", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
            while (!oReader.EndOfStream)
            {
                string line = oReader.ReadLine();
                if (line != string.Empty)
                {
                    Match m = null;
                    if (regexcomment.Match(line).Success)
                    {
                        m = regexcomment.Match(line);
                        Trace.WriteLine($"Skipping Comment: {m.Groups[0].Value}");
                    }
                    else if (regexsection.Match(line).Success)
                    {
                        m = regexsection.Match(line);
                        Trace.WriteLine($"Adding section [{m.Groups[1].Value}]");
                        tempsection = AddSection(m.Groups[1].Value);
                    }
                    else if (regexkey.Match(line).Success && tempsection != null)
                    {
                        m = regexkey.Match(line);
                        Trace.WriteLine($"Adding Key [{m.Groups[1].Value}]=[{m.Groups[2].Value}]");
                        tempsection.AddKey(m.Groups[1].Value).Value = m.Groups[2].Value;
                    }
                    else if (tempsection != null)
                    {
                        //  Handle Key without value
                        Trace.WriteLine($"Adding Key [{line}]");
                        tempsection.AddKey(line);
                    }
                    else
                    {
                        //  This should not occur unless the tempsection is not created yet...
                        Trace.WriteLine($"Skipping unknown type of data: {line}");
                    }
                }
            }
            oReader.Close();
        }

        // Used to save the data back to the file or your choice
        public void Save(string sFileName)
        {
            StreamWriter oWriter = new StreamWriter(sFileName, false);
            foreach (IniSection s in Sections)
            {
                Trace.WriteLine($"Writing Section: [{s.Name}]");
                oWriter.WriteLine($"[{s.Name}]");
                foreach (IniSection.IniKey k in s.Keys)
                {
                    if (k.Value != string.Empty)
                    {
                        Trace.WriteLine($"Writing Key: {k.Name}={k.Value}");
                        oWriter.WriteLine($"{k.Name}={k.Value}");
                    }
                    else
                    {
                        Trace.WriteLine($"Writing Key: {k.Name}");
                        oWriter.WriteLine($"{k.Name}");
                    }
                }
            }
            oWriter.Close();
        }

        // Gets all the sections names
        public ICollection Sections => m_sections.Values;

        // Adds a section to the IniFile object, returns a IniSection object to the new or existing object
        public IniSection AddSection(string sSection)
        {
            IniSection s;
            sSection = sSection.Trim();
            // Trim spaces
            if (m_sections.ContainsKey(sSection))
            {
                s = (IniSection)m_sections[sSection];
            }
            else
            {
                s = new IniSection(this, sSection);
                m_sections[sSection] = s;
            }
            return s;
        }

        // Removes a section by its name sSection, returns trus on success
        public bool RemoveSection(string sSection)
        {
            sSection = sSection.Trim();
            return RemoveSection(GetSection(sSection));
        }

        // Removes section by object, returns trus on success
        public bool RemoveSection(IniSection section)
        {
            if (section != null)
            {
                try
                {
                    m_sections.Remove(section.Name);
                    return true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
            return false;
        }

        //  Removes all existing sections, returns trus on success
        public bool RemoveAllSections()
        {
            m_sections.Clear();
            return (m_sections.Count == 0);
        }

        // Returns an IniSection to the section by name, NULL if it was not found
        public IniSection GetSection(string sSection)
        {
            sSection = sSection.Trim();
            // Trim spaces
            if (m_sections.ContainsKey(sSection))
            {
                return (IniSection)m_sections[sSection];
            }
            return null;
        }

        //  Returns a KeyValue in a certain section
        public string GetKeyValue(string sSection, string sKey)
        {
            IniSection s = GetSection(sSection);
            IniSection.IniKey k = s?.GetKey(sKey);
            if (k != null)
            {
                return k.Value;
            }
            return string.Empty;
        }

        // Sets a KeyValuePair in a certain section
        public bool SetKeyValue(string sSection, string sKey, string sValue)
        {
            IniSection s = AddSection(sSection);
            IniSection.IniKey k = s?.AddKey(sKey);
            if (k != null)
            {
                k.Value = sValue;
                return true;
            }
            return false;
        }

        // Renames an existing section returns true on success, false if the section didn't exist or there was another section with the same sNewSection
        public bool RenameSection(string sSection, string sNewSection)
        {
            //  Note string trims are done in lower calls.
            bool bRval = false;
            IniSection s = GetSection(sSection);
            if (s != null)
            {
                bRval = s.SetName(sNewSection);
            }
            return bRval;
        }

        // Renames an existing key returns true on success, false if the key didn't exist or there was another section with the same sNewKey
        public bool RenameKey(string sSection, string sKey, string sNewKey)
        {
            //  Note string trims are done in lower calls.
            IniSection s = GetSection(sSection);
            if (s != null)
            {
                IniSection.IniKey k = s.GetKey(sKey);
                if (k != null)
                {
                    return k.SetName(sNewKey);
                }
            }
            return false;
        }

        // IniSection class 
        public class IniSection
        {
            //  IniFile IniFile object instance
            private readonly IniFile m_pIniFile;
            //  Name of the section
            private string m_sSection;
            //  List of IniKeys in the section
            private Hashtable m_keys;

            // Constuctor so objects are internally managed
            protected internal IniSection(IniFile parent, string sSection)
            {
                m_pIniFile = parent;
                m_sSection = sSection;
                m_keys = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            }

            // Returns and hashtable of keys associated with the section
            public ICollection Keys => m_keys.Values;

            // Returns the section name
            public string Name => m_sSection;

            // Adds a key to the IniSection object, returns a IniKey object to the new or existing object
            public IniKey AddKey(string sKey)
            {
                sKey = sKey.Trim();
                IniKey k = null;
                if (sKey.Length != 0)
                {
                    if (m_keys.ContainsKey(sKey))
                    {
                        k = (IniKey)m_keys[sKey];
                    }
                    else
                    {
                        k = new IniKey(this, sKey);
                        m_keys[sKey] = k;
                    }
                }
                return k;
            }

            // Removes a single key by string
            public bool RemoveKey(string sKey)
            {
                return RemoveKey(GetKey(sKey));
            }

            // Removes a single key by IniKey object
            public bool RemoveKey(IniKey key)
            {
                if (key != null)
                {
                    try
                    {
                        m_keys.Remove(key.Name);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
                return false;
            }

            // Removes all the keys in the section
            public bool RemoveAllKeys()
            {
                m_keys.Clear();
                return (m_keys.Count == 0);
            }

            // Returns a IniKey object to the key by name, NULL if it was not found
            public IniKey GetKey(string sKey)
            {
                sKey = sKey.Trim();
                if (m_keys.ContainsKey(sKey))
                {
                    return (IniKey)m_keys[sKey];
                }
                return null;
            }

            // Sets the section name, returns true on success, fails if the section
            // name sSection already exists
            public bool SetName(string sSection)
            {
                sSection = sSection.Trim();
                if (sSection.Length != 0)
                {
                    // Get existing section if it even exists...
                    IniSection s = m_pIniFile.GetSection(sSection);
                    if (s != this && s != null) return false;
                    try
                    {
                        // Remove the current section
                        m_pIniFile.m_sections.Remove(m_sSection);
                        // Set the new section name to this object
                        m_pIniFile.m_sections[sSection] = this;
                        // Set the new section name
                        m_sSection = sSection;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
                return false;
            }

            // Returns the section name
            public string GetName()
            {
                return m_sSection;
            }

            // IniKey class
            public class IniKey
            {
                //  Name of the Key
                private string m_sKey;
                //  Value associated
                private string m_sValue;
                //  Pointer to the parent CIniSection
                private IniSection m_section;

                // Constuctor so objects are internally managed
                protected internal IniKey(IniSection parent, string sKey)
                {
                    m_section = parent;
                    m_sKey = sKey;
                }

                // Returns the name of the Key
                public string Name => m_sKey;

                // Sets or Gets the value of the key
                public string Value
                {
                    get
                    {
                        return m_sValue;
                    }
                    set
                    {
                        m_sValue = value;
                    }
                }

                // Sets the value of the key
                public void SetValue(string sValue)
                {
                    m_sValue = sValue;
                }
                // Returns the value of the Key
                public string GetValue()
                {
                    return m_sValue;
                }

                // Sets the key name
                // Returns true on success, fails if the section name sKey already exists
                public bool SetName(string sKey)
                {
                    sKey = sKey.Trim();
                    if (sKey.Length != 0)
                    {
                        IniKey k = m_section.GetKey(sKey);
                        if (k != this && k != null) return false;
                        try
                        {
                            // Remove the current key
                            m_section.m_keys.Remove(m_sKey);
                            // Set the new key name to this object
                            m_section.m_keys[sKey] = this;
                            // Set the new key name
                            m_sKey = sKey;
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message);
                        }
                    }
                    return false;
                }

                // Returns the name of the Key
                public string GetName()
                {
                    return m_sKey;
                }
            } // End of IniKey class
        } // End of IniSection class
    }
} // End of IniFile class