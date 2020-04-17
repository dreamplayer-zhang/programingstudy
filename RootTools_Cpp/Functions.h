#pragma once

#include <cstdio>
//#include <mutex>  // vs2013 에서 컴파일 안되서 주석
#include <memory>

using namespace std;

static class Functions 
{
public:
    template< typename T >
    inline static void SafeDelete(T& p) { delete p; (p) = NULL; }

    template< typename T >
    inline static void SafeDeleteArray(T& p) { delete[] p; (p) = NULL; }

    template< typename T >
    inline static void SafeDeleteArrayObject(T& p, int n)
    {
        if (p) { for (int i = 0; i < n; i++) { SAFE_DELETE((p)[i]); } delete[](p); p = NULL; }
    }

    template< typename T >
    inline static void SafeDeleteArrayArray(T& p, int n)
    {
        if (p) { for (int i = 0; i < n; i++) { SAFE_DELETE_ARRAY((p)[i]); } delete[](p); p = NULL; }
    }

    inline static int GetWidth(RECT rt)
    {
        return abs(rt.right - rt.left);
    }

    inline static int GetHeight(RECT rt)
    {
        return abs(rt.bottom - rt.top);
    }
};

