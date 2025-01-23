#pragma once
#include "CLIProxy.h"

using namespace System;
using namespace System::IO;
using namespace System::Reflection;

Assembly^ LoadFromSameFolder(Object^ sender, ResolveEventArgs^ args)
{
	String^ StartupPath = Path::GetDirectoryName(Assembly::GetExecutingAssembly()->Location);
	array<String^>^ AssemblyNameParts = args->Name->Split(',');

	if (AssemblyNameParts->Length <= 0)
		return nullptr;

	String^ Name = AssemblyNameParts[0];

	if (Name->Contains(".resources"))
		return nullptr;

	String^ AssemblyPath = Path::Combine(StartupPath, Name + ".dll");
	return (File::Exists(AssemblyPath) ? Assembly::LoadFrom(AssemblyPath) : nullptr);
}

__declspec(dllexport) void Init()
{
	AppDomain^ CurrentDomain = AppDomain::CurrentDomain;
	CurrentDomain->AssemblyResolve += gcnew ResolveEventHandler(LoadFromSameFolder);
}

__declspec(dllexport) short RunProxy(HWND PhotoshopWindowHandle, void *FilterRectordPtr, void *LastParamsPtr)
{
	IntPtr PhotoshopWindowPointer = IntPtr(PhotoshopWindowHandle);
	IntPtr FilterRectordPointer(FilterRectordPtr);
	IntPtr LastParamsPointer = IntPtr(LastParamsPtr);
	return ShaderPluginGUI::Program::Main(PhotoshopWindowPointer, FilterRectordPointer, LastParamsPointer);
}