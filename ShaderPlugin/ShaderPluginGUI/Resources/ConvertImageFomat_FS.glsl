#version 330
uniform sampler2D TextureUnit0;

in vec2 f_UV;

layout(location = 0) out vec4 FragColor;

uniform int EditingMode = 0;
/*
        Nothing = 0,      // 0
        RXXX_TO_RRR1,     // 1
        RAXX_TO_RRRA,     // 2
		RGBA_TO_RAGB,     // 4
        RGXX_TO_RG01,     // 8
        RGAX_TO_RG0A,     // 16
		RGBA_TO_RGAB,     // 32
        RGBX_TO_RGB1,     // 64
		RGBA_TO_RGBA,	  // 128
        Depth16_PS_To_GL, // 256
        Depth16_GL_To_PS  // 512
		FlipUV_Y		  // 1024
*/

void main()
{
	vec4 Tex = texture(TextureUnit0, f_UV);

	if (EditingMode == 0)
	{
		FragColor = Tex;
		return;
	}

	
	if ((EditingMode & 256) == 256) // Photoshop 16 bit image to OpenGL 16 bit Image (max values are 32768 in Photoshop and 65536 at OpenGL)
		Tex *= 65536.0 / 32768.0;
	else if ((EditingMode & 512) == 512) // OpenGL 16 bit image to Photoshop 16 bit Image
		Tex *= 32768.0 / 65536.0;

	switch (EditingMode & 255)
	{
		case 1: // RXXX_TO_RRR1
			Tex = vec4(Tex.rrr, 1.0);
			break;

		case 2: // RAXX_TO_RRRA
			Tex = Tex.rrrg;
			break;

		case 4: // RGBA_TO_RAGB
			Tex = Tex.ragb;
			break;

		case 8: // RGXX_TO_RG01
			Tex = vec4(Tex.rg, 0.0, 1.0);
			break;

		case 16: // RGAX_TO_RG0A
			Tex = vec4(Tex.rg, 0.0, Tex.b);
			break;

		case 32: // RGBA_TO_RGAB
			Tex = Tex.rgab;
			break;

		case 64: // RGBX_TO_RGB1
			Tex = vec4(Tex.rgb, 1.0);
			break;

		default:
			break;
	}

	FragColor = Tex;
}