
 
float4 _EdgeColor;
float _Threshold=0.5;

// Sobel Edge Detection with Quantized Diagonals
inline void SobelEdgeDetection_float(float2 uv, out float4 edge)
{
	float2 texelSize = 1.1 / _ScreenParams.xy;

	float depthCenter = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture, uv).r;
	float depthRight = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture, uv + float2(texelSize.x, 0)).r;
	float depthLeft = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture, uv - float2(texelSize.x, 0)).r;
	float depthUp = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture, uv + float2(0, texelSize.y)).r;
	float depthDown = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture, uv - float2(0, texelSize.y)).r;

	// Compute Sobel edge gradients
	float dX = depthRight - depthLeft;
	float dY = depthUp - depthDown;

	// Normalize edge direction
	float2 edgeDir = normalize(float2(dX, dY));

	// Quantize edge directions to 2x1 or 3x1 steps
	float2 snappedEdgeDir;
	if (abs(edgeDir.x) > abs(edgeDir.y))
	{
		snappedEdgeDir = float2(sign(edgeDir.x), round(edgeDir.y * 2) / 2);
	}
	else
	{
		snappedEdgeDir = float2(round(edgeDir.x * 2) / 2, sign(edgeDir.y));
	}
	// Compute edge mask
	float edgeStrength = length(snappedEdgeDir);
	edge = float4(step(0.01, edgeStrength)*0,depthCenter*depthCenter*depthCenter*0,edgeDir.r,1);
}

