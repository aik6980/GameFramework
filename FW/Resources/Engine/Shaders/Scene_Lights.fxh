#ifndef SCENE_LIGHTS_FXH
#define SCENE_LIGHTS_FXH

#define DIRLIGHT( objName, direction, color ) \
float3 direction : DIRECTION <		\
string Object = #objName;			\
string Space = "World";				\
>;									\
float3 color : COLOR <				\
string Object = #objName;			\
>;

#define POINTLIGHT( objName, position, color ) \
float3 position : POSITION <		\
string Object = #objName;			\
string Space = "World";				\
>;									\
float3 color : COLOR <				\
string Object = #objName;			\
>;

BEGINCB(cbLights)
	DIRLIGHT( MainLight, g_MainLightDir, g_MainLightCol)
	POINTLIGHT(PointLight0, g_PointLight0Pos, g_PointLight0Col)
ENDCB

#endif