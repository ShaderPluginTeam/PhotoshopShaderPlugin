#pragma once
#define WIN32_LEAN_AND_MEAN             // Исключите редко используемые компоненты из заголовков Windows
#include "windows.h"

__declspec(dllexport) void Init();
__declspec(dllexport) short RunProxy(HWND PhotoshopWindowHandle, void *FilterRectordPtr, void *LastParamsPtr);