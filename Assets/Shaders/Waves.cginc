
float _NetworkTime;

float WaterHeight(float3 pos)
{
	float time = _NetworkTime;
	float f0 = sin(time * 0.7 + pos.x * 0.2);
	float f1 = sin(time + pos.z * -0.7 + cos(time * 0.7 + pos.x * 0.1 - pos.z * 0.07) * 2 + f0);
	float f2 = sin(time * 0.3 + pos.z * -0.07);
	float f3 = sin(time * 0.4 + pos.x * 0.08);
	
	return f1 * 0.23 + (f2 * f3) * 0.7;
}