using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ShaderPluginGUI
{
    [Serializable]
    [XmlRoot(ElementName = "Shader")]
    public class ShaderXML
    {
        public const string LastShaderFile = "LastShader.xml";

        #region Shaders Data
        public MultiPassBuffers MultiPassBuffers = MultiPassBuffers.NoBuffers;
        public ShaderXML_Image Image = null;
        public ShaderXML_BufferA BufferA = null;
        public ShaderXML_BufferB BufferB = null;
        public ShaderXML_BufferC BufferC = null;
        public ShaderXML_BufferD BufferD = null;
        #endregion

        public ShaderXML()
        {
        }

        public ShaderXML(MultiPassBuffers MultiPassBuffers, ShaderXML_Image Image,
            ShaderXML_BufferA BufferA = null, ShaderXML_BufferB BufferB = null,
            ShaderXML_BufferC BufferC = null, ShaderXML_BufferD BufferD = null)
        {
            this.MultiPassBuffers = MultiPassBuffers;
            this.Image = Image;
            this.BufferA = BufferA;
            this.BufferB = BufferB;
            this.BufferC = BufferC;
            this.BufferD = BufferD;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + MultiPassBuffers.GetHashCode();
            hash = hash * 31 + Image.GetHashCode();
            if (BufferA != null) hash = hash * 31 + BufferA.GetHashCode();
            if (BufferB != null) hash = hash * 31 + BufferB.GetHashCode();
            if (BufferC != null) hash = hash * 31 + BufferC.GetHashCode();
            if (BufferD != null) hash = hash * 31 + BufferD.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ShaderXML shaderXML = (ShaderXML)obj;
            return MultiPassBuffers == shaderXML.MultiPassBuffers && Image == shaderXML.Image &&
                BufferA == shaderXML.BufferA && BufferB == shaderXML.BufferB && BufferC == shaderXML.BufferC && BufferD == shaderXML.BufferD;
        }

        public static bool operator ==(ShaderXML A, ShaderXML B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() == B?.GetType());

            return A.Equals(B);
        }

        public static bool operator !=(ShaderXML A, ShaderXML B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() != B?.GetType());

            return !A.Equals(B);
        }

        public static bool Save(ShaderXML ShaderXml, string FullPath)
        {
            if (File.Exists(FullPath))
            {
                try
                {
                    if (Load(FullPath) == ShaderXml)
                        return true;
                }
                catch { }
            }

            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(FullPath, new XmlWriterSettings() { Indent = true, IndentChars = "\t", OmitXmlDeclaration = true }))
                {
                    #region Remove "xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" ..."
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    #endregion

                    XmlSerializer serializer = new XmlSerializer(typeof(ShaderXML));
                    serializer.Serialize(xmlWriter, ShaderXml, namespaces);
                    xmlWriter.Close();
                    return File.Exists(FullPath);
                }
            }
            catch
            {
                return false;
            }
        }

        public static ShaderXML Load(string XmlFile)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ShaderXML));
            if (!File.Exists(XmlFile))
                return null;

            try
            {
                using (FileStream ShaderFile = new FileStream(XmlFile, FileMode.Open))
                {
                    ShaderXML S = (ShaderXML)serializer.Deserialize(ShaderFile);
                    ShaderFile.Close();
                    return S;
                }
            }
            catch
            {
                return null;
            }
        }
    }

    public struct ShaderXML_TextureFilters
    {
        public TextureMagFilter TextureMagFilter;
        public TextureMinFilter TextureMinFilter;
        public TextureWrapMode TextureWrapModeS;
        public TextureWrapMode TextureWrapModeT;

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (int)TextureMagFilter;
            hash = hash * 31 + (int)TextureMinFilter;
            hash = hash * 31 + (int)TextureWrapModeS;
            hash = hash * 31 + (int)TextureWrapModeT;
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ShaderXML_TextureFilters BufferA = (ShaderXML_TextureFilters)obj;
            return TextureMagFilter == BufferA.TextureMagFilter && TextureMinFilter == BufferA.TextureMinFilter &&
                TextureWrapModeS == BufferA.TextureWrapModeS && TextureWrapModeT == BufferA.TextureWrapModeT;
        }

        public static bool operator ==(ShaderXML_TextureFilters A, ShaderXML_TextureFilters B)
        {
            return A.Equals(B);
        }

        public static bool operator !=(ShaderXML_TextureFilters A, ShaderXML_TextureFilters B)
        {
            return !A.Equals(B);
        }
    }

    public class ShaderXML_Image
    {
        public string Shader_VS = Properties.Resources.Edit_VS;
        public string Shader_FS = Properties.Resources.EditImage_FS;
        public ShaderXML_TextureFilters TextureFilter;
        public ShaderXML_TextureFilters TextureFilterBufferA;
        public ShaderXML_TextureFilters TextureFilterBufferB;
        public ShaderXML_TextureFilters TextureFilterBufferC;
        public ShaderXML_TextureFilters TextureFilterBufferD;

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Shader_VS + Shader_FS).GetHashCode();
            hash = hash * 31 + TextureFilter.GetHashCode();
            hash = hash * 31 + TextureFilterBufferA.GetHashCode();
            hash = hash * 31 + TextureFilterBufferB.GetHashCode();
            hash = hash * 31 + TextureFilterBufferC.GetHashCode();
            hash = hash * 31 + TextureFilterBufferD.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ShaderXML_Image Image = (ShaderXML_Image)obj;
            return Shader_VS == Image.Shader_VS && Shader_FS == Image.Shader_FS && TextureFilter == Image.TextureFilter &&
                TextureFilterBufferA == Image.TextureFilterBufferA &&
                TextureFilterBufferB == Image.TextureFilterBufferB &&
                TextureFilterBufferC == Image.TextureFilterBufferC &&
                TextureFilterBufferD == Image.TextureFilterBufferD;
        }

        public static bool operator ==(ShaderXML_Image A, ShaderXML_Image B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() == B?.GetType());

            return A.Equals(B);
        }

        public static bool operator !=(ShaderXML_Image A, ShaderXML_Image B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() != B?.GetType());

            return !A.Equals(B);
        }
    }

    public class ShaderXML_BufferA
    {
        public string Shader_VS = Properties.Resources.Edit_VS;
        public string Shader_FS = Properties.Resources.EditBufferA_FS;
        public ShaderXML_TextureFilters TextureFilter;

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Shader_VS + Shader_FS).GetHashCode();
            hash = hash * 31 + TextureFilter.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ShaderXML_BufferA BufferA = (ShaderXML_BufferA)obj;
            return Shader_VS == BufferA.Shader_VS && Shader_FS == BufferA.Shader_FS && TextureFilter == BufferA.TextureFilter;
        }

        public static bool operator ==(ShaderXML_BufferA A, ShaderXML_BufferA B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() == B?.GetType());

            return A.Equals(B);
        }

        public static bool operator !=(ShaderXML_BufferA A, ShaderXML_BufferA B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() != B?.GetType());

            return !A.Equals(B);
        }
    }

    public class ShaderXML_BufferB
    {
        public string Shader_VS = Properties.Resources.Edit_VS;
        public string Shader_FS = Properties.Resources.EditBufferB_FS;
        public ShaderXML_TextureFilters TextureFilter;
        public ShaderXML_TextureFilters TextureFilterBufferA;

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Shader_VS + Shader_FS).GetHashCode();
            hash = hash * 31 + TextureFilter.GetHashCode();
            hash = hash * 31 + TextureFilterBufferA.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ShaderXML_BufferB BufferB = (ShaderXML_BufferB)obj;
            return Shader_VS == BufferB.Shader_VS && Shader_FS == BufferB.Shader_FS && TextureFilter == BufferB.TextureFilter && TextureFilterBufferA == BufferB.TextureFilterBufferA;
        }

        public static bool operator ==(ShaderXML_BufferB A, ShaderXML_BufferB B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() == B?.GetType());

            return A.Equals(B);
        }

        public static bool operator !=(ShaderXML_BufferB A, ShaderXML_BufferB B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() != B?.GetType());

            return !A.Equals(B);
        }
    }

    public class ShaderXML_BufferC
    {
        public string Shader_VS = Properties.Resources.Edit_VS;
        public string Shader_FS = Properties.Resources.EditBufferC_FS;
        public ShaderXML_TextureFilters TextureFilter;
        public ShaderXML_TextureFilters TextureFilterBufferA;
        public ShaderXML_TextureFilters TextureFilterBufferB;

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Shader_VS + Shader_FS).GetHashCode();
            hash = hash * 31 + TextureFilter.GetHashCode();
            hash = hash * 31 + TextureFilterBufferA.GetHashCode();
            hash = hash * 31 + TextureFilterBufferB.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ShaderXML_BufferC BufferC = (ShaderXML_BufferC)obj;
            return Shader_VS == BufferC.Shader_VS && Shader_FS == BufferC.Shader_FS && TextureFilter == BufferC.TextureFilter &&
                TextureFilterBufferA == BufferC.TextureFilterBufferA &&
                TextureFilterBufferB == BufferC.TextureFilterBufferB;
        }

        public static bool operator ==(ShaderXML_BufferC A, ShaderXML_BufferC B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() == B?.GetType());

            return A.Equals(B);
        }

        public static bool operator !=(ShaderXML_BufferC A, ShaderXML_BufferC B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() != B?.GetType());

            return !A.Equals(B);
        }
    }

    public class ShaderXML_BufferD
    {
        public string Shader_VS = Properties.Resources.Edit_VS;
        public string Shader_FS = Properties.Resources.EditBufferD_FS;
        public ShaderXML_TextureFilters TextureFilter;
        public ShaderXML_TextureFilters TextureFilterBufferA;
        public ShaderXML_TextureFilters TextureFilterBufferB;
        public ShaderXML_TextureFilters TextureFilterBufferC;

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Shader_VS + Shader_FS).GetHashCode();
            hash = hash * 31 + TextureFilter.GetHashCode();
            hash = hash * 31 + TextureFilterBufferA.GetHashCode();
            hash = hash * 31 + TextureFilterBufferB.GetHashCode();
            hash = hash * 31 + TextureFilterBufferC.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ShaderXML_BufferD BufferD = (ShaderXML_BufferD)obj;
            return Shader_VS == BufferD.Shader_VS && Shader_FS == BufferD.Shader_FS && TextureFilter == BufferD.TextureFilter &&
                TextureFilterBufferA == BufferD.TextureFilterBufferA &&
                TextureFilterBufferB == BufferD.TextureFilterBufferB &&
                TextureFilterBufferC == BufferD.TextureFilterBufferC;
        }

        public static bool operator ==(ShaderXML_BufferD A, ShaderXML_BufferD B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() == B?.GetType());

            return A.Equals(B);
        }

        public static bool operator !=(ShaderXML_BufferD A, ShaderXML_BufferD B)
        {
            if (A?.GetType() == null)
                return (A?.GetType() != B?.GetType());

            return !A.Equals(B);
        }
    }
}