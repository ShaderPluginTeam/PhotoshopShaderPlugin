using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using PS_Structures;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using PixelFormat_NET = System.Drawing.Imaging.PixelFormat;

namespace ShaderPluginGUI
{
    public partial class MainWindow
    {
        int Zoom_Index = 0;
        List<float> Zoom_List = new List<float>();
        Point Mouse_LocationOld = new Point();
        PointF Zoom_CenterPoint = new PointF();
        float PreviewPosition = 0.5f;

        private void GlControl_Load(object sender, EventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            Engine.Init();
            Engine.GetTextureFromPhotoshop();
            Engine.GetPhotoshopBackgroundForegroundColors();

            #region Load Last Shader File
            textEditorImage_VS.Text = textEditorBufferA_VS.Text = textEditorBufferB_VS.Text = textEditorBufferC_VS.Text = textEditorBufferD_VS.Text = Properties.Resources.Edit_VS;
            textEditorImage_FS.Text = Properties.Resources.EditImage_FS;
            textEditorBufferA_FS.Text = Properties.Resources.EditBufferA_FS;
            textEditorBufferB_FS.Text = Properties.Resources.EditBufferB_FS;
            textEditorBufferC_FS.Text = Properties.Resources.EditBufferC_FS;
            textEditorBufferD_FS.Text = Properties.Resources.EditBufferD_FS;

            SetDefaultTextureParameters();

            ShaderXML shaderXML = ShaderXML.Load(Path.Combine(Program.StartupPath, ShaderXML.LastShaderFile));
            if (shaderXML != null)
                ApplyLoadedShaderXML(shaderXML);
            #endregion

            if (PhotoshopRunLastFilterEnabled) // Need "Last Filter"
            {
                if (CompileShader())
                {
                    Engine.ApplyToPhotoshop();
                    Program.Result = PSPluginErrorCodes.NoError;
                }
                else
                    Program.Result = PSPluginErrorCodes.ParamError;

                ApplyLastParams_NeedCloseWindow = true;
                return;
            }

            checkBox_sRGB.IsChecked = Program.filterRecord?.depth == 32;

            GlControl_Resize(sender, e);
            ZoomUpdate(false);

            CompileShader(); // Compile default shader
        }

        private void GlControl_HandleDestroyed(object sender, EventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            SaveLastShaders();
            Engine.Free(); // Free all: Shaders, Textures...
        }

        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    UpdateColorPickerColor(e.Location);
                    popup_ColorPicker.IsOpen = true;
                    glControl.Cursor = Cursors.Cross;
                    break;

                case MouseButtons.Middle:
                    glControl.Cursor = Cursors.Hand;
                    PreviewPosition = e.Location.X / (float)glControl.ClientSize.Width;
                    glControl.Invalidate();
                    break;

                case MouseButtons.Right:
                    Mouse_LocationOld = e.Location;
                    glControl.Cursor = Cursors.SizeAll;
                    break;

                case MouseButtons.XButton1:
                    ZoomUpdate(false);
                    break;

                case MouseButtons.XButton2:
                    for (int i = 0; i < ZoomList.Count; i++)
                    {
                        if (ZoomList[i] == 1.0f)
                        {
                            ZoomListIndex = i;
                            ZoomUpdate(true);
                        }
                    }
                    break;
            }
        }

        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            glControl.Cursor = Cursors.Default;
            popup_ColorPicker.IsOpen = false;
        }

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    UpdateColorPickerColor(e.Location);
                    popup_ColorPicker.HorizontalOffset = e.X + 14;
                    popup_ColorPicker.VerticalOffset = e.Y + 24;
                    break;

                case MouseButtons.Middle:
                    glControl.Cursor = Cursors.Hand;
                    PreviewPosition = e.Location.X / (float)glControl.ClientSize.Width;
                    glControl.Invalidate();
                    break;

                case MouseButtons.Right:
                    glControl.Cursor = Cursors.SizeAll;
                    MoveImage(e.Location);
                    break;
            }
        }

        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0 && Zoom_Index < Zoom_List.Count - 1)
                Zoom_Index++;
            else if (e.Delta < 0 && Zoom_Index > 0)
                Zoom_Index--;

            MoveImage(Mouse_LocationOld);
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            if (glControl.ClientSize.Width <= 0 || glControl.ClientSize.Height <= 0)
                return;

            GL.Viewport(0, 0, glControl.ClientSize.Width, glControl.ClientSize.Height);
            Engine.MVPMatrix = Matrix4.CreateOrthographic(glControl.ClientSize.Width, glControl.ClientSize.Height, -1f, 1f); // MV - Identity
            ZoomUpdate(true);

            InvalidateVisual();
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            Engine.GetTexturesSize(out int TextureWidth, out int TextureHeight);

            float Zoom = GetZoom;
            float X1 = -Zoom_CenterPoint.X * Zoom;
            float X2 = X1 + TextureWidth * Zoom;
            float Y2 = Zoom_CenterPoint.Y * Zoom;
            float Y1 = Y2 - TextureHeight * Zoom;

            Engine.Draw(X1, X2, Y1, Y2, PreviewPosition);

            glControl.SwapBuffers();
        }

        public Image Image
        {
            get
            {
                if (glControl.IsHandleCreated)
                    glControl.MakeCurrent();

                if (!GL.IsTexture(Engine.TextureIDs[1]))
                    return null;

                int TextureID = Engine.PrepateTexture(Engine.TextureIDs[1], TexturePrepareMode.FlipUV_Y, false);
                if (!GL.IsTexture(TextureID))
                    return null;

                GL.BindTexture(TextureTarget.Texture2D, TextureID);
                GL.PixelStore(PixelStoreParameter.PackAlignment, 4);

                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out int Texture_Width);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int Texture_Height);
                GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureInternalFormat, out int Texture_PixelInternalFormat);
                PixelFormat_NET PF = TexFormatConverter.GetPixelFormat((PixelInternalFormat)Texture_PixelInternalFormat);
                PixelFormat GL_PF = TexFormatConverter.GetGLPixelFormat(PF);

                Bitmap BMP = new Bitmap(Texture_Width, Texture_Height, PF);
                BitmapData BmpData = BMP.LockBits(new Rectangle(0, 0, Texture_Width, Texture_Height), ImageLockMode.WriteOnly, BMP.PixelFormat);
                GL.GetTexImage(TextureTarget.Texture2D, 0, GL_PF, PixelType.UnsignedByte, BmpData.Scan0);
                BMP.UnlockBits(BmpData);
                BmpData = null;
                GL.DeleteTexture(TextureID);
                return BMP;
            }
            set
            {
                if (glControl.IsHandleCreated)
                    glControl.MakeCurrent();

                Engine.GetTexturesSize(out int OrigTexWidth, out int OrigTexHeight);

                if (value != null)
                {
                    if (value.PixelFormat != PixelFormat_NET.Format32bppArgb || value.Width != OrigTexWidth || value.Height != OrigTexHeight)
                    {
                        Bitmap Image_Result = new Bitmap(OrigTexWidth, OrigTexHeight);
                        using (Graphics G = Graphics.FromImage(Image_Result))
                            G.DrawImage(value, 0, 0, OrigTexWidth, OrigTexHeight);
                        value = Image_Result;
                    }

                    Bitmap BMP = (Bitmap)value.Clone();
                    Zoom_CenterPoint = new PointF(BMP.Width * 0.5f, BMP.Height * 0.5f);

                    PixelFormat GL_PF = TexFormatConverter.GetGLPixelFormat(BMP.PixelFormat);
                    PixelInternalFormat GL_PIF = TexFormatConverter.GetGLPixelInternalFormat(BMP.PixelFormat);

                    BitmapData BmpData = BMP.LockBits(new Rectangle(Point.Empty, BMP.Size), ImageLockMode.ReadOnly, BMP.PixelFormat);

                    GL.BindTexture(TextureTarget.Texture2D, 0);

                    for (int i = 0; i < Engine.TextureIDs.Length; i++)
                        if (GL.IsTexture(Engine.TextureIDs[i]))
                            GL.DeleteTexture(Engine.TextureIDs[i]);

                    Engine.TextureIDs[0] = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, Engine.TextureIDs[0]);
                    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, GL_PIF, BMP.Width, BMP.Height, 0, GL_PF, PixelType.UnsignedByte, BmpData.Scan0);
                    BMP.UnlockBits(BmpData);
                    BMP.Dispose();

                    Engine.TextureIDs[1] = Engine.PrepateTexture(Engine.TextureIDs[0], TexturePrepareMode.FlipUV_Y, Engine.UseMipMaps);
                    if (GL.IsTexture(Engine.TextureIDs[0]))
                        GL.DeleteTexture(Engine.TextureIDs[0]);
                    Engine.TextureIDs[0] = Engine.PrepateTexture(Engine.TextureIDs[1], TexturePrepareMode.Nothing, Engine.UseMipMaps);

                    int MultiPassBuffersCount = (int)Engine.MultiPassBuffers;
                    if (MultiPassBuffersCount > 0)
                    {
                        Engine.TextureIDs[2] = Engine.PrepateTexture(Engine.TextureIDs[1], TexturePrepareMode.Nothing, Engine.UseMipMaps);
                        if (MultiPassBuffersCount > 1)
                        {
                            Engine.TextureIDs[3] = Engine.PrepateTexture(Engine.TextureIDs[1], TexturePrepareMode.Nothing, Engine.UseMipMaps);
                            if (MultiPassBuffersCount > 2)
                            {
                                Engine.TextureIDs[4] = Engine.PrepateTexture(Engine.TextureIDs[1], TexturePrepareMode.Nothing, Engine.UseMipMaps);
                                if (MultiPassBuffersCount > 3)
                                    Engine.TextureIDs[5] = Engine.PrepateTexture(Engine.TextureIDs[1], TexturePrepareMode.Nothing, Engine.UseMipMaps);
                            }
                        }
                    }
                }

                ZoomUpdate(false);
            }
        }

        /// <summary>
        /// Need apply plugin with last params or just open plugin window.
        /// </summary>
        bool PhotoshopRunLastFilterEnabled
        {
            get
            {
                if (Program.LastParamsPtr != IntPtr.Zero)
                {
                    byte[] Bytes = new byte[1]; // Must be same as at C++ part
                    Marshal.Copy(Program.LastParamsPtr, Bytes, 0, Bytes.Length);
                    return (Bytes[0] != 0); // First byte - "Last Filter"
                }
                return false;
            }
            set
            {
                if (Program.LastParamsPtr != IntPtr.Zero)
                    Marshal.Copy(new byte[] { (byte)(value ? 1 : 0) }, 0, Program.LastParamsPtr, 1);
            }
        }

        bool SaveLastShaders()
        {
            return ShaderXML.Save(MakeShaderXMLforSaving(), Path.Combine(Program.StartupPath, ShaderXML.LastShaderFile));
        }

        private void MoveImage(Point NewMouseCoords)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            #region Zoom_CenterPoint
            Zoom_CenterPoint = new PointF(
                Zoom_CenterPoint.X - (NewMouseCoords.X - Mouse_LocationOld.X) / GetZoom,
                Zoom_CenterPoint.Y - (NewMouseCoords.Y - Mouse_LocationOld.Y) / GetZoom);
            Mouse_LocationOld = NewMouseCoords;

            float WidthZ = glControl.ClientSize.Width / GetZoom;
            float HeightZ = glControl.ClientSize.Height / GetZoom;
            float Half_WidthZ = WidthZ * 0.5f;
            float Half_HeightZ = HeightZ * 0.5f;

            Engine.GetTexturesSize(out int TextureWidth, out int TextureHeight);

            if (TextureWidth > WidthZ)
            {
                if (Zoom_CenterPoint.X - Half_WidthZ < 0f)
                    Zoom_CenterPoint.X = Half_WidthZ;

                if (Zoom_CenterPoint.X + Half_WidthZ > TextureWidth)
                    Zoom_CenterPoint.X = TextureWidth - Half_WidthZ;
            }
            else
                Zoom_CenterPoint.X = TextureWidth * 0.5f;

            if (TextureHeight > HeightZ)
            {
                if (Zoom_CenterPoint.Y - Half_HeightZ < 0f)
                    Zoom_CenterPoint.Y = Half_HeightZ;

                if (Zoom_CenterPoint.Y + Half_HeightZ > TextureHeight)
                    Zoom_CenterPoint.Y = TextureHeight - Half_HeightZ;
            }
            else
                Zoom_CenterPoint.Y = TextureHeight * 0.5f;
            #endregion

            glControl.Invalidate();
            InvalidateVisual();
        }

        private void UpdateColorPickerColor(Point MouseLocation)
        {
            ColorRGBA PickedColor = GetColorAtPoint(MouseLocation);

            textBlock_ColorPicker1.Text = String.Format("R: {1}{0}G: {2}{0}B: {3}{0}A: {4}",
                Environment.NewLine, PickedColor.R, PickedColor.G, PickedColor.B, PickedColor.A);

            textBlock_ColorPicker2.Text = String.Format("R: {1}{0}G: {2}{0}B: {3}{0}A: {4}",
                Environment.NewLine, PickedColor.RByte, PickedColor.GByte, PickedColor.BByte, PickedColor.AByte);
        }

        public ColorRGBA GetColorAtPoint(Point MouseLocation)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            Engine.GetTexturesSize(out int TextureWidth, out int TextureHeight);

            float Zoom = GetZoom;
            float X1 = -Zoom_CenterPoint.X * Zoom;
            float X2 = X1 + TextureWidth * Zoom;
            float Y2 = Zoom_CenterPoint.Y * Zoom;
            float Y1 = Y2 - TextureHeight * Zoom;

            float PosX = MouseLocation.X - glControl.ClientRectangle.Width * 0.5f;
            float PosY = glControl.ClientRectangle.Height * 0.5f - MouseLocation.Y;

            if (PosX >= X1 && PosX <= X2 && PosY >= Y1 && PosY <= Y2)
            {
                float x = MathHelper.Clamp((PosX - X1) / (X2 - X1), 0, 1);
                float y = MathHelper.Clamp((PosY - Y1) / (Y2 - Y1), 0, 1);
                int X = (int)(x * (TextureWidth - 1));
                int Y = (int)(y * (TextureHeight - 1));

                int TextureIndex = (PreviewPosition >= x ? 1 : 0) + (int)Engine.MultiPassBuffers; // Image + Buffers
                float[] PixelColor = new float[4]; // RGBA
                GL.GetTextureSubImage(Engine.TextureIDs[TextureIndex], 0, X, Y, 0, 1, 1, 1, PixelFormat.Rgba, PixelType.Float, sizeof(float) * PixelColor.Length, PixelColor);
                return new ColorRGBA(PixelColor[0], PixelColor[1], PixelColor[2], PixelColor[3]);
            }
            else
                return new ColorRGBA(0f, 0f, 0f, 0f);
        }

        #region Zoom
        public PointF ZoomCenterPoint
        {
            get
            {
                Engine.GetTexturesSize(out int TextureWidth, out int TextureHeight);
                return new PointF(Zoom_CenterPoint.X / TextureWidth, Zoom_CenterPoint.Y / TextureHeight);
            }
            set
            {
                Engine.GetTexturesSize(out int TextureWidth, out int TextureHeight);
                Zoom_CenterPoint.X = value.X * TextureWidth;
                Zoom_CenterPoint.Y = value.Y * TextureHeight;
                MoveImage(Mouse_LocationOld);
            }
        }

        public List<float> ZoomList
        {
            get { return Zoom_List; }
            set
            {
                Zoom_List = value;

                if (Zoom_Index > Zoom_List.Count - 1)
                    Zoom_Index = Zoom_List.Count - 1;
            }
        }

        public int ZoomListIndex
        {
            get { return Zoom_Index; }
            set
            {
                if (value < 0)
                    Zoom_Index = 0;
                else if (value > Zoom_List.Count - 1)
                    Zoom_Index = Zoom_List.Count - 1;
                else
                    Zoom_Index = value;
            }
        }

        public float GetZoom
        {
            get
            {
                if (Zoom_Index < 0 || Zoom_Index >= Zoom_List.Count)
                    return 1f;
                return Zoom_List[Zoom_Index];
            }
        }

        public float StretchZoom { get; private set; } = 0f;

        public void ZoomUpdate(bool RestoreLastZoom = false)
        {
            if (glControl.IsHandleCreated)
                glControl.MakeCurrent();

            // Save Old Zoom!
            float LastZoom = GetZoom;
            bool IsLastZoomIsStretch = (Zoom_Index == Zoom_List.IndexOf(StretchZoom));

            Zoom_List.Clear();
            Zoom_Index = 0;

            // StretchZoom
            Engine.GetTexturesSize(out int TextureWidth, out int TextureHeight);
            float DeltaW = glControl.ClientSize.Width / (float)TextureWidth;
            float DeltaH = glControl.ClientSize.Height / (float)TextureHeight;
            StretchZoom = (DeltaW > DeltaH ? DeltaH : DeltaW);
            Zoom_List.Add(StretchZoom);

            float MaxZoom = 128f;
            float MinZoom = (float)Math.Pow(2.0, Math.Ceiling(Math.Log(64.0 / Math.Max(TextureWidth, TextureHeight), 2.0)));

            // 1, 2, 4, 8 ... MaxZoom
            for (float i = 1f; i <= MaxZoom; i *= 2f)
                Zoom_List.Add(i);

            // 0.5, 0.25, 0.125 ... MinZoom
            for (float i = 0.5f; i >= MinZoom; i *= 0.5f)
                Zoom_List.Add(i);

            Zoom_List = Zoom_List.OrderBy(z => z).Distinct().ToList();

            if (RestoreLastZoom && !IsLastZoomIsStretch) // Restore Last Zoom
            {
                int MinDeltaIndex = 0;
                float MinDelta = float.MaxValue;
                for (int i = 0; i < Zoom_List.Count; i++)
                {
                    float Delta = Math.Abs(Zoom_List[i] - LastZoom);
                    if (Delta < MinDelta)
                    {
                        MinDeltaIndex = i;
                        MinDelta = Delta;
                    }
                }
                Zoom_Index = MinDeltaIndex;
            }
            else
                Zoom_Index = Zoom_List.IndexOf(StretchZoom);

            MoveImage(Mouse_LocationOld);
        }
        #endregion
    }
}