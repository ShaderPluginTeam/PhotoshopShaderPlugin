using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ShaderPluginGUI
{
    [XmlRoot(ElementName = "Config")]
    public class SettingsXML
    {
        public const string XmlFile = "Settings.xml";

        public static bool Save(SettingsXML SettingsXml, string FullPath)
        {
            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(FullPath, new XmlWriterSettings() { Indent = true, IndentChars = "\t", OmitXmlDeclaration = true }))
                {
                    #region Remove 'xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" ...'
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    #endregion

                    XmlSerializer serializer = new XmlSerializer(typeof(SettingsXML));
                    serializer.Serialize(xmlWriter, SettingsXml, namespaces);
                    xmlWriter.Close();
                    return File.Exists(FullPath);
                }
            }
            catch
            {
                return false;
            }
        }

        public static SettingsXML Load(string XmlFile)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SettingsXML));
            if (File.Exists(XmlFile)) //If file exist
            {
                try
                {
                    using (FileStream SettingsFile = new FileStream(XmlFile, FileMode.Open))
                    {
                        SettingsXML S = (SettingsXML)serializer.Deserialize(SettingsFile);
                        SettingsFile.Close();
                        return S;
                    }
                }
                catch
                {
                    return null;
                }
            }
            else //File not exist
            {
                if (Save(new SettingsXML(), XmlFile)) //Save new SettingsFile
                {
                    using (FileStream SettingsFile = new FileStream(XmlFile, FileMode.Open))
                    {
                        SettingsXML S = (SettingsXML)serializer.Deserialize(SettingsFile);
                        SettingsFile.Close();
                        return S;
                    }
                }
                else
                    return null;
            }
        }

        #region Settings
        public FormWindowState WindowState = FormWindowState.Normal;

        public TextureMagFilter MagFilter = TextureMagFilter.Nearest;
        public TextureMinFilter MinFilter = TextureMinFilter.Nearest;
        public bool UseMipMaps = true;
        public ColorRGBA BackgroundColor = new ColorRGBA(0.25f, 0.25f, 0.25f, 0f);

        public _PreviewLine PreviewLine = new _PreviewLine();
        public class _PreviewLine
        {
            public ColorRGBA LineColor = new ColorRGBA(0.7f, 0.7f, 0.7f, 0f);
            public float LineWidth = 2f;
        }

        public _Grid Grid = new _Grid();
        public class _Grid
        {
            public ColorRGBA GridColor1 = new ColorRGBA(0.8f, 0.8f, 0.8f, 0f);
            public ColorRGBA GridColor2 = new ColorRGBA(1.0f, 1.0f, 1.0f, 0f);
            public float GridSize = 16f;
        }
        #endregion
    }
}