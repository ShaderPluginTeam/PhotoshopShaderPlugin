using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using PixelFormat_NET = System.Drawing.Imaging.PixelFormat;

namespace ShaderPluginGUI
{
    public static class TexFormatConverter
    {
        public static PixelFormat GetGLPixelFormat(int colorChannels, bool haveTransparency)
        {
             
            switch (colorChannels)
            {
                case 1:
                    return haveTransparency ? PixelFormat.Rg : PixelFormat.Red;
                case 2:
                    return haveTransparency ? PixelFormat.Rgb : PixelFormat.Rg;
                case 3:
                    return haveTransparency ? PixelFormat.Rgba : PixelFormat.Rgb;
                default:
                    return PixelFormat.Rgba;
            }
        }

        public static PixelInternalFormat GetGLPixelInternalFormat(int Depth, int colorChannels, bool Transparency)
        {
            switch (Depth)
            {
                case 8:
                default:
                    switch (colorChannels)
                    {
                        case 1:
                            return Transparency ? PixelInternalFormat.Rg8 : PixelInternalFormat.R8;
                        case 2:
                            return Transparency ? PixelInternalFormat.Rgb8 : PixelInternalFormat.Rg8;
                        case 3:
                            return Transparency ? PixelInternalFormat.Rgba8 : PixelInternalFormat.Rgb8;
                        default:
                            return PixelInternalFormat.Rgba8;
                    }
                case 16:
                    switch (colorChannels)
                    {
                        case 1:
                            return Transparency ? PixelInternalFormat.Rg16f : PixelInternalFormat.R16f;
                        case 2:
                            return Transparency ? PixelInternalFormat.Rgb16f : PixelInternalFormat.Rg16f;
                        case 3:
                            return Transparency ? PixelInternalFormat.Rgba16f : PixelInternalFormat.Rgb16f;
                        default:
                            return PixelInternalFormat.Rgba16f;
                    }
                case 32:
                    switch (colorChannels)
                    {
                        case 1:
                            return Transparency ? PixelInternalFormat.Rg32f : PixelInternalFormat.R32f;
                        case 2:
                            return Transparency ? PixelInternalFormat.Rgb32f : PixelInternalFormat.Rg32f;
                        case 3:
                            return Transparency ? PixelInternalFormat.Rgba32f : PixelInternalFormat.Rgb32f;
                        default:
                            return PixelInternalFormat.Rgba16f;
                    }
            }
        }

        public static PixelType GetGLPixelType(int Depth)
        {
            switch (Depth)
            {
                case 8:
                default:
                    return PixelType.UnsignedByte;

                case 16:
                    return PixelType.UnsignedShort;

                case 32:
                    return PixelType.Float;
            }
        }

        public static PixelFormat GetGLPixelFormat(PixelFormat_NET PF)
        {
            switch (PF)
            {
                case PixelFormat_NET.Indexed:
                case PixelFormat_NET.Format1bppIndexed:
                case PixelFormat_NET.Format4bppIndexed:
                case PixelFormat_NET.Format8bppIndexed:
                    return PixelFormat.ColorIndex;

                case PixelFormat_NET.Alpha:
                case PixelFormat_NET.PAlpha:
                    return PixelFormat.Alpha;

                case PixelFormat_NET.Format16bppGrayScale:
                    return PixelFormat.Alpha16IccSgix;
                case PixelFormat_NET.Format16bppRgb555:
                case PixelFormat_NET.Format16bppRgb565:
                    return PixelFormat.Bgr;
                case PixelFormat_NET.Format16bppArgb1555:
                    return PixelFormat.Bgra;

                case PixelFormat_NET.Format24bppRgb:
                case PixelFormat_NET.Format48bppRgb:
                    return PixelFormat.Bgr;

                case PixelFormat_NET.Format32bppRgb:
                case PixelFormat_NET.Format32bppArgb:
                case PixelFormat_NET.Format32bppPArgb:
                case PixelFormat_NET.Format64bppArgb:
                case PixelFormat_NET.Format64bppPArgb:
                    return PixelFormat.Bgra;

                default:
                    return PixelFormat.Bgra;
            }
        }

        public static PixelInternalFormat GetGLPixelInternalFormat(PixelFormat_NET PF)
        {
            switch (PF)
            {
                case PixelFormat_NET.Indexed:
                case PixelFormat_NET.Format1bppIndexed:
                case PixelFormat_NET.Format4bppIndexed:
                case PixelFormat_NET.Format8bppIndexed:
                    return PixelInternalFormat.Rgba;

                case PixelFormat_NET.Alpha:
                case PixelFormat_NET.PAlpha:
                    return PixelInternalFormat.Alpha8;

                case PixelFormat_NET.Format16bppGrayScale:
                    return PixelInternalFormat.Alpha16;

                case PixelFormat_NET.Format16bppRgb555:
                    return PixelInternalFormat.Rgb5;

                case PixelFormat_NET.Format16bppRgb565:
                    return PixelInternalFormat.R5G6B5IccSgix;

                case PixelFormat_NET.Format16bppArgb1555:
                    return PixelInternalFormat.Rgb5A1;

                case PixelFormat_NET.Format24bppRgb:
                    return PixelInternalFormat.Rgb8;

                case PixelFormat_NET.Format32bppRgb:
                case PixelFormat_NET.Format32bppArgb:
                case PixelFormat_NET.Format32bppPArgb:
                    return PixelInternalFormat.Rgba8;

                case PixelFormat_NET.Format48bppRgb:
                    return PixelInternalFormat.Rgb16;

                case PixelFormat_NET.Format64bppArgb:
                case PixelFormat_NET.Format64bppPArgb:
                    return PixelInternalFormat.Rgba16;

                default:
                    return PixelInternalFormat.Rgba;
            }
        }

        public static PixelFormat_NET GetPixelFormat(PixelInternalFormat PIF)
        {
            switch (PIF)
            {
                case PixelInternalFormat.Alpha:
                case PixelInternalFormat.Alpha8:
                case PixelInternalFormat.R8:
                case PixelInternalFormat.R8ui:
                    return PixelFormat_NET.Alpha;

                case PixelInternalFormat.Rgb:
                case PixelInternalFormat.Rgb8:
                case PixelInternalFormat.Rgb8ui:
                    return PixelFormat_NET.Format24bppRgb;

                case PixelInternalFormat.Rgba:
                case PixelInternalFormat.Rgba8:
                case PixelInternalFormat.Rgba8ui:
                    return PixelFormat_NET.Format32bppArgb;

                case PixelInternalFormat.Alpha16:
                case PixelInternalFormat.R16:
                case PixelInternalFormat.R16ui:
                    return PixelFormat_NET.Format16bppGrayScale;

                case PixelInternalFormat.Rgb5:
                    return PixelFormat_NET.Format16bppRgb555;

                case PixelInternalFormat.Rgb16:
                    return PixelFormat_NET.Format48bppRgb;

                case PixelInternalFormat.Rgb5A1:
                    return PixelFormat_NET.Format16bppArgb1555;

                case PixelInternalFormat.Rgba16:
                    return PixelFormat_NET.Format64bppArgb;

                case PixelInternalFormat.R5G6B5IccSgix:
                    return PixelFormat_NET.Format16bppRgb565;

                default:
                    return PixelFormat_NET.Format32bppArgb;
            }
        }
    }
}
