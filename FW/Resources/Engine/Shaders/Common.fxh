#ifndef 	FXCCOMMON_FXH_
#define		FXCCOMMON_FXH_

// default option
#ifndef DIRECT3D_VERSION
	#define DIRECT3D_VERSION 0xb00 // VERSION 11
	//#define DIRECT3D_VERSION 0x900 // VERSION 9
#endif

// D3D defines
#define _D3D9_ 	(DIRECT3D_VERSION<=0x900)
#define _D3D1X_ (DIRECT3D_VERSION>=0xa00)

// helper functions
#define SCRIPT_DEFINITION( scriptClass, scriptOrder ) \
\
float Script : STANDARDSGLOBAL <	\
string UIWidget = "none";			\
string ScriptClass = scriptClass;	\
string ScriptOrder = scriptOrder;	\
string ScriptOutput = "color";		\
string Script = "Technique=Technique?Main:Main9;"; \
> = 0.8;					

// variables
#if _D3D9_
	#define BEGINCB( name )
#elif _D3D1X_
	#define BEGINCB( name )	cbuffer name {	
#else
	#error "[ERROR] shader version undefined"
#endif

#if _D3D9_
	#define ENDCB
#elif _D3D1X_
	#define ENDCB	};	
#else
	#error "[ERROR] shader version undefined"
#endif

// rendertarget
#define RT_COLOR( varName, ratiox, ratioy, colorFmt ) \
\
texture2D varName : RENDERCOLORTARGET <	\
	string  UIWidget = "none";			\
	float2 	ViewPortRatio = { ratiox, ratioy };	\
	int		Miplevels = 1;	\
	string 	Format = colorFmt; \
>;

#define RT_DEPTH( varName, ratiox, ratioy, depthFmt ) \
\
texture2D varName : RENDERDEPTHSTENCILTARGET < \
	string UIWidget = "None"; \
    float2 ViewPortRatio = { ratiox, ratioy }; \
    string Format = depthFmt; \
>;

// texture
#define TEXTURE2D_GENERATOR( varName, generatorFuncName, format, w, h) \
\
Texture2D varName \
< \
  string ResourceType = "2D"; \
  string Format = format; \
  string Function = generatorFuncName; \
  int Width = w; \
  int Height = h; \
>; 

#define TEXTURE2D( varName, resourceName ) \
\
Texture2D varName < \
    string ResourceName = resourceName; \
    string UIName =  #varName; \
    string ResourceType = "2D"; \
>;

#if _D3D9_
	#include "RenderState9.fxh"
#elif _D3D1X_
	#include "RenderState11.fxh"	
#else
	#error "[ERROR] shader version undefined"
#endif

// Common operation
#define ConvertSNormToUVCoord(v) (0.5*(v) + 0.5)
#define ConvertUVCoordToSNorm(v) (2*(v) - 1) 

#endif