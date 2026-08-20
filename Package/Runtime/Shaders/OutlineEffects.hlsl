#ifndef OUTLINE_EFFECTS_INCLUDED
#define OUTLINE_EFFECTS_INCLUDED

// Full precision outline calculation function handling both inner and outer passes
void OutlineEffects_float(
    UnityTexture2D MainTex,
    UnitySamplerState Sampler,
    float2 UV,
    float2 TexelSize,
    float4 Bounds,
    float OutlineThickness,
    float2 OutlineBias,
    float4 OutlineColor,
    float IsInnerOutline,
    float PixelSnap,
    float SquareCorners,
    out float4 OutColor,
    out float OutAlpha
)
{
    // Fallback bounds validation to prevent division by zero or invalid indexing
    float4 bounds = (Bounds.z > Bounds.x && Bounds.w > Bounds.y) ? Bounds : float4(0.0, 0.0, 1.0, 1.0);

    // Establish baseline UV coordinates, applying snap alignment if pixel-art mode is enabled
    float2 sampleBaseUV = UV;
    if (PixelSnap > 0.5)
    {
        // Snap coordinates to the nearest physical texel grid intersection
        sampleBaseUV = (floor(UV / TexelSize + 0.001) + 0.5) * TexelSize;
    }

    // Compute safe inset margins to prevent bleeding or clamping issues in packed texture atlases
    float2 halfRect = (bounds.zw - bounds.xy) * 0.5;
    float2 sampleInset = min(TexelSize * 0.5, halfRect);
    float2 sampleInsetMin = bounds.xy + sampleInset;
    float2 sampleInsetMax = bounds.zw - sampleInset;

    // Small boundary epsilon to handle floating-point precision inaccuracies
    float2 eps = TexelSize * 0.1;

    // Check if the center coordinate falls strictly inside target bounds
    bool centerInBounds = (sampleBaseUV.x >= bounds.x - eps.x && sampleBaseUV.x <= bounds.z + eps.x &&
                           sampleBaseUV.y >= bounds.y - eps.y && sampleBaseUV.y <= bounds.w + eps.y);
    
    float4 centerColor = float4(0.0, 0.0, 0.0, 0.0);
    if (centerInBounds)
    {
        // Clamp sample coordinate to inset limits to avoid neighbor bleeding
        float2 clampedCenterUV = clamp(sampleBaseUV, sampleInsetMin, sampleInsetMax);
        centerColor = MainTex.SampleLevel(Sampler, clampedCenterUV, 0);
    }
    float centerAlpha = centerColor.a;

    // Determine search radius based on requested outline thickness
    float maxDistance = max(0.0, OutlineThickness);
    int radius = (int) ceil(maxDistance);
    float minDistanceOuter = 999.0;
    float minDistanceInner = 999.0;
    const float kAlphaCutoff = 0.5;

    // Iterate through a local neighborhood matrix to evaluate surrounding alpha transparency
    for (int x = -radius; x <= radius; x++)
    {
        for (int y = -radius; y <= radius; y++)
        {
            float2 sampleOffset = float2((float) x, (float) y);
            
            // Apply horizontal/vertical bias scaling to stretch or squish the outline kernel footprint
            float2 biasedOffset = sampleOffset / max(OutlineBias, float2(0.001, 0.001));

            // Choose distance metric based on user setting
            float dist;
            if (SquareCorners > 0.5)
            {
                // Chebyshev distance for sharp, squared-off corners
                dist = max(abs(biasedOffset.x), abs(biasedOffset.y));
            }
            else
            {
                // Euclidean distance for smooth, rounded corners (default)
                dist = length(biasedOffset);
            }

            // Process only samples falling within the active thickness search boundary
            if (dist <= maxDistance + 0.5)
            {
                float2 sampleUV = sampleBaseUV + sampleOffset * TexelSize;
                
                // Confirm that the neighbor coordinate stays within safe sprite bounds
                bool isInsideBounds = (sampleUV.x >= bounds.x - eps.x && sampleUV.x <= bounds.z + eps.x &&
                                       sampleUV.y >= bounds.y - eps.y && sampleUV.y <= bounds.w + eps.y);
                
                float sampleAlpha = 0.0;
                if (isInsideBounds)
                {
                    float2 clampedSampleUV = clamp(sampleUV, sampleInsetMin, sampleInsetMax);
                    sampleAlpha = MainTex.SampleLevel(Sampler, clampedSampleUV, 0).a;
                }
                
                // Track minimum distance to opaque pixels (outer outline) vs transparent pixels (inner outline)
                if (sampleAlpha > kAlphaCutoff)
                {
                    minDistanceOuter = min(minDistanceOuter, dist);
                }
                else
                {
                    minDistanceInner = min(minDistanceInner, dist);
                }
            }
        }
    }

    // Generate smooth transition masks for both outer and inner outline regions
    float outerMask = saturate((maxDistance - minDistanceOuter) + 0.5);
    float innerMask = saturate((maxDistance - minDistanceInner) + 0.5);

    // Hard-step masks if pixel snap is active to eliminate anti-aliased blurring
    if (PixelSnap > 0.5)
    {
        outerMask = (minDistanceOuter <= maxDistance + 0.001) ? 1.0 : 0.0;
        innerMask = (minDistanceInner <= maxDistance + 0.001) ? 1.0 : 0.0;
    }

    // Exclude the center graphic silhouette from the outer outline mask
    outerMask = saturate(outerMask - centerAlpha);

    // Route pixel composition based on whether an inner or outer outline layout was requested
    if (IsInnerOutline > 0.5)
    {
        // Inner outline mode: Restrict outline rendering strictly inside the sprite's solid area
        float innerCoverage = step(0.01, centerAlpha);
        innerMask = saturate(innerMask) * innerCoverage;

        float alphaThreshold = (PixelSnap > 0.5) ? step(kAlphaCutoff, centerAlpha) : 1.0;

        OutColor = lerp(centerColor, OutlineColor, innerMask * OutlineColor.a) * alphaThreshold;
        OutAlpha = centerAlpha * alphaThreshold;
    }
    else
    {
        // Outer outline mode: Composite outline behind the main graphic texture
        innerMask = saturate(innerMask * centerAlpha);
        float4 outlineComposite = float4(OutlineColor.rgb, outerMask * OutlineColor.a);
        
        OutColor = lerp(outlineComposite, centerColor, centerAlpha);
        OutAlpha = max(centerAlpha, outerMask * OutlineColor.a);
    }
}

// Half precision wrapper function for compatibility with mobile or low-precision Shader Graph passes
void OutlineEffects_half(
    UnityTexture2D MainTex,
    UnitySamplerState Sampler,
    half2 UV,
    half2 TexelSize,
    half4 Bounds,
    half OutlineThickness,
    half2 OutlineBias,
    half4 OutlineColor,
    half IsInnerOutline,
    half PixelSnap,
    half SquareCorners,
    out half4 OutColor,
    out half OutAlpha
)
{
    float4 outColorFloat;
    float outAlphaFloat;

    // Forward parameters to the high-precision implementation to prevent visual artifacts
    OutlineEffects_float(
        MainTex,
        Sampler,
        (float2) UV,
        (float2) TexelSize,
        (float4) Bounds,
        (float) OutlineThickness,
        (float2) OutlineBias,
        (float4) OutlineColor,
        (float) IsInnerOutline,
        (float) PixelSnap,
        (float) SquareCorners,
        outColorFloat,
        outAlphaFloat
    );

    OutColor = (half4) outColorFloat;
    OutAlpha = (half) outAlphaFloat;
}

#endif
