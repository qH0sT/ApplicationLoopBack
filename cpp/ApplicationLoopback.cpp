// ApplicationLoopback.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <Windows.h>
#include <iostream>
#include "LoopbackCapture.h"

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        std::cout << "Application Loopback Library attached." << std::endl;
        break;
    case DLL_PROCESS_DETACH:
        std::cout << "Application Loopback Library detached." << std::endl;
        break;
    }
    return TRUE;
}