
float _NetworkTime;

float WaterHeight(float3 pos)
{
	float time = _NetworkTime;
	return sin(time + pos.x * 1.1f) * 0.2f;
}