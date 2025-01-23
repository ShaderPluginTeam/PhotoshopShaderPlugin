#pragma once
#define WIN32_LEAN_AND_MEAN             // Disable rarely used components from Windows headers https://devblogs.microsoft.com/oldnewthing/20091130-00/?p=15863
#include "windows.h"

__declspec(dllexport) void Init();
__declspec(dllexport) short RunProxy(HWND PhotoshopWindowHandle, void *FilterRectordPtr, void *LastParamsPtr);