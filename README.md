# Shader Plugin for Photoshop
Shader Plugin allows to process images in Photoshop with GLSL shaders. It's will be usefull for technical artists for creation shader effects, masks, noise textures in Photoshop. Also you can process images with multipass filters like FXAA (by NVIDIA) inside Photoshop!!!

<p align="center">
<img src="https://github.com/user-attachments/assets/5213f779-1536-435f-848b-ddd50eeb012e" alt="Screenshot" width="60%">
</p>

**☕Support us on:** https://hellmapper.itch.io/photoshop-shader-plugin

## Features:
* Support 8/16/32 bit images (color and grayscale).
* Uniforms with usefull image information from Photoshop.
* 4 additional buffers for making complex effects.
* Syntax Highlighting
* 8K Image Support
* Errors highlighting. Full errors list can be shown in separent window (just click on statusbar).

## System Requiremets:
Windows 7+ (x64 only)\
Photoshop CS5+ (tested on CS6 .. 2022)\
.Net Framework 4.6.2+

## License
MIT

## Where to find full Plugin Sources (why C# and CLR part only)?
ADOBE® PHOTOSHOP® SDK License Agreement restrict to distribute SDK parts (include modified) as code.\
This makes unavailable to share c++ part.\
The biggest problem: they use custom build tool and process for generating resources.\
Anybody can download SDK from Adobe site, but we can't share sources which use this SDK: any Photoshop C++ Plugin require resource files from SDK Samples when building.\
Any way you can use it without problems.

## Authors:
Alex Zelensky\
Vladimir Rymkevich

## How to Compile
1. Open solution `ShaderPlugin\ShaderPlugin.sln` and Build solution.
2. Create Plugins folder (like `C:\Program Files\Adobe\Adobe Photoshop CC 2018\Plug-ins\ShaderPlugin`).
3. Copy `Release` folder content (for example `ShaderPlugin\bin\x64\Release`) to Plugins folder.
4. Copy precompiled plugin `ShaderPlugin.8bf` from `Precompiled\Release` to Plugins folder.
5. Copy GLSL shaders from `Shaders` folder to User Documents `Documents\PhotoshopShaderPlugin\Shaders`.
