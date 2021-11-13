
float _NetworkTime;

float WaterHeight(float3 pos)
{
	float time = _NetworkTime;
	return 
		sin(time + pos.z * -0.7 + sin(time * 0.5 + pos.x * 0.2)) * 0.23 + 
		(sin(time * 0.3 + pos.z * -0.07) * sin(time * 0.4 + pos.x * 0.08)) * 0.7;
}