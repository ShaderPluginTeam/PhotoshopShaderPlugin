using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Windows;
using System.Text.RegularExpressions;

namespace ShaderPluginGUI
{
    public delegate void ShaderCompileErrorHandler(string Name, Tuple<ShaderErrorType, string>[] Errors);

    public class Shader
    {
        public string Name, VS_Code, FS_Code;

        public int ProgramID = 0; //Shader program ID
        public int VS_ID = 0, FS_ID = 0; //Vertex, Fragment Shader ID

        public event ShaderCompileErrorHandler ShaderCompileError;

        public AttributeInfo[] Attributes = null;
        public UniformInfo[] Uniforms = null;

        public Shader()
        {
            Name = VS_Code = FS_Code = String.Empty;
        }

        public Shader(String Name, String VS_Code, String FS_Code)
        {
            this.Name = Name;
            this.VS_Code = VS_Code;
            this.FS_Code = FS_Code;
        }

        public bool Load()
        {
            if (GL.IsProgram(ProgramID))
                return true;

            ProgramID = GL.CreateProgram();

            LoadShaderFromString(VS_Code, ShaderType.VertexShader);
            LoadShaderFromString(FS_Code, ShaderType.FragmentShader);

            Link();
            TextureUnits();

            #region Check for Errors
            bool HasErrors = false;
            string InfoLog = String.Empty;
            List<Tuple<ShaderErrorType, string>> Errors = new List<Tuple<ShaderErrorType, string>>();
            GL.GetProgram(ProgramID, GetProgramParameterName.LinkStatus, out int LinkStatus);
            if (LinkStatus != 1)
            {
                InfoLog = GL.GetShaderInfoLog(VS_ID);
                if (InfoLog != String.Empty)
                {
                    Errors.Add(new Tuple<ShaderErrorType, string>(ShaderErrorType.Vertex, InfoLog));
                    HasErrors = true;
                }

                InfoLog = GL.GetShaderInfoLog(FS_ID);
                if (InfoLog != String.Empty)
                {
                    Errors.Add(new Tuple<ShaderErrorType, string>(ShaderErrorType.Fragment, InfoLog));
                    HasErrors = true;
                }
            }
            else
            {
                VS_Code = null;
                FS_Code = null;
            }

            // Check shader for errors
            InfoLog = GL.GetProgramInfoLog(ProgramID);
            if (InfoLog.Trim() != String.Empty)
            {
                Errors.Add(new Tuple<ShaderErrorType, string>(ShaderErrorType.Program, InfoLog));
                HasErrors = true;
            }
            #endregion

            if (HasErrors)
                OnShaderCompileError(Name, Errors.ToArray());

            return !HasErrors;
        }

        void LoadShaderFromString(String code, ShaderType type)
        {
            try
            {
                switch (type)
                {
                    case ShaderType.VertexShader:
                        CompileShader(code, type, out VS_ID);
                        break;
                    case ShaderType.FragmentShader:
                        CompileShader(code, type, out FS_ID);
                        break;
                }
            }
            catch
            {
                OnShaderCompileError(Name, new Tuple<ShaderErrorType, string>[] { new Tuple<ShaderErrorType, string>(ShaderErrorType.LoadFromString, code) });
            }
        }

        void CompileShader(String code, ShaderType type, out int shader)
        {
            shader = GL.CreateShader(type);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);
            GL.AttachShader(ProgramID, shader);
        }

        void Link()
        {
            GL.LinkProgram(ProgramID);

            GL.GetProgram(ProgramID, GetProgramParameterName.ActiveAttributes, out int AttributeCount);
            GL.GetProgram(ProgramID, GetProgramParameterName.ActiveUniforms, out int UniformCount);

            Attributes = new AttributeInfo[AttributeCount];
            Uniforms = new UniformInfo[UniformCount];

            for (int i = 0; i < AttributeCount; i++)
            {
                AttributeInfo info = new AttributeInfo();
                info.name = GL.GetActiveAttrib(ProgramID, i, out info.size, out info.type);
                info.address = GL.GetAttribLocation(ProgramID, info.name);
                Attributes[i] = info;
            }

            for (int i = 0; i < UniformCount; i++)
            {
                UniformInfo info = new UniformInfo();
                info.name = GL.GetActiveUniform(ProgramID, i, out info.size, out info.type);
                info.address = GL.GetUniformLocation(ProgramID, info.name);
                Uniforms[i] = info;
            }
        }

        void TextureUnits()
        {
            GL.UseProgram(ProgramID);

            for (int i = 0; i < Engine.TextureImageUnits; i++)
            {
                int TextureUnitLocation = GetUniform("TextureUnit" + i.ToString());
                if (TextureUnitLocation != -1)
                    GL.Uniform1(TextureUnitLocation, i);
            }

            GL.UseProgram(0);
        }

        public void EnableVertexAttribArrays()
        {
            for (int i = 0; i < Attributes.Length; i++)
                GL.EnableVertexAttribArray(Attributes[i].address);
        }

        public void DisableVertexAttribArrays()
        {
            for (int i = 0; i < Attributes.Length; i++)
                GL.DisableVertexAttribArray(Attributes[i].address);
        }

        public int GetAttribute(string name)
        {
            foreach (var item in Attributes)
                if (item.name == name)
                    return item.address;
            return -1;
        }

        public int GetUniform(string name)
        {
            foreach (var item in Uniforms)
                if (item.name == name)
                    return item.address;
            return -1;
        }

        public void Free()
        {
            GL.UseProgram(0);

            if (GL.IsShader(VS_ID))
            {
                GL.DetachShader(ProgramID, VS_ID);
                GL.DeleteShader(VS_ID);
                VS_ID = 0;
            }

            if (GL.IsShader(FS_ID))
            {
                GL.DetachShader(ProgramID, FS_ID);
                GL.DeleteShader(FS_ID);
                FS_ID = 0;
            }

            if (GL.IsProgram(ProgramID))
            {
                GL.DeleteProgram(ProgramID);
                ProgramID = 0;
            }

            Attributes = null;
            Uniforms = null;
        }

        private void OnShaderCompileError(string Name, Tuple<ShaderErrorType, string>[] Errors)
        {
            ShaderCompileError?.Invoke(Name, Errors);
        }

        public static int[] GetErrorsFromLog(string Log)
        {
            List<int> ErrLines = new List<int>();

            MatchCollection matches_nv = Regex.Matches(Log, @"^(-?\d+)\((\d+)\)\s:\s(error|warning)", RegexOptions.Multiline);
            if (matches_nv.Count > 0)
            {
                foreach (Match match_nv in matches_nv)
                    if (match_nv.Groups.Count > 2 && int.TryParse(match_nv.Groups[2].Value, out int ErrLineNV))
                        ErrLines.Add(ErrLineNV);
            }

            MatchCollection matches_other = Regex.Matches(Log, @"^(ERROR|WARNING):\s(-?\d+):(\d+):\s", RegexOptions.Multiline);
            if (matches_other.Count > 0)
            {
                foreach (Match match_other in matches_other)
                    if (match_other.Groups.Count > 3 && int.TryParse(match_other.Groups[3].Value, out int ErrLineOther))
                        ErrLines.Add(ErrLineOther);
            }

            return ErrLines.Distinct().ToArray();
        }
    }

    public class UniformInfo
    {
        public String name = "";
        public int address = -1;
        public int size = 0;
        public ActiveUniformType type;
    }

    public class AttributeInfo
    {
        public String name = "";
        public int address = -1;
        public int size = 0;
        public ActiveAttribType type;
    }

    public enum ShaderErrorType
    {
        Vertex,
        Fragment,
        Program,
        LoadFromString
    }
}
