using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using PS_Structures;

namespace ShaderPluginGUI
{
    public static class Engine
    {
        public static bool IsInitialized = false;
        public static DrawModes DrawMode = DrawModes.RGBA;
        public static MultiPassBuffersDrawMode MultiPassBufferDrawMode = MultiPassBuffersDrawMode.NoBuffers;
        public static Color4 GLClearColor = new Color4(0.25f, 0.25f, 0.25f, 0f);
        public static Color4 PreviewLineColor = new Color4(0.7f, 0.7f, 0.7f, 0f);
        public static float PreviewLineWidth = 2f;
        public static Color4 GridColor1 = new Color4(0.8f, 0.8f, 0.8f, 0f);
        public static Color4 GridColor2 = new Color4(1f, 1f, 1f, 0f);
        public static float GridSize = 16f;

        public static int[] TextureIDs = new int[6]; // Original, ProcessedImage, BufferA, BufferB, BufferC, BufferD
        public static Shader ViewShader, ViewLineShader, ViewGridShader, ConvertImageFomatShader, EditShaderImage, EditShaderBufferA, EditShaderBufferB, EditShaderBufferC, EditShaderBufferD;
        public static event ShaderCompileErrorHandler ShaderError;

        public static Matrix4 MVPMatrix = Matrix4.Identity;

        static MultiPassBuffers MultPassBuffers = MultiPassBuffers.NoBuffers;
        static TextureMagFilter TexMagFilter = TextureMagFilter.Nearest;
        static TextureMinFilter TexMinFilter = TextureMinFilter.Nearest;
        static bool Use_MipMaps = true;
        static int VertexBufferID, UVBufferID;
        static Vector2[] VBO_Vertexes = new Vector2[4];
        static Vector2[] VBO_UVs = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };
        static TimeSpan EngineRunningTime = new TimeSpan();
        static ColorRGBA PhotoshopColorBG = new ColorRGBA(1f, 1f, 1f, 1f);
        static ColorRGBA PhotoshopColorFG = new ColorRGBA(0f, 0f, 0f, 1f);

        #region TextureImageUnits
        /// <summary>
        /// MaxTextureImageUnits - This is the number of fragment shader texture image units.
        /// </summary>
        public static int MaxTextureImageUnits = 0; //FS
        public static int MaxVertexTextureImageUnits = 0; //VS
        public static int MaxGeometryTextureImageUnits = 0; //GS

        /// <summary>
        /// TextureImageUnits = Min(MaxTextureImageUnits, MaxVertexTextureImageUnits, MaxGeometryTextureImageUnits)
        /// </summary>
        public static int TextureImageUnits = 0;
        #endregion

        public static void Init()
        {
            EngineRunningTime = DateTime.Now.TimeOfDay;

            // Get Texture Units Count
            MaxTextureImageUnits = GL.GetInteger(GetPName.MaxTextureImageUnits); //FS TextureImageUnits Only
            MaxVertexTextureImageUnits = GL.GetInteger(GetPName.MaxVertexTextureImageUnits); //VS
            MaxGeometryTextureImageUnits = GL.GetInteger(GetPName.MaxGeometryTextureImageUnits); //GS
            TextureImageUnits = Math.Min(Math.Min(MaxTextureImageUnits, MaxVertexTextureImageUnits), MaxGeometryTextureImageUnits);

            #region Shaders
            ViewShader = new Shader("View", Properties.Resources.View_VS, Properties.Resources.View_FS);
            ViewShader.ShaderCompileError += OnShaderError;
            ViewShader.Load();

            ViewLineShader = new Shader("ViewLine", Properties.Resources.ViewLine_VS, Properties.Resources.ViewLine_FS);
            ViewLineShader.ShaderCompileError += OnShaderError;
            ViewLineShader.Load();

            ViewGridShader = new Shader("ViewGrid", Properties.Resources.View_VS, Properties.Resources.ViewGrid_FS);
            ViewGridShader.ShaderCompileError += OnShaderError;
            ViewGridShader.Load();

            ConvertImageFomatShader = new Shader("ConvertImageFomat", Properties.Resources.ConvertImageFomat_VS, Properties.Resources.ConvertImageFomat_FS);
            ConvertImageFomatShader.ShaderCompileError += OnShaderError;
            ConvertImageFomatShader.Load();
            #endregion

            VertexBufferID = GL.GenBuffer();
            UVBufferID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(VBO_Vertexes.Length * Vector2.SizeInBytes), VBO_Vertexes, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, UVBufferID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(VBO_UVs.Length * Vector2.SizeInBytes), VBO_UVs, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            IsInitialized = true;
        }

        internal static bool Compile(ShaderXML shaderXML)
        {
            if (!IsInitialized)
                return false;

            bool Result = true;

            Shader Shader_EditingImage = new Shader("EditingImage", shaderXML.Image.Shader_VS, shaderXML.Image.Shader_FS);
            Shader_EditingImage.ShaderCompileError += OnShaderError;
            if (Shader_EditingImage.Load())
            {
                EditShaderImage?.Free();
                EditShaderImage = Shader_EditingImage;
            }
            else
            {
                Shader_EditingImage.Free();
                Result = false;
            }

            int MultiPassBuffersCount = (int)MultiPassBuffers;
            if (MultiPassBuffersCount > 0)
            {
                #region BufferA
                Shader Shader_EditingBufferA = new Shader("EditingBufferA", shaderXML.BufferA.Shader_VS, shaderXML.BufferA.Shader_FS);
                Shader_EditingBufferA.ShaderCompileError += OnShaderError;
                if (Shader_EditingBufferA.Load())
                {
                    EditShaderBufferA?.Free();
                    EditShaderBufferA = Shader_EditingBufferA;
                }
                else
                {
                    Shader_EditingBufferA.Free();
                    Result = false;
                }
                #endregion

                if (MultiPassBuffersCount > 1)
                {
                    #region BufferB
                    Shader Shader_EditingBufferB = new Shader("EditingBufferB", shaderXML.BufferB.Shader_VS, shaderXML.BufferB.Shader_FS);
                    Shader_EditingBufferB.ShaderCompileError += OnShaderError;
                    if (Shader_EditingBufferB.Load())
                    {
                        EditShaderBufferB?.Free();
                        EditShaderBufferB = Shader_EditingBufferB;
                    }
                    else
                    {
                        Shader_EditingBufferB.Free();
                        Result = false;
                    }
                    #endregion

                    if (MultiPassBuffersCount > 2)
                    {
                        #region BufferC
                        Shader Shader_EditingBufferC = new Shader("EditingBufferC", shaderXML.BufferC.Shader_VS, shaderXML.BufferC.Shader_FS);
                        Shader_EditingBufferC.ShaderCompileError += OnShaderError;
                        if (Shader_EditingBufferC.Load())
                        {
                            EditShaderBufferC?.Free();
                            EditShaderBufferC = Shader_EditingBufferC;
                        }
                        else
                        {
                            Shader_EditingBufferC.Free();
                            Result = false;
                        }
                        #endregion

                        if (MultiPassBuffersCount > 3)
                        {
                            #region BufferD
                            Shader Shader_EditingBufferD = new Shader("EditingBufferD", shaderXML.BufferD.Shader_VS, shaderXML.BufferD.Shader_FS);
                            Shader_EditingBufferD.ShaderCompileError += OnShaderError;
                            if (Shader_EditingBufferD.Load())
                            {
                                EditShaderBufferD?.Free();
                                EditShaderBufferD = Shader_EditingBufferD;
                            }
                            else
                            {
                                Shader_EditingBufferD.Free();
                                Result = false;
                            }
                            #endregion
                        }
                    }
                }
            }

            return Result;
        }

        private static void OnShaderError(string Name, Tuple<ShaderErrorType, string>[] Errors)
        {
            ShaderError?.Invoke(Name, Errors);
        }

        public static void Draw(float X1, float X2, float Y1, float Y2, float PreviewPosition = 0.5f)
        {
            if (!IsInitialized)
                return;

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);
            GL.ClearColor(GLClearColor);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            int[] ViewportSizes = new int[4];
            GL.GetInteger(GetPName.Viewport, ViewportSizes);
            int ViewportWidth = ViewportSizes[2];
            float PreviewPos = PreviewPosition * 2f - 1f;

            #region Draw Line
            #region Update Vertexes For Line Render
            VBO_Vertexes = new Vector2[] { new Vector2(PreviewPos, -1), new Vector2(PreviewPos, 1) };
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(VBO_Vertexes.Length * Vector2.SizeInBytes), VBO_Vertexes);
            #endregion

            GL.LineWidth(PreviewLineWidth);

            GL.UseProgram(ViewLineShader.ProgramID);

            GL.Uniform4(ViewLineShader.GetUniform("LineColor"), PreviewLineColor);

            ViewLineShader.EnableVertexAttribArrays();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferID);
            GL.VertexAttribPointer(ViewLineShader.GetAttribute("v_Position"), 2, VertexAttribPointerType.Float, false, 0, 0);

            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            ViewLineShader.DisableVertexAttribArrays();
            #endregion

            #region Draw Grid
            #region Update Vertexes
            VBO_Vertexes = new Vector2[] { new Vector2(X1, Y1), new Vector2(X1, Y2), new Vector2(X2, Y2), new Vector2(X2, Y1) };
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(VBO_Vertexes.Length * Vector2.SizeInBytes), VBO_Vertexes);
            #endregion

            GL.UseProgram(ViewGridShader.ProgramID);

            GetTexturesSize(out int TextureWidth, out int TextureHeight);
            Vector2 TextureSize = new Vector2(TextureWidth, TextureHeight);
            float Zoom = Math.Abs(X2 - X1) / TextureWidth;

            GL.UniformMatrix4(ViewGridShader.GetUniform("MVP"), false, ref MVPMatrix);
            GL.Uniform2(ViewGridShader.GetUniform("Size"), TextureSize / Math.Max(2, GridSize) * Zoom);
            GL.Uniform4(ViewGridShader.GetUniform("ColorDark"), GridColor1);
            GL.Uniform4(ViewGridShader.GetUniform("ColorLight"), GridColor2);

            ViewGridShader.EnableVertexAttribArrays();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferID);
            GL.VertexAttribPointer(ViewGridShader.GetAttribute("v_Position"), 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, UVBufferID);
            GL.VertexAttribPointer(ViewGridShader.GetAttribute("v_UV"), 2, VertexAttribPointerType.Float, false, 0, 0);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            ViewGridShader.DisableVertexAttribArrays();
            #endregion

            #region Draw Image
            #region Update Vertexes
            VBO_Vertexes = new Vector2[] { new Vector2(X1, Y1), new Vector2(X1, Y2), new Vector2(X2, Y2), new Vector2(X2, Y1) };
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(VBO_Vertexes.Length * Vector2.SizeInBytes), VBO_Vertexes);
            #endregion

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            for (int i = 0; i < TextureIDs.Length; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                GL.BindTexture(TextureTarget.Texture2D, TextureIDs[i]);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TexMagFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TexMinFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }

            GL.UseProgram(ViewShader.ProgramID);

            GL.UniformMatrix4(ViewShader.GetUniform("MVP"), false, ref MVPMatrix);
            GL.Uniform2(ViewShader.GetUniform("DrawMode"), (int)DrawMode, (int)MultiPassBufferDrawMode);

            PreviewPos = MathHelper.Clamp((ViewportWidth * (PreviewPosition - 0.5f) - X1) / (X2 - X1), 0f, 1f);
            GL.Uniform1(ViewShader.GetUniform("PreviewPosition"), PreviewPos);

            ViewShader.EnableVertexAttribArrays();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferID);
            GL.VertexAttribPointer(ViewShader.GetAttribute("v_Position"), 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, UVBufferID);
            GL.VertexAttribPointer(ViewShader.GetAttribute("v_UV"), 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            ViewShader.DisableVertexAttribArrays();
            #endregion
        }

        public static void Edit(ShaderXML shaderXML)
        {
            if (!IsInitialized)
                return;

            if (EditShaderImage == null)
                return;

            GetTexturesSize(out int Tex_Width, out int Tex_Height);

            int[] ViewportSizes = new int[4];
            GL.GetInteger(GetPName.Viewport, ViewportSizes);
            GL.Viewport(0, 0, Tex_Width, Tex_Height);

            int MultiPassBuffersCount = (int)MultiPassBuffers;
            if (MultiPassBuffersCount > 0)
            {
                EditBufferA(shaderXML.BufferA, Tex_Width, Tex_Height, ViewportSizes[2], ViewportSizes[3]);

                if (MultiPassBuffersCount > 1)
                {
                    EditBufferB(shaderXML.BufferB, Tex_Width, Tex_Height, ViewportSizes[2], ViewportSizes[3]);

                    if (MultiPassBuffersCount > 2)
                    {
                        EditBufferC(shaderXML.BufferC, Tex_Width, Tex_Height, ViewportSizes[2], ViewportSizes[3]);

                        if (MultiPassBuffersCount > 3)
                            EditBufferD(shaderXML.BufferD, Tex_Width, Tex_Height, ViewportSizes[2], ViewportSizes[3]);
                    }
                }
            }

            EditImage(shaderXML.Image, Tex_Width, Tex_Height, ViewportSizes[2], ViewportSizes[3]);

            GL.Viewport(ViewportSizes[0], ViewportSizes[1], ViewportSizes[2], ViewportSizes[3]);

            if (UseMipMaps)
            {
                for (int i = 1; i < TextureIDs.Length; i++)
                {
                    GL.BindTexture(TextureTarget.Texture2D, TextureIDs[i]);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                }
            }
        }

        private static void EditBufferA(ShaderXML_BufferA BufferA, float TextureWidth, float TextureHeight, float ViewportWidth, float ViewportHeight)
        {
            int OutTextureIndex = 2; // Buffer A
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[OutTextureIndex]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            int FBO = GL.GenFramebuffer(); //frame buffer renderer for editing
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureIDs[OutTextureIndex], 0);

            if (!CheckFramebuffer(FBO, "FBO: Edit Buffer A")) // Check Framebuffer
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
                GL.DeleteFramebuffer(FBO);
                return;
            }

            // Drawing Setup
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Set Shader
            GL.UseProgram(EditShaderBufferA.ProgramID);
            SetShaderUniforms(TextureWidth, TextureHeight, ViewportWidth, ViewportHeight); // Set Parameters

            //Bind Textures
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[0]); // Original
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)BufferA.TextureFilter.TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)BufferA.TextureFilter.TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)BufferA.TextureFilter.TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)BufferA.TextureFilter.TextureWrapModeT);

            // Draw
            EditShaderBufferA.EnableVertexAttribArrays();
            GL.BindBuffer(BufferTarget.ArrayBuffer, UVBufferID);
            GL.VertexAttribPointer(EditShaderBufferA.GetAttribute("v_UV"), 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            EditShaderBufferA.DisableVertexAttribArrays();

            // Free
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
            GL.DeleteFramebuffer(FBO);
        }

        private static void EditBufferB(ShaderXML_BufferB BufferB, float TextureWidth, float TextureHeight, float ViewportWidth, float ViewportHeight)
        {
            int OutTextureIndex = 3; // Buffer B
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[OutTextureIndex]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            int FBO = GL.GenFramebuffer(); //frame buffer renderer for editing
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureIDs[OutTextureIndex], 0);

            if (!CheckFramebuffer(FBO, "FBO: Edit Buffer B")) // Check Framebuffer
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
                GL.DeleteFramebuffer(FBO);
                return;
            }

            // Drawing Setup
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Set Shader
            GL.UseProgram(EditShaderBufferB.ProgramID);
            SetShaderUniforms(TextureWidth, TextureHeight, ViewportWidth, ViewportHeight); // Set Parameters

            //Bind Textures
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[0]); // Original
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)BufferB.TextureFilter.TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)BufferB.TextureFilter.TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)BufferB.TextureFilter.TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)BufferB.TextureFilter.TextureWrapModeT);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[2]); // Buffer A
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)BufferB.TextureFilterBufferA.TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)BufferB.TextureFilterBufferA.TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)BufferB.TextureFilterBufferA.TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)BufferB.TextureFilterBufferA.TextureWrapModeT);

            // Draw
            EditShaderBufferB.EnableVertexAttribArrays();
            GL.BindBuffer(BufferTarget.ArrayBuffer, UVBufferID);
            GL.VertexAttribPointer(EditShaderBufferB.GetAttribute("v_UV"), 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            EditShaderBufferB.DisableVertexAttribArrays();

            // Free
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
            GL.DeleteFramebuffer(FBO);
        }

        private static void EditBufferC(ShaderXML_BufferC BufferC, float TextureWidth, float TextureHeight, float ViewportWidth, float ViewportHeight)
        {
            int OutTextureIndex = 4; // Buffer C
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[OutTextureIndex]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            int FBO = GL.GenFramebuffer(); //frame buffer renderer for editing
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureIDs[OutTextureIndex], 0);

            if (!CheckFramebuffer(FBO, "FBO: Edit Buffer C")) // Check Framebuffer
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
                GL.DeleteFramebuffer(FBO);
                return;
            }

            // Drawing Setup
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Set Shader
            GL.UseProgram(EditShaderBufferC.ProgramID);
            SetShaderUniforms(TextureWidth, TextureHeight, ViewportWidth, ViewportHeight); // Set Parameters

            //Bind Textures
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[0]); // Original
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)BufferC.TextureFilter.TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)BufferC.TextureFilter.TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)BufferC.TextureFilter.TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)BufferC.TextureFilter.TextureWrapModeT);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[2]); // Buffer A
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)BufferC.TextureFilterBufferA.TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)BufferC.TextureFilterBufferA.TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)BufferC.TextureFilterBufferA.TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)BufferC.TextureFilterBufferA.TextureWrapModeT);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[3]); // Buffer B
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)BufferC.TextureFilterBufferB.TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)BufferC.TextureFilterBufferB.TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)BufferC.TextureFilterBufferB.TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)BufferC.TextureFilterBufferB.TextureWrapModeT);

            // Draw
            EditShaderBufferC.EnableVertexAttribArrays();
            GL.BindBuffer(BufferTarget.ArrayBuffer, UVBufferID);
            GL.VertexAttribPointer(EditShaderBufferC.GetAttribute("v_UV"), 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            EditShaderBufferC.DisableVertexAttribArrays();

            // Free
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
            GL.DeleteFramebuffer(FBO);
        }

        private static void EditBufferD(ShaderXML_BufferD BufferD, float TextureWidth, float TextureHeight, float ViewportWidth, float ViewportHeight)
        {
            int OutTextureIndex = 5; // Buffer D
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[OutTextureIndex]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            int FBO = GL.GenFramebuffer(); //frame buffer renderer for editing
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureIDs[OutTextureIndex], 0);

            if (!CheckFramebuffer(FBO, "FBO: Edit Buffer D")) // Check Framebuffer
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
                GL.DeleteFramebuffer(FBO);
                return;
            }

            // Drawing Setup
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Set Shader
            GL.UseProgram(EditShaderBufferD.ProgramID);
            SetShaderUniforms(TextureWidth, TextureHeight, ViewportWidth, ViewportHeight); // Set Parameters

            //Bind Textures
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[0]); // Original
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)BufferD.TextureFilter.TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)BufferD.TextureFilter.TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)BufferD.TextureFilter.TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)BufferD.TextureFilter.TextureWrapModeT);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[2]); // Buffer A
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)BufferD.TextureFilterBufferA.TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)BufferD.TextureFilterBufferA.TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)BufferD.TextureFilterBufferA.TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)BufferD.TextureFilterBufferA.TextureWrapModeT);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[3]); // Buffer B
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)BufferD.TextureFilterBufferB.TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)BufferD.TextureFilterBufferB.TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)BufferD.TextureFilterBufferB.TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)BufferD.TextureFilterBufferB.TextureWrapModeT);

            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[4]); // Buffer C
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)BufferD.TextureFilterBufferC.TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)BufferD.TextureFilterBufferC.TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)BufferD.TextureFilterBufferC.TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)BufferD.TextureFilterBufferC.TextureWrapModeT);

            // Draw
            EditShaderBufferD.EnableVertexAttribArrays();
            GL.BindBuffer(BufferTarget.ArrayBuffer, UVBufferID);
            GL.VertexAttribPointer(EditShaderBufferD.GetAttribute("v_UV"), 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            EditShaderBufferD.DisableVertexAttribArrays();

            // Free
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
            GL.DeleteFramebuffer(FBO);
        }

        private static void EditImage(ShaderXML_Image Image, float TextureWidth, float TextureHeight, float ViewportWidth, float ViewportHeight)
        {
            int OutTextureIndex = 1; // Image
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[OutTextureIndex]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            int FBO = GL.GenFramebuffer(); //frame buffer renderer for editing
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureIDs[OutTextureIndex], 0);

            if (!CheckFramebuffer(FBO, "FBO: Edit Image")) // Check Framebuffer
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
                GL.DeleteFramebuffer(FBO);
                return;
            }

            // Drawing setup
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Set Shader
            GL.UseProgram(EditShaderImage.ProgramID);
            SetShaderUniforms(TextureWidth, TextureHeight, ViewportWidth, ViewportHeight); // Set Parameters

            //Bind Textures
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[0]); // Original
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Image.TextureFilter.TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Image.TextureFilter.TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Image.TextureFilter.TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Image.TextureFilter.TextureWrapModeT);

            int MultiPassBuffersCount = (int)MultiPassBuffers;
            if (MultiPassBuffersCount > 0)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, TextureIDs[2]); // Buffer A
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Image.TextureFilterBufferA.TextureMagFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Image.TextureFilterBufferA.TextureMinFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Image.TextureFilterBufferA.TextureWrapModeS);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Image.TextureFilterBufferA.TextureWrapModeT);
            }

            if (MultiPassBuffersCount > 1)
            {
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, TextureIDs[3]); // Buffer B
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Image.TextureFilterBufferB.TextureMagFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Image.TextureFilterBufferB.TextureMinFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Image.TextureFilterBufferB.TextureWrapModeS);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Image.TextureFilterBufferB.TextureWrapModeT);
            }

            if (MultiPassBuffersCount > 2)
            {
                GL.ActiveTexture(TextureUnit.Texture3);
                GL.BindTexture(TextureTarget.Texture2D, TextureIDs[4]); // Buffer C
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Image.TextureFilterBufferC.TextureMagFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Image.TextureFilterBufferC.TextureMinFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Image.TextureFilterBufferC.TextureWrapModeS);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Image.TextureFilterBufferC.TextureWrapModeT);
            }

            if (MultiPassBuffersCount > 3)
            {
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2D, TextureIDs[5]); // Buffer D
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Image.TextureFilterBufferC.TextureMagFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Image.TextureFilterBufferC.TextureMinFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Image.TextureFilterBufferC.TextureWrapModeS);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Image.TextureFilterBufferC.TextureWrapModeT);
            }

            // Draw
            EditShaderImage.EnableVertexAttribArrays();
            GL.BindBuffer(BufferTarget.ArrayBuffer, UVBufferID);
            GL.VertexAttribPointer(EditShaderImage.GetAttribute("v_UV"), 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            EditShaderImage.DisableVertexAttribArrays();

            // Free
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
            GL.DeleteFramebuffer(FBO);
        }

        private static void SetShaderUniforms(float TextureWidth, float TextureHeight, float ViewportWidth, float ViewportHeight)
        {
            int UnioformID = EditShaderImage.GetUniform("iColorBG");
            if (UnioformID >= 0)
                GL.Uniform3(UnioformID, PhotoshopColorBG);

            UnioformID = EditShaderImage.GetUniform("iColorFG");
            if (UnioformID >= 0)
                GL.Uniform3(UnioformID, PhotoshopColorFG);

            UnioformID = EditShaderImage.GetUniform("iImageSize");
            if (UnioformID >= 0)
                GL.Uniform2(UnioformID, TextureWidth, TextureHeight);

            UnioformID = EditShaderImage.GetUniform("iViewSize");
            if (UnioformID >= 0)
                GL.Uniform2(UnioformID, ViewportWidth, ViewportHeight);

            UnioformID = EditShaderImage.GetUniform("iRandom");
            if (UnioformID >= 0)
            {
                Random R = new Random();
                GL.Uniform4(UnioformID, (float)R.NextDouble(), (float)R.NextDouble(), (float)R.NextDouble(), (float)R.NextDouble());
            }

            UnioformID = EditShaderImage.GetUniform("iTime");
            if (UnioformID >= 0)
                GL.Uniform1(UnioformID, (float)(DateTime.Now.TimeOfDay - EngineRunningTime).TotalSeconds);

            UnioformID = EditShaderImage.GetUniform("iDate");
            if (UnioformID >= 0)
            {
                DateTime dateTime = DateTime.Now;
                GL.Uniform4(UnioformID, dateTime.Year, dateTime.Month, dateTime.Day, (float)dateTime.TimeOfDay.TotalSeconds);
            }
        }

        private static bool CheckFramebuffer(int FBO, string Name = "Edit")
        {
            FramebufferErrorCode FramebufferStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (FramebufferStatus != FramebufferErrorCode.FramebufferComplete)
            {
                MessageBox.Show("FBO error: " + FramebufferStatus.ToString(), Name, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public static int PrepateTexture(int SrcTextureID, TexturePrepareMode PrepareMode, bool GenMipMaps = true)
        {
            if (!GL.IsTexture(SrcTextureID))
                return -1;

            GL.BindTexture(TextureTarget.Texture2D, SrcTextureID);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out int Tex_Width);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int Tex_Height);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            int[] ViewportSizes = new int[4];
            GL.GetInteger(GetPName.Viewport, ViewportSizes);
            GL.Viewport(0, 0, Tex_Width, Tex_Height);

            int DstTextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DstTextureID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            
            if (Tex_Width * Tex_Height * 4 * (long)sizeof(float) > UInt32.MaxValue) // More then 4 Gb (16K images, 8-bit only)
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Tex_Width, Tex_Height, 0, PixelFormat.Rgba, PixelType.Byte, new byte[Tex_Width * 4, Tex_Height]);
            else
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Tex_Width, Tex_Height, 0, PixelFormat.Rgba, PixelType.Float, new float[Tex_Width * 4, Tex_Height]);

            int FBO = GL.GenFramebuffer(); //frame buffer renderer for editing
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, DstTextureID, 0);

            // Check Framebuffer
            FramebufferErrorCode FramebufferStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (FramebufferStatus != FramebufferErrorCode.FramebufferComplete)
            {
                MessageBox.Show("FBO error: " + FramebufferStatus.ToString(), "PrepateTexture", MessageBoxButton.OK, MessageBoxImage.Error);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
                GL.DeleteFramebuffer(FBO);

                if (GL.IsTexture(DstTextureID))
                    GL.DeleteTexture(DstTextureID);

                return -1;
            }

            // Draw
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.ClearColor(Color4.DarkGray);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(ConvertImageFomatShader.ProgramID);
            // Set Parameters
            GL.Uniform1(ConvertImageFomatShader.GetUniform("EditingMode"), (int)PrepareMode);

            // Bind Texture
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, SrcTextureID);

            ConvertImageFomatShader.EnableVertexAttribArrays();

            GL.BindBuffer(BufferTarget.ArrayBuffer, UVBufferID);
            GL.VertexAttribPointer(ConvertImageFomatShader.GetAttribute("v_UV"), 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            ConvertImageFomatShader.DisableVertexAttribArrays();

            // Free
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Bind default FrameBuffer
            GL.DeleteFramebuffer(FBO);
            GL.Viewport(ViewportSizes[0], ViewportSizes[1], ViewportSizes[2], ViewportSizes[3]);

            if (GenMipMaps)
            {
                GL.BindTexture(TextureTarget.Texture2D, DstTextureID);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            return DstTextureID;
        }

        public static void GetTextureFromPhotoshop()
        {
            var filterRecord = Program.filterRecord;
            if (filterRecord == null)
                return;

            var bigDoc = filterRecord.bigDocumentData;

            if (bigDoc.floatCoord32.v < 0)
            {
                bigDoc.inRect32 = new VRect()
                {
                    left = (short)-bigDoc.floatCoord32.h,
                    top = (short)-bigDoc.floatCoord32.v,
                    right = (short)(bigDoc.wholeSize32.h - bigDoc.floatCoord32.h),
                    bottom = (short)(bigDoc.wholeSize32.v - bigDoc.floatCoord32.v)
                };
            }
            else
                bigDoc.inRect32 = bigDoc.filterRect32;
            bigDoc.PluginUsing32BitCoordinates = 1;
            filterRecord.bigDocumentData = bigDoc;
            //max channel
            int channels = filterRecord.planes;
            filterRecord.inLoPlane = 0;
            filterRecord.inHiPlane = (short)(channels - 1);
            filterRecord.Write();
            filterRecord.advanceState();
            filterRecord.Read();

            bigDoc = filterRecord.bigDocumentData;

            int Width = bigDoc.inRect32.right - bigDoc.inRect32.left;
            int Height = bigDoc.inRect32.bottom - bigDoc.inRect32.top;
            int RowBytes = filterRecord.inRowBytes;             // Row bytes (with padding)
            int Channels = filterRecord.planes;                 // Channels count (from 1, can be bigger then 4)
            int ColorChannels = Math.Max(filterRecord.inLayerPlanes,
                 Math.Min((int)filterRecord.inNonLayerPlanes, 3));                 //Color channels without alpha on image or mask
            bool haveTransparency = (filterRecord.inTransparencyMask == 1) &&
                ColorChannels != Channels
                || filterRecord.inNonLayerPlanes >= 4; //Is there alpha channel? If masks selected - always false


            int Depth = filterRecord.depth;                     // Depth bits (default 8)
            int DepthBytes = Depth / 8;                                 // Bytes per channel (default 1)

            int ImageChannels = ColorChannels +
                (haveTransparency ? 1 : 0);                             // OpenGL texture channels count [1 .. 4]

            int SrcChannelsSizeBytes = Channels * DepthBytes;           // Source texture channels components size in bytes
            int DstChannelsSizeBytes = ImageChannels * DepthBytes;      // OpenGL texture channels components size in bytes
            int DstRowBytes = Width * DstChannelsSizeBytes;             // OpenGL texture row size in bytes

            IntPtr PixelsPointer = filterRecord.inData;

            // DUMP
            // ImageDump.DumpToFile(@"E:\Dumps\dump", PixelsBytes, Program.filterRecord.ptrData);

            byte[] BytesRGBA = new byte[DstRowBytes * Height];

            Parallel.For(0, Height, i =>
            {
                int IndexSource = i * RowBytes;
                int IndexDestination = i * DstRowBytes;
                Parallel.For(0, Width, j =>
                {
                    Marshal.Copy(PixelsPointer + IndexSource + j * SrcChannelsSizeBytes, BytesRGBA, IndexDestination + j * DstChannelsSizeBytes, DstChannelsSizeBytes);
                });
            });

            PixelFormat pixelFormat = TexFormatConverter.GetGLPixelFormat(ColorChannels, haveTransparency);
            PixelInternalFormat pixelInternalFormat = TexFormatConverter.GetGLPixelInternalFormat(Depth, ColorChannels, haveTransparency);
            PixelType pixelType = TexFormatConverter.GetGLPixelType(Depth);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            if (GL.IsTexture(TextureIDs[0]))
                GL.DeleteTexture(TextureIDs[0]);

            TextureIDs[0] = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[0]);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, pixelInternalFormat, Width, Height, 0, pixelFormat, pixelType, BytesRGBA);

            #region Convert all to RGBA32F
            if (GL.IsTexture(TextureIDs[1]))
                GL.DeleteTexture(TextureIDs[1]);

            TexturePrepareMode PrepModeMask = (Depth == 16 ? TexturePrepareMode.Depth16_PS_To_GL : TexturePrepareMode.Nothing);
            PrepModeMask |= TexturePrepareMode.FlipUV_Y;

            switch (pixelFormat)
            {
                case PixelFormat.Red:
                    PrepModeMask |= TexturePrepareMode.RXXX_TO_RRR1;
                    break;

                case PixelFormat.Rg:
                    PrepModeMask |= (haveTransparency ? TexturePrepareMode.RAXX_TO_RRRA : TexturePrepareMode.RGXX_TO_RG01);
                    break;

                case PixelFormat.Rgb:
                    PrepModeMask |= (haveTransparency ? TexturePrepareMode.RGAX_TO_RG0A : TexturePrepareMode.RGBX_TO_RGB1);
                    break;

                default:
                case PixelFormat.Rgba:
                    PrepModeMask |= TexturePrepareMode.RGBA_TO_RGBA;
                    break;
            }

            TextureIDs[1] = PrepateTexture(TextureIDs[0], PrepModeMask, UseMipMaps);

            if (GL.IsTexture(TextureIDs[0]))
                GL.DeleteTexture(TextureIDs[0]);

            TextureIDs[0] = PrepateTexture(TextureIDs[1], TexturePrepareMode.Nothing, UseMipMaps);
            #endregion

            GC.Collect();
        }

        internal static void ApplyToPhotoshop()
        {
            var filterRecord = Program.filterRecord;
            if (filterRecord == null)
                return;
            var bigDoc = filterRecord.bigDocumentData;

            int Width = bigDoc.inRect32.right - bigDoc.inRect32.left;
            int Height = bigDoc.inRect32.bottom - bigDoc.inRect32.top;
            int RowBytes = filterRecord.inRowBytes;             // Row bytes (with padding)
            int Channels = filterRecord.planes;                 // Channels count (from 1, can be bigger then 4)
            int ColorChannels = Math.Max(filterRecord.inLayerPlanes,
                 Math.Min((int)filterRecord.inNonLayerPlanes, 3));                 //Color channels without alpha on image or mask
            bool haveTransparency = (filterRecord.inTransparencyMask == 1) &&
                ColorChannels != Channels
                || filterRecord.inNonLayerPlanes >= 4; //Is there alpha channel? If masks selected - always false


            int Depth = filterRecord.depth;                             // Depth bits (default 8)
            int DepthBytes = Depth / 8;                                 // Bytes per channel (default 1)

            int ImageChannels = ColorChannels +
                (haveTransparency ? 1 : 0);                             // OpenGL texture channels count [1 .. 4]

            int DstChannelsSizeBytes = Channels * DepthBytes;           // Destination texture channels components size in bytes
            int SrcChannelsSizeBytes = 4 * DepthBytes;                  // OpenGL texture RGBA channels components size in bytes
            int SrcRowBytes = Width * SrcChannelsSizeBytes;             // OpenGL texture row size in bytes


            filterRecord.outRect = filterRecord.inRect;
            bigDoc.outRect32 = filterRecord.bigDocumentData.inRect32;
            bigDoc.PluginUsing32BitCoordinates = 1;
            filterRecord.bigDocumentData = bigDoc;
            filterRecord.outLoPlane = filterRecord.inLoPlane;
            filterRecord.outHiPlane = filterRecord.inHiPlane;

            filterRecord.Write();
            filterRecord.advanceState();

            filterRecord.Read();
            IntPtr outDataPtr = filterRecord.outData;

            PixelFormat pixelFormat = TexFormatConverter.GetGLPixelFormat(ColorChannels, haveTransparency);
            PixelType pixelType = TexFormatConverter.GetGLPixelType(Depth);

            #region Convert RGBA32F to PS format
            if (GL.IsTexture(TextureIDs[0]))
                GL.DeleteTexture(TextureIDs[0]);

            TexturePrepareMode PrepModeMask = (Depth == 16 ? TexturePrepareMode.Depth16_GL_To_PS : TexturePrepareMode.Nothing);
            PrepModeMask |= TexturePrepareMode.FlipUV_Y;

            switch (pixelFormat)
            {
                default:
                    PrepModeMask |= TexturePrepareMode.RGBA_TO_RGBA;
                    break;

                case PixelFormat.Rg:
                    PrepModeMask |= (haveTransparency ? TexturePrepareMode.RGBA_TO_RAGB : TexturePrepareMode.Nothing);
                    break;

                case PixelFormat.Rgb:
                    PrepModeMask |= (haveTransparency ? TexturePrepareMode.RGBA_TO_RGAB : TexturePrepareMode.Nothing);
                    break;
            }

            TextureIDs[0] = PrepateTexture(TextureIDs[1], PrepModeMask, false);
            #endregion

            byte[] BytesRGBA = new byte[SrcRowBytes * Height];

            GL.BindTexture(TextureTarget.Texture2D, TextureIDs[0]);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, pixelType, BytesRGBA);

            Parallel.For(0, Height, i =>
            {
                int IndexSource = i * SrcRowBytes; // OpenGL Data
                int IndexDestination = i * RowBytes; // Photoshop Data
                Parallel.For(0, Width, j =>
                {
                    Marshal.Copy(BytesRGBA, IndexSource + j * SrcChannelsSizeBytes, outDataPtr + IndexDestination + j * DstChannelsSizeBytes, DstChannelsSizeBytes);
                });
            });

            GC.Collect();
        }

        public static bool RunColorPicker(ColorRGBA InColor, out ColorRGBA ResultColor)
        {
            ResultColor = InColor;

            if (!IsInitialized)
                return false;
            
            ColorServicesInfo service = ColorServicesInfo.Instantiate();
            service.colorComponents[0] = InColor.RByte;
            service.colorComponents[1] = InColor.GByte;
            service.colorComponents[2] = InColor.BByte;
            service.colorComponents[3] = InColor.AByte;

            if (Program.filterRecord.colorServices(ref service) < 0)
                return false;

            ResultColor = new ColorRGBA((byte)service.colorComponents[0], (byte)service.colorComponents[1], (byte)service.colorComponents[2], (byte)service.colorComponents[3]);
            return true;
        }

        public static void GetPhotoshopBackgroundForegroundColors()
        {
            if (!IsInitialized)
                return;

            ColorServicesInfo SrvBG = ColorServicesInfo.Instantiate();
            ColorServicesInfo SrvFG = ColorServicesInfo.Instantiate();
            SrvBG.selector = SrvFG.selector = ColorServicesConsts.plugIncolorServicesGetSpecialColor;

            SrvBG.selectorParameter.specialColorID = ColorServicesConsts.plugIncolorServicesBackgroundColor;
            SrvFG.selectorParameter.specialColorID = ColorServicesConsts.plugIncolorServicesForegroundColor;

            if (Program.filterRecord.colorServices(ref SrvBG) < 0 || Program.filterRecord.colorServices(ref SrvFG) < 0)
                return;

            PhotoshopColorBG = new ColorRGBA((byte)SrvBG.colorComponents[0], (byte)SrvBG.colorComponents[1], (byte)SrvBG.colorComponents[2], (byte)SrvBG.colorComponents[3]);
            PhotoshopColorFG = new ColorRGBA((byte)SrvFG.colorComponents[0], (byte)SrvFG.colorComponents[1], (byte)SrvFG.colorComponents[2], (byte)SrvFG.colorComponents[3]);
        }

        #region Properties
        public static TextureMagFilter MagFilter
        {
            get { return TexMagFilter; }
            set
            {
                TexMagFilter = value;

                if (!IsInitialized)
                    return;

                GL.BindTexture(TextureTarget.Texture2D, TextureIDs[1]);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TexMagFilter);
            }
        }

        public static TextureMinFilter MinFilter
        {
            get { return TexMinFilter; }
            set
            {
                TexMinFilter = value;

                if (!IsInitialized)
                    return;

                GL.BindTexture(TextureTarget.Texture2D, TextureIDs[1]);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TexMinFilter);
            }
        }

        public static bool UseMipMaps
        {
            get { return Use_MipMaps; }
            set
            {
                Use_MipMaps = value;

                if (!IsInitialized)
                    return;

                for (int i = 0; i < TextureIDs.Length; i++)
                {
                    if (!GL.IsTexture(TextureIDs[i]))
                        continue;

                    GL.BindTexture(TextureTarget.Texture2D, TextureIDs[i]);

                    if (value)
                        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    else
                    {
                        GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out int Texture_Width);
                        GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int Texture_Height);
                        GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureInternalFormat, out int Texture_PixelInternalFormat);

                        int TextureID_New = GL.GenTexture();
                        int FBO = GL.GenFramebuffer();
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureIDs[i], 0);
                        GL.BindTexture(TextureTarget.Texture2D, TextureID_New);
                        GL.CopyTexImage2D(TextureTarget.Texture2D, 0, (InternalFormat)Texture_PixelInternalFormat, 0, 0, Texture_Width, Texture_Height, 0);

                        GL.DeleteTexture(TextureIDs[i]);
                        TextureIDs[i] = TextureID_New;
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                        GL.DeleteFramebuffer(FBO);
                    }
                }
            }
        }

        public static bool UseFramebufferSrgb
        {
            get
            {
                if (!IsInitialized)
                    return false;

                GL.GetBoolean(GetPName.FramebufferSrgb, out bool IsSrgb);
                return IsSrgb;
            }
            set
            {
                if (!IsInitialized)
                    return;

                if (value)
                    GL.Enable(EnableCap.FramebufferSrgb);
                else
                    GL.Disable(EnableCap.FramebufferSrgb);
            }
        }

        public static MultiPassBuffers MultiPassBuffers
        {
            get
            {
                return MultPassBuffers;
            }
            set
            {
                if (!IsInitialized)
                    return;

                if ((int)MultPassBuffers < (int)value)
                {
                    for (int i = 2 + (int)MultPassBuffers; i < 2 + (int)value; i++)
                    {
                        if (GL.IsTexture(TextureIDs[i]))
                            GL.DeleteTexture(TextureIDs[i]);

                        TextureIDs[i] = PrepateTexture(TextureIDs[0], TexturePrepareMode.Nothing, UseMipMaps);
                    }
                }
                else if ((int)MultPassBuffers > (int)value)
                {
                    for (int i = 2 + (int)value; i < TextureIDs.Length; i++)
                    {
                        if (GL.IsTexture(TextureIDs[i]))
                            GL.DeleteTexture(TextureIDs[i]);
                    }
                }
                MultPassBuffers = value;
            }
        }
        #endregion

        public static void GetTexturesSize(out int TextureWidth, out int TextureHeight)
        {
            TextureWidth = 0;
            TextureHeight = 0;

            if (!IsInitialized)
                return;

            for (int i = 0; i < TextureIDs.Length; i++)
            {
                if (!GL.IsTexture(TextureIDs[i]))
                    continue;

                GL.BindTexture(TextureTarget.Texture2D, TextureIDs[i]);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out TextureWidth);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out TextureHeight);
                return;
            }
        }

        public static void Free()
        {
            IsInitialized = false;

            GL.BindTexture(TextureTarget.Texture2D, 0);

            for (int i = 0; i < TextureIDs.Length; i++)
            {
                if (GL.IsTexture(TextureIDs[i]))
                    GL.DeleteTexture(TextureIDs[i]);

                TextureIDs[i] = 0;
            }

            GL.UseProgram(0);

            ViewShader?.Free();
            ViewShader = null;
            ViewLineShader?.Free();
            ViewLineShader = null;
            ViewGridShader?.Free();
            ViewGridShader = null;
            ConvertImageFomatShader?.Free();
            ConvertImageFomatShader = null;

            EditShaderImage?.Free();
            EditShaderImage = null;
            EditShaderBufferA?.Free();
            EditShaderBufferA = null;
            EditShaderBufferB?.Free();
            EditShaderBufferB = null;
            EditShaderBufferC?.Free();
            EditShaderBufferC = null;
            EditShaderBufferD?.Free();
            EditShaderBufferD = null;
            ShaderError = null;

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(VertexBufferID);
            GL.DeleteBuffer(UVBufferID);
            VertexBufferID = UVBufferID = 0;

            GC.Collect();
        }
    }

    public enum DrawModes : int
    {
        RGBA = 0,
        RGB,
        Red,
        Green,
        Blue,
        Alpha,
    }

    [Flags]
    public enum TexturePrepareMode : int
    {
        Nothing = 0,
        RXXX_TO_RRR1 = 1,
        RAXX_TO_RRRA = 2,
        RGBA_TO_RAGB = 4,
        RGXX_TO_RG01 = 8,
        RGAX_TO_RG0A = 16,
        RGBA_TO_RGAB = 32,
        RGBX_TO_RGB1 = 64,
        RGBA_TO_RGBA = 128,
        Depth16_PS_To_GL = 256,
        Depth16_GL_To_PS = 512,
        FlipUV_Y = 1024
    }

    public enum MultiPassBuffers
    {
        NoBuffers   = 0,
        BufferA     = 1,
        BufferAB    = 2,
        BufferABC   = 3,
        BufferABCD  = 4
    }

    public enum MultiPassBuffersDrawMode
    {
        NoBuffers = 0,
        BufferA = 1,
        BufferB = 2,
        BufferC = 3,
        BufferD = 4
    }
}